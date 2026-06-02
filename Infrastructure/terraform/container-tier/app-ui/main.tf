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
  features {}
  subscription_id = var.subscription_id
}

data "azurerm_resource_group" "main" {
  name = var.resource_group_name
}

data "azurerm_container_app_environment" "main" {
  name                = var.container_app_environment_name
  resource_group_name = data.azurerm_resource_group.main.name
}

data "azurerm_container_registry" "main" {
  name                = var.acr_name
  resource_group_name = data.azurerm_resource_group.main.name
}

data "azurerm_user_assigned_identity" "ui" {
  name                = var.ui_app_identity_name
  resource_group_name = data.azurerm_resource_group.main.name
}

data "azurerm_storage_account" "main" {
  name                = var.storage_account_name
  resource_group_name = data.azurerm_resource_group.main.name
}

resource "azurerm_container_app" "ui" {
  name                         = var.ui_container_app_name
  container_app_environment_id = data.azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.main.name
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [data.azurerm_user_assigned_identity.ui.id]
  }

  registry {
    server   = data.azurerm_container_registry.main.login_server
    identity = data.azurerm_user_assigned_identity.ui.id
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
        value = var.api_uri
      }

      env {
        name  = "SeededPractitionerId"
        value = var.seeded_practitioner_id
      }

      env {
        name  = "ApplicationInsights__ConnectionString"
        value = var.application_insights_connection_string
      }

      env {
        name  = "DataProtection__BlobUri"
        value = "https://${data.azurerm_storage_account.main.name}.blob.core.windows.net/${var.dataprotection_container}/ui-keys.xml"
      }

      env {
        name  = "AZURE_CLIENT_ID"
        value = data.azurerm_user_assigned_identity.ui.client_id
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

    allow_insecure_connections = false
  }

  tags = local.tags
}

locals {
  tags = {
    project     = "hello-buddy"
    environment = "prod"
    managed_by  = "terraform"
    assessment  = "com712"
    module      = "container-tier-app-ui"
  }
}
