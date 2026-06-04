[CmdletBinding()]
param(
    [string]$Server = "localhost",
    [int]$Port = 3306,
    [string]$User = "root",
    [string]$Password = "P3nyf@n01",
    [string]$MySqlExePath,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$resetScript = Join-Path $PSScriptRoot "reset-local-admin-data.ps1"
if (-not (Test-Path $resetScript)) {
    throw "Could not find reset script at '$resetScript'."
}

& $resetScript `
    -Server $Server `
    -Port $Port `
    -User $User `
    -Password $Password `
    -MySqlExePath $MySqlExePath `
    -Force:$Force
