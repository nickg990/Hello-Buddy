<#
.SYNOPSIS
    Rebuilds the Azure MySQL database to the same level as local, in one command.

.DESCRIPTION
    The Azure MySQL Flexible Server is VNet-injected (private only), so it cannot be
    reached directly from a laptop. This script orchestrates the rebuild end-to-end
    from your local machine by driving Azure:

      1. terraform apply (data-tier)  -> ensures server + EMPTY canine_physiotherapy DB
      2. upload the 5 SQL scripts      -> published-programmes/reset/ blob prefix
      3. run a Container Apps JOB       -> mysql:8.0 inside the VNet pulls each script
                                          via SAS and pipes it to mysql (curl|mysql)
      4. wait for the job to Succeed + print its logs (SHOW TABLES verify)
      5. teardown                      -> delete the job + reset/ blobs

    A Container Apps Job (not 'az containerapp exec') is used on purpose: exec is
    an interactive, rate-limited WebSocket tunnel (HTTP 429, ~600s cooldown) and
    is quote-hostile. A job is non-interactive run-to-completion, and the command
    is passed via a YAML file + base64 env var, so no SAS '&'/quotes/spaces ever
    hit PowerShell/cmd/az parsers.

    Login rows are NOT created by SQL; the API PractitionerLoginSeedHostedService
    creates them at startup (requires Seed__PractitionerLogin__Enabled=true).

.NOTES
    Prerequisites:
      - az login (correct subscription)
      - The signed-in user has Get on the Key Vault (to read mysql-admin-password)
      - Terraform installed (only needed unless -SkipTerraform)
#>
[CmdletBinding()]
param(
    [string]$ResourceGroup = "rg-hellobuddy-prod",
    [string]$StorageAccount = "sthellobuddyprod",
    [string]$Container = "published-programmes",
    [string]$KeyVaultName = "kv-hellobuddy-prod",
    [string]$MySqlFqdn = "mysql-hellobuddy-prod.mysql.database.azure.com",
    [string]$MySqlUser = "hellobuddyadmin",
    [string]$Database = "canine_physiotherapy",
    [string]$EnvironmentName,                 # auto-detected if not supplied
    [string]$TempAppName = "mysql-reset",
    [int]$SasTtlMinutes = 60,
    [int]$LogTimeoutSeconds = 30,
    [switch]$SkipTerraform
)

$ErrorActionPreference = "Stop"

function Invoke-Az {
    # Plain (non-advanced) function: $args captures all tokens verbatim, so
    # short flags like '-o none' are passed straight to az instead of being
    # matched against PowerShell common parameters (-OutVariable/-OutBuffer).
    $output = & az @args
    if ($LASTEXITCODE -ne 0) {
        throw "az $($args -join ' ') failed (exit $LASTEXITCODE)."
    }
    return $output
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path
$sqlDir = Join-Path $repoRoot "Canine Physio Database\Build and Initialise"
$tfDir = Join-Path $repoRoot "Infrastructure\terraform\data-tier"

# Order matters: schema -> reference -> login/attribution -> demo seed -> email rollback.
$scripts = @(
    "Canine Physio DB Scripts v2.3 (fresh).sql",
    "Canine Physio DB Day 1 Initialise v2.4.sql",
    "Canine Physio DB Scripts - Increment 8 - Login and Attribution.sql",
    "Canine Physio DB MSc Assessment Seed v1.sql",
    "Canine Physio DB Scripts - Increment 9 Rollback - Remove Programme Email Send Audit.sql"
)

foreach ($s in $scripts) {
    $p = Join-Path $sqlDir $s
    if (-not (Test-Path $p)) { throw "Required SQL script not found: $p" }
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Azure MySQL rebuild -> parity with local" -ForegroundColor Cyan
Write-Host " Server   : $MySqlFqdn" -ForegroundColor Cyan
Write-Host " Database : $Database (will be DROPPED + recreated by v2.3)" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "WARNING: This DESTROYS existing Azure data and rebuilds from seed." -ForegroundColor Yellow

# ---------------------------------------------------------------------------
# 0) Confirm az login
# ---------------------------------------------------------------------------
Invoke-Az account show -o none | Out-Null

# ---------------------------------------------------------------------------
# 0b) Ensure the MySQL Flexible Server is running (it auto-stops via the
#     Automation Account schedule, DEC-002). Start it and wait until ready.
# ---------------------------------------------------------------------------
$mysqlName = $MySqlFqdn.Split('.')[0]
Write-Host "==> Checking MySQL server state ($mysqlName)" -ForegroundColor Cyan
$state = (Invoke-Az mysql flexible-server show -g $ResourceGroup -n $mysqlName --query "state" -o tsv)
Write-Host "    State: $state" -ForegroundColor DarkGray

if ($state -ne "Ready") {
    if ($state -eq "Stopped") {
        Write-Host "==> Starting MySQL server (this can take a few minutes)" -ForegroundColor Cyan
        Invoke-Az mysql flexible-server start -g $ResourceGroup -n $mysqlName -o none | Out-Null
    }
    else {
        Write-Host "==> MySQL not Ready (state=$state); waiting for it to become Ready" -ForegroundColor Yellow
    }

    $ready = $false
    for ($i = 0; $i -lt 60; $i++) {
        $state = & az mysql flexible-server show -g $ResourceGroup -n $mysqlName --query "state" -o tsv 2>$null
        if ($state -eq "Ready") { $ready = $true; break }
        Start-Sleep -Seconds 10
    }
    if (-not $ready) { throw "MySQL server '$mysqlName' did not reach Ready state (last: $state)." }
}
Write-Host "    MySQL server is Ready." -ForegroundColor DarkGray

# ---------------------------------------------------------------------------
# 1) Terraform apply (ensures server + empty DB exist). Skippable.
# ---------------------------------------------------------------------------
if (-not $SkipTerraform) {
    Write-Host "==> terraform apply (data-tier)" -ForegroundColor Cyan
    Push-Location $tfDir
    try {
        if (-not (Test-Path .terraform)) { terraform init | Out-Host }
        terraform apply -auto-approve | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "terraform apply failed." }
    }
    finally { Pop-Location }
}
else {
    Write-Host "==> SkipTerraform set; assuming server + empty DB already exist." -ForegroundColor Yellow
}

# ---------------------------------------------------------------------------
# 2) Read MySQL admin password from Key Vault (never printed)
# ---------------------------------------------------------------------------
Write-Host "==> Reading mysql-admin-password from Key Vault $KeyVaultName" -ForegroundColor Cyan
$MySqlPassword = (Invoke-Az keyvault secret show --vault-name $KeyVaultName --name "mysql-admin-password" --query "value" -o tsv)
if ([string]::IsNullOrWhiteSpace($MySqlPassword)) { throw "Could not read mysql-admin-password from Key Vault." }

# ---------------------------------------------------------------------------
# 3) Resolve the Container Apps environment (must be VNet-integrated one)
# ---------------------------------------------------------------------------
if (-not $EnvironmentName) {
    Write-Host "==> Auto-detecting Container Apps environment" -ForegroundColor Cyan
    $EnvironmentName = (Invoke-Az containerapp env list -g $ResourceGroup --query "[0].name" -o tsv)
    if ([string]::IsNullOrWhiteSpace($EnvironmentName)) { throw "No Container Apps environment found in $ResourceGroup." }
}
Write-Host "    Environment: $EnvironmentName" -ForegroundColor DarkGray

# ---------------------------------------------------------------------------
# 3b) Fetch the storage account key (control-plane; works with Contributor).
#     Avoids needing data-plane 'Storage Blob Data *' RBAC for the signed-in
#     user. All blob operations below use --auth-mode key.
# ---------------------------------------------------------------------------
Write-Host "==> Reading storage account key for $StorageAccount" -ForegroundColor Cyan
$StorageKey = (Invoke-Az storage account keys list --account-name $StorageAccount -g $ResourceGroup --query "[0].value" -o tsv)
if ([string]::IsNullOrWhiteSpace($StorageKey)) { throw "Could not read storage account key for $StorageAccount." }

# ---------------------------------------------------------------------------
# 4) Upload the 5 scripts to a temp blob prefix
# ---------------------------------------------------------------------------
Write-Host "==> Uploading SQL scripts to $Container/reset/" -ForegroundColor Cyan
foreach ($s in $scripts) {
    Invoke-Az storage blob upload `
        --account-name $StorageAccount --auth-mode key --account-key $StorageKey `
        --container-name $Container --name "reset/$s" `
        --file (Join-Path $sqlDir $s) --overwrite -o none | Out-Null
}

# ---------------------------------------------------------------------------
# 5) Run the SQL via a Container Apps JOB (not exec).
#    Why a job, not 'az containerapp exec':
#      - exec is an interactive WebSocket tunnel that is rate-limited (HTTP 429
#        with a ~600s cooldown) and quote-hostile -> the source of the flakiness.
#      - A job is non-interactive run-to-completion: no WebSocket, no rate limit.
#      - The command is supplied via a YAML file + a base64 env var, so NO SAS
#        '&', spaces or quotes ever pass through PowerShell/cmd/az parsers.
# ---------------------------------------------------------------------------
$expiry = (Get-Date).ToUniversalTime().AddMinutes($SasTtlMinutes).ToString("yyyy-MM-ddTHH:mmZ")

# Environment resource id + location for the job YAML.
$envId = (Invoke-Az containerapp env show -g $ResourceGroup -n $EnvironmentName --query "id" -o tsv)
$envLocation = (Invoke-Az containerapp env show -g $ResourceGroup -n $EnvironmentName --query "location" -o tsv)
if ([string]::IsNullOrWhiteSpace($envId)) { throw "Could not resolve environment id for $EnvironmentName." }

# Assemble the shell script: apply each script in order, then verify.
# 'set -e' aborts on the first failure so the job reports Failed.
$lines = @("set -e")
foreach ($s in $scripts) {
    $sas = (Invoke-Az storage blob generate-sas `
            --account-name $StorageAccount --auth-mode key --account-key $StorageKey `
            --container-name $Container --name "reset/$s" `
            --permissions r --expiry $expiry --https-only --full-uri -o tsv)
    if ([string]::IsNullOrWhiteSpace($sas)) { throw "Failed to generate SAS for reset/$s." }

    $lines += "echo '==> $s'"
    $lines += "curl -sSL '$sas' | mysql -h $MySqlFqdn -u $MySqlUser --ssl-mode=REQUIRED"
}
$lines += "echo '==> Verify'"
$lines += "mysql -h $MySqlFqdn -u $MySqlUser --ssl-mode=REQUIRED -D $Database -e 'SHOW TABLES; SELECT COUNT(*) AS practitioners FROM Practitioner;'"
$lines += "echo '==> DONE'"

$script = ($lines -join "`n")
$b64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($script))

# Delete any prior job of the same name (idempotent re-runs).
$existing = (Invoke-Az containerapp job list -g $ResourceGroup `
        --query "[?name=='$TempAppName'].name | [0]" -o tsv)
if (-not [string]::IsNullOrWhiteSpace($existing)) {
    Write-Host "==> Removing prior job '$TempAppName'" -ForegroundColor Yellow
    Invoke-Az containerapp job delete -g $ResourceGroup -n $TempAppName --yes -o none | Out-Null
}

# Delete any leftover temp CONTAINER APP of the same name from earlier exec-based
# runs (different resource type, but clean it up so the slate is clear).
$leftoverApp = (Invoke-Az containerapp list -g $ResourceGroup `
        --query "[?name=='$TempAppName'].name | [0]" -o tsv)
if (-not [string]::IsNullOrWhiteSpace($leftoverApp)) {
    Write-Host "==> Removing leftover container app '$TempAppName'" -ForegroundColor Yellow
    Invoke-Az containerapp delete -g $ResourceGroup -n $TempAppName --yes -o none | Out-Null
}

# Write the job YAML. Values are single-quoted: base64 and the password contain
# no single quotes, so YAML parses them literally with no escaping needed.
$yamlPath = Join-Path $env:TEMP "mysql-reset-job.yaml"
$yaml = @"
location: $envLocation
properties:
  environmentId: $envId
  configuration:
    triggerType: Manual
    replicaTimeout: 600
    replicaRetryLimit: 0
    manualTriggerConfig:
      parallelism: 1
      replicaCompletionCount: 1
  template:
    containers:
      - name: mysql-reset
        image: mysql:8.0
        resources:
          cpu: 0.5
          memory: 1Gi
        env:
          - name: MYSQL_PWD
            value: '$MySqlPassword'
          - name: B64SCRIPT
            value: '$b64'
        command:
          - /bin/sh
          - -c
        args:
          - echo "`$B64SCRIPT" | base64 -d | sh
"@
Set-Content -Path $yamlPath -Value $yaml -Encoding ASCII

try {
    Write-Host "==> Creating job '$TempAppName'" -ForegroundColor Cyan
    Invoke-Az containerapp job create -g $ResourceGroup -n $TempAppName --yaml $yamlPath -o none | Out-Null

    Write-Host "==> Starting job and waiting for completion" -ForegroundColor Cyan
    $startJson = (Invoke-Az containerapp job start -g $ResourceGroup -n $TempAppName -o json) | ConvertFrom-Json
    $execName = $startJson.name
    if ([string]::IsNullOrWhiteSpace($execName)) { throw "Job start did not return an execution name." }
    Write-Host "    Execution: $execName" -ForegroundColor DarkGray

    $status = "Running"
    for ($i = 0; $i -lt 60; $i++) {
        Start-Sleep -Seconds 10
        $status = & az containerapp job execution show -g $ResourceGroup -n $TempAppName `
            --job-execution-name $execName --query "properties.status" -o tsv 2>$null
        Write-Host "    status: $status" -ForegroundColor DarkGray
        if ($status -in @("Succeeded", "Failed", "Degraded")) { break }
    }

    # Best-effort log fetch, bounded by a timeout so it can never hang the run.
    # 'az containerapp job logs show' reads from Log Analytics, which lags on
    # ingestion and can block; we run it in a background job and give up after
    # $LogTimeoutSeconds. This is diagnostics only — success/failure was already
    # decided by the execution status above, so a missing log never hides an error.
    Write-Host "==> Job logs (best-effort, ${LogTimeoutSeconds}s timeout)" -ForegroundColor Cyan
    $logJob = Start-Job -ScriptBlock {
        param($rg, $name, $exec)
        az containerapp job logs show -g $rg -n $name --container "mysql-reset" --execution $exec --tail 200 2>$null
    } -ArgumentList $ResourceGroup, $TempAppName, $execName

    if (Wait-Job $logJob -Timeout $LogTimeoutSeconds) {
        Receive-Job $logJob | ForEach-Object { Write-Host $_ }
    }
    else {
        Write-Host "    (log fetch timed out; skipping. Job status was: $status)" -ForegroundColor DarkGray
        Stop-Job $logJob -ErrorAction SilentlyContinue
    }
    Remove-Job $logJob -Force -ErrorAction SilentlyContinue

    if ($status -ne "Succeeded") {
        throw "Job did not succeed (status: $status). Inspect Log Analytics for container 'mysql-reset', execution '$execName'."
    }
}
finally {
    # -----------------------------------------------------------------------
    # 6) Teardown (always, even on failure)
    # -----------------------------------------------------------------------
    Write-Host "==> Teardown: deleting job + reset/ blobs" -ForegroundColor Cyan
    try { & az containerapp job delete -g $ResourceGroup -n $TempAppName --yes -o none 2>&1 | Out-Null } catch {}
    if (Test-Path $yamlPath) { Remove-Item $yamlPath -Force -ErrorAction SilentlyContinue }
    if ($StorageKey) {
        try {
            & az storage blob delete-batch --account-name $StorageAccount --auth-mode key --account-key $StorageKey `
                --source $Container --pattern "reset/*" -o none 2>&1 | Out-Null
        } catch {}
    }
}

Write-Host ""
Write-Host "Azure DB rebuilt to local level." -ForegroundColor Green
Write-Host "NOTE: restart the API container app so PractitionerLoginSeedHostedService creates login rows" -ForegroundColor Yellow
Write-Host "      (requires Seed__PractitionerLogin__Enabled=true + InitialPassword on the API app)." -ForegroundColor Yellow
