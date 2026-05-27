output "resource_group_name" {
  description = "Name of the provisioned resource group."
  value       = azurerm_resource_group.main.name
}

output "resource_group_location" {
  description = "Location of the provisioned resource group."
  value       = azurerm_resource_group.main.location
}

output "key_vault_name" {
  description = "Name of the Key Vault."
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "URI of the Key Vault (used when referencing secrets in Container Apps)."
  value       = azurerm_key_vault.main.vault_uri
}

output "mysql_server_name" {
  description = "Name of the MySQL Flexible Server."
  value       = azurerm_mysql_flexible_server.main.name
}

output "mysql_fqdn" {
  description = "Fully-qualified domain name of the MySQL Flexible Server."
  value       = azurerm_mysql_flexible_server.main.fqdn
}

output "mysql_database_name" {
  description = "Name of the database created on the MySQL Flexible Server."
  value       = azurerm_mysql_flexible_database.canine_physio.name
}

output "mysql_connection_string_secret_name" {
  description = "Key Vault secret name holding the MySQL connection string."
  value       = azurerm_key_vault_secret.mysql_connection_string.name
}

output "mysql_connection_string_secret_id" {
  description = "Key Vault secret ID for the MySQL connection string (use in Container Apps env var references)."
  value       = azurerm_key_vault_secret.mysql_connection_string.id
  sensitive   = true
}

output "automation_account_name" {
  description = "Name of the Automation Account managing the MySQL start/stop schedule."
  value       = azurerm_automation_account.main.name
}

output "automation_account_principal_id" {
  description = "Principal ID of the Automation Account managed identity (for auditing)."
  value       = azurerm_automation_account.main.identity[0].principal_id
}
