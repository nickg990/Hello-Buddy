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

data "azurerm_user_assigned_identity" "pdf" {
  name                = var.pdf_app_identity_name
  resource_group_name = data.azurerm_resource_group.main.name
}

resource "azurerm_container_app" "pdf" {
  name                         = var.pdf_container_app_name
  container_app_environment_id = data.azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.main.name
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [data.azurerm_user_assigned_identity.pdf.id]
  }

  registry {
    server   = data.azurerm_container_registry.main.login_server
    identity = data.azurerm_user_assigned_identity.pdf.id
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
        value = var.application_insights_connection_string
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
}

locals {
  tags = {
    project     = "hello-buddy"
    environment = "prod"
    managed_by  = "terraform"
    assessment  = "com712"
    module      = "container-tier-app-pdf"
  }
}
