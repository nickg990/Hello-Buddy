[CmdletBinding()]
param(
    [string]$SourcePath = (Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "Canine Physio App\Canine Physio App\Resources\Images"),
    [string]$ConnectionString = "UseDevelopmentStorage=true",
    [string]$Container = "published-programmes",
    [string]$Prefix = "exercise-media/images"
)

# ---------------------------------------------------------------------------
# Seeds the local Azurite blob store with exercise library images so the
# exercise image picker has content to list during local testing.
#
# Azurite starts EMPTY on every fresh container (and after -ResetAzurite), so
# this uploads the sample JPGs into the same container/prefix the app serves
# via its managed /exercise-media/ proxy:
#   published-programmes/exercise-media/images/<name>.jpg
#
# Idempotent: re-running overwrites the same blobs. Only *.jpg are uploaded.
# ---------------------------------------------------------------------------

$ErrorActionPreference = "Stop"

$az = Get-Command az -ErrorAction SilentlyContinue
if (-not $az) {
    throw "Azure CLI (az) is required to seed local exercise images. Install it or skip with -SkipImageSeed."
}

if (-not (Test-Path $SourcePath)) {
    throw "Exercise image source folder not found: $SourcePath"
}

$jpgs = Get-ChildItem -Path $SourcePath -Filter *.jpg -File -ErrorAction SilentlyContinue
if (-not $jpgs -or $jpgs.Count -eq 0) {
    Write-Host "No .jpg files found in $SourcePath; nothing to seed." -ForegroundColor Yellow
    return
}

Write-Host "Seeding $($jpgs.Count) exercise image(s) into $Container/$Prefix ..." -ForegroundColor Cyan

# Azurite may run an older Storage API than the installed az CLI advertises.
# Skip the version check so uploads don't fail with InvalidHeaderValue.
$env:AZURE_STORAGE_SKIP_API_VERSION_CHECK = "true"

# Ensure the target container exists in Azurite (safe to call repeatedly).
az storage container create `
    --name $Container `
    --connection-string $ConnectionString `
    --only-show-errors | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Failed to create/verify container '$Container' in Azurite (az exit code $LASTEXITCODE). Is Azurite running with --skipApiVersionCheck?"
}

# Upload only JPGs, flattening into the target prefix, overwriting existing blobs.
az storage blob upload-batch `
    --destination $Container `
    --destination-path $Prefix `
    --source $SourcePath `
    --pattern "*.jpg" `
    --connection-string $ConnectionString `
    --overwrite `
    --only-show-errors | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Failed to upload exercise images to $Container/$Prefix (az exit code $LASTEXITCODE)."
}

Write-Host "Seeded exercise images to $Container/$Prefix (connection: $ConnectionString)." -ForegroundColor Green
Write-Host "Set the admin setting FileStorage.ImageLibraryFolder = $Prefix/ to point the picker here." -ForegroundColor Green
