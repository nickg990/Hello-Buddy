# Hello Buddy Cloud Admin

## Azure Resource Inventory

## Purpose

This document lists the recommended Azure resources for the Hello Buddy Cloud Admin production environment, with each resource's purpose, exposure level, and justification.

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

- internal only.

Scaling intent:

- minimum `0` replicas;
- maximum `3` replicas.

Why required:

- separates business logic from the UI and enables independent scaling.

### 5. PDF Container App

Recommended name:

- `ca-hello-buddy-pdf`

Purpose:

- internal PDF rendering service.

Ingress:

- internal only.

Scaling intent:

- minimum `0` replicas;
- maximum `2` replicas.

Why required:

- isolates the heaviest per-request workload from the UI.

### 6. Azure Database for MySQL Flexible Server

Recommended name:

- `mysql-hellobuddy-prod`

Purpose:

- durable structured persistence for case, programme, session, exercise-selection, and publish metadata.

Tier intent:

- Burstable `B1ms` to start.

Exposure:

- private only.

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

### 8. Blob Container

Recommended name:

- `published-programmes`

Purpose:

- stores generated PDF outputs for the application.

Why required:

- creates a clear and controlled object store boundary for owner-facing documents.

### 9. Azure Key Vault

Recommended name:

- `kv-hellobuddy-prod`

Purpose:

- stores database connection secrets, storage configuration, and any other sensitive settings.

Used by:

- API container app;
- PDF container app if needed;
- deployment pipeline.

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

Use managed identities for container apps where possible.

Recommended identity usage:

- UI: minimal access, likely none beyond internal API calls;
- API: access to Key Vault and Blob Storage;
- PDF: access only if direct write or secret lookup is required.

### Role assignment intent

- Blob Storage contributor or narrower scoped role for API where needed;
- Key Vault secrets access for API and PDF where needed;
- least privilege applied consistently.

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

- public edge restricted to UI only;
- internal services never called directly by users;
- database not publicly accessible;
- secrets stored outside application code.

## Resource Summary Table

| Resource                | Type                       | Exposure | Purpose                                      |
| ----------------------- | -------------------------- | -------- | -------------------------------------------- |
| `rg-hellobuddy-prod`    | Resource Group             | n/a      | Production resource boundary                 |
| `acrhellobuddyprod`     | Azure Container Registry   | private  | Stores container images                      |
| `cae-hellobuddy-prod`   | Container Apps Environment | private  | Shared managed app runtime                   |
| `ca-hello-buddy-ui`     | Container App              | public   | Practitioner frontend                        |
| `ca-hello-buddy-api`    | Container App              | internal | Business logic and persistence orchestration |
| `ca-hello-buddy-pdf`    | Container App              | internal | PDF rendering service                        |
| `mysql-hellobuddy-prod` | MySQL Flexible Server      | private  | Structured business persistence              |
| `sthellobuddyprod`      | Storage Account            | private  | PDF object storage                           |
| `published-programmes`  | Blob Container             | private  | Published PDF files                          |
| `kv-hellobuddy-prod`    | Key Vault                  | private  | Secrets management                           |
| `log-hellobuddy-prod`   | Log Analytics              | private  | Central logging                              |
| `appi-hellobuddy-prod`  | Application Insights       | private  | Telemetry and tracing                        |

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
