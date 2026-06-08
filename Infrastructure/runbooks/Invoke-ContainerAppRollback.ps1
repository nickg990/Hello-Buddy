[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("ui", "api", "pdf")]
    [string]$Component,
    [string]$ResourceGroupName = "rg-hellobuddy-prod",
    [string]$TargetRevision
)

$ErrorActionPreference = "Stop"

$appName = switch ($Component) {
    "ui" { "ca-hello-buddy-ui" }
    "api" { "ca-hello-buddy-api" }
    "pdf" { "ca-hello-buddy-pdf" }
}

Write-Host "==> Listing revisions for $appName" -ForegroundColor Cyan
$revisionsJson = az containerapp revision list --resource-group $ResourceGroupName --name $appName --output json
if ($LASTEXITCODE -ne 0) {
    throw "Failed to list revisions for $appName"
}

$revisions = $revisionsJson | ConvertFrom-Json
if (-not $revisions -or $revisions.Count -lt 1) {
    throw "No revisions found for $appName"
}

if (-not $TargetRevision) {
    $candidate = $revisions |
        Where-Object { $_.properties.active -eq $true } |
        Sort-Object -Property { $_.properties.createdTime } -Descending |
        Select-Object -Skip 1 -First 1

    if (-not $candidate) {
        throw "Could not infer previous active revision. Pass -TargetRevision explicitly."
    }

    $TargetRevision = $candidate.name
}

Write-Host "==> Rolling back $appName to revision $TargetRevision" -ForegroundColor Yellow
az containerapp ingress traffic set --resource-group $ResourceGroupName --name $appName --revision-weight "$TargetRevision=100"
if ($LASTEXITCODE -ne 0) {
    throw "Failed to set rollback traffic for $appName"
}

Write-Host "Rollback command completed for $appName." -ForegroundColor Green
