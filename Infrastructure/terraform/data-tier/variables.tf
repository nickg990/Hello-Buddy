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

variable "mysql_subnet_id" {
  description = "Subnet ID delegated to MySQL Flexible Server (from vnet-tier output)."
  type        = string
}

variable "mysql_private_dns_zone_id" {
  description = "Private DNS zone ID for MySQL (from vnet-tier output)."
  type        = string
}
