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
# Data sources — resources provisioned by vnet-tier and data-tier modules.
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

# Existing Automation Account (provisioned out-of-band, DEC-002). Its system-
# assigned managed identity runs the scheduled Scale-ContainersUp/Down runbooks
# and therefore needs write access to the container apps (see role assignments
# near the end of this file). Read-only data source — never managed here.
data "azurerm_automation_account" "scaling" {
  name                = var.automation_account_name
  resource_group_name = data.azurerm_resource_group.main.name
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
# Storage Account + Blob containers for published PDFs and DataProtection.
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

resource "azurerm_storage_container" "dataprotection_keys" {
  name                  = "dataprotection-keys"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

# ---------------------------------------------------------------------------
# Managed identities — one per Container App (DEC-009).
# ---------------------------------------------------------------------------
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

# ----- UI: DataProtection keys (scoped to container, not account) ----------
resource "azurerm_role_assignment" "ui_app_dataprotection_contributor" {
  scope                = "${azurerm_storage_account.main.id}/blobServices/default/containers/${azurerm_storage_container.dataprotection_keys.name}"
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.ui_app.principal_id
}

# ----- API: Blob + KV + SAS delegation ------------------------------------
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

# ----- UI: Key Vault read (DB-backed login reads ConnectionStrings--CaninePhysioDb) -----
resource "azurerm_key_vault_access_policy" "ui_app" {
  key_vault_id = data.azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.ui_app.principal_id

  secret_permissions = ["Get", "List"]
}

# ---------------------------------------------------------------------------
# Key Vault secrets (app-config-shaped names for AddAzureKeyVault)
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

# Seeded login initial password — consumed by the API via AddAzureKeyVault
# (config key Seed:PractitionerLogin:InitialPassword).
resource "azurerm_key_vault_secret" "seed_initial_password" {
  name         = "Seed--PractitionerLogin--InitialPassword"
  value        = var.seed_initial_password
  key_vault_id = data.azurerm_key_vault.main.id
}

# ---------------------------------------------------------------------------
# Container Apps Environment — VNet-integrated via subnet delegation.
# ---------------------------------------------------------------------------
resource "azurerm_container_app_environment" "main" {
  name                           = var.container_app_environment_name
  location                       = data.azurerm_resource_group.main.location
  resource_group_name            = data.azurerm_resource_group.main.name
  log_analytics_workspace_id     = azurerm_log_analytics_workspace.main.id
  infrastructure_subnet_id       = var.subnet_apps_id

  tags = local.tags

  # The azurerm provider cannot read these attributes back from Azure on import,
  # so a re-import leaves Terraform wanting to destroy/recreate the environment.
  # Ignoring them keeps the already-provisioned environment in place. The config
  # values above still apply on first creation.
  lifecycle {
    ignore_changes = [
      log_analytics_workspace_id,
      infrastructure_resource_group_name,
    ]
  }
}

# ===========================================================================
# Container Apps (DEC-009: UI / API / PDF split)
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
    min_replicas = var.container_min_replicas
    max_replicas = var.container_max_replicas

    custom_scale_rule {
      name             = "http-concurrency"
      custom_rule_type = "http"
      metadata = {
        concurrentRequests = "50"
        cooldownPeriod     = tostring(var.container_scale_cooldown_period)
        pollingInterval    = tostring(var.container_scale_polling_interval)
      }
    }

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
    min_replicas = var.container_min_replicas
    max_replicas = var.container_max_replicas

    custom_scale_rule {
      name             = "http-concurrency"
      custom_rule_type = "http"
      metadata = {
        concurrentRequests = "50"
        cooldownPeriod     = tostring(var.container_scale_cooldown_period)
        pollingInterval    = tostring(var.container_scale_polling_interval)
      }
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

      # Enables PractitionerLoginSeedHostedService at API startup. The initial
      # password is read from Key Vault (Seed--PractitionerLogin--InitialPassword).
      env {
        name  = "Seed__PractitionerLogin__Enabled"
        value = "true"
      }

      env {
        name  = "Storage__BlobServiceUri"
        value = azurerm_storage_account.main.primary_blob_endpoint
      }

      env {
        name  = "Storage__PublishedProgrammesContainer"
        value = azurerm_storage_container.published_programmes.name
      }

      # Required by ValidateExerciseMediaPolicy (CR-002): must be an absolute
      # URL when Storage:Mode resolves to Azure. Exercise media is written to
      # the same container as published programmes under an "exercise-media/"
      # key prefix, so the canonical base is the blob service endpoint + the
      # container name (no trailing slash).
      env {
        name  = "Storage__ExerciseMediaBaseUrl"
        value = "${trimsuffix(azurerm_storage_account.main.primary_blob_endpoint, "/")}/${azurerm_storage_container.published_programmes.name}"
      }

      env {
        name  = "PdfService__Uri"
        value = "https://${azurerm_container_app.pdf[0].ingress[0].fqdn}"
      }

      env {
        name  = "ApplicationInsights__ConnectionString"
        value = azurerm_application_insights.main.connection_string
      }

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
    min_replicas = var.container_min_replicas
    max_replicas = var.container_max_replicas

    custom_scale_rule {
      name             = "http-concurrency"
      custom_rule_type = "http"
      metadata = {
        concurrentRequests = "50"
        cooldownPeriod     = tostring(var.container_scale_cooldown_period)
        pollingInterval    = tostring(var.container_scale_polling_interval)
      }
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
        name  = "KeyVault__Uri"
        value = data.azurerm_key_vault.main.vault_uri
      }

      # DB-backed login/admin services read ConnectionStrings:CaninePhysioDb
      # from Key Vault. Without this the UI falls back to NoOp auth services.
      env {
        name  = "Auth__UseDbBackedServices"
        value = "true"
      }

      env {
        name  = "SeededPractitionerId"
        value = var.seeded_practitioner_id
      }

      env {
        name  = "MediaSearch__VideoProviders__0__Description"
        value = var.media_search_provider_description
      }

      env {
        name  = "MediaSearch__VideoProviders__0__BaseUrl"
        value = var.media_search_provider_base_url
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
    azurerm_key_vault_access_policy.ui_app,
    azurerm_key_vault_secret.connection_string,
  ]
}

# ===========================================================================
# Migration job (R2-S7): caj-hellobuddy-migrate
# Runs inside the VNet-integrated CAE so it has private line-of-sight to MySQL.
# Manual trigger only — never runs automatically during a deploy.
# ===========================================================================

# ----- Migration: UAMI -------------------------------------------------------
resource "azurerm_user_assigned_identity" "migrate_app" {
  name                = var.migrate_app_identity_name
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  tags                = local.tags
}

# ----- Migration: AcrPull ----------------------------------------------------
resource "azurerm_role_assignment" "migrate_app_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.migrate_app.principal_id
}

# ----- Migration: Key Vault Get (connection string only) ---------------------
resource "azurerm_key_vault_access_policy" "migrate_app" {
  key_vault_id = data.azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.migrate_app.principal_id

  secret_permissions = ["Get"]
}

# ----- Migration: Container App Job ------------------------------------------
resource "azurerm_container_app_job" "migrate" {
  count                        = var.deploy_container_apps ? 1 : 0
  name                         = var.migrate_container_app_job_name
  location                     = data.azurerm_resource_group.main.location
  resource_group_name          = data.azurerm_resource_group.main.name
  container_app_environment_id = azurerm_container_app_environment.main.id

  # Migration scripts can take up to 5 minutes; no retries (forward-only).
  replica_timeout_in_seconds = 300
  replica_retry_limit        = 0

  manual_trigger_config {
    parallelism              = 1
    replica_completion_count = 1
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.migrate_app.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.migrate_app.id
  }

  # Inject the connection string from Key Vault via the job's managed identity.
  # The secret is read at job startup — no plaintext credential in the image.
  secret {
    name                = "db-connection-string"
    key_vault_secret_id = azurerm_key_vault_secret.connection_string.id
    identity            = azurerm_user_assigned_identity.migrate_app.id
  }

  template {
    container {
      name   = "migrate"
      image  = var.migrate_app_image
      cpu    = 0.25
      memory = "0.5Gi"

      # Connection string surfaced as DB_CONNECTION_STRING; parsed by migrate.sh.
      env {
        name        = "DB_CONNECTION_STRING"
        secret_name = "db-connection-string"
      }

      # Set SEED_BASELINE=true for the one-time first-adoption run against an
      # already-populated production database (records without re-executing).
      env {
        name  = "SEED_BASELINE"
        value = var.migrate_seed_baseline
      }

      # Set RESET_TRACKING=true for a one-off clean rebuild: drops the
      # _migrations metadata DB so every script re-runs (0010 rebuilds the
      # app DB from scratch). Destructive — disposable/seed data only.
      env {
        name  = "RESET_TRACKING"
        value = var.migrate_reset_tracking
      }
    }
  }

  tags = local.tags

  depends_on = [
    azurerm_role_assignment.migrate_app_acr_pull,
    azurerm_key_vault_access_policy.migrate_app,
    azurerm_key_vault_secret.connection_string,
  ]
}

# ---------------------------------------------------------------------------
# Scheduled scaling — grant the Automation Account managed identity permission
# to write to each container app (needed to set scale.minReplicas from the
# Scale-ContainersUp / Scale-ContainersDown runbooks).
#
# Scoped to the individual apps (not the resource group) for least privilege —
# Key Vault, Storage, ACR and MySQL are deliberately left untouched. Count-
# guarded like the apps so the grants are recreated automatically on every two-
# phase deploy: the apps are destroyed in Phase A (deploy_container_apps=false)
# and recreated in Phase B, which previously wiped a manual grant and broke
# scaling with a 403. Managing the grant here makes it self-healing.
# ---------------------------------------------------------------------------
resource "azurerm_role_assignment" "automation_ui_contributor" {
  count                = var.deploy_container_apps ? 1 : 0
  scope                = azurerm_container_app.ui[0].id
  role_definition_name = "Contributor"
  principal_id         = data.azurerm_automation_account.scaling.identity[0].principal_id
}

resource "azurerm_role_assignment" "automation_api_contributor" {
  count                = var.deploy_container_apps ? 1 : 0
  scope                = azurerm_container_app.api[0].id
  role_definition_name = "Contributor"
  principal_id         = data.azurerm_automation_account.scaling.identity[0].principal_id
}

resource "azurerm_role_assignment" "automation_pdf_contributor" {
  count                = var.deploy_container_apps ? 1 : 0
  scope                = azurerm_container_app.pdf[0].id
  role_definition_name = "Contributor"
  principal_id         = data.azurerm_automation_account.scaling.identity[0].principal_id
}

locals {
  tags = {
    project     = "hello-buddy"
    environment = "prod"
    managed_by  = "terraform"
    assessment  = "com712"
    module      = "container-tier"
  }
}
