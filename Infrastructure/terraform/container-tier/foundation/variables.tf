variable "subscription_id" {
  description = "Azure subscription ID."
  type        = string
}

variable "resource_group_name" {
  description = "Existing resource group from vnet-tier."
  type        = string
  default     = "rg-hellobuddy-prod"
}

variable "key_vault_name" {
  description = "Existing key vault from data-tier."
  type        = string
  default     = "kv-hellobuddy-prod"
}

variable "subnet_apps_id" {
  description = "Subnet ID delegated to Container Apps Environment."
  type        = string
}

variable "log_analytics_workspace_name" {
  type    = string
  default = "log-hellobuddy-prod"
}

variable "application_insights_name" {
  type    = string
  default = "appi-hellobuddy-prod"
}

variable "acr_name" {
  type    = string
  default = "acrhellobuddyprod"
}

variable "storage_account_name" {
  type    = string
  default = "sthellobuddyprod"
}

variable "container_app_environment_name" {
  type    = string
  default = "cae-hellobuddy-prod"
}

variable "ui_app_identity_name" {
  type    = string
  default = "uami-hellobuddy-ui"
}

variable "api_app_identity_name" {
  type    = string
  default = "uami-hellobuddy-api"
}

variable "pdf_app_identity_name" {
  type    = string
  default = "uami-hellobuddy-pdf"
}
