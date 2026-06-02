[CmdletBinding()]
param(
    [switch]$SkipPdf,
    [switch]$SkipApi,
    [switch]$SkipUi,
    [switch]$NoNewWindows
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$adminRoot = Join-Path $repoRoot "Canine Physio Admin"

if (-not (Test-Path $adminRoot)) {
    throw "Could not find 'Canine Physio Admin' at $adminRoot"
}

function Start-ServiceProcess {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Title,

        [Parameter(Mandatory = $true)]
        [string]$Command
    )

    if ($NoNewWindows) {
        Write-Host "[$Title] $Command" -ForegroundColor Cyan
        Start-Process -FilePath "powershell.exe" -ArgumentList "-NoLogo", "-NoExit", "-Command", "Set-Location '$adminRoot'; $Command" -WindowStyle Normal | Out-Null
        return
    }

    Start-Process -FilePath "powershell.exe" -ArgumentList "-NoLogo", "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle='$Title'; Set-Location '$adminRoot'; $Command" -WindowStyle Normal | Out-Null
}

if (-not $SkipPdf) {
    Start-ServiceProcess -Title "HelloBuddy PDF" -Command "dotnet run --arch x86 --project src/HelloBuddy.PdfService/HelloBuddy.PdfService.csproj --launch-profile http"
}

if (-not $SkipApi) {
    Start-ServiceProcess -Title "HelloBuddy API" -Command "dotnet run --arch x86 --project src/HelloBuddy.Api/HelloBuddy.Api.csproj --launch-profile http"
}

if (-not $SkipUi) {
    Start-ServiceProcess -Title "HelloBuddy UI" -Command "`$env:Api__Uri='http://localhost:5080'; dotnet run --arch x86 --project src/HelloBuddy.Ui/HelloBuddy.Ui.csproj --launch-profile http"
}

Write-Host "Launched requested local services." -ForegroundColor Green
Write-Host "UI:  http://localhost:5046" -ForegroundColor Green
Write-Host "API: http://localhost:5080/healthz" -ForegroundColor Green
Write-Host "PDF: http://localhost:5081/healthz" -ForegroundColor Green
