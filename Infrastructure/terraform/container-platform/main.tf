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

# ---------------------------------------------------------------------------
# Data sources — resources provisioned by the data-tier module on Day 1.
# ---------------------------------------------------------------------------
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

# ---------------------------------------------------------------------------
# Log Analytics workspace + Application Insights — shared by all three apps.
# ---------------------------------------------------------------------------
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

# ---------------------------------------------------------------------------
# Azure Container Registry — pull via managed identity (AcrPull).
# ---------------------------------------------------------------------------
resource "azurerm_container_registry" "main" {
  name                = var.acr_name
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  sku                 = "Basic"
  admin_enabled       = false

  tags = local.tags
}

# ---------------------------------------------------------------------------
# Storage Account + Blob container for published programme PDFs.
# ---------------------------------------------------------------------------
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

# DataProtection keys for the UI Container App. Persisting here lets
# antiforgery tokens and auth cookies survive revision swaps and
# multi-replica scale-out instead of dying with the ephemeral container
# filesystem (closes TD-006).
resource "azurerm_storage_container" "dataprotection_keys" {
  name                  = "dataprotection-keys"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

# ---------------------------------------------------------------------------
# Managed identities — one per Container App (DEC-009).
# Using user-assigned (not system-assigned) so AcrPull can be granted before
# the Container App resource exists, avoiding a chicken-and-egg on first pull.
# ---------------------------------------------------------------------------
resource "azurerm_user_assigned_identity" "ui_app" {
  name                = var.ui_app_identity_name
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name

  tags = local.tags
}

resource "azurerm_user_assigned_identity" "api_app" {
  name                = var.api_app_identity_name
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name

  tags = local.tags
}

resource "azurerm_user_assigned_identity" "pdf_app" {
  name                = var.pdf_app_identity_name
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name

  tags = local.tags
}

# ----- AcrPull for all three -----------------------------------------------
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

# ----- UI-only role grant (DataProtection keys) ----------------------------
# Scoped to just the dataprotection-keys container, not the storage account,
# so the UI identity has zero reach into published-programmes or anywhere
# else on the account.
resource "azurerm_role_assignment" "ui_app_dataprotection_contributor" {
  scope                = "${azurerm_storage_account.main.id}/blobServices/default/containers/${azurerm_storage_container.dataprotection_keys.name}"
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.ui_app.principal_id
}

# ----- API-only role grants (KV + Blob) ------------------------------------
resource "azurerm_role_assignment" "api_app_blob_contributor" {
  scope                = "${azurerm_storage_account.main.id}/blobServices/default/containers/${azurerm_storage_container.published_programmes.name}"
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.api_app.principal_id
}

# Required so the API can call GetUserDelegationKey when minting short-lived
# SAS URLs for the download link (DEC-011). Storage Blob Data Contributor at
# container scope does not include the
# Microsoft.Storage/.../generateUserDelegationKey/action; Storage Blob
# Delegator at the account scope does, with no data-plane permissions
# attached.
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

# ---------------------------------------------------------------------------
# Key Vault secrets named to match the app's configuration keys.
# AddAzureKeyVault translates `--` to `:`, so these become
# `ConnectionStrings:CaninePhysioDb` and `ApplicationInsights:ConnectionString`.
# ---------------------------------------------------------------------------
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

# ---------------------------------------------------------------------------
# Container Apps Environment
# ---------------------------------------------------------------------------
resource "azurerm_container_app_environment" "main" {
  name                       = var.container_app_environment_name
  location                   = data.azurerm_resource_group.main.location
  resource_group_name        = data.azurerm_resource_group.main.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id

  tags = local.tags
}

# ===========================================================================
# Container Apps (DEC-009: UI / API / PDF split)
# Deployment order (per Azure Deployment Order Checklist):
#   api -> pdf -> ui  (Terraform resolves via references).
# ===========================================================================

# ---------------------------------------------------------------------------
# PDF service — internal ingress only, Chromium-backed, fixed 1 replica.
# ---------------------------------------------------------------------------
resource "azurerm_container_app" "pdf" {
  count                        = var.deploy_container_apps ? 1 : 0
  name                         = var.pdf_container_app_name
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.main.name
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.pdf_app.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.pdf_app.id
  }

  template {
    # Fixed replicas: low-volume MVP. Queue-based scaling is Increment 2.
    min_replicas = 1
    max_replicas = 1

    container {
      name   = "pdf"
      image  = var.pdf_app_image
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      env {
        name  = "ApplicationInsights__ConnectionString"
        value = azurerm_application_insights.main.connection_string
      }

      liveness_probe {
        transport               = "HTTP"
        port                    = 8080
        path                    = "/healthz"
        initial_delay           = 10
        interval_seconds        = 30
        failure_count_threshold = 3
      }

      readiness_probe {
        transport               = "HTTP"
        port                    = 8080
        path                    = "/healthz"
        interval_seconds        = 10
        failure_count_threshold = 3
      }
    }
  }

  ingress {
    external_enabled = false
    target_port      = 8080
    transport        = "auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  tags = local.tags

  depends_on = [
    azurerm_role_assignment.pdf_app_acr_pull,
  ]
}

# ---------------------------------------------------------------------------
# API — internal ingress only, owns DbContext + Blob + PDF service client.
# ---------------------------------------------------------------------------
resource "azurerm_container_app" "api" {
  count                        = var.deploy_container_apps ? 1 : 0
  name                         = var.api_container_app_name
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.main.name
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.api_app.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.api_app.id
  }

  template {
    min_replicas = 1
    max_replicas = 3

    http_scale_rule {
      name                = "http-concurrency"
      concurrent_requests = "50"
    }

    container {
      name   = "api"
      image  = var.api_app_image
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      env {
        name  = "KeyVault__Uri"
        value = data.azurerm_key_vault.main.vault_uri
      }

      env {
        name  = "Storage__BlobServiceUri"
        value = azurerm_storage_account.main.primary_blob_endpoint
      }

      env {
        name  = "Storage__PublishedProgrammesContainer"
        value = azurerm_storage_container.published_programmes.name
      }

      env {
        name  = "PdfService__Uri"
        value = "https://${azurerm_container_app.pdf[0].ingress[0].fqdn}"
      }

      env {
        name  = "ApplicationInsights__ConnectionString"
        value = azurerm_application_insights.main.connection_string
      }

      # DefaultAzureCredential needs the client id of the UAMI when multiple
      # are attached. We only attach one, but set explicitly for clarity.
      env {
        name  = "AZURE_CLIENT_ID"
        value = azurerm_user_assigned_identity.api_app.client_id
      }

      liveness_probe {
        transport               = "HTTP"
        port                    = 8080
        path                    = "/healthz"
        initial_delay           = 10
        interval_seconds        = 30
        failure_count_threshold = 3
      }

      readiness_probe {
        transport               = "HTTP"
        port                    = 8080
        path                    = "/healthz"
        interval_seconds        = 10
        failure_count_threshold = 3
      }
    }
  }

  ingress {
    external_enabled = false
    target_port      = 8080
    transport        = "auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  tags = local.tags

  depends_on = [
    azurerm_role_assignment.api_app_acr_pull,
    azurerm_role_assignment.api_app_blob_contributor,
    azurerm_key_vault_access_policy.api_app,
    azurerm_key_vault_secret.connection_string,
    azurerm_key_vault_secret.appinsights_connection_string,
  ]
}

# ---------------------------------------------------------------------------
# UI — external HTTPS ingress; only egress is HTTPS to internal API FQDN.
# ---------------------------------------------------------------------------
resource "azurerm_container_app" "ui" {
  count                        = var.deploy_container_apps ? 1 : 0
  name                         = var.ui_container_app_name
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.main.name
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.ui_app.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.ui_app.id
  }

  template {
    min_replicas = 1
    max_replicas = 3

    http_scale_rule {
      name                = "http-concurrency"
      concurrent_requests = "50"
    }

    container {
      name   = "ui"
      image  = var.ui_app_image
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      env {
        name  = "Api__Uri"
        value = "https://${azurerm_container_app.api[0].ingress[0].fqdn}"
      }

      env {
        name  = "SeededPractitionerId"
        value = var.seeded_practitioner_id
      }

      env {
        name  = "ApplicationInsights__ConnectionString"
        value = azurerm_application_insights.main.connection_string
      }

      env {
        name  = "DataProtection__BlobUri"
        value = "https://${azurerm_storage_account.main.name}.blob.core.windows.net/${azurerm_storage_container.dataprotection_keys.name}/ui-keys.xml"
      }

      env {
        name  = "AZURE_CLIENT_ID"
        value = azurerm_user_assigned_identity.ui_app.client_id
      }

      liveness_probe {
        transport               = "HTTP"
        port                    = 8080
        path                    = "/healthz"
        initial_delay           = 10
        interval_seconds        = 30
        failure_count_threshold = 3
      }

      readiness_probe {
        transport               = "HTTP"
        port                    = 8080
        path                    = "/healthz"
        interval_seconds        = 10
        failure_count_threshold = 3
      }
    }
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  tags = local.tags

  depends_on = [
    azurerm_role_assignment.ui_app_acr_pull,
    azurerm_role_assignment.ui_app_dataprotection_contributor,
  ]
}

locals {
  tags = {
    project     = "hello-buddy"
    environment = "prod"
    managed_by  = "terraform"
    assessment  = "com712"
    module      = "container-platform"
  }
}
