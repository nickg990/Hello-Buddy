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

variable "media_search_provider_description" {
  description = "Description shown in the UI video search provider dropdown (e.g., Cloud storage)."
  type        = string
  default     = "Google Drive"
}

variable "media_search_provider_base_url" {
  description = "Base URL opened by the UI video search provider."
  type        = string
  default     = "https://drive.google.com/drive/u/1/folders/13mCIF8x8VNVfg30xbbnrAxKEFRh2QF9C"
}

variable "deploy_container_apps" {
  description = "Set to false on first apply (foundation only); set to true after images are in ACR."
  type        = bool
  default     = false
}

variable "container_min_replicas" {
  description = "Minimum replicas for all container apps. Must remain 0 to allow scale-to-zero."
  type        = number
  default     = 0

  validation {
    condition     = var.container_min_replicas == 0
    error_message = "Container configuration previously used a minimum of 1 replica, which prevented containers from scaling down to zero and incurred constant idle costs. Set min replicas to 0."
  }
}

variable "container_max_replicas" {
  description = "Maximum replicas for all container apps."
  type        = number
  default     = 1

  validation {
    condition     = var.container_max_replicas == 1
    error_message = "Container max replicas must be 1 for this cost-control profile."
  }
}

variable "container_scale_cooldown_period" {
  description = "KEDA cooldown period in seconds for HTTP scaler rules."
  type        = number
  default     = 120

  validation {
    condition     = var.container_scale_cooldown_period == 120
    error_message = "Container scaler cooldown period must be 120 seconds for this environment."
  }
}

variable "container_scale_polling_interval" {
  description = "KEDA polling interval in seconds for HTTP scaler rules."
  type        = number
  default     = 30

  validation {
    condition     = var.container_scale_polling_interval == 30
    error_message = "Container scaler polling interval must be 30 seconds for this environment."
  }
}
