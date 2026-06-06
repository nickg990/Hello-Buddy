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

variable "container_min_replicas" {
  description = "Minimum replicas for the PDF container app. Must remain 0 to allow scale-to-zero."
  type        = number
  default     = 0

  validation {
    condition     = var.container_min_replicas == 0
    error_message = "Container configuration previously used a minimum of 1 replica, which prevented containers from scaling down to zero and incurred constant idle costs. Set min replicas to 0."
  }
}

variable "container_max_replicas" {
  description = "Maximum replicas for the PDF container app."
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
