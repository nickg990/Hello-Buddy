variable "subscription_id" {
  description = "Azure subscription ID."
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group (created by this module)."
  type        = string
  default     = "rg-hellobuddy-prod"
}

variable "location" {
  description = "Azure region for all resources."
  type        = string
  default     = "ukwest"
}

variable "vnet_name" {
  type    = string
  default = "vnet-hellobuddy-prod"
}

variable "vnet_address_space" {
  type    = string
  default = "10.10.0.0/16"
}

variable "subnet_mysql_name" {
  description = "Subnet delegated to MySQL Flexible Server."
  type        = string
  default     = "subnet-mysql"
}

variable "subnet_mysql_prefix" {
  type    = string
  default = "10.10.1.0/24"
}

variable "subnet_apps_name" {
  description = "Subnet delegated to Container Apps Environment."
  type        = string
  default     = "subnet-apps"
}

variable "subnet_apps_prefix" {
  type    = string
  default = "10.10.2.0/24"
}
