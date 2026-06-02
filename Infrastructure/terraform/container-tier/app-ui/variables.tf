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

variable "ui_app_identity_name" {
  type    = string
  default = "uami-hellobuddy-ui"
}

variable "storage_account_name" {
  type    = string
  default = "sthellobuddyprod"
}

variable "ui_container_app_name" {
  type    = string
  default = "ca-hello-buddy-ui"
}

variable "ui_app_image" {
  type = string
}

variable "api_uri" {
  type = string
}

variable "seeded_practitioner_id" {
  type    = string
  default = "1"
}

variable "dataprotection_container" {
  type    = string
  default = "dataprotection-keys"
}

variable "application_insights_connection_string" {
  type      = string
  sensitive = true
}
