output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "resource_group_location" {
  value = azurerm_resource_group.main.location
}

output "vnet_id" {
  value = azurerm_virtual_network.main.id
}

output "vnet_name" {
  value = azurerm_virtual_network.main.name
}

output "subnet_mysql_id" {
  description = "Subnet ID delegated to MySQL Flexible Server."
  value       = azurerm_subnet.subnet_mysql.id
}

output "subnet_apps_id" {
  description = "Subnet ID delegated to Container Apps Environment."
  value       = azurerm_subnet.subnet_apps.id
}

output "private_dns_zone_mysql_id" {
  description = "Private DNS zone ID for MySQL (privatelink.mysql.database.azure.com)."
  value       = azurerm_private_dns_zone.mysql.id
}

output "nsg_mysql_id" {
  description = "NSG ID protecting the MySQL data-tier subnet (apps-only on 3306)."
  value       = azurerm_network_security_group.mysql.id
}

output "nsg_mysql_name" {
  description = "NSG name protecting the MySQL data-tier subnet."
  value       = azurerm_network_security_group.mysql.name
}
