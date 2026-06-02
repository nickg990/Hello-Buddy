output "acr_login_server" {
  value = azurerm_container_registry.main.login_server
}

output "acr_name" {
  value = azurerm_container_registry.main.name
}

output "resource_group_name" {
  value = data.azurerm_resource_group.main.name
}

output "container_app_environment_name" {
  value = azurerm_container_app_environment.main.name
}

output "application_insights_connection_string" {
  value     = azurerm_application_insights.main.connection_string
  sensitive = true
}

output "storage_account_name" {
  value = azurerm_storage_account.main.name
}

output "published_programmes_container" {
  value = azurerm_storage_container.published_programmes.name
}

output "dataprotection_container" {
  value = azurerm_storage_container.dataprotection_keys.name
}

output "blob_service_uri" {
  value = azurerm_storage_account.main.primary_blob_endpoint
}

output "key_vault_uri" {
  value = data.azurerm_key_vault.main.vault_uri
}

output "ui_identity_id" {
  value = azurerm_user_assigned_identity.ui_app.id
}

output "ui_identity_client_id" {
  value = azurerm_user_assigned_identity.ui_app.client_id
}

output "api_identity_id" {
  value = azurerm_user_assigned_identity.api_app.id
}

output "api_identity_client_id" {
  value = azurerm_user_assigned_identity.api_app.client_id
}

output "pdf_identity_id" {
  value = azurerm_user_assigned_identity.pdf_app.id
}

output "pdf_identity_client_id" {
  value = azurerm_user_assigned_identity.pdf_app.client_id
}
