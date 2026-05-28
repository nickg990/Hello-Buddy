$wsId = 'ffaf8cce-dfc8-433d-a48b-2784a8c7b858'
$token = az account get-access-token --resource https://api.loganalytics.io --query accessToken -o tsv
if (-not $token) { Write-Host "Failed to get token"; exit 1 }
$headers = @{ Authorization = "Bearer $token"; 'Content-Type' = 'application/json' }

$queries = @(
    @{ label = 'Requests (last 15m)'; q = "AppRequests | where TimeGenerated > ago(15m) | summarize cnt=count(), avg_ms=toint(avg(DurationMs)) by Name, ResultCode | top 15 by cnt desc" },
    @{ label = 'Dependencies (last 15m)'; q = "AppDependencies | where TimeGenerated > ago(15m) | summarize cnt=count(), avg_ms=toint(avg(DurationMs)) by Type, Target, ResultCode | top 10 by cnt desc" },
    @{ label = 'Recent requests with OperationId (correlation)'; q = "AppRequests | where TimeGenerated > ago(15m) | project TimeGenerated, Name, ResultCode, DurationMs=toint(DurationMs), OperationId | top 8 by TimeGenerated desc" },
    @{ label = 'Traces (logs) last 15m sample'; q = "AppTraces | where TimeGenerated > ago(15m) | project TimeGenerated, Message, SeverityLevel, OperationId | top 6 by TimeGenerated desc" }
)

foreach ($qq in $queries) {
    Write-Host ""
    Write-Host ("=== " + $qq.label + " ===")
    $bodyJson = @{ query = $qq.q } | ConvertTo-Json -Compress
    try {
        $resp = Invoke-RestMethod -Uri "https://api.loganalytics.io/v1/workspaces/$wsId/query" -Method POST -Headers $headers -Body $bodyJson
        $table = $resp.tables[0]
        if ($null -eq $table -or $table.rows.Count -eq 0) {
            Write-Host '(no rows)'
            continue
        }
        $cols = $table.columns | ForEach-Object { $_.name }
        Write-Host ($cols -join " | ")
        $table.rows | ForEach-Object {
            $row = @()
            for ($i = 0; $i -lt $_.Count; $i++) { $row += [string]$_[$i] }
            Write-Host ($row -join " | ")
        }
    }
    catch {
        Write-Host ("QUERY ERR: " + $_.Exception.Message)
        if ($_.ErrorDetails) { Write-Host $_.ErrorDetails.Message }
    }
}
