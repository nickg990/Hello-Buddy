output "key_vault_name" {
  description = "Name of the Key Vault."
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "URI of the Key Vault."
  value       = azurerm_key_vault.main.vault_uri
}

output "mysql_server_name" {
  description = "Name of the MySQL Flexible Server."
  value       = azurerm_mysql_flexible_server.main.name
}

output "mysql_fqdn" {
  description = "Fully-qualified domain name of the MySQL Flexible Server (private)."
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

output "automation_account_name" {
  description = "Name of the Automation Account managing the MySQL start/stop schedule."
  value       = azurerm_automation_account.main.name
}
