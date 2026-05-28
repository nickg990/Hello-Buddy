# Day 4a — Three-container split (UI / API / PDF)

**Controller chat handoff. Open a fresh chat for this. Time budget: 5–6 hours. Includes a fallback gate at the halfway mark.**

## Why we're doing this

Day 3 shipped one Container App, which is below the assessment floor per the [Option C Brief](Option%20C%20Architectural%20Brief%20and%20Requirements.md) and well below the distinction-level architecture in [Azure Architecture Diagram Specification.md](Azure%20Architecture%20Diagram%20Specification.md). Going to three containers (UI / API / PDF) matches the diagram spec exactly, gives the report a real "service discovery, internal ingress, scaling per service" story, and unlocks the rubric's "future-facing managed-platform trade-offs" discussion honestly. The UI/API split was originally deferred per the brief's Release 1 advice — overriding that call today because we have the time and the marks differential is material.

Supersedes **DEC-007** (single container) and the prior call to defer UI/API split to Increment 2.

## Target topology

```
Browser ─HTTPS─> ca-hello-buddy-ui (external)
                       │ HTTPS, internal FQDN
                       ▼
                 ca-hello-buddy-api (internal)
                       ├──> MySQL Flexible Server (private)
                       ├──> Blob Storage (managed identity)
                       └──> ca-hello-buddy-pdf (internal) ─> returns PDF bytes
                              ▲
All three ───> App Insights / Log Analytics (same instance, shared OperationId)
ACR ──> all three (managed-identity pull)
KV ──> api (and pdf if needed)
```

## What each container does

| Container                | Owns                                                                                                                                                                   | Talks to                            | Ingress            |
| ------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | ------------------ |
| **`ca-hello-buddy-ui`**  | Razor views, MVC controllers that are now thin (parse request → call API → render view), antiforgery, cookie auth, static assets, layout                               | API (HTTPS, typed `HttpClient`)     | **External HTTPS** |
| **`ca-hello-buddy-api`** | EF Core `CaninePhysioDbContext`, all MySQL reads/writes, `IFileStore` for Blob writes, business rules, authorisation policies. Exposes JSON endpoints the UI consumes. | MySQL, Blob, PDF service, Key Vault | **Internal only**  |
| **`ca-hello-buddy-pdf`** | PuppeteerSharp + Chromium. One endpoint `POST /render` taking HTML, returning bytes.                                                                                   | (nothing outbound except telemetry) | **Internal only**  |

The DTOs you already have (`ProgrammeVm`, `CaseRow`, `CaseDetailVm` etc.) become the API contract. No new model classes needed — they just move into a shared `HelloBuddy.Contracts` project that both UI and API reference.

## Project layout after split

```
src/
  HelloBuddy.Contracts/         # NEW — shared DTOs (ProgrammeVm, CaseRow, CaseDetailVm, ProgrammeBuilderForm, RenderRequest, RenderResponse)
  HelloBuddy.Admin.Core/        # unchanged — DbContext + generated entities + identity, now only referenced by Api
  HelloBuddy.Admin.Pdf/         # unchanged at the abstraction level — IPdfRenderer + PuppeteerPdfRenderer + new HttpPdfRenderer
  HelloBuddy.Ui/                # NEW — Razor views + thin controllers using typed HttpClient against Api
  HelloBuddy.Api/               # NEW — minimal API, EF Core, IFileStore, calls Pdf service
  HelloBuddy.PdfService/        # NEW — minimal API wrapping IPdfRenderer
```

Old `HelloBuddy.Admin` web project becomes `HelloBuddy.Ui`. The Razor views move across unchanged. Controllers stay but are gutted: they no longer inject `CaninePhysioDbContext`, they inject a typed `IAdminApiClient` (HttpClient wrapper) and call it.

## API contract (v1 — minimum to make the existing UI work)

| Method | Path                                | Returns                  | Replaces                                                         |
| ------ | ----------------------------------- | ------------------------ | ---------------------------------------------------------------- |
| GET    | `/api/cases`                        | `IReadOnlyList<CaseRow>` | `CasesController.Index` DbContext query                          |
| GET    | `/api/cases/{id:long}`              | `CaseDetailVm`           | `CaseDetailController.Index` query                               |
| GET    | `/api/programmes/{id:long}`         | `ProgrammeVm`            | `ProgrammesController.Builder` GET query                         |
| PUT    | `/api/programmes/{id:long}`         | `ProgrammeVm` (updated)  | `ProgrammesController.Builder` POST                              |
| POST   | `/api/programmes/{id:long}/publish` | `{ "blobUri": "..." }`   | `ProgrammesController.Publish` (also writes blob inside the API) |

All endpoints take an `X-Practitioner-Id` header. The UI reads it from `IConfiguration` (same seeded id you already use) and adds it to every outbound API call via a `DelegatingHandler`. The API enforces it (rejects missing header with 401). For Release 1 with one seeded practitioner this is sufficient — proper Entra-issued JWTs are TD-005.

## PDF service contract

| Method | Path       | Body                                   | Returns                 |
| ------ | ---------- | -------------------------------------- | ----------------------- |
| POST   | `/render`  | `{ "html": "..." }` (application/json) | `application/pdf` bytes |
| GET    | `/healthz` | —                                      | `200 OK`                |

API calls PDF service via typed `HttpClient` bound to `PdfService:Uri` env var.

## Sequence (target ~5–6 hours)

### Phase 1 — Build the artefacts locally (90 min)

1. Create `HelloBuddy.Contracts` project. Move DTOs from `HelloBuddy.Admin/Models/` into it. Both UI and API will reference this.
2. Create `HelloBuddy.PdfService` (minimal API). One endpoint `POST /render`, reuses `PuppeteerPdfRenderer`. Add `/healthz`. Reuse the Day 3 admin Dockerfile pattern verbatim — same base image, same Chromium deps.
3. Create `HelloBuddy.Api` (minimal API). Move `CaninePhysioDbContext` registration here, `IFileStore` here, the publish flow here. Wire up the five endpoints above. Add `[Authorize]` policy that checks `X-Practitioner-Id` header. Inject typed `HttpClient` for PDF service. Add `/healthz`.
4. Rename `HelloBuddy.Admin` → `HelloBuddy.Ui`. Strip out DbContext, IFileStore, PuppeteerPdfRenderer registrations. Add typed `IAdminApiClient` with methods matching the five endpoints. Add `DelegatingHandler` that injects `X-Practitioner-Id`. Update all four controllers to call `IAdminApiClient` instead of `_db`. Views and Models (now Contracts) unchanged.
5. Local smoke test: `dotnet run` all three projects, hit UI on localhost:5000, verify cases list → case detail → builder → publish works end-to-end. **Gate 1.**

### Phase 1.5 — Decision gate (5 min)

If Phase 1 is not complete and green by your halfway-time mark, **stop and execute the fallback** described at the bottom of this brief. Do not push partial work to Azure.

### Phase 2 — Containerise (45 min)

6. Three Dockerfiles. UI and API use the standard aspnet base. PDF uses the Chromium-capable base from Day 3. All built with `az acr build` (DEC-006).
   - `acrhellobuddyprod.azurecr.io/hello-buddy-ui:v1`
   - `acrhellobuddyprod.azurecr.io/hello-buddy-api:v1`
   - `acrhellobuddyprod.azurecr.io/hello-buddy-pdf:v1`

### Phase 3 — Terraform (90 min)

7. In `Infrastructure/terraform/container-platform/`:
   - Existing `ca-hello-buddy-admin` resource — **rename in state** to `ca-hello-buddy-ui` (use `terraform state mv`, then update config). External ingress stays. Strip MySQL, Blob, PDF env vars — UI only needs `Api:Uri`, `SeededPractitionerId`, App Insights connection string.
   - **New** `azurerm_container_app.api` — internal ingress only, target port 8080. System-assigned managed identity. Role grants: KV `get`/`list` secret, Storage Blob Data Contributor on `sthellobuddyprod`/`published-programmes`, AcrPull. Env vars: `ConnectionStrings__CaninePhysioDb` (KV ref), `Storage__BlobServiceUri`, `PdfService__Uri`, App Insights. Scaling: min 1, max 3 on CPU 70% and HTTP concurrency 50.
   - **New** `azurerm_container_app.pdf` — internal ingress only, target port 8080. System-assigned managed identity (AcrPull only). Scaling: min 1, max 1 (justify in report: low-volume MVP, queue-based scaling is Increment 2 story). Liveness probe on `/healthz`.
   - On `ca-hello-buddy-ui`: add env var `Api__Uri=https://ca-hello-buddy-api.internal.<env-default-domain>`.
   - On `ca-hello-buddy-api`: add env var `PdfService__Uri=https://ca-hello-buddy-pdf.internal.<env-default-domain>`.
8. `terraform plan` carefully. Expect: 1 rename (or 1 destroy + 1 create if state mv too risky — accept the few minutes of downtime), 2 creates, several role assignments.
9. `terraform apply`.

### Phase 4 — Verify against the deployed environment (45 min)

10. Re-run DoD steps 1–6 from Day 3 against the new live UI URL.
11. **New evidence to capture for the report:**
    - App Insights end-to-end transaction on a publish click — should show four hops (UI → API → PDF, UI → API, API → Blob) under one OperationId
    - Each container has its own AppRequests stream
    - Container Apps "Replicas" view showing each app independently
    - `az containerapp ingress show` for each app proving external/internal split
12. Run a brief load test against UI (even just `for ($i=0; $i -lt 20; $i++) { Invoke-WebRequest ... }`) to demonstrate independent scaling behaviour — UI replicas scale, PDF stays fixed.

### Phase 5 — Documentation (30 min)

13. Update [Azure Production Architecture Diagram.md](Azure%20Production%20Architecture%20Diagram.md) — three containers, internal hops, public boundary on UI only. Match the labels in [Azure Architecture Diagram Specification.md](Azure%20Architecture%20Diagram%20Specification.md) verbatim so the marker can map diagram → spec → reality.
14. Update [Azure Resource Inventory.md](Azure%20Resource%20Inventory.md) — three Container Apps instead of one.
15. DecisionLog entries (in `Canine Physio App/DecisionLog.md`):
    - **DEC-009** — Supersede DEC-007. Split admin into three Container Apps (UI / API / PDF) per the Option C Brief target architecture and Azure Architecture Diagram Specification. Reasoning: assessment rubric weights multi-container architecture, service discovery, and independent scaling heavily; cost of split was bounded; UI/API split also clears TD-002 and the future TD-005.
    - **DEC-010** — `X-Practitioner-Id` header as service-to-service identity for Release 1; Entra JWT is the production target (logged as TD-005).
16. Update [../Technical Debt/TD-001 Admin Standards Deviations.md](../Technical%20Debt/TD-001%20Admin%20Standards%20Deviations.md):
    - **Close** TD-002 (PDF split — done)
    - **Close** C6 (liveness/readiness — done via `/healthz` on api + pdf)
    - **Close** D-09 partially (DI extension methods now naturally land per-project)
    - **Add** TD-005 — Entra-issued JWT for inter-service auth (currently `X-Practitioner-Id` header)
    - **Add** TD-006 — UI cookie / antiforgery DataProtection keys still ephemeral; move to Blob (was C9; promote to TD because multi-container makes it bite harder if any container restarts mid-session)

## Definition of done

1. Three Container Apps in the resource group: `ca-hello-buddy-ui` (external), `ca-hello-buddy-api` (internal), `ca-hello-buddy-pdf` (internal). All Healthy, all with replicas.
2. Three images in ACR tagged `:v1`.
3. Publish flow works end-to-end against the deployed UI URL.
4. App Insights end-to-end transaction view shows shared OperationId across all three apps.
5. Architecture diagram updated and matches reality.
6. DEC-009 and DEC-010 logged; TD-002, C6 closed; TD-005, TD-006 added.

## Fallback if Phase 1 isn't green by your halfway mark

Do not push partial work. Execute this instead, costing ~2 hours:

1. Abandon the UI/API split. Keep `HelloBuddy.Admin` as a single web app.
2. Just split out `HelloBuddy.PdfService` (Phase 1 step 2 only).
3. Deploy two containers: `ca-hello-buddy-admin` (existing, plus `HttpPdfRenderer` swap) + `ca-hello-buddy-pdf` (new).
4. Log DEC-009 as "two-container split adopted; three-container topology deferred to Increment 2" instead.
5. You still satisfy the assessment floor; you lose the distinction-level UI/API discussion. Acceptable Plan B.

## Out of scope today

- DataProtection-to-Blob (Friday morning, but now promoted to TD-006 because multi-container makes it riskier — do it Friday before filming, no excuses)
- EF split-query warning fix
- WAF/Front Door (D-27)
- Entra B2C / real auth (TD-005)

## Report back to controller chat with

1. Which DoD items passed (or which fallback was taken)
2. New DEC entries and TD-001 items
3. Final container app names and URLs
4. Any surprises affecting Friday's filming
5. A note on the end-to-end App Insights screenshot — that's the centrepiece evidence for the report

## Heads-up on Friday risk

If today's split eats more than 6 hours, Friday morning gets crowded (DataProtection-to-Blob + diagram polish + final smoke + filming). The fallback gate above exists for exactly that scenario — please trust it and take Plan B if Phase 1 isn't clean by halfway. A working two-container demo beats a broken three-container one every time.

## Context the new chat will need (read these first)

- [Option C Architectural Brief and Requirements.md](Option%20C%20Architectural%20Brief%20and%20Requirements.md) — assessment scope
- [Azure Architecture Diagram Specification.md](Azure%20Architecture%20Diagram%20Specification.md) — exact diagram target labels and boundaries
- [Azure Deployment Order Checklist.md](Azure%20Deployment%20Order%20Checklist.md) — deploy ordering for `api` then `pdf` then `ui`
- [Azure Resource Inventory.md](Azure%20Resource%20Inventory.md) — what already exists from Days 1–3
- [Five-Day Delivery Plan.md](Five-Day%20Delivery%20Plan.md) — week context
- [../Technical Debt/TD-001 Admin Standards Deviations.md](../Technical%20Debt/TD-001%20Admin%20Standards%20Deviations.md) — current debt register
- [../Canine Physio App/DecisionLog.md](../Canine%20Physio%20App/DecisionLog.md) — DEC-001 … DEC-008 already logged

## Day 3 starting state (what's already deployed)

- Resource group with: MySQL Flexible Server (seeded), Key Vault (populated), Storage Account `sthellobuddyprod` (+ `published-programmes` container), ACR `acrhellobuddyprod`, Container Apps Environment, **one** Container App `ca-hello-buddy-admin` running image `acrhellobuddyprod.azurecr.io/hello-buddy-admin:v2`, App Insights (wired to admin app), Log Analytics workspace.
- Live URL: `https://ca-hello-buddy-admin.mangocliff-d46e000d.uksouth.azurecontainerapps.io`
- Six-of-six DoD passed against this URL on Day 3.
- After today this URL becomes `ca-hello-buddy-ui.<...>` (or stays the same if you rename in Terraform state cleanly).
