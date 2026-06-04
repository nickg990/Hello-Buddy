# Hello Buddy Cloud Admin — Day 4a three-container deploy
#
# Two-phase Terraform apply with three docker builds between:
#   Phase A : Foundation (ACR, Storage, Log Analytics, App Insights,
#             Container Apps Environment, UAMIs x3, role grants, KV secrets).
#   Build   : az acr build for ui, api, pdf images.
#   Phase B : Three Container App resources referencing the freshly pushed images.
#
# Re-run safe: re-running produces a no-op apply if nothing has changed
# in Terraform and pushes new image tags based on the git short SHA.

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
    [switch] $PdfOnly
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
    @{ Name = "hello-buddy-ui"; Dockerfile = "Dockerfile.ui"; VarName = "ui_app_image"; Component = "ui"; AppName = "ca-hello-buddy-ui";  StreamingUnsafe = $false }
    @{ Name = "hello-buddy-api"; Dockerfile = "Dockerfile.api"; VarName = "api_app_image"; Component = "api"; AppName = "ca-hello-buddy-api"; StreamingUnsafe = $false }
    # PDF image runs apt-get install chromium whose progress output contains the Unicode arrow
    # character '→' (U+2192) which cp1252 cannot encode. Always skip streaming for this image.
    @{ Name = "hello-buddy-pdf"; Dockerfile = "Dockerfile.pdf"; VarName = "pdf_app_image"; Component = "pdf"; AppName = "ca-hello-buddy-pdf"; StreamingUnsafe = $true  }
)

foreach ($img in $images) {
    $img.Ref = "$loginServer/$($img.Name):$ImageTag"
}

Write-Host "==> Images will be tagged as:" -ForegroundColor Cyan
$images | ForEach-Object { Write-Host "    $($_.Ref)" }

$componentSelectionCount = @($UiOnly, $ApiOnly, $PdfOnly).Where({ $_ }).Count
if ($componentSelectionCount -gt 1) {
    throw "Select only one of -UiOnly, -ApiOnly, or -PdfOnly."
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

    Push-Location $adminSolution
    try {
        foreach ($img in $targetImages) {
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
    }
    finally {
        Pop-Location
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
# Phase B — three Container Apps
# ----------------------------------------------------------------------------
Push-Location $tfDir
try {
    Write-Host "==> Phase B: terraform apply (Container Apps, deploy_container_apps=true)" -ForegroundColor Cyan
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
