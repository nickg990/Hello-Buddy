output "pdf_internal_fqdn" {
  value = azurerm_container_app.pdf.ingress[0].fqdn
}
