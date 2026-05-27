variable "subscription_id" {
  description = "Azure subscription ID."
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group."
  type        = string
  default     = "rg-hellobuddy-prod"
}

variable "location" {
  description = "Azure region for all resources."
  type        = string
  default     = "uksouth"
}

variable "key_vault_name" {
  description = "Name of the Azure Key Vault (globally unique, 3–24 alphanumeric/hyphens)."
  type        = string
  default     = "kv-hellobuddy-prod"
}

variable "mysql_server_name" {
  description = "Name of the MySQL Flexible Server (globally unique)."
  type        = string
  default     = "mysql-hellobuddy-prod"
}

variable "mysql_admin_username" {
  description = "MySQL administrator login name."
  type        = string
  sensitive   = true
}

variable "mysql_admin_password" {
  description = "MySQL administrator password. Must meet Azure complexity requirements."
  type        = string
  sensitive   = true
}

variable "developer_ip" {
  description = "Developer machine public IP for the temporary MySQL firewall rule. Remove before final evidence capture (US-29)."
  type        = string
}

variable "mysql_location" {
  description = "Azure region for the MySQL Flexible Server. May differ from the main location if quota is unavailable in that region."
  type        = string
  default     = ""
}
