# Hello Buddy Cloud Admin — Two-phase container deploy (R2-S7: includes migration job)
#
# Two-phase Terraform apply with docker builds between:
#   Phase A : Foundation (ACR, Storage, Log Analytics, App Insights,
#             Container Apps Environment, UAMIs x4, role grants, KV secrets).
#   Build   : az acr build for ui, api, pdf, migrate images.
#   Phase B : Three Container Apps + migration job referencing freshly pushed images.
#
# Re-run safe: re-running produces a no-op apply if nothing has changed
# in Terraform and pushes new image tags based on the git short SHA.
#
# Migration opt-in:
#   -RunMigrations  After apply, trigger caj-hellobuddy-migrate and tail its logs.
#                   Does NOT run automatically — must be explicitly passed.

[CmdletBinding()]
param(
    [string] $ImageTag,
    [string] $SubnetAppsId,
    [string] $ResourceGroupName = "rg-hellobuddy-prod",
    [switch] $SkipBuild,
    [switch] $FoundationOnly,
    [switch] $AppsOnly,
    [switch] $UiOnly,
    [switch] $ApiOnly,
    [switch] $PdfOnly,
    [switch] $MigrateOnly,
    [switch] $RunMigrations
)

$ErrorActionPreference = "Stop"

# ----------------------------------------------------------------------------
# Helpers
# ----------------------------------------------------------------------------
$repoRoot = (Resolve-Path "$PSScriptRoot\..\..\..").Path
$adminSolution = Join-Path $repoRoot "Canine Physio Admin"
$tfDir = $PSScriptRoot

if (-not $ImageTag) {
    Push-Location $repoRoot
    try {
        $sha = (git rev-parse --short HEAD) 2>$null
    }
    catch {
        $sha = $null
    }
    Pop-Location
    if (-not $sha) { $sha = Get-Date -Format "yyyyMMddHHmm" }
    $ImageTag = $sha
}

$acrName = "acrhellobuddyprod"
$loginServer = "$acrName.azurecr.io"

$images = @(
    @{ Name = "hello-buddy-ui";      Dockerfile = "Dockerfile.ui";      VarName = "ui_app_image";      Component = "ui";      AppName = "ca-hello-buddy-ui";     StreamingUnsafe = $false; BuildContext = $adminSolution }
    @{ Name = "hello-buddy-api";     Dockerfile = "Dockerfile.api";     VarName = "api_app_image";     Component = "api";     AppName = "ca-hello-buddy-api";    StreamingUnsafe = $false; BuildContext = $adminSolution }
    # PDF image runs apt-get install chromium whose progress output contains the Unicode arrow
    # character '→' (U+2192) which cp1252 cannot encode. Always skip streaming for this image.
    @{ Name = "hello-buddy-pdf";     Dockerfile = "Dockerfile.pdf";     VarName = "pdf_app_image";     Component = "pdf";     AppName = "ca-hello-buddy-pdf";    StreamingUnsafe = $true;  BuildContext = $adminSolution }
    # Migration job image — debian-slim + mysql-client + migrate.sh + SQL files (R2-S7).
    # Build context is the db-migrate folder itself so source packing never walks the
    # rest of the repo (avoids Windows long-path errors in unrelated projects).
    # Also runs apt-get, so treat as StreamingUnsafe.
    @{ Name = "hello-buddy-migrate"; Dockerfile = "Dockerfile.migrate"; VarName = "migrate_app_image"; Component = "migrate"; AppName = "caj-hellobuddy-migrate"; StreamingUnsafe = $true;  BuildContext = (Join-Path $repoRoot "Infrastructure\tools\db-migrate") }
)

foreach ($img in $images) {
    $img.Ref = "$loginServer/$($img.Name):$ImageTag"
}

Write-Host "==> Images will be tagged as:" -ForegroundColor Cyan
$images | ForEach-Object { Write-Host "    $($_.Ref)" }

$componentSelectionCount = @($UiOnly, $ApiOnly, $PdfOnly, $MigrateOnly).Where({ $_ }).Count
if ($componentSelectionCount -gt 1) {
    throw "Select only one of -UiOnly, -ApiOnly, -PdfOnly, or -MigrateOnly."
}

$componentDeploy = $componentSelectionCount -eq 1
if ($componentDeploy -and $FoundationOnly) {
    throw "-FoundationOnly cannot be combined with component-only deployment switches."
}

$targetImages = if ($UiOnly) {
    @($images | Where-Object { $_.Component -eq "ui" })
}
elseif ($ApiOnly) {
    @($images | Where-Object { $_.Component -eq "api" })
}
elseif ($PdfOnly) {
    @($images | Where-Object { $_.Component -eq "pdf" })
}
elseif ($MigrateOnly) {
    @($images | Where-Object { $_.Component -eq "migrate" })
}
else {
    $images
}

function Get-ImageVarArgs {
    $args = @()
    foreach ($img in $images) {
        $args += "-var"
        $args += "$($img.VarName)=$($img.Ref)"
    }
    return $args
}

# ----------------------------------------------------------------------------
# Phase A — foundation (skipped when -AppsOnly is set)
# ----------------------------------------------------------------------------
if ($AppsOnly) {
    Write-Host "==> AppsOnly set; skipping Phase A foundation apply." -ForegroundColor Yellow

    Push-Location $tfDir
    try {
        if (-not (Test-Path .terraform)) {
            Write-Host "==> terraform init"
            terraform init | Out-Host
        }
    }
    finally {
        Pop-Location
    }
}
elseif (-not $componentDeploy) {
    Push-Location $tfDir
    try {
        if (-not (Test-Path .terraform)) {
            Write-Host "==> terraform init"
            terraform init | Out-Host
        }

        Write-Host "==> Phase A: terraform apply (foundation, deploy_container_apps=false)" -ForegroundColor Cyan
        $imgArgs = Get-ImageVarArgs
        $subnetArg = if ($SubnetAppsId) { @("-var", "subnet_apps_id=$SubnetAppsId") } else { @() }
        terraform apply -auto-approve `
            -var "deploy_container_apps=false" `
            @subnetArg `
            @imgArgs | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "Phase A apply failed." }
    }
    finally {
        Pop-Location
    }

    if ($FoundationOnly) {
        Write-Host "==> FoundationOnly set; stopping here." -ForegroundColor Yellow
        return
    }
}

# ----------------------------------------------------------------------------
# Build + push images (az acr build — DEC-006)
# ----------------------------------------------------------------------------
if (-not $SkipBuild) {
    # az CLI streams remote build logs through a Python process that defaults
    # to cp1252 on Windows; non-ANSI bytes (e.g. apt's '→') crash the CLI even
    # though the ACR build itself is fine. Belt-and-braces: enable Python's
    # full UTF-8 mode in the child process, set an error-tolerant io encoding,
    # switch the active console code page to UTF-8 (65001), and align the
    # PowerShell host's own output encoding. The ':replace' suffix is the
    # final safety net so any stray byte becomes '?' rather than a crash.
    $env:PYTHONUTF8       = '1'
    $env:PYTHONIOENCODING = 'utf-8:replace'

    $prevChcp = $null
    try {
        $chcpOut = chcp 2>$null
        if ($chcpOut -match '(\d+)\s*$') { $prevChcp = [int]$Matches[1] }
        chcp 65001 | Out-Null
    } catch {}
    $prevOutEnc = [Console]::OutputEncoding
    try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch {}

    # Build each image using its own build context (ui/api/pdf use AdminSolution, migrate uses repoRoot).
    try {
        foreach ($img in $targetImages) {
            $buildContext = $img.BuildContext
            Push-Location $buildContext
            try {
                if ($img.StreamingUnsafe) {
                    # This image runs apt-get during build; apt progress output contains Unicode
                    # characters that cp1252 cannot encode. Skip streaming entirely.
                    Write-Host "==> az acr build -t $($img.Name):$ImageTag -f $($img.Dockerfile) [--no-logs: apt Unicode output]" -ForegroundColor Cyan
                    az acr build `
                        --registry $acrName `
                        --image "$($img.Name):$ImageTag" `
                        --file $img.Dockerfile `
                        --no-logs `
                        . | Out-Host
                    if ($LASTEXITCODE -ne 0) { throw "az acr build failed for $($img.Name)." }
                }
                else {
                    Write-Host "==> az acr build -t $($img.Name):$ImageTag -f $($img.Dockerfile)" -ForegroundColor Cyan
                    az acr build `
                        --registry $acrName `
                        --image "$($img.Name):$ImageTag" `
                        --file $img.Dockerfile `
                        . | Out-Host
                    if ($LASTEXITCODE -ne 0) {
                        Write-Host "==> Streamed build failed for $($img.Name); retrying with --no-logs (logs available via 'az acr task logs')" -ForegroundColor Yellow
                        az acr build `
                            --registry $acrName `
                            --image "$($img.Name):$ImageTag" `
                            --file $img.Dockerfile `
                            --no-logs `
                            . | Out-Host
                        if ($LASTEXITCODE -ne 0) { throw "az acr build failed for $($img.Name)." }
                    }
                }
            }
            finally {
                Pop-Location
            }
        }
    }
    finally {
        try { if ($prevChcp) { chcp $prevChcp | Out-Null } } catch {}
        try { [Console]::OutputEncoding = $prevOutEnc } catch {}
    }
}
else {
    Write-Host "==> SkipBuild set; using existing image tags" -ForegroundColor Yellow
}

# ----------------------------------------------------------------------------
# Component-only deployment — update the selected Container App image directly.
# ----------------------------------------------------------------------------
if ($componentDeploy) {
    foreach ($img in $targetImages) {
        Write-Host "==> az containerapp update --name $($img.AppName) --image $($img.Ref)" -ForegroundColor Cyan
        az containerapp update `
            --resource-group $ResourceGroupName `
            --name $img.AppName `
            --image $img.Ref | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "az containerapp update failed for $($img.AppName)." }
    }

    Write-Host "==> Component-only deployment completed." -ForegroundColor Green
    return
}

# ----------------------------------------------------------------------------
# Phase B — Container Apps + migration job
# ----------------------------------------------------------------------------
Push-Location $tfDir
try {
    Write-Host "==> Phase B: terraform apply (Container Apps + migration job, deploy_container_apps=true)" -ForegroundColor Cyan
    $imgArgs = Get-ImageVarArgs
    $subnetArg = if ($SubnetAppsId) { @("-var", "subnet_apps_id=$SubnetAppsId") } else { @() }
    terraform apply -auto-approve `
        -var "deploy_container_apps=true" `
        @subnetArg `
        @imgArgs | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Phase B apply failed." }

    Write-Host ""
    Write-Host "==> Outputs:" -ForegroundColor Green
    terraform output | Out-Host
}
finally {
    Pop-Location
}

# ----------------------------------------------------------------------------
# Optional: trigger migration job (-RunMigrations)
# Not run automatically — must be passed explicitly.
# ----------------------------------------------------------------------------
if ($RunMigrations) {
    $jobName = "caj-hellobuddy-migrate"
    Write-Host ""
    Write-Host "==> -RunMigrations: starting $jobName" -ForegroundColor Cyan
    $execJson = az containerapp job start `
        --resource-group $ResourceGroupName `
        --name $jobName `
        --output json 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Failed to start migration job: $execJson" }

    $execName = ($execJson | ConvertFrom-Json).name
    Write-Host "==> Job execution started: $execName"
    Write-Host "==> Polling for completion (max 5 min)..."

    $timeout  = [DateTime]::UtcNow.AddMinutes(5)
    $jobStatus = $null
    while ([DateTime]::UtcNow -lt $timeout) {
        Start-Sleep -Seconds 10
        $statusJson = az containerapp job execution show `
            --resource-group $ResourceGroupName `
            --name $jobName `
            --job-execution-name $execName `
            --output json 2>&1
        if ($LASTEXITCODE -eq 0) {
            $jobStatus = ($statusJson | ConvertFrom-Json).properties.status
            Write-Host "    status: $jobStatus"
            if ($jobStatus -in @("Succeeded", "Failed", "Unknown")) { break }
        }
    }

    Write-Host ""
    Write-Host "==> Fetching job logs:" -ForegroundColor Cyan
    # Container App Job logs are surfaced through Log Analytics (there is no
    # 'job execution logs show' subcommand). Ingestion lags by a minute or two,
    # so poll the workspace a few times before giving up.
    $logWorkspaceName = "log-hellobuddy-prod"
    $customerId = az monitor log-analytics workspace show `
        --resource-group $ResourceGroupName `
        --workspace-name $logWorkspaceName `
        --query customerId -o tsv 2>$null
    if ($customerId) {
        $kql = "ContainerAppConsoleLogs_CL | where ContainerGroupName_s == '$execName' | project TimeGenerated, Log_s | order by TimeGenerated asc | take 200"
        $logsShown = $false
        for ($i = 0; $i -lt 6; $i++) {
            $logRows = az monitor log-analytics query `
                --workspace $customerId `
                --analytics-query $kql `
                --output table 2>$null
            if ($LASTEXITCODE -eq 0 -and $logRows) {
                $logRows | Out-Host
                $logsShown = $true
                break
            }
            Write-Host "    (logs not yet available, retrying in 20s...)"
            Start-Sleep -Seconds 20
        }
        if (-not $logsShown) {
            Write-Host "    Logs not yet ingested. Query later with:" -ForegroundColor Yellow
            Write-Host "    az monitor log-analytics query --workspace $customerId --analytics-query `"$kql`" --output table"
        }
    }
    else {
        Write-Host "    Could not resolve Log Analytics workspace '$logWorkspaceName' to fetch logs." -ForegroundColor Yellow
    }

    if ($jobStatus -ne "Succeeded") {
        throw "Migration job finished with status '$jobStatus'. Check logs above."
    }
    Write-Host "==> Migration job completed successfully." -ForegroundColor Green
}
