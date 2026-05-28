variable "subscription_id" {
  description = "Azure subscription ID."
  type        = string
}

variable "resource_group_name" {
  description = "Existing resource group from the data-tier module."
  type        = string
  default     = "rg-hellobuddy-prod"
}

variable "key_vault_name" {
  description = "Existing Key Vault from the data-tier module."
  type        = string
  default     = "kv-hellobuddy-prod"
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

# ----- Three Container Apps (Day 4a) ---------------------------------------

variable "ui_app_identity_name" {
  description = "User-assigned managed identity for the UI container (ACR pull only)."
  type        = string
  default     = "uami-hellobuddy-ui"
}

variable "api_app_identity_name" {
  description = "User-assigned managed identity for the API container (ACR pull, KV get/list, Blob contributor)."
  type        = string
  default     = "uami-hellobuddy-api"
}

variable "pdf_app_identity_name" {
  description = "User-assigned managed identity for the PDF container (ACR pull only)."
  type        = string
  default     = "uami-hellobuddy-pdf"
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
  description = "Fully-qualified image reference for the UI app, e.g. acrhellobuddyprod.azurecr.io/hello-buddy-ui:v1"
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
  description = "Practitioner id sent by UI to API in the X-Practitioner-Id header (Release 1; TD-005 replaces with Entra JWT)."
  type        = string
  default     = "1"
}

variable "deploy_container_apps" {
  description = "Whether to create the three Container App resources. Set to false on the first apply (foundation only); set to true after images have been pushed to ACR."
  type        = bool
  default     = false
}
