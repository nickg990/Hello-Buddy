# Hello Buddy Cloud Admin — MSc Assessment Decision Log

Decisions recorded here are those with cost, architecture, or operational significance to the COM712 Assessment 3 submission. Each entry captures the choice made, the rationale, and the evidence supporting it.

---

## DEC-001 — MySQL Flexible Server tier and compute size

**Date:** 27 May 2026
**Area:** Data tier cost management
**Decision:** Deploy Azure Database for MySQL Flexible Server using the Burstable tier, B1ms SKU (1 vCore, 2 GiB RAM), 20 GB storage, 1-day backup retention, geo-redundant backup disabled.

**Rationale:**
The assessment workload is a single-practitioner demo with low and intermittent query volume. The Burstable tier is explicitly designed for workloads with flexible compute requirements that do not need full CPU continuously. B1ms is the smallest available Flexible Server SKU and is sufficient for the first-increment vertical slice. General Purpose and Memory Optimised tiers would add significant cost with no benefit at this scale.

**Cost basis (UK South, pay-as-you-go, May 2026):**

| Component                             | Rate                                   | Monthly cost      |
| ------------------------------------- | -------------------------------------- | ----------------- |
| Compute — B1ms                        | $12.41/month (full 720 hrs)            | $12.41            |
| Storage — 20 GB                       | $0.115/GB/month                        | $2.30             |
| Backup storage (1-day retention, LRS) | Free up to 100% of provisioned storage | $0.00             |
| IOPS                                  | $0.20/million (pay-per-use)            | ~$0.00            |
| **Unscheduled total**                 |                                        | **~$14.71/month** |

**Alternatives considered:**

- B2ms (2 vCore): $99.28/month compute — not justified for a two-practitioner demo workload.
- General Purpose D2ds: $124–163/month — appropriate for production multi-tenant scale, not for assessment MVP.

---

## DEC-002 — Azure Automation scheduled start/stop for MySQL

**Date:** 27 May 2026
**Area:** Data tier cost management
**Decision:** Deploy an Azure Automation Account (Basic SKU) with scheduled runbooks to start and stop the MySQL Flexible Server on a business-hours-only pattern. The Automation Account is provisioned as part of the data-tier Terraform module.

**Schedule:**

| Day              | Start                  | Stop  |
| ---------------- | ---------------------- | ----- |
| Monday           | 06:00                  | 19:00 |
| Tuesday–Thursday | 07:00                  | 19:00 |
| Friday           | 07:00                  | 18:00 |
| Saturday–Sunday  | — (server remains off) | —     |

Timezone: `Europe/London` (handles BST/GMT transition automatically).

**Rationale:**
MySQL Flexible Server charges for compute by the hour regardless of traffic — there is no scale-to-zero for the database engine. Stopping the server outside working hours and over the weekend reduces active compute hours from 720/month to approximately 65/week (≈ 282/month), a saving of roughly 61%.

**Implementation:**
Runbooks use `Connect-AzAccount -Identity` (managed identity) and `Invoke-AzRestMethod` to call the MySQL start/stop REST API. No additional Az module installation is required as `Az.Accounts` is pre-installed in all Automation Accounts. The Automation Account's managed identity is granted `Contributor` scoped to the MySQL server resource only (least-privilege).

**Cost basis:**

| Component                                    | Unscheduled       | Scheduled (~65 hrs/week)         |
| -------------------------------------------- | ----------------- | -------------------------------- |
| Compute                                      | $12.41/month      | ~$5.50/month                     |
| Storage                                      | $2.30/month       | $2.30/month (unchanged)          |
| Automation jobs (~43 runs/month, each <60 s) | —                 | $0.00 (within 500 min free tier) |
| **Total**                                    | **~$14.71/month** | **~$7.80/month**                 |

**Saving:** approximately $6.90/month (~47%) on the data tier during the assessment window.

**Note for report:** MySQL stop/start is appropriate during development and for the assessment period. In a production deployment serving active practitioners, the server would remain running during business hours by default and this schedule would be replaced by monitoring-driven scaling decisions.

---

## DEC-003 — MySQL Flexible Server deployed to UK West (data tier only)

**Date:** 27 May 2026
**Area:** Region selection
**Decision:** The MySQL Flexible Server is provisioned in **UK West**. All other resources (Resource Group, Key Vault, Automation Account) remain in **UK South**.

**Rationale:**
The first `terraform apply` failed with `ProvisionNotSupportedForRegion` for MySQL Flexible Server B1ms in UK South. Capability probing via `az rest` against `/subscriptions/.../providers/Microsoft.DBforMySQL/locations/{region}/capabilities` confirmed that, for this subscription on this date, the capabilities endpoint itself returned `InternalServerError` for `northeurope`, `uksouth`, and `eastus`, while returning `OK` for `westeurope` and `ukwest`. UK West is the closest available region and was selected to keep data-residency in the UK.

**Implementation:**
The Terraform module exposes a `mysql_location` variable that defaults to the same region as the rest of the data tier but is overridden in `terraform.tfvars` to `ukwest`. The MySQL server has no `zone` pin so Azure is free to place it in any available zone within UK West.

**Cost / latency impact:**
Inter-region latency UK West ↔ UK South is typically <10 ms — negligible for this workload. There is no inter-region data egress charge for Azure-internal traffic between paired UK regions in the same subscription. No cost change versus the original UK South plan.

**Note for report:** This is a real-world capacity issue that demonstrates the value of declarative IaC — region migration was a single variable change plus a fresh apply, not a manual portal recreation. Future production deployments should adopt either UK West permanently or implement a region-fallback pattern at the Terraform variable level.

---

## DEC-004 — `AllowAzureServices` firewall rule on MySQL (assessment scope only)

**Date:** 27 May 2026
**Area:** Network security
**Decision:** Add a firewall rule named `AllowAzureServices` (start/end IP `0.0.0.0`) to the MySQL Flexible Server to permit connections from Azure Cloud Shell and other Azure-internal services during the assessment phase.

**Rationale:**
The DevBox used for development sits behind a corporate egress firewall that blocks outbound TCP/3306. Azure Cloud Shell is the practical workaround for running the database initialisation and seed scripts. The `AllowAzureServices` rule (start IP `0.0.0.0`) is Azure's documented mechanism for enabling Cloud Shell and other PaaS-internal traffic to reach a MySQL Flexible Server with public networking.

**Security implications:**
This rule permits TCP/3306 from any Azure tenant, not just our own. Authentication is still required (strong admin password, SSL/TLS enforced). For the assessment window this is an acceptable trade-off, but it is **not appropriate for production**.

**Lockdown plan (Friday US-29):**
On Friday the firewall posture is tightened:

1. ~~The `developer-ip-temp` and `devbox-ip-temp` rules are removed.~~ **Done 2026-05-28 (Day 4b)** — `az mysql flexible-server firewall-rule list` now returns `AllowAzureServices` only.
2. The `AllowAzureServices` rule is removed once the Container Apps subnet has direct line-of-sight to MySQL via VNet integration (or, if VNet integration is descoped, a single rule scoped to the Container Apps egress IP range).
3. Final state: MySQL is reachable only from the Hello Buddy Container Apps environment.

**Note for report:** This decision is documented as a deliberate, time-bounded compromise for the assessment seed workflow. The lockdown step on Friday is the evidence that the production posture differs from the development posture.

---

## DEC-005 — Local MySQL on DevBox for application development loop

**Date:** 27 May 2026
**Area:** Developer environment
**Decision:** Install MySQL Server 8.0 natively on the DevBox (via `winget install Oracle.MySQL`) and seed it from the same three SQL scripts that initialise Azure MySQL. The .NET web application uses this local instance for the F5 debug loop on Day 2–3. Azure MySQL remains the deployment target from Thursday onwards.

**Rationale:**
The DevBox blocks outbound TCP/3306 to `*.mysql.database.azure.com` (corporate egress firewall — see DEC-004 context). Without local MySQL, the F5 debug loop cannot run because the application cannot reach a database. The Windows laptop alternative is unavailable because the .NET runtime is disabled by corporate policy. Codespaces was considered but adds an extra hosted environment and would consume free quota for a problem that a native install solves at zero cost.

A native install (not a Docker container) was chosen at the user's request to keep the dev environment as conventional as possible and avoid coupling the inner dev loop to a container runtime.

**Configuration split:**

| File                           | Connection target                                                           | Active when                          |
| ------------------------------ | --------------------------------------------------------------------------- | ------------------------------------ |
| `appsettings.Development.json` | `Server=localhost;Database=canine_physiotherapy;Uid=root;Pwd=<dev-only>;`   | `ASPNETCORE_ENVIRONMENT=Development` |
| `appsettings.json`             | Pulled from Key Vault secret `mysql-connection-string` via Managed Identity | `ASPNETCORE_ENVIRONMENT=Production`  |

Application code does not branch on environment — only the configuration source changes.

**Schema parity:**
Both the local and Azure databases are built from the same three scripts (`v2.3 (fresh).sql`, `Day 1 Initialise v2.4.sql`, `MSc Assessment Seed v1.sql`). Any schema change goes into a versioned script and is applied to both environments in the same order. This is the lightweight migration pattern for the assessment; a production deployment would adopt EF Core migrations or Flyway.

**Convergence point:**
Thursday (US-15–US-19) is the first time the app talks to the real Azure MySQL — via the container deployment, where Container Apps' outbound networking does have line-of-sight to MySQL. Risks deferred to Thursday:

- SSL/TLS certificate-trust chain (Azure MySQL requires SSL; the connection string in Key Vault includes `SslMode=Required`)
- Azure-MySQL-specific behaviour quirks (case sensitivity, `lower_case_table_names`, etc.)

**Mitigation:** Wednesday afternoon spot-check via Cloud Shell — run a handful of representative queries from Cloud Shell against Azure MySQL to confirm behaviour matches local before the Thursday container build.

**Note for report:** This is a development-environment decision driven by infrastructure constraint, not an architectural choice about the runtime data tier. The production target is unambiguously Azure MySQL Flexible Server (DEC-001). The local MySQL instance has no deployment role and no Day-1 cost.

---

## DEC-006 — Build container image with `az acr build` instead of local Docker

**Date:** 28 May 2026
**Area:** Build pipeline / DevBox network constraint
**Decision:** The admin-app container image is built and pushed to Azure Container Registry using `az acr build` (server-side build inside ACR) rather than `docker build` + `docker push` from the DevBox.

**Trigger / evidence:**

- During Day 3 deployment the local `docker build` failed at `apt-get update` with HTTP 470 from the Fastly CDN edge in front of `deb.debian.org`. Swapping to `cloudfront.debian.net` (the official alternate mirror) returned the same 470. Both responses came from Fastly's edge before reaching the mirror, indicating a DevBox-side egress block on the Debian package mirrors rather than a transient upstream outage.
- `az acr build` runs the build inside the ACR build host (which has unrestricted egress) and pushed the resulting image to `acrhellobuddyprod.azurecr.io/hello-buddy-admin:v1`. Build succeeded; Container App picked up the image and the deployed admin app passed all six DoD checks.

**Rationale:**

- The brief requires the image to be deployed today; waiting on a network-egress exception would slow delivery.
- `az acr build` is a first-party Azure path with no licensing implications and uses the same Dockerfile, so the artifact is bit-identical to what a local build would have produced.
- The deployment loop (rebuild + redeploy) is one command longer but still entirely scriptable: `az acr build ... -t :vN` then `terraform apply -var admin_app_image=...:vN`.

**Trade-offs:**

- Local-loop debugging of the image itself (`docker run -it`) is not possible from the DevBox. Mitigated by always-available Container App console logs (`az containerapp logs show`) and Application Insights traces.
- One known cosmetic issue: `az acr build` streams logs to the local CLI, which crashes on the Unicode right-arrow character that `apt` emits (`UnicodeEncodeError` on Windows `cp1252`). The ACR-side build continues unaffected. Workaround: `--no-logs` flag plus `az acr task list-runs` to confirm status.

**Cost impact:** Negligible — ACR Basic includes build minutes; one run is ~90 seconds.

---

## DEC-007 — Single-container deployment for admin app (defer split)

**Date:** 28 May 2026
**Area:** Application architecture
**Status:** **Superseded 2026-05-28 by DEC-009.** Retained as historical record; the single-container shape was the deployed Day 3 reality but was replaced the same day by the three-container topology when time allowed and the assessment marks differential was confirmed.

**Decision:** The admin web app and the Puppeteer-based PDF renderer are deployed as a single container image and a single Container App revision (`ca-hello-buddy-admin`), rather than being split into separate web and PDF services as initially sketched in the Five-Day Delivery Plan.

**Rationale:**

- The brief explicitly states that one container is acceptable for the assessment scope ("if it works end-to-end as one container, ship it").
- A single image keeps the deploy loop short (one `az acr build` + one Terraform apply) and avoids the need for an internal HTTP boundary, service discovery, or inter-service auth between web and PDF.
- PuppeteerSharp + system Chromium runs in-process inside the ASP.NET Core host; PDF generation latency at the deployed revision was ~5.6 seconds end-to-end for a 10-exercise programme (App Insights `POST Programmes/Publish [id]`, DoD walkthrough), which is acceptable for a synchronous admin action.
- The single image is small enough (~250 MB published with Chromium, fonts, libnss/libatk dependencies, .NET 9 ASP.NET runtime) that cold-start is not a concern at one replica.

**Trade-offs / debt:**

- Vertical scaling of the PDF renderer is coupled to the web tier — if many concurrent publishes were expected, the PDF work should be moved to a queue-backed worker (Container App job or Azure Functions). Not in scope for the assessment.
- Recorded as TD-002 (architecture-level technical debt) so the split path is captured for a real production rollout.

---

## DEC-008 — Storage account key used for DevBox blob inspection (not for the app)

**Date:** 28 May 2026
**Area:** Identity / operational verification
**Decision:** The admin app authenticates to the published-programmes blob container exclusively via its user-assigned managed identity (`uami-hellobuddy-admin`, scoped role `Storage Blob Data Contributor` on the container). For the Day 3 verification step (DoD #5 — list + download the published PDF from the DevBox), an account-key call (`az storage blob list/download --account-key`) was used because the signed-in developer identity does not have a Storage Blob Data RBAC grant.

**Rationale:**

- The runtime path (application → blob) is the one that matters for the production posture, and it is fully managed-identity-based.
- Granting the developer `Storage Blob Data Reader` would be the cleaner verification path, but adds an extra RBAC grant whose only purpose is hands-on inspection.
- Account-key access remains gated by Entra ID (the developer must already be authorised on the storage account control plane to issue `account keys list`).

**Trade-off / follow-up:** A reviewer should expect to see `account_keys list` events in storage activity logs for the demo window. Long-term, the account-key path should be disabled (`allowSharedKeyAccess = false` on the storage account) once verification tooling moves to managed-identity-only — captured as TD-003.

---

## DEC-009 — Three-container topology (UI / API / PDF) supersedes DEC-007

**Date:** 28 May 2026
**Area:** Application architecture
**Decision:** The admin solution is split into three Container Apps in the same Container Apps Environment:

- `ca-hello-buddy-ui` — external HTTPS ingress, Razor views + thin MVC controllers, no data-plane reach
- `ca-hello-buddy-api` — internal ingress only, owns `CaninePhysioDbContext`, `IFileStore`, the publish flow, and the SAS-mint path; calls the PDF service
- `ca-hello-buddy-pdf` — internal ingress only, wraps PuppeteerSharp + Chromium behind `POST /render`, fixed 1 replica

Each app has its own user-assigned managed identity (`uami-hellobuddy-ui|api|pdf`) with least-privilege RBAC: UI has AcrPull plus DataProtection-keys-container Blob Contributor only; API has AcrPull, KV Secrets User, published-programmes container Blob Contributor, account-scope Blob Delegator; PDF has AcrPull only.

**Rationale:**

- [Option C Brief](Option%20C%20Architectural%20Brief%20and%20Requirements.md) §"Target Architecture" mandates _at minimum_ two application containers (`hello-buddy-admin-web` + `hello-buddy-pdf-service`), and the rubric explicitly rewards _"at least two containers with clear responsibility boundaries"_. DEC-007's single-container ship was below that floor.
- [Azure Architecture Diagram Specification](Azure%20Architecture%20Diagram%20Specification.md) describes the three-container topology (UI / API / PDF) as the assessed shape, with boundary rules explicitly stating _"API and PDF services are internal only"_.
- The three-container split unlocks distinction-level discussion of service discovery, internal ingress, per-service scaling rules, and managed-identity RBAC scoping that a single container cannot evidence.
- `IPdfRenderer` was already an abstraction in `HelloBuddy.Admin.Pdf`, so the cost of the split was contained: a new `HttpPdfRenderer` swaps in for `PuppeteerPdfRenderer` behind the same interface, and the existing DTOs were lifted into a new shared `HelloBuddy.Contracts` project.

**Trade-offs / debt:**

- Synchronous request path: UI → API → PDF → Blob. PDF generation is still on the publish request thread (~5.6 s end-to-end). The residual queue-backed-worker portion of the original TD-002 stays open for production scale-out.
- Inter-service authentication is header-based (`X-Practitioner-Id`) rather than JWT — see DEC-010 and the TD-005 follow-up.
- Three managed identities, three role assignments, three Container Apps to operate — modestly higher operational surface than DEC-007's single app, accepted for the assessment because the IaC fully encapsulates it.

**Evidence (Day 4b):** App Insights end-to-end transaction view on a publish click shows four hops under one OperationId (browser → ui → api → pdf, plus api → blob). Each Container App emits AppRequests independently and shares the App Insights instance via Key Vault reference. `az containerapp ingress show` confirms `external = true` on ui, `external = false` on api and pdf.

---

## DEC-010 — `X-Practitioner-Id` header as Release 1 service-to-service identity

**Date:** 28 May 2026
**Area:** Identity / inter-service auth
**Decision:** UI propagates a single header `X-Practitioner-Id` on every outbound call to the API (via a `PractitionerHeaderHandler` `DelegatingHandler`). The API rejects any request missing that header with HTTP 401. The value is read from configuration (`SeededPractitionerId`) on the UI side and used directly as the practitioner identity context inside the API. Network-level trust is provided by Container Apps' internal-ingress boundary — the API FQDN resolves only within the same Environment, so no caller outside that perimeter can reach it.

**Alternative considered:** Issue a workload-identity JWT from the UI's UAMI and validate it inside the API with `AddJwtBearer` + token audience checks. Rejected for Release 1 because (a) it requires Entra App Registrations and audience-scope configuration that adds two days of work, (b) the assessment has a single seeded practitioner identity so the value of cryptographic auth is symbolic rather than operational, and (c) the network boundary plus the absence of any public path to the API meet the actual threat model.

**Trade-off / follow-up:** A leaked or spoofed `X-Practitioner-Id` from inside the Environment would let any compromised app act as any practitioner. Captured as TD-005 (Entra-issued JWT inter-service auth) for the production redesign.

---

## DEC-011 — SAS-based download for published PDFs (not anonymous public read)

**Date:** 28 May 2026
**Area:** Identity / blob access
**Decision:** The published-programmes container stays private (`allowBlobPublicAccess = false` at the storage account, container public-access level `none`). The admin downloads PDFs via a UI link that the UI controller redirects to a user-delegation SAS URL minted by the API. SAS TTL = 30 minutes. The API's UAMI is granted **Storage Blob Delegator** at the storage account scope (no data-plane permissions attached) so it can call `GetUserDelegationKey` without holding an account key.

**Alternative considered:** Set the container's public-access level to `blob`, exposing PDF URLs anonymously. Rejected because (a) it would require relaxing `allowBlobPublicAccess` at the account level, weakening posture for every future container, and (b) every published PDF would be permanently world-readable to anyone who guessed the deterministic filename pattern (`programme-{id}-{yyyyMMdd-HHmmss}.pdf`) — guessable within a small search space.

**Trade-off:** The admin cannot share the URL itself outside the 30-minute window — they download the PDF and attach it to WhatsApp (or similar) instead. This matches the actual workflow described in the user journey. The download path also synthesises a SAS per request, which is acceptable at assessment scale but is captured as TD-007 for a production redesign behind Azure Front Door + signed URLs.

**Evidence (Day 4b):** A real download link issued during verification carried `se=2026-05-28T14:16:26Z` against an `st=2026-05-28T13:41:26Z` start (≈ 30 min total window); hand-editing `se=` to a past time returns HTTP 403 from the blob endpoint; anonymous GET on the same URL without the SAS returns HTTP 409 `PublicAccessNotPermitted`.

---

## DEC-012 — DataProtection keys persisted to Blob Storage (UI container)

**Date:** 28 May 2026
**Area:** Identity / cookie + antiforgery durability
**Decision:** The UI Container App persists ASP.NET Core DataProtection keys to a dedicated private blob container `dataprotection-keys` on `sthellobuddyprod`, writing to a single blob `ui-keys.xml`. The UI's UAMI (`uami-hellobuddy-ui`) is granted `Storage Blob Data Contributor` scoped _only_ to that container — it has no reach into `published-programmes` or any other container on the account. `AddDataProtection().PersistKeysToAzureBlobStorage(...).SetApplicationName("HelloBuddy.Ui")` is gated on the `DataProtection:BlobUri` config value so local dev still uses the default ephemeral on-disk key store.

**Rationale:**

- Default ASP.NET Core key storage is the container filesystem (`/root/.aspnet/DataProtection-Keys`), which is ephemeral. With multi-replica scale-out (UI scale rule `min 1 / max 3`) every replica generates its own key ring, so antiforgery tokens and auth cookies issued by replica A are rejected by replica B.
- Any revision swap (push a new image and `containerapp update --image …`) destroys the key ring, immediately invalidating every active browser session. That is a meaningful demo-day risk: a routine `:v3` push would break sessions mid-filming.
- Blob storage is durable, encrypted at rest, and reachable via UAMI without any account-key or connection-string handling.
- Scoping the role assignment to the single container (not the storage account) keeps the UI identity's data-plane reach minimal — it cannot see, list, or read `published-programmes`.

**Trade-off / follow-up:** Keys are not encrypted with Key Vault. `ProtectKeysWithAzureKeyVault(...)` is the next step for a production posture but requires a KV key (not a secret) and a second role grant on KV. Acceptable for assessment scale where the blob container itself is private and account-scoped Entra ID is the access boundary.

**Evidence:** Pushing `hello-buddy-ui:v2` after the change and swapping the revision: the existing browser session continued to work without antiforgery rejection (was previously the failure mode in pre-DEC-012 dev tests). The `dataprotection-keys` container shows `ui-keys.xml` after the first key-rotation interval. Closes TD-001 row C9 (DataProtection follow-up).

---
