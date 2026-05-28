param([string]$BaseUrl = "https://ca-hello-buddy-admin.mangocliff-d46e000d.uksouth.azurecontainerapps.io")

$session = $null
$b = Invoke-WebRequest "$BaseUrl/Programmes/1/Builder" -UseBasicParsing -TimeoutSec 60 -SessionVariable session
$tok = [regex]::Match($b.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
Write-Host "Token len: $($tok.Length)"Write-Host ("Cookies on session: {0}" -f (($session.Cookies.GetCookies($BaseUrl) | ForEach-Object { $_.Name }) -join ', '))
$fields = [ordered]@{}
foreach ($m in [regex]::Matches($b.Content, '<input[^>]*name="([^"]+)"[^>]*value="([^"]*)"')) {
    $n = $m.Groups[1].Value; $v = $m.Groups[2].Value
    if ($fields.Contains($n)) { $fields[$n] = @($fields[$n]) + $v } else { $fields[$n] = $v }
}
foreach ($m in [regex]::Matches($b.Content, '<textarea[^>]*name="([^"]+)"[^>]*>([^<]*)</textarea>')) {
    $fields[$m.Groups[1].Value] = $m.Groups[2].Value
}

$origReps = [string]$fields['Exercises[0].Reps']
Write-Host "Original Exercises[0].Reps = '$origReps'"
$newReps = if ([int]::TryParse($origReps, [ref]$null)) { ([int]$origReps + 1).ToString() } else { '7' }
$fields['Exercises[0].Reps'] = $newReps
Write-Host "New Exercises[0].Reps = '$newReps'"

$body = New-Object System.Collections.ArrayList
$seenRvt = $false
foreach ($k in $fields.Keys) {
    $val = $fields[$k]
    $list = if ($val -is [array]) { $val } else { , $val }
    foreach ($vv in $list) {
        if ($k -eq '__RequestVerificationToken') {
            if ($seenRvt) { continue }
            $seenRvt = $true
        }
        [void]$body.Add("$([uri]::EscapeDataString($k))=$([uri]::EscapeDataString([string]$vv))")
    }
}
$bodyStr = ($body -join '&')

try {
    $p = Invoke-WebRequest "$BaseUrl/Programmes/1/Builder" -Method POST -Body $bodyStr `
        -ContentType 'application/x-www-form-urlencoded' -WebSession $session -UseBasicParsing `
        -Headers @{ 'Referer' = "$BaseUrl/Programmes/1/Builder" } `
        -TimeoutSec 60 -MaximumRedirection 5
    Write-Host "POST status: $($p.StatusCode) len=$($p.Content.Length)"
    if ($p.Content -match 'Saved|saved|success') { Write-Host '  contains saved message' }
}
catch {
    Write-Host "POST ERR: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        Write-Host "  HTTP: $([int]$_.Exception.Response.StatusCode)"
        try {
            $stream = $_.Exception.Response.GetResponseStream()
            $sr = New-Object System.IO.StreamReader($stream)
            $body = $sr.ReadToEnd()
            Write-Host "  Body first 800:"
            Write-Host ($body.Substring(0, [Math]::Min(800, $body.Length)))
        }
        catch { Write-Host "  (could not read body)" }
    }
    exit 1
}

$b2 = Invoke-WebRequest "$BaseUrl/Programmes/1/Builder" -UseBasicParsing -TimeoutSec 60
$after = [regex]::Match($b2.Content, 'name="Exercises\[0\]\.Reps"[^>]*value="([^"]*)"').Groups[1].Value
Write-Host "After reload Exercises[0].Reps = '$after' (expected '$newReps')"
if ($after -eq $newReps) { Write-Host "DoD 3 PASS" } else { Write-Host "DoD 3 FAIL" }
