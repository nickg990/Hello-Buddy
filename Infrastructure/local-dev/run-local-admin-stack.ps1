[CmdletBinding()]
param(
    [switch]$SkipAzurite,
    [switch]$ResetAzurite,
    [switch]$SkipPdf,
    [switch]$SkipApi,
    [switch]$SkipUi,
    [switch]$NoNewWindows,
    [int]$StartupTimeoutSeconds = 90,
    [switch]$NoBrowser
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$adminRoot = Join-Path $repoRoot "Canine Physio Admin"

if (-not (Test-Path $adminRoot)) {
    throw "Could not find 'Canine Physio Admin' at $adminRoot"
}

function Ensure-Azurite {
    if ($SkipAzurite) {
        Write-Host "Skipping Azurite startup (SkipAzurite switch set)." -ForegroundColor Yellow
        return
    }

    $docker = Get-Command docker -ErrorAction SilentlyContinue
    if (-not $docker) {
        throw "Docker is required to run Azurite locally. Install/start Docker Desktop or rerun with -SkipAzurite and set Storage:Mode=FileSystem."
    }

    $containerName = "hellobuddy-azurite"
    $existingContainer = docker ps -a --filter "name=^/${containerName}$" --format "{{.Names}}"

    if ($ResetAzurite -and $existingContainer) {
        docker rm -f $containerName | Out-Null
        $existingContainer = ""
    }

    if (-not $existingContainer) {
        Write-Host "Starting Azurite container..." -ForegroundColor Cyan
        docker run -d --name $containerName -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite azurite --blobHost 0.0.0.0 --queueHost 0.0.0.0 --tableHost 0.0.0.0 | Out-Null
    }
    else {
        $running = docker ps --filter "name=^/${containerName}$" --format "{{.Names}}"
        if (-not $running) {
            Write-Host "Starting existing Azurite container..." -ForegroundColor Cyan
            docker start $containerName | Out-Null
        }
    }

    $started = $false
    for ($i = 0; $i -lt 20; $i++) {
        try {
            $client = [System.Net.Sockets.TcpClient]::new()
            $task = $client.ConnectAsync("127.0.0.1", 10000)
            $task.Wait(500)
            if ($client.Connected) {
                $started = $true
                $client.Dispose()
                break
            }
            $client.Dispose()
        }
        catch {
        }
        Start-Sleep -Milliseconds 500
    }

    if (-not $started) {
        throw "Azurite did not become reachable on localhost:10000."
    }

    Write-Host "Azurite ready at localhost:10000 (blob), 10001 (queue), 10002 (table)." -ForegroundColor Green
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

function Wait-ForHttpReady {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$Url,

        [Parameter(Mandatory = $true)]
        [datetime]$Deadline
    )

    while ((Get-Date) -lt $Deadline) {
        try {
            $response = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec 3
            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
                Write-Host "[$Name] Ready at $Url" -ForegroundColor Green
                return
            }
        }
        catch {
            # Service may still be booting; keep polling until timeout.
        }
    }

    throw "Timed out waiting for $Name at $Url after $StartupTimeoutSeconds seconds."
}

function Open-UiInBrowser {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url
    )

    if ($NoBrowser) {
        return
    }

    $chrome = Get-Command chrome -ErrorAction SilentlyContinue
    if ($chrome) {
        Start-Process -FilePath $chrome.Source -ArgumentList $Url | Out-Null
        return
    }

    $chromeExe = "C:\Program Files\Google\Chrome\Application\chrome.exe"
    if (Test-Path $chromeExe) {
        Start-Process -FilePath $chromeExe -ArgumentList $Url | Out-Null
        return
    }

    Start-Process $Url | Out-Null
}

Ensure-Azurite

if (-not $SkipPdf) {
    Start-ServiceProcess -Title "HelloBuddy PDF" -Command "dotnet run --arch x86 --project src/HelloBuddy.PdfService/HelloBuddy.PdfService.csproj --launch-profile http"
}

if (-not $SkipApi) {
    Start-ServiceProcess -Title "HelloBuddy API" -Command "dotnet run --arch x86 --project src/HelloBuddy.Api/HelloBuddy.Api.csproj --launch-profile http"
}

if (-not $SkipUi) {
    Start-ServiceProcess -Title "HelloBuddy UI" -Command "`$env:Api__Uri='http://localhost:5080'; dotnet run --arch x86 --project src/HelloBuddy.Ui/HelloBuddy.Ui.csproj --launch-profile http"
}

$deadline = (Get-Date).AddSeconds($StartupTimeoutSeconds)

if (-not $SkipPdf) {
    Wait-ForHttpReady -Name "HelloBuddy PDF" -Url "http://localhost:5081/healthz" -Deadline $deadline
}

if (-not $SkipApi) {
    Wait-ForHttpReady -Name "HelloBuddy API" -Url "http://localhost:5080/healthz" -Deadline $deadline
}

if (-not $SkipUi) {
    Wait-ForHttpReady -Name "HelloBuddy UI" -Url "http://localhost:5046" -Deadline $deadline
    Open-UiInBrowser -Url "http://localhost:5046"
}

Write-Host "Launched requested local services." -ForegroundColor Green
Write-Host "UI:  http://localhost:5046" -ForegroundColor Green
Write-Host "API: http://localhost:5080/healthz" -ForegroundColor Green
Write-Host "PDF: http://localhost:5081/healthz" -ForegroundColor Green
Write-Host "Azurite blob endpoint: http://127.0.0.1:10000/devstoreaccount1" -ForegroundColor Green
