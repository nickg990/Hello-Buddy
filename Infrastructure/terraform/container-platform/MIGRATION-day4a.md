# Day 4a state migration: single Container App -> UI / API / PDF

## What changes in state

The Day 3 config had:

- `azurerm_user_assigned_identity.admin_app` (Azure name `uami-hellobuddy-admin`)
- `azurerm_role_assignment.admin_app_acr_pull`
- `azurerm_role_assignment.admin_app_blob_contributor`
- `azurerm_key_vault_access_policy.admin_app`
- `azurerm_container_app.admin` (Azure name `ca-hello-buddy-admin`)

The Day 4a config replaces these with three apps + three identities + per-app role grants.

Container App names are immutable in Azure, so `ca-hello-buddy-admin` cannot be renamed in place — it must be destroyed and `ca-hello-buddy-ui` created. This is the "destroy + create" path the brief offers as an alternative to `terraform state mv` and is the path we take.

Expect the Terraform plan to show approximately:

- **Destroy** (5): `azurerm_container_app.admin[0]`, `azurerm_user_assigned_identity.admin_app`, `azurerm_role_assignment.admin_app_acr_pull`, `azurerm_role_assignment.admin_app_blob_contributor`, `azurerm_key_vault_access_policy.admin_app`
- **Create** (3 identities + 5 role grants + 1 KV policy + 3 container apps = 12)

Unchanged: ACR, Storage account + container, Key Vault secrets, App Insights, Log Analytics, Container Apps Environment.

## Run order

```pwsh
cd C:\Projects\Hello-Buddy\Infrastructure\terraform\container-platform

# Optional dry-run with the new variable names so you can read the plan first
terraform plan -var "deploy_container_apps=true" `
               -var "ui_app_image=acrhellobuddyprod.azurecr.io/hello-buddy-ui:v1" `
               -var "api_app_image=acrhellobuddyprod.azurecr.io/hello-buddy-api:v1" `
               -var "pdf_app_image=acrhellobuddyprod.azurecr.io/hello-buddy-pdf:v1"

# Full two-phase deploy (foundation -> az acr build x3 -> container apps)
./deploy.ps1
```

## Downtime window

The live UI URL changes from `ca-hello-buddy-admin.<env>.azurecontainerapps.io` to `ca-hello-buddy-ui.<env>.azurecontainerapps.io`. Old URL returns 404 for a few minutes; new URL becomes live as soon as `azurerm_container_app.ui` apply completes. Acceptable for an MSc assessment env with no real users.

## Manual rollback (if needed)

Old image tag `hello-buddy-admin:v2` is still in ACR. To restore:

1. `git checkout` the Day 3 main.tf / variables.tf / deploy.ps1.
2. `terraform apply` — recreates `ca-hello-buddy-admin` with the old image.
