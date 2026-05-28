param([string]$BaseUrl = "https://ca-hello-buddy-admin.mangocliff-d46e000d.uksouth.azurecontainerapps.io")

# 1. GET builder page to obtain antiforgery token + cookie (Publish form is on same page)
$session = $null
$b = Invoke-WebRequest "$BaseUrl/Programmes/1/Builder" -UseBasicParsing -TimeoutSec 60 -SessionVariable session
$rvt = [regex]::Match($b.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
Write-Host "Got token, length $($rvt.Length)"

# 2. POST to /Programmes/1/Publish
$bodyStr = "__RequestVerificationToken=" + [uri]::EscapeDataString($rvt)
try {
    $start = Get-Date
    $p = Invoke-WebRequest "$BaseUrl/Programmes/1/Publish" -Method POST -Body $bodyStr `
        -ContentType 'application/x-www-form-urlencoded' -WebSession $session -UseBasicParsing `
        -Headers @{ 'Referer' = "$BaseUrl/Programmes/1/Builder" } `
        -TimeoutSec 180 -MaximumRedirection 5
    $dur = (Get-Date) - $start
    Write-Host ("Publish POST status: {0} len={1} duration={2:N1}s" -f $p.StatusCode, $p.Content.Length, $dur.TotalSeconds)
    if ($p.Content -match 'Wrote\s+([^\s<]+)\s+\((\d[\d,]*)\s+bytes\)') {
        Write-Host ("  PUBLISHED: {0} ({1} bytes)" -f $matches[1], $matches[2])
    }
    elseif ($p.Content -match 'Published|published') {
        $snippet = $p.Content -split "`n" | Select-String -Pattern 'published|wrote|programme' | Select-Object -First 5
        Write-Host "  Found publish-related text:"
        $snippet | ForEach-Object { Write-Host "    $_" }
    }
    else {
        Write-Host "  No publish confirmation text found"
    }
}
catch {
    Write-Host "Publish ERR: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        Write-Host "  HTTP: $([int]$_.Exception.Response.StatusCode)"
        try {
            $stream = $_.Exception.Response.GetResponseStream()
            $sr = New-Object System.IO.StreamReader($stream)
            $body = $sr.ReadToEnd()
            Write-Host "  Body first 1200:"
            Write-Host ($body.Substring(0, [Math]::Min(1200, $body.Length)))
        }
        catch { Write-Host "  (could not read body)" }
    }
    exit 1
}

# 3. List blobs to confirm
Write-Host "`nBlobs in published-programmes:"
az storage blob list --account-name sthellobuddyprod --container-name published-programmes `
    --auth-mode login --query '[].{name:name,size:properties.contentLength,modified:properties.lastModified}' `
    -o table
