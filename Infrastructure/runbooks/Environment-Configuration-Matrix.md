# Environment Configuration Matrix

This matrix defines required settings for Release 1 prototype operations.

## Application Settings

| Key | Local Dev | Azure | Required | Notes |
| --- | --- | --- | --- | --- |
| ConnectionStrings:CaninePhysioDb | Local MySQL connection string | Key Vault secret `ConnectionStrings--CaninePhysioDb` | Yes | API startup depends on this value. |
| KeyVault:Uri | Optional | `https://<key-vault>.vault.azure.net/` | Azure only | API resolves layered secrets from Key Vault. |
| Storage:Mode | `Azurite` (default local) | `Azure` | Yes | Controls blob provider. |
| Storage:ConnectionString | `UseDevelopmentStorage=true` (Azurite) | Not used in Azure mode | Local Azurite lane | Local blob emulation path. |
| Storage:BlobServiceUri | Optional local | Storage account blob endpoint | Azure only | Used by API in Azure mode. |
| Storage:PublishedProgrammesContainer | `published-programmes` | `published-programmes` | Yes | Shared for publish artifacts. |
| Storage:ExerciseMediaBaseUrl | Absolute local blob URL | Absolute Azure blob URL | Yes | Must be absolute when blob-backed storage is enabled. |
| PdfService:Uri | `http://localhost:5081` | Internal ACA PDF FQDN | Yes | API to PDF service call path. |
| Api:Uri (UI) | `http://localhost:5080` | Internal ACA API FQDN | Yes | UI to API call path. |
| SeededPractitionerId (UI) | `1` | `1` or environment value | Yes | Header injection id for demo/testing path. |
| Security:AllowedPractitionerIds | Optional | Optional allow-list | No | Optional boundary hardening. |
| ApplicationInsights:ConnectionString | Optional | Required in Azure | Azure recommended | Enables release telemetry checks. |
| DataProtection:BlobUri (UI) | Optional local | Required in Azure | Azure only | Shared DP keys in storage container. |

## Release Gate Variables

| Variable | Purpose | Required |
| --- | --- | --- |
| HELLOBUDDY_TEST_DB_CONNECTION | Real-DB integration tests | Yes for full local gate |
| HELLOBUDDY_TEST_DB_RESET_CONNECTION | Integration reset override | Optional |
| HELLOBUDDY_RUN_AZURITE_TESTS | Enable Azurite test lane | Optional |
| HELLOBUDDY_AZURITE_CONNECTION | Custom Azurite connection | Optional |

## Infrastructure Inputs

| Terraform Variable | Scope | Required | Notes |
| --- | --- | --- | --- |
| subscription_id | All tiers | Yes | Azure subscription context. |
| subnet_apps_id | container-tier | Yes | Delegated subnet for ACA environment. |
| deploy_container_apps | container-tier | Yes | `false` for foundation phase, `true` for app phase. |
| ui_app_image / api_app_image / pdf_app_image | container-tier | Yes for app phase | Fully qualified ACR image refs. |
| mysql_admin_password | data-tier | Yes | Stored in Key Vault secret. |

## Validation Checklist

- Confirm `deploy.ps1` mode matches intent (`full`, `-AppsOnly`, or component-only).
- Confirm image tag exists in ACR before app-only deploy.
- Confirm required secrets and env vars resolve in active revision.
- Confirm `/healthz` endpoints pass after deployment.
