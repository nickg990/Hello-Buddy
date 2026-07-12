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
#                   When passed ALONE (no build/deploy switches) it skips the
#                   Terraform apply and image builds entirely and just starts +
#                   tails the existing job — a lightweight "run migrations now".
#
# Exercise library import:
#   -ExerciseImport         Generate exercise-import.sql from the markdown, rebuild
#                           the migrate image, apply the job with the chosen import
#                           mode, run it, then reset the mode to "off".
#   -ImportMode update|replace
#                           update  = upsert exercises (nothing deleted).
#                           replace = delete exercises not referenced by any
#                                     SessionExercise, then upsert (in-use ones
#                                     are retained and overwritten). Default: update.

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
    [switch] $RunMigrations,
    [switch] $ExerciseImport,
    [ValidateSet("update", "replace")]
    [string] $ImportMode = "update",
    [switch] $ResetTracking
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
# Start the migration/import job and tail its logs. Shared by -RunMigrations
# and -ExerciseImport. Throws if the job finishes in any state but Succeeded.
# ----------------------------------------------------------------------------
function Invoke-MigrateJobRun {
    param(
        [Parameter(Mandatory)] [string] $ResourceGroupName,
        [string] $JobName = "caj-hellobuddy-migrate"
    )

    Write-Host ""
    Write-Host "==> Starting job $JobName" -ForegroundColor Cyan
    $execJson = az containerapp job start `
        --resource-group $ResourceGroupName `
        --name $JobName `
        --output json 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Failed to start job: $execJson" }

    $execName = ($execJson | ConvertFrom-Json).name
    Write-Host "==> Job execution started: $execName"
    Write-Host "==> Polling for completion (max 5 min)..."

    $timeout = [DateTime]::UtcNow.AddMinutes(5)
    $jobStatus = $null
    while ([DateTime]::UtcNow -lt $timeout) {
        Start-Sleep -Seconds 10
        $statusJson = az containerapp job execution show `
            --resource-group $ResourceGroupName `
            --name $JobName `
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
        throw "Job '$JobName' finished with status '$jobStatus'. Check logs above."
    }
    Write-Host "==> Job completed successfully." -ForegroundColor Green
}

# ----------------------------------------------------------------------------
# Return the currently-deployed image reference for a Container App, so a
# job-only Terraform apply can keep the web apps pinned to their live images
# (the *_app_image variables default to "" and would otherwise blank them).
# ----------------------------------------------------------------------------
function Get-LiveAppImage {
    param(
        [Parameter(Mandatory)] [string] $ResourceGroupName,
        [Parameter(Mandatory)] [string] $AppName
    )
    $ref = az containerapp show `
        --resource-group $ResourceGroupName `
        --name $AppName `
        --query "properties.template.containers[0].image" -o tsv 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($ref)) {
        throw "Could not read live image for $AppName; refusing to re-apply Terraform (would blank the app image)."
    }
    return $ref.Trim()
}

# ----------------------------------------------------------------------------
# Standalone -RunMigrations: when passed with no build/deploy switches, skip
# Terraform and image builds entirely and just start + tail the existing job.
# ----------------------------------------------------------------------------
if ($RunMigrations -and -not ($ExerciseImport -or $FoundationOnly -or $AppsOnly -or $componentDeploy)) {
    Write-Host "==> -RunMigrations (standalone): starting existing job without deploy." -ForegroundColor Cyan
    Invoke-MigrateJobRun -ResourceGroupName $ResourceGroupName -JobName "caj-hellobuddy-migrate"
    return
}

# ----------------------------------------------------------------------------
# -ExerciseImport: generate SQL from the markdown, rebuild the migrate image,
# apply the job with EXERCISE_IMPORT=<mode>, run it, then reset the mode to off.
# The web apps are left untouched (pinned to their live images).
# ----------------------------------------------------------------------------
if ($ExerciseImport) {
    if ($FoundationOnly -or $AppsOnly -or $componentDeploy) {
        throw "-ExerciseImport cannot be combined with -FoundationOnly, -AppsOnly, or component-only switches."
    }

    $migrateImg = $images | Where-Object { $_.Component -eq "migrate" } | Select-Object -First 1
    $importGenerator = Join-Path $migrateImg.BuildContext "exercise-import\Generate-ExerciseImportSql.ps1"

    Write-Host "==> Exercise import (mode=$ImportMode)" -ForegroundColor Cyan
    Write-Host "==> Generating exercise-import.sql from markdown"
    & $importGenerator
    if ($LASTEXITCODE -ne 0) { throw "Exercise import SQL generation failed." }

    # Build + push the migrate image (bakes in the freshly generated SQL).
    if (-not $SkipBuild) {
        $env:PYTHONUTF8 = '1'
        $env:PYTHONIOENCODING = 'utf-8:replace'
        Push-Location $migrateImg.BuildContext
        try {
            Write-Host "==> az acr build -t $($migrateImg.Name):$ImageTag -f $($migrateImg.Dockerfile) [--no-logs]" -ForegroundColor Cyan
            az acr build `
                --registry $acrName `
                --image "$($migrateImg.Name):$ImageTag" `
                --file $migrateImg.Dockerfile `
                --no-logs `
                . | Out-Host
            if ($LASTEXITCODE -ne 0) { throw "az acr build failed for $($migrateImg.Name)." }
        }
        finally {
            Pop-Location
        }
    }
    else {
        Write-Host "==> SkipBuild set; using existing migrate image tag $ImageTag" -ForegroundColor Yellow
    }

    # Pin the web apps to their live images so this job-only apply cannot blank them.
    $liveUi = Get-LiveAppImage -ResourceGroupName $ResourceGroupName -AppName "ca-hello-buddy-ui"
    $liveApi = Get-LiveAppImage -ResourceGroupName $ResourceGroupName -AppName "ca-hello-buddy-api"
    $livePdf = Get-LiveAppImage -ResourceGroupName $ResourceGroupName -AppName "ca-hello-buddy-pdf"

    $pinnedImageArgs = @(
        "-var", "ui_app_image=$liveUi",
        "-var", "api_app_image=$liveApi",
        "-var", "pdf_app_image=$livePdf",
        "-var", "migrate_app_image=$($migrateImg.Ref)"
    )
    $subnetArg = if ($SubnetAppsId) { @("-var", "subnet_apps_id=$SubnetAppsId") } else { @() }

    Push-Location $tfDir
    try {
        if (-not (Test-Path .terraform)) {
            Write-Host "==> terraform init"
            terraform init | Out-Host
        }

        Write-Host "==> terraform apply (job env EXERCISE_IMPORT=$ImportMode)" -ForegroundColor Cyan
        terraform apply -auto-approve `
            -var "deploy_container_apps=true" `
            -var "exercise_import_mode=$ImportMode" `
            @subnetArg `
            @pinnedImageArgs | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "Terraform apply (exercise import mode) failed." }
    }
    finally {
        Pop-Location
    }

    try {
        Invoke-MigrateJobRun -ResourceGroupName $ResourceGroupName -JobName "caj-hellobuddy-migrate"
    }
    finally {
        # Always reset the mode back to off so a later migration run is unaffected.
        Push-Location $tfDir
        try {
            Write-Host "==> terraform apply (reset exercise_import_mode=off)" -ForegroundColor Cyan
            terraform apply -auto-approve `
                -var "deploy_container_apps=true" `
                -var "exercise_import_mode=off" `
                @subnetArg `
                @pinnedImageArgs | Out-Host
            if ($LASTEXITCODE -ne 0) { Write-Host "WARNING: failed to reset exercise_import_mode to off -- do so manually." -ForegroundColor Yellow }
        }
        finally {
            Pop-Location
        }
    }

    Write-Host "==> Exercise import completed." -ForegroundColor Green
    return
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
    $resetArg = if ($ResetTracking) { @("-var", "migrate_reset_tracking=true") } else { @() }
    terraform apply -auto-approve `
        -var "deploy_container_apps=true" `
        @subnetArg `
        @resetArg `
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
# Not run automatically — must be passed explicitly. The standalone fast path
# (no build/deploy switches) is handled earlier and returns before this point;
# here we run after a full/partial apply has completed.
# ----------------------------------------------------------------------------
if ($RunMigrations) {
    Invoke-MigrateJobRun -ResourceGroupName $ResourceGroupName -JobName "caj-hellobuddy-migrate"
}
