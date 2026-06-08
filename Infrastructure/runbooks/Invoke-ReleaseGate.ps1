[CmdletBinding()]
param(
    [switch]$SkipUi,
    [switch]$SkipInMemory,
    [switch]$SkipIntegration,
    [switch]$RunAzuriteLane,
    [string]$DotnetArchitecture = "x86"
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$adminRoot = Join-Path $repoRoot "Canine Physio Admin"

if (-not (Test-Path $adminRoot)) {
    throw "Could not find Canine Physio Admin at $adminRoot"
}

function Invoke-TestProject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath,
        [Parameter(Mandatory = $true)]
        [string]$Label
    )

    Write-Host "==> $Label" -ForegroundColor Cyan
    dotnet test $ProjectPath --arch $DotnetArchitecture
    if ($LASTEXITCODE -ne 0) {
        throw "Test lane failed: $Label"
    }
}

Push-Location $adminRoot
try {
    if (-not $SkipUi) {
        Invoke-TestProject -ProjectPath "tests/HelloBuddy.Ui.Tests/HelloBuddy.Ui.Tests.csproj" -Label "UI smoke/controller tests"
    }

    if (-not $SkipInMemory) {
        Invoke-TestProject -ProjectPath "tests/HelloBuddy.Api.InMemoryTests/HelloBuddy.Api.InMemoryTests.csproj" -Label "API in-memory tests"
    }

    if (-not $SkipIntegration) {
        Invoke-TestProject -ProjectPath "tests/HelloBuddy.Api.IntegrationTests/HelloBuddy.Api.IntegrationTests.csproj" -Label "API real-DB integration tests"
    }

    if ($RunAzuriteLane) {
        Write-Host "==> API Azurite integration lane" -ForegroundColor Cyan
        $env:HELLOBUDDY_RUN_AZURITE_TESTS = "true"
        dotnet test "tests/HelloBuddy.Api.IntegrationTests/HelloBuddy.Api.IntegrationTests.csproj" --arch $DotnetArchitecture
        if ($LASTEXITCODE -ne 0) {
            throw "Test lane failed: API Azurite integration tests"
        }
    }

    Write-Host "Release gate passed." -ForegroundColor Green
}
finally {
    Pop-Location
}
