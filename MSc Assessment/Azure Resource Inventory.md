# Hello Buddy Cloud Admin

## Azure Resource Inventory

## Purpose

This document is the **as-built** inventory of the Azure resources deployed for the Hello Buddy Cloud Admin production environment (UK West, resource group `rg-hellobuddy-prod`), with each resource's purpose, exposure level, and justification. All resources below are provisioned via Terraform and verified in the Azure portal / CLI.

It is intended to support:

- implementation planning;
- Azure setup;
- assessment reporting;
- architecture-diagram labelling.

## Environment Scope

This inventory assumes:

- one production environment;
- development spun up only when needed;
- two physiotherapists;
- low traffic and modest PDF generation;
- Azure Container Apps chosen over AKS.

## Resource Group Structure

### Recommended resource group

- `rg-hellobuddy-prod`

Purpose:

- holds the production infrastructure for the assessment and MVP deployment.

Optional later resource group:

- `rg-hellobuddy-dev`

Purpose:

- temporary development and test environment when needed.

## Core Azure Resources

### 1. Azure Container Registry

Recommended name:

- `acrhellobuddyprod`

Purpose:

- stores the container images for UI, API, and PDF services.

Used by:

- Container Apps deployment pipeline;
- CI or manual image push workflow.

Why required:

- provides a secure, Azure-native image registry for container deployment.

### 2. Azure Container Apps Environment

Recommended name:

- `cae-hellobuddy-prod`

Purpose:

- shared runtime environment for the three container apps.

Used by:

- `hello-buddy-ui`
- `hello-buddy-api`
- `hello-buddy-pdf`

Why required:

- gives a common managed environment for networking, scaling, and observability.

### 3. UI Container App

Recommended name:

- `ca-hello-buddy-ui`

Purpose:

- public-facing practitioner web frontend.

Ingress:

- external.

Scaling intent:

- minimum `1` replica;
- maximum `3` replicas.

Why required:

- provides the client-visible application entry point.

### 4. API Container App

Recommended name:

- `ca-hello-buddy-api`

Purpose:

- internal application service for data access, validation, draft save, publish orchestration, and metadata persistence.

Ingress:

- internal only (`external = false`, target port `8080`).

Scaling:

- minimum `1` replica;
- maximum `3` replicas;
- HTTP scale rule at `50` concurrent requests.

Why required:

- separates business logic from the UI and enables independent scaling.

### 5. PDF Container App

Recommended name:

- `ca-hello-buddy-pdf`

Purpose:

- internal PDF rendering service (PuppeteerSharp / Chromium, `POST /render`).

Ingress:

- internal only.

Scaling:

- fixed at `1` replica (min `1` / max `1`) to keep the Chromium-backed renderer warm.

Why required:

- isolates the heaviest per-request workload from the UI.

### 6. Azure Database for MySQL Flexible Server

Recommended name:

- `mysql-hellobuddy-prod`

Purpose:

- durable structured persistence for case, programme, session, exercise-selection, and publish metadata.

Tier:

- Burstable `B_Standard_B1ms`, MySQL `8.0.21`.

Exposure:

- private only — VNet-integrated into the delegated subnet `subnet-mysql` (no public endpoint), resolved via the private DNS zone `privatelink.mysql.database.azure.com`, TLS enforced.

Database:

- `canine_physiotherapy`.

Why required:

- provides durable business-data storage that is more defensible than in-memory or blob-only persistence for this scenario.

### 7. Azure Blob Storage Account

Recommended name:

- `sthellobuddyprod`

Purpose:

- stores published PDF files.

Suggested configuration:

- standard GPv2;
- hot tier;
- private container.

Why required:

- provides durable object storage for published programme PDFs.

### 8. Blob Containers

Two private containers in `sthellobuddyprod`:

- `published-programmes` — stores generated programme PDFs; downloads issued via time-limited SAS URLs (no anonymous access).
- `dataprotection-keys` — stores the ASP.NET Core Data Protection key ring written/read by the UI's managed identity (enables stable cookie/antiforgery protection across UI replicas).

Why required:

- creates clear, separately scoped object-store boundaries for owner-facing documents and platform key material.

### 9. Azure Key Vault

Recommended name:

- `kv-hellobuddy-prod`

Purpose:

- stores database connection secrets and the Application Insights connection string.

Secrets present:

- `ConnectionStrings--CaninePhysioDb`, `mysql-connection-string`, `mysql-admin-password`, `ApplicationInsights--ConnectionString`.

Used by:

- **API** and **PDF** container apps — they read secrets **directly from Key Vault via their user-assigned managed identity** (using `KeyVault__Uri` + `AZURE_CLIENT_ID`), not via Container Apps secret references.
- The **UI does not read Key Vault** in production; it relies on the Data Protection blob container instead.

Why required:

- avoids secret leakage in source control and container configuration.

### 10. Log Analytics Workspace

Recommended name:

- `log-hellobuddy-prod`

Purpose:

- central log store for Container Apps and Azure monitoring data.

Why required:

- supports monitoring, troubleshooting, and assessment evidence capture.

### 11. Application Insights

Recommended name:

- `appi-hellobuddy-prod`

Purpose:

- application telemetry, request traces, dependency traces, and performance monitoring.

Why required:

- provides observability evidence beyond basic platform metrics.

### 12. Virtual Network and Subnets

Recommended name:

- `vnet-hellobuddy-prod` (`10.10.0.0/16`).

Subnets:

- `subnet-apps` (`10.10.2.0/24`) — delegated to `Microsoft.App/environments`; hosts the Container Apps environment and the three apps.
- `subnet-mysql` (`10.10.1.0/24`) — delegated to `Microsoft.DBforMySQL/flexibleServers`; hosts the private MySQL Flexible Server.

Why required:

- provides the private network boundary that keeps MySQL and the internal apps off the public internet.

### 13. Private DNS Zone

Recommended name:

- `privatelink.mysql.database.azure.com`, linked to the VNet.

Why required:

- resolves the MySQL private endpoint name to its private IP inside the VNet.

### 14. Network Security Group (MySQL tier)

Recommended name:

- `nsg-subnet-mysql`, associated to `subnet-mysql`.

Rules:

- `Allow-Apps-To-MySQL-3306` (priority 100) — inbound TCP `3306` from `subnet-apps` (`10.10.2.0/24`) only.
- `Deny-All-Other-Inbound` (priority 4000) — explicit deny of all other inbound to the subnet.

Why required:

- adds an explicit, auditable L4 defence-in-depth rule on top of MySQL's private-access design. No NSG is placed on `subnet-apps` because it is delegated to `Microsoft.App/environments` (platform-managed networking) and HTTPS is enforced at the Container Apps ingress (`allowInsecure = false`).

### 15. User-Assigned Managed Identities

Three identities, one per app — `uami-hellobuddy-ui`, `uami-hellobuddy-api`, `uami-hellobuddy-pdf`:

- `uami-hellobuddy-ui` — `AcrPull` on the registry + `Storage Blob Data Contributor` scoped to the `dataprotection-keys` container only (exactly two role assignments — least privilege).
- `uami-hellobuddy-api` — `AcrPull` + `Key Vault Secrets User` + `Storage Blob Data Contributor` (programme blob access).
- `uami-hellobuddy-pdf` — `AcrPull` + `Key Vault Secrets User`.

Why required:

- removes stored credentials from app configuration; each app authenticates to Azure resources with its own least-privilege identity.

## Optional Azure Resources

These are optional rather than mandatory for the initial assessment deployment.

### Azure Front Door

Recommended name:

- `afd-hellobuddy-prod`

Purpose:

- custom domain entry point, TLS, and edge routing.

Use if:

- a polished public endpoint is needed for the final demo or assessment screenshots.

### Azure DNS Zone

Recommended name:

- based on chosen custom domain

Purpose:

- DNS hosting for the public application URL.

Use if:

- custom domain is used.

## Identity And Access Model

### Managed identity strategy

Each container app has its own user-assigned managed identity (see resource 15):

- UI: `AcrPull` + `Storage Blob Data Contributor` scoped to the `dataprotection-keys` container only; does **not** read Key Vault.
- API: `AcrPull` + `Key Vault Secrets User` + `Storage Blob Data Contributor` (programme blob).
- PDF: `AcrPull` + `Key Vault Secrets User`.

### Role assignment summary

- least privilege applied consistently — roles scoped to the specific resource/container rather than the resource group;
- secrets read directly from Key Vault by API and PDF via managed identity;
- no shared credentials or connection strings stored in app configuration.

## Networking Model

### Public access

Only:

- `ca-hello-buddy-ui`

### Internal access only

- `ca-hello-buddy-api`
- `ca-hello-buddy-pdf`
- MySQL Flexible Server
- Blob private container access

### Security model summary

Defence is layered:

- **L7** — HTTPS enforced at the Container Apps ingress (`allowInsecure = false`); public edge restricted to the UI only;
- **App boundary** — API and PDF are internal-only and never called directly by users;
- **Data tier** — MySQL has no public endpoint (private access in a delegated subnet) and is further protected by `nsg-subnet-mysql`, which allows inbound `3306` only from `subnet-apps`;
- **Identity** — per-app managed identities with least-privilege RBAC; secrets read from Key Vault, never stored in application code.

## Resource Summary Table

| Resource                                | Type                       | Exposure | Purpose                                      |
| --------------------------------------- | -------------------------- | -------- | -------------------------------------------- |
| `rg-hellobuddy-prod`                    | Resource Group             | n/a      | Production resource boundary                 |
| `acrhellobuddyprod`                     | Azure Container Registry   | private  | Stores container images (UI / API / PDF)     |
| `cae-hellobuddy-prod`                   | Container Apps Environment | private  | Shared managed app runtime                   |
| `ca-hello-buddy-ui`                     | Container App              | public   | Practitioner frontend (min 1 / max 3)        |
| `ca-hello-buddy-api`                    | Container App              | internal | Business logic & persistence (min 1 / max 3) |
| `ca-hello-buddy-pdf`                    | Container App              | internal | PDF rendering service (fixed 1)              |
| `mysql-hellobuddy-prod`                 | MySQL Flexible Server      | private  | Structured business persistence              |
| `sthellobuddyprod`                      | Storage Account            | private  | Blob object storage                          |
| `published-programmes`                  | Blob Container             | private  | Published programme PDFs (SAS download)      |
| `dataprotection-keys`                   | Blob Container             | private  | ASP.NET Core Data Protection key ring        |
| `kv-hellobuddy-prod`                    | Key Vault                  | private  | Secrets (read by API & PDF via MI)           |
| `log-hellobuddy-prod`                   | Log Analytics              | private  | Central logging                              |
| `appi-hellobuddy-prod`                  | Application Insights       | private  | Telemetry and tracing                        |
| `vnet-hellobuddy-prod`                  | Virtual Network            | n/a      | `10.10.0.0/16` private network boundary      |
| `subnet-apps`                           | Subnet (delegated)         | n/a      | `10.10.2.0/24` Container Apps environment    |
| `subnet-mysql`                          | Subnet (delegated)         | n/a      | `10.10.1.0/24` MySQL Flexible Server         |
| `privatelink.mysql.database.azure.com`  | Private DNS Zone           | private  | MySQL private name resolution                |
| `nsg-subnet-mysql`                      | Network Security Group     | n/a      | L4 allow apps→3306, deny other inbound       |
| `uami-hellobuddy-ui/api/pdf`            | Managed Identity (x3)      | n/a      | Per-app least-privilege RBAC                 |

## Minimum Required For The Assessment

If scope must stay tight, the minimum practical Azure set is:

- Azure Container Registry
- Azure Container Apps Environment
- UI Container App
- API Container App
- PDF Container App
- MySQL Flexible Server
- Storage Account with Blob container
- Log Analytics

## Recommendation

Keep the production inventory intentionally small. The goal is not to show every Azure service available. The goal is to show a coherent, cost-aware, secure architecture with clear container boundaries and enough supporting managed services to justify the design academically.
