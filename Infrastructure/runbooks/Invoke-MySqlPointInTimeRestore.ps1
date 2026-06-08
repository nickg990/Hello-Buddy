[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [string]$ServerName,
    [Parameter(Mandatory = $true)]
    [string]$RestoreServerName,
    [Parameter(Mandatory = $true)]
    [string]$RestoreTimeUtc,
    [string]$Location = "uksouth"
)

$ErrorActionPreference = "Stop"

# Ensure the provided restore time is valid ISO UTC and in the past.
$parsedRestoreTime = [DateTime]::Parse($RestoreTimeUtc)
if ($parsedRestoreTime.Kind -ne [System.DateTimeKind]::Utc) {
    throw "RestoreTimeUtc must be a UTC timestamp, for example 2026-06-08T08:30:00Z"
}

if ($parsedRestoreTime -ge [DateTime]::UtcNow) {
    throw "RestoreTimeUtc must be in the past."
}

Write-Host "==> Starting point-in-time restore" -ForegroundColor Cyan
Write-Host "Source server:  $ServerName"
Write-Host "Target server:  $RestoreServerName"
Write-Host "Restore time:   $RestoreTimeUtc"

az mysql flexible-server restore `
    --resource-group $ResourceGroupName `
    --name $RestoreServerName `
    --source-server $ServerName `
    --restore-time $RestoreTimeUtc `
    --location $Location

if ($LASTEXITCODE -ne 0) {
    throw "Point-in-time restore command failed."
}

Write-Host "Restore command submitted successfully." -ForegroundColor Green
