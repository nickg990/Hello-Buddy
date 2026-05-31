# ===========================================================================
# Network Security Group — MySQL data tier (defence-in-depth, L4 segmentation)
# ---------------------------------------------------------------------------
# Rationale:
#   The MySQL Flexible Server already has NO public endpoint (private access
#   via delegated subnet + private DNS). This NSG adds an EXPLICIT, auditable
#   tier-to-tier rule so that ONLY the Container Apps subnet (subnet-apps) may
#   reach MySQL on 3306, and all other inbound is denied.
#
#   It is applied to subnet-mysql ONLY. We deliberately do NOT place an NSG on
#   subnet-apps (delegated to Microsoft.App/environments) because that subnet
#   has platform-managed networking and HTTPS is already enforced at the
#   Container Apps ingress (allowInsecure = false).
#
# Safety:
#   - Azure adds default rules (AllowVnetInBound 65000, AllowAzureLBInBound
#     65001, DenyAllInBound 65500) which remain in effect. The custom rules
#     below sit ABOVE those defaults.
#   - We keep an explicit allow for the apps subnet so the running API/PDF
#     containers continue to reach MySQL after association.
# ===========================================================================

resource "azurerm_network_security_group" "mysql" {
  name                = "nsg-${var.subnet_mysql_name}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  tags = local.tags
}

# Allow MySQL (3306) inbound ONLY from the Container Apps subnet.
resource "azurerm_network_security_rule" "mysql_allow_apps" {
  name                        = "Allow-Apps-To-MySQL-3306"
  priority                    = 100
  direction                   = "Inbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range           = "*"
  destination_port_range      = "3306"
  source_address_prefix       = var.subnet_apps_prefix
  destination_address_prefix  = var.subnet_mysql_prefix
  resource_group_name         = azurerm_resource_group.main.name
  network_security_group_name = azurerm_network_security_group.mysql.name
}

# Explicit deny of all other inbound to the MySQL subnet (above the default
# 65500 DenyAllInBound — makes the intent visible and auditable).
resource "azurerm_network_security_rule" "mysql_deny_other_inbound" {
  name                        = "Deny-All-Other-Inbound"
  priority                    = 4000
  direction                   = "Inbound"
  access                      = "Deny"
  protocol                    = "*"
  source_port_range           = "*"
  destination_port_range      = "*"
  source_address_prefix       = "*"
  destination_address_prefix  = var.subnet_mysql_prefix
  resource_group_name         = azurerm_resource_group.main.name
  network_security_group_name = azurerm_network_security_group.mysql.name
}

# Associate the NSG with the MySQL subnet.
resource "azurerm_subnet_network_security_group_association" "mysql" {
  subnet_id                 = azurerm_subnet.subnet_mysql.id
  network_security_group_id = azurerm_network_security_group.mysql.id
}
