# Local MySQL Server 8.4 setup for DevBox (DEC-005).
# Must be run elevated. Idempotent-ish: skips re-init if data dir already populated.
# Dev-only: never used in Azure. Root password is intentionally simple.

$ErrorActionPreference = 'Stop'
Start-Transcript -Path 'C:\Projects\Hello-Buddy\Infrastructure\local-dev\setup-local-mysql.log' -Force | Out-Null

$bin = 'C:\Program Files\MySQL\MySQL Server 8.4\bin'
$baseDir = 'C:\Program Files\MySQL\MySQL Server 8.4'
$dataDir = 'C:\ProgramData\MySQL\MySQL Server 8.4\Data'
$cfgDir = 'C:\ProgramData\MySQL\MySQL Server 8.4'
$cfgFile = Join-Path $cfgDir 'my.ini'
$service = 'MySQL84'
$rootPwd = 'devroot'

New-Item -ItemType Directory -Path $cfgDir -Force | Out-Null

# Always rewrite my.ini so retries pick up corrected options.
if (Test-Path $cfgFile) { Remove-Item $cfgFile -Force }
@"
[mysqld]
basedir="$baseDir"
datadir="$dataDir"
port=3306
[client]
port=3306
"@ | Set-Content -Path $cfgFile -Encoding ASCII
Write-Host "Wrote $cfgFile"

# Treat data dir as initialised only if the privilege table file exists (last to be written).
$isInit = Test-Path (Join-Path $dataDir 'mysql\user.ibd')
if (-not $isInit) {
    # Stop and remove service before wiping data dir.
    $svc = Get-Service -Name $service -ErrorAction SilentlyContinue
    if ($svc) {
        if ($svc.Status -eq 'Running') { Stop-Service -Name $service -Force }
        & "$bin\mysqld.exe" --remove $service | Out-Null
    }
    if (Test-Path $dataDir) { Remove-Item $dataDir -Recurse -Force }
    New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
    Write-Host "Initialising data directory (insecure)..."
    & "$bin\mysqld.exe" --defaults-file="$cfgFile" --initialize-insecure --console 2>&1 | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "mysqld --initialize-insecure failed ($LASTEXITCODE)" }
}
else {
    Write-Host "Data directory already initialised — skipping."
}

if (-not (Get-Service -Name $service -ErrorAction SilentlyContinue)) {
    Write-Host "Installing service $service..."
    & "$bin\mysqld.exe" --install $service --defaults-file="$cfgFile"
    if ($LASTEXITCODE -ne 0) { throw "mysqld --install failed ($LASTEXITCODE)" }
}
else {
    Write-Host "Service $service already installed."
}

Set-Service -Name $service -StartupType Automatic
if ((Get-Service -Name $service).Status -ne 'Running') {
    Write-Host "Starting service $service..."
    try {
        Start-Service -Name $service -ErrorAction Stop
    }
    catch {
        Write-Host "Service start failed. Last 40 lines of MySQL error log:"
        $errLog = Join-Path $dataDir ($env:COMPUTERNAME + '.err')
        if (Test-Path $errLog) { Get-Content $errLog -Tail 40 | Out-Host } else { Get-ChildItem $dataDir -Filter '*.err' | ForEach-Object { Get-Content $_.FullName -Tail 40 | Out-Host } }
        throw
    }
}
Get-Service -Name $service | Format-Table Name, Status, StartType -AutoSize

# Set root password. After --initialize-insecure root has no password; this sets it to $rootPwd.
# Safe to re-run: ALTER USER will fail if the password is already $rootPwd via different auth — we ignore that.
Write-Host "Setting root password..."
$sql = "ALTER USER 'root'@'localhost' IDENTIFIED BY '$rootPwd'; FLUSH PRIVILEGES;"
$sql | & "$bin\mysql.exe" -u root --skip-password 2>&1 | Tee-Object -Variable alterOut | Out-Host
if ($LASTEXITCODE -ne 0) {
    Write-Host "(Password may already be set; trying with password)"
    $sql | & "$bin\mysql.exe" -u root "-p$rootPwd" 2>&1 | Out-Host
}

# Add bin to user PATH (persistent) if not already present.
$userPath = [System.Environment]::GetEnvironmentVariable('Path', 'User')
if (-not ($userPath -split ';' | Where-Object { $_ -eq $bin })) {
    Write-Host "Adding $bin to user PATH..."
    [System.Environment]::SetEnvironmentVariable('Path', ($userPath.TrimEnd(';') + ';' + $bin), 'User')
}

Write-Host "`nDone. Open a new shell so PATH picks up mysql.exe, or refresh in-place:"
Write-Host '  $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")'
Stop-Transcript | Out-Null
