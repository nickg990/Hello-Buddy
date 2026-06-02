terraform {
  required_version = ">= 1.7.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.110"
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

data "azurerm_client_config" "current" {}

data "azurerm_resource_group" "main" {
  name = var.resource_group_name
}

data "azurerm_key_vault" "main" {
  name                = var.key_vault_name
  resource_group_name = data.azurerm_resource_group.main.name
}

data "azurerm_key_vault_secret" "existing_mysql_connection_string" {
  name         = "mysql-connection-string"
  key_vault_id = data.azurerm_key_vault.main.id
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = var.log_analytics_workspace_name
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = local.tags
}

resource "azurerm_application_insights" "main" {
  name                = var.application_insights_name
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"

  tags = local.tags
}

resource "azurerm_container_registry" "main" {
  name                = var.acr_name
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  sku                 = "Basic"
  admin_enabled       = false

  tags = local.tags
}

resource "azurerm_storage_account" "main" {
  name                            = var.storage_account_name
  resource_group_name             = data.azurerm_resource_group.main.name
  location                        = data.azurerm_resource_group.main.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  account_kind                    = "StorageV2"
  access_tier                     = "Hot"
  min_tls_version                 = "TLS1_2"
  shared_access_key_enabled       = true
  allow_nested_items_to_be_public = false

  blob_properties {
    versioning_enabled = false
  }

  tags = local.tags
}

resource "azurerm_storage_container" "published_programmes" {
  name                  = "published-programmes"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "dataprotection_keys" {
  name                  = "dataprotection-keys"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

resource "azurerm_user_assigned_identity" "ui_app" {
  name                = var.ui_app_identity_name
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "api_app" {
  name                = var.api_app_identity_name
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "pdf_app" {
  name                = var.pdf_app_identity_name
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  tags                = local.tags
}

resource "azurerm_role_assignment" "ui_app_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.ui_app.principal_id
}

resource "azurerm_role_assignment" "api_app_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.api_app.principal_id
}

resource "azurerm_role_assignment" "pdf_app_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.pdf_app.principal_id
}

resource "azurerm_role_assignment" "ui_app_dataprotection_contributor" {
  scope                = "${azurerm_storage_account.main.id}/blobServices/default/containers/${azurerm_storage_container.dataprotection_keys.name}"
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.ui_app.principal_id
}

resource "azurerm_role_assignment" "api_app_blob_contributor" {
  scope                = "${azurerm_storage_account.main.id}/blobServices/default/containers/${azurerm_storage_container.published_programmes.name}"
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.api_app.principal_id
}

resource "azurerm_role_assignment" "api_app_blob_delegator" {
  scope                = azurerm_storage_account.main.id
  role_definition_name = "Storage Blob Delegator"
  principal_id         = azurerm_user_assigned_identity.api_app.principal_id
}

resource "azurerm_key_vault_access_policy" "api_app" {
  key_vault_id = data.azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.api_app.principal_id

  secret_permissions = ["Get", "List"]
}

resource "azurerm_key_vault_secret" "connection_string" {
  name         = "ConnectionStrings--CaninePhysioDb"
  value        = data.azurerm_key_vault_secret.existing_mysql_connection_string.value
  key_vault_id = data.azurerm_key_vault.main.id
}

resource "azurerm_key_vault_secret" "appinsights_connection_string" {
  name         = "ApplicationInsights--ConnectionString"
  value        = azurerm_application_insights.main.connection_string
  key_vault_id = data.azurerm_key_vault.main.id
}

resource "azurerm_container_app_environment" "main" {
  name                       = var.container_app_environment_name
  location                   = data.azurerm_resource_group.main.location
  resource_group_name        = data.azurerm_resource_group.main.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  infrastructure_subnet_id   = var.subnet_apps_id

  tags = local.tags

  lifecycle {
    ignore_changes = [
      log_analytics_workspace_id,
      infrastructure_resource_group_name,
    ]
  }
}

locals {
  tags = {
    project     = "hello-buddy"
    environment = "prod"
    managed_by  = "terraform"
    assessment  = "com712"
    module      = "container-tier-foundation"
  }
}
