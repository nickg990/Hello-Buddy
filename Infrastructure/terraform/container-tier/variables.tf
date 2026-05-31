variable "subscription_id" {
  description = "Azure subscription ID."
  type        = string
}

variable "resource_group_name" {
  description = "Existing resource group from the vnet-tier module."
  type        = string
  default     = "rg-hellobuddy-prod"
}

variable "key_vault_name" {
  description = "Existing Key Vault from the data-tier module."
  type        = string
  default     = "kv-hellobuddy-prod"
}

variable "subnet_apps_id" {
  description = "Subnet ID delegated to Container Apps Environment (from vnet-tier output)."
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
  description = "Azure Container Registry name (3-50 alphanumeric, globally unique)."
  type        = string
  default     = "acrhellobuddyprod"
}

variable "storage_account_name" {
  description = "Storage account name (3-24 lowercase alphanumeric, globally unique)."
  type        = string
  default     = "sthellobuddyprod"
}

variable "container_app_environment_name" {
  type    = string
  default = "cae-hellobuddy-prod"
}

# ----- Three Container Apps (DEC-009) --------------------------------------

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

variable "ui_container_app_name" {
  type    = string
  default = "ca-hello-buddy-ui"
}

variable "api_container_app_name" {
  type    = string
  default = "ca-hello-buddy-api"
}

variable "pdf_container_app_name" {
  type    = string
  default = "ca-hello-buddy-pdf"
}

variable "ui_app_image" {
  description = "Fully-qualified image reference for the UI app."
  type        = string
  default     = ""
}

variable "api_app_image" {
  description = "Fully-qualified image reference for the API app."
  type        = string
  default     = ""
}

variable "pdf_app_image" {
  description = "Fully-qualified image reference for the PDF service."
  type        = string
  default     = ""
}

variable "seeded_practitioner_id" {
  description = "Practitioner ID sent by UI to API in the X-Practitioner-Id header."
  type        = string
  default     = "1"
}

variable "deploy_container_apps" {
  description = "Set to false on first apply (foundation only); set to true after images are in ACR."
  type        = bool
  default     = false
}
