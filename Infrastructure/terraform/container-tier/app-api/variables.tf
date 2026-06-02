variable "subscription_id" {
  type = string
}

variable "resource_group_name" {
  type    = string
  default = "rg-hellobuddy-prod"
}

variable "container_app_environment_name" {
  type    = string
  default = "cae-hellobuddy-prod"
}

variable "acr_name" {
  type    = string
  default = "acrhellobuddyprod"
}

variable "api_app_identity_name" {
  type    = string
  default = "uami-hellobuddy-api"
}

variable "storage_account_name" {
  type    = string
  default = "sthellobuddyprod"
}

variable "key_vault_name" {
  type    = string
  default = "kv-hellobuddy-prod"
}

variable "api_container_app_name" {
  type    = string
  default = "ca-hello-buddy-api"
}

variable "api_app_image" {
  type = string
}

variable "published_programmes_container" {
  type    = string
  default = "published-programmes"
}

variable "pdf_service_uri" {
  type = string
}

variable "application_insights_connection_string" {
  type      = string
  sensitive = true
}
