# Hello Buddy Cloud Admin — Containerised Cloud Deployment

## COM712 Assessment 3 (Option C) — Technical Report

**Student:** Nick (G62723)
**Module:** COM712 Cloud Computing
**Assessment:** 3 — Option C (Containerised Cloud Solution)
**Cloud platform:** Microsoft Azure (UK West)
**Date:** June 2026

---

## 1. Executive Summary

Small canine physiotherapy practices typically author rehabilitation programmes by hand and distribute them as ad hoc documents, which causes duplicated effort, weak version control, poor auditability, and inconsistent owner instructions. To address this, I designed, built, and deployed *Hello Buddy Cloud Admin*: a containerised, practitioner-facing administration system that stores synthetic case data, supports draft programme creation, generates a branded owner-facing PDF through a dedicated rendering service, and persists published outputs to cloud object storage.

The solution was deployed to **Azure Container Apps** as three single-purpose containers — a public web UI, an internal API, and an internal PDF renderer — supported by Azure Database for MySQL Flexible Server, Azure Blob Storage, Azure Key Vault, Azure Container Registry, and the Azure Monitor / Application Insights / Log Analytics observability stack. All infrastructure is defined in Terraform, all images are built and stored in a private registry, and all runtime identity is provided by per-app user-assigned managed identities with least-privilege role assignments.

Measured outcomes include a working end-to-end publish flow (browser → UI → API → PDF → Blob correlated under a single Application Insights operation), demonstrable horizontal autoscaling of the UI under load, a private-only data tier with an explicit network security group, and a seven-day cost footprint well inside the £30/month design budget. Azure Container Apps was chosen over both an always-on VM baseline and Azure Kubernetes Service; the comparison and trade-offs are defended in Section 3.

---

## 2. Scenario and Requirements

### 2.1 The problem

Hello Buddy is a canine physiotherapy brand. A practitioner assesses a dog, then prescribes a structured rehabilitation programme of exercises (repetitions, sets, hold time, frequency, and notes) that the owner follows at home. Today these programmes are produced manually, which makes them slow to author, easy to mis-version, and difficult to audit.

### 2.2 Practitioner needs (functional)

The assessed vertical slice supports a practitioner to:

- browse synthetic owner, pet, and treatment-case data;
- create and edit a draft rehabilitation programme linked to a case;
- preview the owner-facing output and validate missing fields before publishing;
- publish a final branded PDF through a dedicated rendering service;
- download the published PDF via a time-limited secure link;
- rely on operational logging of programme-created, publish-requested, PDF-generated, and file-persisted events.

### 2.3 Non-functional and security constraints

- **Containerisation and portability:** at least two single-purpose images, runnable locally via Docker Compose and in the cloud via the same images.
- **Security:** no secrets in source control or image layers; least-privilege managed identities; public exposure limited to a single HTTPS ingress; the database is never publicly reachable.
- **Data ethics:** only synthetic data is used for development, testing, screenshots, and this report. The domain is health-*adjacent* (animal rehabilitation), so although it is not personal health data under UK GDPR, owner and pet records are treated with the same care — minimised, synthetic, and access-controlled — to model responsible handling.
- **Observability and performance:** logs, metrics, and traces sufficient to demonstrate health and to diagnose a fault; the public service must demonstrate autoscaling under scripted load.
- **Cost and sustainability:** the design must support a defensible seven-day cost estimate against an always-on VM baseline and a carbon comparison using a published method.

### 2.4 Workload and traffic assumptions

The production profile is deliberately small: a single production environment, approximately two practitioners, a typical operating window of roughly 09:00–17:00 on working days, low interactive traffic, and moderate PDF-generation volume. These assumptions drive every sizing and scaling decision below and underpin the cost model in Section 7.

---

## 3. Design Justification

### 3.1 Platform choice: Azure Container Apps

I selected Azure Container Apps (ACA) as the runtime. ACA provides managed HTTP ingress, per-service revisions and autoscaling, internal service-to-service networking, and first-class integration with Azure Monitor — all without the operational overhead of managing a Kubernetes control plane and node pool. For a low-volume, cost-sensitive workload with a clear two-/three-container shape, ACA delivers the strongest balance of separation of concerns, security, scalability, and affordability.

### 3.2 Compute model comparison

The table below compares the assessed ACA deployment against the two principal alternatives the rubric calls for — a self-managed VM baseline and serverless-container platforms (Azure Container Instances as the Azure equivalent, and AWS Fargate as the cross-cloud reference). Azure Kubernetes Service is discussed separately in 3.3.

| Dimension | Always-on VM (IaaS baseline) | Serverless containers (ACI / AWS Fargate) | **Azure Container Apps (chosen)** |
| --- | --- | --- | --- |
| **Scaling** | Manual or VMSS; you size and patch the host; no scale-to-zero | Per-container; Fargate scales tasks, ACI is per-group and has weaker autoscale primitives | Declarative HTTP/CPU/concurrency rules per app; scale-to-zero for internal services |
| **Networking** | Full control, full responsibility (NSGs, LB, routing all manual) | Basic; ACI VNet integration is limited; Fargate uses task ENIs in a VPC | Managed ingress + built-in internal service discovery; VNet integration with delegated subnet |
| **Operational complexity** | Highest — OS patching, runtime, web server, restarts all owned by you | Low for single tasks, but orchestration/observability is bring-your-own | Low — platform owns the host; revisions, rollout, and health are managed |
| **Observability** | DIY agents and dashboards | Basic container logs; richer telemetry is bring-your-own | Native Log Analytics + Application Insights, distributed tracing across apps |
| **Cost behaviour** | Pays 24×7 regardless of traffic; cheapest only at sustained high utilisation | Pay-per-execution; good for bursts, weaker for an always-warm public endpoint | Consumption billing; keep only the UI warm, let API/PDF scale down; cheapest for bursty/low-volume |
| **Portability** | Image is portable; host config is not | Image-portable; platform glue (task defs) is provider-specific | Image-portable; built on OSS KEDA/Dapr/Envoy, so concepts transfer to Kubernetes |

**Decision (defended).** ACA is chosen over a VM because it removes host maintenance, gives cleaner autoscaling and observability evidence, and is cheaper for a bursty, low-utilisation workload. The honest counter-argument is that a single right-sized, reserved VM can be cheaper at *sustained high* utilisation and gives lower-level control; this workload is the opposite profile (intermittent, office-hours, two users), so the counter-argument does not apply here. ACA is chosen over plain ACI because ACI's autoscaling and observability primitives are weaker, which would make the scaling and tracing evidence harder to produce.

### 3.3 Why Azure Kubernetes Service was evaluated and rejected

AKS is technically valid and offers stronger Kubernetes-native control, richer scheduling, and a better fit for large multi-service systems. It was rejected on cost and operational-overhead grounds: node pools are paid for even when idle, small workloads still need capacity for system and application pods, and ingress/monitoring add further overhead. Even on the AKS Free tier (no control-plane charge), node compute alone makes a ~£30/month target unrealistic. For two practitioners working office hours, the fixed cost floor and cluster-management burden of AKS are not justified.

### 3.4 Data tier: managed MySQL over a database container

Azure Database for MySQL Flexible Server (Burstable `B_Standard_B1ms`) was chosen over running MySQL in a container in the cloud. This separates stateful database operations from stateless application compute and gives managed backups, patching, and resilience — materially more credible than a self-hosted database pod. The counter-argument is higher service cost and reduced symmetry with the local Docker Compose stack (which does use a MySQL container); this is accepted because schema parity is maintained by applying the same SQL scripts to both environments.

### 3.5 Dedicated PDF service

PDF rendering (PuppeteerSharp driving headless Chromium) is isolated in its own container. This satisfies the multi-container requirement with a *meaningful* boundary, isolates the heaviest per-request workload from the UI, and creates an explicit, observable service interaction. The counter-argument is added complexity over inline generation; this is accepted because it enables independent scaling and is the correct shape for the queue-backed scale-out path discussed in Section 8.

---

## 4. Architecture

### 4.1 Logical architecture

The system is three single-purpose application containers in one Azure Container Apps environment, plus managed Azure services for data, secrets, registry, and observability.

- **`ca-hello-buddy-ui`** — public HTTPS ingress; server-rendered ASP.NET Core (Razor) practitioner UI; holds no data-plane credentials and never talks to MySQL directly.
- **`ca-hello-buddy-api`** — internal ingress only (port 8080); owns the `CaninePhysioDbContext`, validation, the publish flow, and the secure-download (SAS) path; calls the PDF service.
- **`ca-hello-buddy-pdf`** — internal ingress only; wraps PuppeteerSharp + Chromium behind `POST /render`; fixed at one warm replica.

The end-to-end publish path is: **browser → UI → API → PDF → Blob**, with the API recording programme/version metadata in MySQL.

### 4.2 Deployment / network topology

- **VNet** `vnet-hellobuddy-prod` (`10.10.0.0/16`).
- **`subnet-apps`** (`10.10.2.0/24`) — delegated to `Microsoft.App/environments`; hosts the Container Apps environment.
- **`subnet-mysql`** (`10.10.1.0/24`) — delegated to `Microsoft.DBforMySQL/flexibleServers`; hosts the private MySQL server.
- **Private DNS zone** `privatelink.mysql.database.azure.com`, VNet-linked, resolves MySQL to a private IP.
- **`nsg-subnet-mysql`** on the MySQL subnet: allow inbound TCP 3306 from `subnet-apps` only (priority 100), deny all other inbound (priority 4000).

The diagrams referenced by this report are maintained in `Azure Production Architecture Diagram.md` (one logical architecture diagram and one network/deployment diagram), aligned verbatim to `Azure Architecture Diagram Specification.md`.

---

## 5. Build and Deploy

### 5.1 Images and registry

Each service has its own Dockerfile and is published as a tagged image to the private Azure Container Registry `acrhellobuddyprod`: `hello-buddy-ui:v2`, `hello-buddy-api:v1`, `hello-buddy-pdf:v1`. Images are built with **`az acr build`** (server-side build inside ACR) rather than a local `docker build`. This was a deliberate decision (DEC-006): the development host's corporate egress blocked the Debian package mirrors during `apt-get`, so a server-side build inside ACR — which has unrestricted egress and uses the identical Dockerfile — produced a bit-equivalent artefact without waiting on a network exception.

### 5.2 Local parity

A `docker-compose.yml` runs the UI, API, PDF, and a MySQL container together for local integration testing, demonstrating containerisation before any cloud deployment. The local and Azure databases are built from the same SQL scripts in the same order, so schema parity is guaranteed; application code does not branch on environment — only the configuration source changes.

### 5.3 Infrastructure as code

All Azure resources are defined in Terraform (azurerm provider), split into a network/data tier and a container-platform tier. A real-world capacity issue surfaced during deployment (DEC-003): MySQL Flexible Server `B1ms` was not provisionable in UK South on that date, so the region was moved to **UK West** via a single Terraform variable change and a fresh apply — a concrete demonstration of the value of declarative IaC over manual portal recreation. The most recent infrastructure change in this submission window was adding the MySQL-tier NSG (`nsg-subnet-mysql`) via a targeted, plan-first Terraform apply.

### 5.4 Secret injection (managed-identity-direct, not secret references)

Secrets are **not** baked into images or held as Container Apps secret references. Instead, the **API and PDF** containers read secrets *directly from Key Vault at runtime via their user-assigned managed identity*, using a `KeyVault__Uri` plus `AZURE_CLIENT_ID` configuration pair resolved by `DefaultAzureCredential`. Key Vault `kv-hellobuddy-prod` holds `ConnectionStrings--CaninePhysioDb`, `mysql-connection-string`, `mysql-admin-password`, and `ApplicationInsights--ConnectionString`. The **UI does not read Key Vault** at all; it instead persists its ASP.NET Core Data Protection key ring to a private blob container (Section 6.3). This keeps each identity's blast radius minimal.

### 5.5 Storage configuration

Storage account `sthellobuddyprod` hosts two private containers: `published-programmes` (generated PDFs, downloaded via time-limited SAS) and `dataprotection-keys` (the UI's Data Protection key ring). `allowBlobPublicAccess` is disabled at the account level; both containers are private with no anonymous access.

---

## 6. Network, Security, and Scaling

### 6.1 Layered network and exposure model

Isolation is layered rather than relying on a single control:

1. **L7 — HTTPS at ingress.** The UI is the only internet-facing service; its ingress enforces HTTPS (`allowInsecure = false`).
2. **App boundary — internal ingress.** The API and PDF apps have `external = false`; their FQDNs resolve only inside the Container Apps environment, so no path from the public internet reaches them.
3. **L4 — network security group.** `nsg-subnet-mysql` permits inbound 3306 only from the apps subnet and denies all other inbound, an explicit, auditable defence-in-depth control on top of the private-access design.
4. **Data tier — private only.** MySQL has no public endpoint; it is VNet-integrated in a delegated subnet and resolved through a private DNS zone, with TLS enforced.

> *Note on NSG placement.* No NSG is applied to `subnet-apps` because it is delegated to `Microsoft.App/environments`, where networking is platform-managed; the HTTPS-only guarantee on that tier is an L7 ingress control, which an NSG (an L3/L4 control) cannot provide. Placing the NSG on the data subnet is where it adds genuine, verifiable value.

### 6.2 Identity and access (least privilege)

Each app has its own user-assigned managed identity:

| Identity | Role assignments |
| --- | --- |
| `uami-hellobuddy-ui` | `AcrPull` (registry) + `Storage Blob Data Contributor` scoped **only** to the `dataprotection-keys` container |
| `uami-hellobuddy-api` | `AcrPull` + `Key Vault Secrets User` + `Storage Blob Data Contributor` (programme blob) + `Storage Blob Delegator` (account scope, for SAS) |
| `uami-hellobuddy-pdf` | `AcrPull` + `Key Vault Secrets User` |

The UI identity, for example, has exactly two role assignments and cannot see, list, or read the `published-programmes` container — least privilege is genuine, not nominal, and is evidenced by a role-assignment listing.

### 6.3 Secure download and session durability

Published PDFs are downloaded via **user-delegation SAS** URLs minted by the API (30-minute TTL) rather than anonymous public read (DEC-011). An expired or query-stripped URL is rejected by the storage endpoint (403 / 409), proving the private posture holds under test. Separately, the UI persists its Data Protection key ring to blob storage (DEC-012) so that antiforgery tokens and auth cookies survive both multi-replica scale-out and revision swaps — without which a routine image push would invalidate every active session mid-demo.

### 6.4 Inter-service authentication

For Release 1, the UI propagates a single `X-Practitioner-Id` header on every API call via a `DelegatingHandler`; the API returns 401 for any request missing it (DEC-010). Network trust is provided by the internal-ingress boundary. A full Entra-issued workload-identity JWT was evaluated and deferred to production (recorded as technical debt), because with a single seeded practitioner and no public path to the API, cryptographic inter-service auth would be symbolic rather than operational at this scale.

### 6.5 Autoscaling

The public UI scales horizontally on an HTTP concurrency rule (`min 1 / max 3`, concurrent-request threshold 50) so it avoids cold starts while still demonstrating a visible scale-out story under load. The API also runs `min 1 / max 3`. The PDF renderer is fixed at one warm replica because Chromium cold-start would otherwise add latency to the synchronous publish path; the report acknowledges (Section 8) that a queue-backed worker is the correct production pattern for bursty publish volume.

---

## 7. Test, Monitor, and Fix

### 7.1 Test strategy

| Test | Method | Outcome |
| --- | --- | --- |
| Local integration | `docker compose up` (UI + API + PDF + MySQL) | All services start and inter-communicate |
| Cloud smoke | `GET /healthz` on the UI public FQDN over HTTPS | `{"status":"ok"}` |
| PDF generation | Publish a programme; confirm the second container renders and the PDF lands in Blob | PDF generated and stored; metadata persisted |
| Load | Concurrent requests against the UI | UI scales out per the concurrency rule |
| Failure | Inject a bad DB connection string into a throwaway API revision | 500 surfaced and diagnosed in App Insights Failures, then reverted |

### 7.2 Monitoring

All three apps emit logs to Log Analytics and telemetry to a shared Application Insights instance. The centrepiece evidence is the **end-to-end transaction view**: a single publish click correlates four hops (browser → UI → API → PDF, plus API → Blob) under one `OperationId`, visible both in the Application Insights timeline and at the data layer via a `union AppRequests, AppDependencies` query filtered on that `OperationId`. The Application Map shows three service nodes plus the blob dependency.

### 7.3 A real fault investigated and fixed

During verification the API returned **HTTP 500** when the Cases page was loaded against an empty/misconfigured database connection. I diagnosed it by streaming the live API container logs (`az containerapp logs show --follow`) while reproducing the request in the browser, which showed the failing data-access call rather than a generic gateway error. The remediation was to ensure the API resolves its connection string from the correct Key Vault secret via managed identity (not a stale local value) and to confirm the authenticated `GET /api/cases` then returned 200. This also informed the deliberate failure test in 7.1: injecting a deliberately bad connection string reproduces the same class of fault and confirms App Insights captures the exception with a stack trace — demonstrating that the observability stack catches problems, not just happy-path traffic.

### 7.4 Availability / SLA discussion

Azure Container Apps, MySQL Flexible Server, Blob Storage, and Key Vault each carry their own Microsoft SLA (typically 99.9% for single-instance/standard configurations). The *effective* availability of the system is the product of the services on the critical path, so the realistic figure is below any single component's headline number. For this assessment the UI runs `min 1` to avoid cold-start unavailability, and the platform reschedules failed replicas automatically; a production hardening step would raise minimum replica counts and consider zone redundancy on the data tier, trading cost for a higher composite SLA.

---

## 8. Cost and Carbon

### 8.1 Seven-day cost (assessed container deployment)

Costs are dominated by the always-warm UI replica and the MySQL compute hour; the internal services contribute little because they run small and scale down. Indicative monthly bands (validated against the Azure Pricing Calculator and the live seven-day Cost Analysis for `rg-hellobuddy-prod`):

| Component | Assumption | Indicative monthly |
| --- | --- | --- |
| MySQL Flexible Server | `B_Standard_B1ms` burstable | £9–£10 |
| MySQL storage | small dataset | < £1 |
| Blob Storage | low PDF volume, hot tier | < £1 |
| UI Container App | small, always warm | £4–£8 |
| API Container App | internal, low volume | £3–£6 |
| PDF Container App | one warm replica, bursty work | £1–£4 |
| Monitoring | light logs/metrics | small, variable |

This lands in a **lean band of roughly £18–£24/month**, with headroom to ~£30/month — inside the design budget. A development cost optimisation (DEC-002) further scheduled the MySQL server to office-hours-only during development, cutting data-tier compute hours by ~60%.

### 8.2 VM baseline comparison (seven days)

An equivalent always-on VM baseline would need to host the web app, the PDF renderer, and a database engine continuously. Even a small general-purpose VM sized to run all three components 24×7 (plus the OS, web server, and runtime that the managed platform otherwise provides) bills for every hour regardless of traffic. For a workload that is busy for perhaps 40 office hours a week and idle the rest, the VM pays for roughly 168 hours/week of compute to serve ~40 hours of demand. The container deployment, by contrast, keeps only the UI warm and lets the API/PDF and the scheduled database track actual usage. The result is that the bursty, office-hours profile of this workload makes the container model cheaper *and* reduces over-provisioning — the VM's only cost advantage (sustained high utilisation) does not exist here.

### 8.3 Carbon

Using the Cloud Carbon Footprint methodology (and Microsoft's Emissions Impact Dashboard guidance), the dominant variable is energy consumed, which tracks compute-hours and utilisation. The same property that makes the container model cheaper — not paying for idle compute — also lowers its energy draw versus an always-on VM serving the same intermittent demand. The honest limitation is that public estimators give *indicative* figures only: exact emissions depend on the data-centre's grid mix and PUE, which are not fully transparent at tenant level, so the carbon claim is directional (containers lower for this profile) rather than an exact kilogram figure.

---

## 9. Trends and Next Steps

- **Serverless containers / scale-to-zero.** ACA's consumption model is part of a broader move toward paying only for work performed; the natural next step is moving the PDF renderer to a **queue-backed worker** (Container Apps job or Functions) so bursty publish volume scales independently of the request thread, decoupling the UI from rendering latency.
- **GitOps.** The Terraform already makes infrastructure reproducible; wiring it into a CI/CD pipeline with `terraform plan` on pull requests and image promotion on merge would make deployments fully declarative and auditable.
- **Green computing.** Beyond scale-to-zero, region selection by carbon intensity and right-sizing on measured utilisation are the highest-leverage sustainability levers.
- **Security hardening.** Replace the `X-Practitioner-Id` header with Entra-issued workload-identity JWTs, encrypt Data Protection keys with a Key Vault key, and disable storage account-key access entirely (all recorded as technical debt).
- **Portability.** Because ACA is built on OSS building blocks (KEDA, Dapr, Envoy), the same images and scaling concepts migrate to AKS or another Kubernetes platform if scale ever justifies it — the application is not locked to the platform.

---

## 10. Ethics and Responsible Data Handling

All data used throughout development, testing, screenshots, the demo, and this report is **synthetic**. The domain is health-adjacent (animal rehabilitation) rather than personal health data, but owner and pet records are nonetheless minimised and access-controlled to model responsible practice. Residual risks are acknowledged: credential leakage (mitigated by managed identities and Key Vault, with no secrets in source control), insecure object storage (mitigated by private containers, disabled public access, and short-lived SAS), and excessive logging (mitigated by configured Log Analytics retention and by not logging secret values). No live personal or clinical data is present in any artefact.

---

## 11. Conclusion

*Hello Buddy Cloud Admin* demonstrates a credible, containerised, cloud-deployed vertical slice that meets the assessment's functional, security, observability, and cost requirements. The three-container Azure Container Apps design gives clean separation of concerns, a layered security posture (L7 HTTPS ingress, internal-only API/PDF, private-access MySQL, and an explicit L4 NSG), genuine least-privilege identity, reproducible Terraform infrastructure, distributed tracing across services, demonstrable autoscaling, and a cost footprint inside budget. The platform choice is defended against both an always-on VM and AKS with evidence-backed trade-offs, a real fault was investigated and fixed using the observability stack, and the next-step roadmap (queue-backed rendering, GitOps, hardened auth) shows critical awareness of where the design would evolve for production.

---

## Appendix A — Resource Inventory

A full as-built resource inventory (every Azure resource with purpose, exposure, and justification) is provided in `Azure Resource Inventory.md`.

## Appendix B — Decision Log

Significant cost, architecture, and operational decisions are recorded as DEC-001 … DEC-012 in `DecisionLog.md`, and standards deviations in `Technical Debt/TD-001 Admin Standards Deviations.md`.

## Appendix C — Evidence Index

Twenty-eight evidence screenshots (configuration and behaviour-under-conditions) are catalogued in `Submission Checklist.md`, including the end-to-end transaction trace, internal-boundary proof, expired-SAS and anonymous-access rejections, least-privilege RBAC listing, private-MySQL networking, and the MySQL-tier NSG.

## References

*To be completed in Harvard style: a mix of academic, vendor (Microsoft Learn / Azure documentation), and professional sources covering Azure Container Apps, MySQL Flexible Server, managed identity, the Cloud Carbon Footprint methodology, and Azure SLAs.*
