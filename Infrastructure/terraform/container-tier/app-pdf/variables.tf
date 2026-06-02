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

variable "pdf_app_identity_name" {
  type    = string
  default = "uami-hellobuddy-pdf"
}

variable "pdf_container_app_name" {
  type    = string
  default = "ca-hello-buddy-pdf"
}

variable "pdf_app_image" {
  type = string
}

variable "application_insights_connection_string" {
  type      = string
  sensitive = true
}
