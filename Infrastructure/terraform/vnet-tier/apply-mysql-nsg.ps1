<#
.SYNOPSIS
    Plans and (on confirmation) applies the MySQL data-tier NSG only.

.DESCRIPTION
    Adds nsg-subnet-mysql with:
      - Allow-Apps-To-MySQL-3306 (subnet-apps -> subnet-mysql:3306)
      - Deny-All-Other-Inbound (to subnet-mysql)
    and associates it to subnet-mysql.

    This is a LIVE change to an existing VNet/subnet that already hosts a
    running MySQL Flexible Server. It does NOT touch subnet-apps. The script
    plans first and requires explicit confirmation before applying.

.NOTES
    Run from: Infrastructure\terraform\vnet-tier
    PowerShell 5.1 compatible.
#>

$ErrorActionPreference = 'Stop'
Set-Location -Path $PSScriptRoot

Write-Host "==> terraform init (upgrade)..." -ForegroundColor Cyan
terraform init -upgrade | Out-Host

Write-Host "==> terraform validate..." -ForegroundColor Cyan
terraform validate | Out-Host

Write-Host "==> terraform plan (NSG resources only)..." -ForegroundColor Cyan
terraform plan `
    -target="azurerm_network_security_group.mysql" `
    -target="azurerm_network_security_rule.mysql_allow_apps" `
    -target="azurerm_network_security_rule.mysql_deny_other_inbound" `
    -target="azurerm_subnet_network_security_group_association.mysql" `
    -out="nsg.tfplan" | Out-Host

Write-Host ""
Write-Host "Review the plan above. It should show 4 resources to ADD and 0 to change/destroy." -ForegroundColor Yellow
$confirm = Read-Host "Type 'apply' to apply this NSG, anything else to abort"

if ($confirm -ne 'apply') {
    Write-Host "Aborted. No changes made. (Plan saved as nsg.tfplan; you can delete it.)" -ForegroundColor Red
    return
}

Write-Host "==> terraform apply nsg.tfplan..." -ForegroundColor Cyan
terraform apply "nsg.tfplan" | Out-Host

Remove-Item -Path "nsg.tfplan" -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "==> Verifying NSG in Azure..." -ForegroundColor Cyan
az network nsg show -g rg-hellobuddy-prod -n nsg-subnet-mysql `
    --query "{name:name, rules:securityRules[].{name:name, priority:priority, access:access, dir:direction, proto:protocol, src:sourceAddressPrefix, dport:destinationPortRange}}" `
    -o jsonc

Write-Host ""
Write-Host "==> Verifying subnet association..." -ForegroundColor Cyan
az network vnet subnet show -g rg-hellobuddy-prod --vnet-name vnet-hellobuddy-prod -n subnet-mysql `
    --query "{subnet:name, nsg:networkSecurityGroup.id}" -o jsonc

Write-Host ""
Write-Host "Done. NSG applied to subnet-mysql. App connectivity to MySQL is preserved via the apps-subnet allow rule." -ForegroundColor Green
Write-Host "SMOKE TEST: load the Cases page in the UI to confirm API->MySQL still works." -ForegroundColor Yellow
