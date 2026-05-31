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
    [switch] $SkipBuild,
    [switch] $FoundationOnly
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
    @{ Name = "hello-buddy-ui"; Dockerfile = "Dockerfile.ui"; VarName = "ui_app_image" }
    @{ Name = "hello-buddy-api"; Dockerfile = "Dockerfile.api"; VarName = "api_app_image" }
    @{ Name = "hello-buddy-pdf"; Dockerfile = "Dockerfile.pdf"; VarName = "pdf_app_image" }
)

foreach ($img in $images) {
    $img.Ref = "$loginServer/$($img.Name):$ImageTag"
}

Write-Host "==> Images will be tagged as:" -ForegroundColor Cyan
$images | ForEach-Object { Write-Host "    $($_.Ref)" }

function Get-ImageVarArgs {
    $args = @()
    foreach ($img in $images) {
        $args += "-var"
        $args += "$($img.VarName)=$($img.Ref)"
    }
    return $args
}

# ----------------------------------------------------------------------------
# Phase A — foundation
# ----------------------------------------------------------------------------
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

# ----------------------------------------------------------------------------
# Build + push images (az acr build — DEC-006)
# ----------------------------------------------------------------------------
if (-not $SkipBuild) {
    Push-Location $adminSolution
    try {
        foreach ($img in $images) {
            Write-Host "==> az acr build -t $($img.Name):$ImageTag -f $($img.Dockerfile)" -ForegroundColor Cyan
            az acr build `
                --registry $acrName `
                --image "$($img.Name):$ImageTag" `
                --file $img.Dockerfile `
                . | Out-Host
            if ($LASTEXITCODE -ne 0) { throw "az acr build failed for $($img.Name)." }
        }
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Host "==> SkipBuild set; using existing image tags" -ForegroundColor Yellow
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
