output "api_internal_fqdn" {
  value = azurerm_container_app.api.ingress[0].fqdn
}
