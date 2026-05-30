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

variable "ui_app_image" {
	description = "Container image for the UI app."
	type        = string
	default     = ""
}

variable "api_app_image" {
	description = "Container image for the API app."
	type        = string
	default     = ""
}

variable "pdf_app_image" {
	description = "Container image for the PDF app."
	type        = string
	default     = ""
}

variable "deploy_container_apps" {
	description = "Whether to deploy the container apps."
	type        = bool
	default     = false
}

variable "seeded_practitioner_id" {
	description = "Practitioner ID for seeded data."
	type        = string
	default     = ""
}
