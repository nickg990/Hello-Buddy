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

function Resolve-MySqlExe {
    param([string]$ExplicitPath)

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        if (-not (Test-Path $ExplicitPath)) {
            throw "MySQL executable not found at '$ExplicitPath'."
        }

        return (Resolve-Path $ExplicitPath).Path
    }

    $mysqlCmd = Get-Command mysql.exe -ErrorAction SilentlyContinue
    if ($mysqlCmd) {
        return $mysqlCmd.Source
    }

    $defaultPath = "C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe"
    if (Test-Path $defaultPath) {
        return $defaultPath
    }

    $dockerCmd = Get-Command docker -ErrorAction SilentlyContinue
    if ($dockerCmd) {
        Write-Host "mysql.exe not found. Falling back to Docker mysql client image." -ForegroundColor Yellow
        return "__docker__"
    }

    throw "Could not find mysql.exe. Add MySQL bin folder to PATH, pass -MySqlExePath explicitly, or install/start Docker for fallback."
}

function Get-ResetScripts {
    param([string]$RepoRoot)

    $dir = Join-Path $RepoRoot "Canine Physio Database\Build and Initialise"
    $scripts = @(
        (Join-Path $dir "Canine Physio DB Scripts v2.3 (fresh).sql"),
        (Join-Path $dir "Canine Physio DB Day 1 Initialise v2.4.sql"),
        (Join-Path $dir "Canine Physio DB Scripts - Increment 8 - Login and Attribution.sql"),
        (Join-Path $dir "Canine Physio DB MSc Assessment Seed v1.sql"),
        (Join-Path $dir "Canine Physio DB Scripts - Increment 9 Rollback - Remove Programme Email Send Audit.sql"),
        (Join-Path $dir "Canine Physio DB Scripts - Release 2 - Exercise Audit.sql"),
        (Join-Path $dir "Canine Physio DB Scripts - Release 2 - App Settings.sql")
    )

    foreach ($path in $scripts) {
        if (-not (Test-Path $path)) {
            throw "Required SQL script not found: $path"
        }
    }

    return $scripts
}

function Invoke-MySqlScriptFile {
    param(
        [string]$MySql,
        [string]$HostName,
        [int]$HostPort,
        [string]$Username,
        [string]$Pwd,
        [string]$ScriptPath
    )

    if ($MySql -eq "__docker__") {
        $dockerHost = if ($HostName -eq "localhost" -or $HostName -eq "127.0.0.1") { "host.docker.internal" } else { $HostName }
        $escapedScriptPath = $ScriptPath.Replace("\\", "/")

        $dockerArgs = @(
            "run", "--rm",
            "-v", "${escapedScriptPath}:/tmp/reset.sql:ro",
            "mysql:8.0",
            "sh", "-c",
            "mysql --protocol=TCP -h $dockerHost -P $HostPort -u $Username -p$Pwd --default-character-set=utf8mb4 --comments < /tmp/reset.sql"
        )

        & docker @dockerArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Docker mysql client failed while executing '$ScriptPath' (exit code $LASTEXITCODE)."
        }

        return
    }

    $command = '"{0}" --protocol=TCP -h {1} -P {2} -u {3} -p{4} --default-character-set=utf8mb4 --comments < "{5}"' -f `
        $MySql, $HostName, $HostPort, $Username, $Pwd, $ScriptPath

    cmd.exe /d /c $command
    if ($LASTEXITCODE -ne 0) {
        throw "mysql.exe failed while executing '$ScriptPath' (exit code $LASTEXITCODE)."
    }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$mysqlExe = Resolve-MySqlExe -ExplicitPath $MySqlExePath
$scripts = Get-ResetScripts -RepoRoot $repoRoot

Write-Host "Reset target: $Server`:$Port (user: $User)" -ForegroundColor Cyan
Write-Host "This will rebuild schema and re-seed the local database back to baseline data." -ForegroundColor Yellow

if (-not $Force) {
    $confirm = Read-Host "Continue? (y/N)"
    if ($confirm -notin @("y", "Y", "yes", "YES")) {
        Write-Host "Cancelled." -ForegroundColor Yellow
        exit 0
    }
}

foreach ($script in $scripts) {
    Write-Host "Applying $(Split-Path $script -Leaf)..." -ForegroundColor Cyan
    Invoke-MySqlScriptFile -MySql $mysqlExe -HostName $Server -HostPort $Port -Username $User -Pwd $Password -ScriptPath $script
}

Write-Host "Local database reset complete. Baseline seed data restored." -ForegroundColor Green
