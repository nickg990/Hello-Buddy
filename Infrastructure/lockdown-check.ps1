$rg = 'rg-hellobuddy-prod'

Write-Host "=== 1. Container Apps ingress topology ===" -ForegroundColor Cyan
az containerapp list -g $rg --query "[].{name:name,external:properties.configuration.ingress.external,fqdn:properties.configuration.ingress.fqdn,targetPort:properties.configuration.ingress.targetPort,transport:properties.configuration.ingress.transport,allowInsecure:properties.configuration.ingress.allowInsecure}" -o table

Write-Host ""
Write-Host "=== 2. Container App Environment internal vs external ===" -ForegroundColor Cyan
az containerapp env list -g $rg --query "[].{name:name,internalOnly:properties.vnetConfiguration.internal,defaultDomain:properties.defaultDomain,staticIp:properties.staticIp}" -o table

Write-Host ""
Write-Host "=== 3. Storage account network + access posture ===" -ForegroundColor Cyan
az storage account show -n sthellobuddyprod -g $rg --query "{publicNetworkAccess:publicNetworkAccess,allowBlobPublicAccess:allowBlobPublicAccess,allowSharedKeyAccess:allowSharedKeyAccess,minTlsVersion:minimumTlsVersion,defaultAction:networkRuleSet.defaultAction,bypass:networkRuleSet.bypass,ipRulesCount:length(networkRuleSet.ipRules)}" -o json

Write-Host ""
Write-Host "=== 4. Storage containers public-access level ===" -ForegroundColor Cyan
$key = az storage account keys list --account-name sthellobuddyprod -g $rg --query '[0].value' -o tsv
az storage container list --account-name sthellobuddyprod --account-key $key --query "[].{name:name,publicAccess:properties.publicAccess}" -o table

Write-Host ""
Write-Host "=== 5. Key Vault network + access ===" -ForegroundColor Cyan
az keyvault show -n kv-hellobuddy-prod -g $rg --query "{publicNetworkAccess:properties.publicNetworkAccess,defaultAction:properties.networkAcls.defaultAction,bypass:properties.networkAcls.bypass,ipRulesCount:length(properties.networkAcls.ipRules),enableRbacAuthorization:properties.enableRbacAuthorization}" -o json
Write-Host "Access policies:"
az keyvault show -n kv-hellobuddy-prod -g $rg --query "properties.accessPolicies[].{objectId:objectId,secrets:permissions.secrets}" -o table

Write-Host ""
Write-Host "=== 6. ACR public access + admin user ===" -ForegroundColor Cyan
az acr show -n acrhellobuddyprod -g $rg --query "{publicNetworkAccess:publicNetworkAccess,adminUserEnabled:adminUserEnabled,anonymousPullEnabled:anonymousPullEnabled,sku:sku.name}" -o json

Write-Host ""
Write-Host "=== 7. MySQL firewall + public access ===" -ForegroundColor Cyan
az mysql flexible-server firewall-rule list -g $rg -n mysql-hellobuddy-prod --query "[].{name:name,start:startIpAddress,end:endIpAddress}" -o table
az mysql flexible-server show -g $rg -n mysql-hellobuddy-prod --query "{publicAccess:network.publicNetworkAccess,state:state}" -o json

Write-Host ""
Write-Host "=== 8. External browser reachability test (each Container App from the public internet) ===" -ForegroundColor Cyan
$apps = az containerapp list -g $rg --query "[].{name:name,fqdn:properties.configuration.ingress.fqdn,external:properties.configuration.ingress.external}" -o json | ConvertFrom-Json
foreach ($a in $apps) {
    $exp = if ($a.external) { 'reachable (external)' } else { 'NOT reachable (internal-only)' }
    $code = 'n/a'
    if ($a.fqdn) {
        try {
            $r = Invoke-WebRequest "https://$($a.fqdn)/" -UseBasicParsing -TimeoutSec 15 -ErrorAction Stop
            $code = "HTTP $($r.StatusCode)"
        }
        catch {
            $msg = $_.Exception.Message
            if ($msg.Length -gt 90) { $msg = $msg.Substring(0, 90) + '...' }
            $code = "ERR: $msg"
        }
    }
    else { $code = '(no public FQDN)' }
    Write-Host ("  {0,-26} external={1,-5} expected={2,-32} actual={3}" -f $a.name, $a.external, $exp, $code)
}

Write-Host ""
Write-Host "=== 9. Probe an internal FQDN from the public internet (must FAIL or NXDOMAIN) ===" -ForegroundColor Cyan
$internalApps = $apps | Where-Object { -not $_.external -and $_.fqdn }
foreach ($a in $internalApps) {
    try {
        $dns = Resolve-DnsName -Name $a.fqdn -Type A -ErrorAction Stop -QuickTimeout
        Write-Host ("  {0} resolves to: {1}" -f $a.fqdn, (($dns | Where-Object { $_.IPAddress } | ForEach-Object { $_.IPAddress }) -join ','))
    }
    catch {
        Write-Host ("  {0} : NXDOMAIN (good - internal-only)" -f $a.fqdn)
    }
}

Write-Host ""
Write-Host "=== 10. Anonymous blob read attempt (should FAIL pre-change) ===" -ForegroundColor Cyan
$blobName = az storage blob list --account-name sthellobuddyprod --container-name published-programmes --account-key $key --query '[0].name' -o tsv
if ($blobName) {
    $blobUrl = "https://sthellobuddyprod.blob.core.windows.net/published-programmes/$blobName"
    try {
        $r = Invoke-WebRequest $blobUrl -UseBasicParsing -TimeoutSec 15 -ErrorAction Stop
        Write-Host ("  ANON GET $blobUrl -> HTTP $($r.StatusCode) (blob is WORLD-READABLE - expected if option A; should be 404 for option B)")
    }
    catch {
        Write-Host ("  ANON GET $blobUrl -> blocked ({0}) - good, container is private" -f $_.Exception.Message.Split([Environment]::NewLine)[0])
    }
}
else {
    Write-Host "  (no blob in container to probe)"
}
