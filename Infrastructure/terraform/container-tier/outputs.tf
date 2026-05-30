output "acr_login_server" {
	description = "Login server for the Azure Container Registry (used by docker push)."
	value       = azurerm_container_registry.main.login_server
}

output "acr_name" {
	value = azurerm_container_registry.main.name
}

output "storage_account_name" {
	value = azurerm_storage_account.main.name
}

output "blob_service_uri" {
	value = azurerm_storage_account.main.primary_blob_endpoint
}

output "published_programmes_container" {
	value = azurerm_storage_container.published_programmes.name
}

output "container_app_environment_name" {
	value = azurerm_container_app_environment.main.name
}

output "container_app_environment_default_domain" {
	value = azurerm_container_app_environment.main.default_domain
}

# --- Identity client ids (DefaultAzureCredential) -------------------------
output "ui_app_identity_client_id" {
	value = azurerm_user_assigned_identity.ui_app.client_id
}

output "api_app_identity_client_id" {
	value = azurerm_user_assigned_identity.api_app.client_id
}

output "pdf_app_identity_client_id" {
	value = azurerm_user_assigned_identity.pdf_app.client_id
}

output "application_insights_connection_string" {
	value     = azurerm_application_insights.main.connection_string
	sensitive = true
}

# --- Container app URLs ---------------------------------------------------
output "ui_app_url" {
	description = "Public HTTPS URL of the UI app (only after deploy_container_apps=true)."
	value       = var.deploy_container_apps ? "https://${azurerm_container_app.ui[0].latest_revision_fqdn}" : "not yet deployed"
}

output "api_internal_fqdn" {
	description = "Internal FQDN of the API app."
	value       = var.deploy_container_apps ? azurerm_container_app.api[0].ingress[0].fqdn : "not yet deployed"
}

output "pdf_internal_fqdn" {
	description = "Internal FQDN of the PDF service."
	value       = var.deploy_container_apps ? azurerm_container_app.pdf[0].ingress[0].fqdn : "not yet deployed"
}
