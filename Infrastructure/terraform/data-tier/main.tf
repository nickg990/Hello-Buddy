terraform {
  required_version = ">= 1.7.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.110"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy    = false
      recover_soft_deleted_key_vaults = true
    }
  }
  subscription_id = var.subscription_id
}

# ---------------------------------------------------------------------------
# Data: current client (used to grant Key Vault access to the deploying user)
# ---------------------------------------------------------------------------
data "azurerm_client_config" "current" {}

# ---------------------------------------------------------------------------
# Resource Group
# ---------------------------------------------------------------------------
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location

  tags = local.tags
}

# ---------------------------------------------------------------------------
# Key Vault
# ---------------------------------------------------------------------------
resource "azurerm_key_vault" "main" {
  name                        = var.key_vault_name
  location                    = azurerm_resource_group.main.location
  resource_group_name         = azurerm_resource_group.main.name
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = "standard"
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false
  enable_rbac_authorization   = false

  # Deploying principal gets full secret access so we can write the
  # MySQL connection string. Additional access policies (for Container Apps
  # managed identity) will be added in the container-platform module.
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    secret_permissions = [
      "Get", "List", "Set", "Delete", "Purge", "Recover"
    ]
  }

  tags = local.tags
}

# ---------------------------------------------------------------------------
# MySQL Flexible Server
# ---------------------------------------------------------------------------
locals {
  mysql_location = var.mysql_location != "" ? var.mysql_location : azurerm_resource_group.main.location
}

resource "azurerm_mysql_flexible_server" "main" {
  name                   = var.mysql_server_name
  location               = local.mysql_location
  resource_group_name    = azurerm_resource_group.main.name
  administrator_login    = var.mysql_admin_username
  administrator_password = var.mysql_admin_password
  sku_name               = "B_Standard_B1ms"
  version                = "8.0.21"
  backup_retention_days       = 1
  geo_redundant_backup_enabled = false

  storage {
    size_gb = 20  # Azure minimum; 5 GB is not a valid option for MySQL Flexible Server
  }

  tags = local.tags
}

# ---------------------------------------------------------------------------
# MySQL Database
# ---------------------------------------------------------------------------
resource "azurerm_mysql_flexible_database" "canine_physio" {
  name                = "canine_physiotherapy"
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_mysql_flexible_server.main.name
  charset             = "utf8mb4"
  collation           = "utf8mb4_0900_ai_ci"
}

# ---------------------------------------------------------------------------
# MySQL Firewall — temporary developer-IP rule
# NOTE: This rule is intentionally temporary. It must be removed before
# final assessment evidence capture (US-29 / Friday COB).
# ---------------------------------------------------------------------------
resource "azurerm_mysql_flexible_server_firewall_rule" "developer_ip" {
  name                = "developer-ip-temp"
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_mysql_flexible_server.main.name
  start_ip_address    = var.developer_ip
  end_ip_address      = var.developer_ip
}

# ---------------------------------------------------------------------------
# Key Vault Secret: MySQL connection string
# ---------------------------------------------------------------------------
resource "azurerm_key_vault_secret" "mysql_connection_string" {
  name         = "mysql-connection-string"
  key_vault_id = azurerm_key_vault.main.id

  value = "Server=${azurerm_mysql_flexible_server.main.fqdn};Port=3306;Database=canine_physiotherapy;Uid=${var.mysql_admin_username};Pwd=${var.mysql_admin_password};SslMode=Required;"

  depends_on = [azurerm_key_vault.main]
}

# ---------------------------------------------------------------------------
# Key Vault Secret: MySQL admin password (also stored individually for
# use by scripts and for display in outputs without embedding in conn string)
# ---------------------------------------------------------------------------
resource "azurerm_key_vault_secret" "mysql_admin_password" {
  name         = "mysql-admin-password"
  key_vault_id = azurerm_key_vault.main.id
  value        = var.mysql_admin_password

  depends_on = [azurerm_key_vault.main]
}

# ---------------------------------------------------------------------------
# Automation Account — scheduled MySQL start / stop
# Scripts use Connect-AzAccount -Identity + Invoke-AzRestMethod so no
# extra modules are required; Az.Accounts is pre-installed in all accounts.
# ---------------------------------------------------------------------------
resource "azurerm_automation_account" "main" {
  name                = "aa-hellobuddy-prod"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku_name            = "Basic"

  identity {
    type = "SystemAssigned"
  }

  tags = local.tags
}

# Least-privilege: Contributor scoped to the MySQL server only, which grants
# the start/stop actions (Microsoft.DBforMySQL/flexibleServers/start|stop/action).
resource "azurerm_role_assignment" "automation_mysql_contributor" {
  scope                = azurerm_mysql_flexible_server.main.id
  role_definition_name = "Contributor"
  principal_id         = azurerm_automation_account.main.identity[0].principal_id
}

# ---------------------------------------------------------------------------
# Runbooks
# ---------------------------------------------------------------------------
resource "azurerm_automation_runbook" "start_mysql" {
  name                    = "Start-MySqlServer"
  location                = azurerm_resource_group.main.location
  resource_group_name     = azurerm_resource_group.main.name
  automation_account_name = azurerm_automation_account.main.name
  log_verbose             = false
  log_progress            = false
  description             = "Starts the Hello Buddy MySQL Flexible Server on the morning schedule."
  runbook_type            = "PowerShell"

  content = <<-SCRIPT
    Connect-AzAccount -Identity
    $sub = "${var.subscription_id}"
    $rg  = "${azurerm_resource_group.main.name}"
    $srv = "${azurerm_mysql_flexible_server.main.name}"
    $uri = "/subscriptions/$sub/resourceGroups/$rg/providers/Microsoft.DBforMySQL/flexibleServers/$srv/start?api-version=2021-05-01"
    Write-Output "Starting: $srv"
    $result = Invoke-AzRestMethod -Method POST -Path $uri
    Write-Output "HTTP $($result.StatusCode)"
    if ($result.StatusCode -notin 200, 202) {
      throw "Start failed — HTTP $($result.StatusCode): $($result.Content)"
    }
  SCRIPT
}

resource "azurerm_automation_runbook" "stop_mysql" {
  name                    = "Stop-MySqlServer"
  location                = azurerm_resource_group.main.location
  resource_group_name     = azurerm_resource_group.main.name
  automation_account_name = azurerm_automation_account.main.name
  log_verbose             = false
  log_progress            = false
  description             = "Stops the Hello Buddy MySQL Flexible Server on the evening schedule."
  runbook_type            = "PowerShell"

  content = <<-SCRIPT
    Connect-AzAccount -Identity
    $sub = "${var.subscription_id}"
    $rg  = "${azurerm_resource_group.main.name}"
    $srv = "${azurerm_mysql_flexible_server.main.name}"
    $uri = "/subscriptions/$sub/resourceGroups/$rg/providers/Microsoft.DBforMySQL/flexibleServers/$srv/stop?api-version=2021-05-01"
    Write-Output "Stopping: $srv"
    $result = Invoke-AzRestMethod -Method POST -Path $uri
    Write-Output "HTTP $($result.StatusCode)"
    if ($result.StatusCode -notin 200, 202) {
      throw "Stop failed — HTTP $($result.StatusCode): $($result.Content)"
    }
  SCRIPT
}

# ---------------------------------------------------------------------------
# Schedules
# timezone = "Europe/London" handles BST/GMT automatically.
# Weekly frequency + week_days controls which days each schedule fires.
#
# Pattern:
#   Mon          — start 06:00
#   Tue–Fri      — start 07:00
#   Mon–Thu      — stop  19:00
#   Fri          — stop  18:00
#   Sat–Sun      — server stays off
# ---------------------------------------------------------------------------

resource "azurerm_automation_schedule" "start_monday" {
  name                    = "weekly-start-mon-0600"
  resource_group_name     = azurerm_resource_group.main.name
  automation_account_name = azurerm_automation_account.main.name
  frequency               = "Week"
  interval                = 1
  timezone                = "Europe/London"
  start_time              = "2026-06-01T06:00:00+01:00"
  week_days               = ["Monday"]
  description             = "Starts MySQL at 06:00 on Mondays (Europe/London)."
}

resource "azurerm_automation_schedule" "start_tue_fri" {
  name                    = "weekly-start-tue-fri-0700"
  resource_group_name     = azurerm_resource_group.main.name
  automation_account_name = azurerm_automation_account.main.name
  frequency               = "Week"
  interval                = 1
  timezone                = "Europe/London"
  start_time              = "2026-05-28T07:00:00+01:00"
  week_days               = ["Tuesday", "Wednesday", "Thursday", "Friday"]
  description             = "Starts MySQL at 07:00 Tue–Fri (Europe/London)."
}

resource "azurerm_automation_schedule" "stop_mon_thu" {
  name                    = "weekly-stop-mon-thu-1900"
  resource_group_name     = azurerm_resource_group.main.name
  automation_account_name = azurerm_automation_account.main.name
  frequency               = "Week"
  interval                = 1
  timezone                = "Europe/London"
  start_time              = "2026-05-28T19:00:00+01:00"
  week_days               = ["Monday", "Tuesday", "Wednesday", "Thursday"]
  description             = "Stops MySQL at 19:00 Mon–Thu (Europe/London)."
}

resource "azurerm_automation_schedule" "stop_friday" {
  name                    = "weekly-stop-fri-1800"
  resource_group_name     = azurerm_resource_group.main.name
  automation_account_name = azurerm_automation_account.main.name
  frequency               = "Week"
  interval                = 1
  timezone                = "Europe/London"
  start_time              = "2026-05-29T18:00:00+01:00"
  week_days               = ["Friday"]
  description             = "Stops MySQL at 18:00 on Fridays (Europe/London)."
}

# ---------------------------------------------------------------------------
# Job schedules — link each runbook to its schedule(s)
# ---------------------------------------------------------------------------
resource "azurerm_automation_job_schedule" "start_mysql_monday" {
  resource_group_name     = azurerm_resource_group.main.name
  automation_account_name = azurerm_automation_account.main.name
  schedule_name           = azurerm_automation_schedule.start_monday.name
  runbook_name            = azurerm_automation_runbook.start_mysql.name
}

resource "azurerm_automation_job_schedule" "start_mysql_tue_fri" {
  resource_group_name     = azurerm_resource_group.main.name
  automation_account_name = azurerm_automation_account.main.name
  schedule_name           = azurerm_automation_schedule.start_tue_fri.name
  runbook_name            = azurerm_automation_runbook.start_mysql.name
}

resource "azurerm_automation_job_schedule" "stop_mysql_mon_thu" {
  resource_group_name     = azurerm_resource_group.main.name
  automation_account_name = azurerm_automation_account.main.name
  schedule_name           = azurerm_automation_schedule.stop_mon_thu.name
  runbook_name            = azurerm_automation_runbook.stop_mysql.name
}

resource "azurerm_automation_job_schedule" "stop_mysql_friday" {
  resource_group_name     = azurerm_resource_group.main.name
  automation_account_name = azurerm_automation_account.main.name
  schedule_name           = azurerm_automation_schedule.stop_friday.name
  runbook_name            = azurerm_automation_runbook.stop_mysql.name
}

# ---------------------------------------------------------------------------
# Locals
# ---------------------------------------------------------------------------
locals {
  tags = {
    project     = "hello-buddy"
    environment = "prod"
    managed_by  = "terraform"
    assessment  = "com712"
  }
}
