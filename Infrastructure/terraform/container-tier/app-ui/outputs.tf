output "ui_app_url" {
  value = "https://${azurerm_container_app.ui.latest_revision_fqdn}"
}
