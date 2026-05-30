variable "subscription_id" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
  default = "ukwest"
}

variable "vnet_name" {
  type = string
  default = "vnet-hellobuddy-prod"
}

variable "vnet_address_space" {
  type = string
  default = "10.10.0.0/16"
}

variable "subnet_pe_name" {
  type = string
  default = "subnet-pe"
}

variable "subnet_pe_prefix" {
  type = string
  default = "10.10.1.0/24"
}

variable "subnet_apps_name" {
  type = string
  default = "subnet-apps"
}

variable "subnet_apps_prefix" {
  type = string
  default = "10.10.2.0/24"
}
