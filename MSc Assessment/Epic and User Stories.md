# Hello Buddy Cloud Admin

## Epic and User Stories — COM712 Assessment 3 (Option C)

This file is the controller backlog for the MSc assessment build. It sequences the work from analysis through to Azure deployment and assessment-evidence capture, using the architecture and decisions already locked in the `MSc Assessment/` reference docs. Other chats pick up individual stories from here.

Status legend: `[ ] not started` · `[~] in progress` · `[x] complete`

---

## Epic

**As a** postgraduate student delivering COM712 Assessment 3 (Option C),
**I want** to design, build, and deploy a containerised first increment of the Hello Buddy Canine Physiotherapy Admin to Azure,
**so that** I can demonstrate a credible practitioner workflow on managed containers with measured performance, security, cost, and sustainability evidence that supports a distinction-level submission.

### Epic acceptance criteria

1. The first-increment vertical slice (seeded case → programme builder → in-page PDF preview → publish → stored PDF) runs end-to-end in Azure.
2. The deployed stack uses Azure Container Apps for compute, Azure Database for MySQL Flexible Server for structured data, Azure Blob Storage for files, Azure Key Vault for secrets, and Azure Monitor / Log Analytics / Application Insights for observability.
3. The Azure MySQL database is initialised from a new assessment-specific script (`Canine Physio Database/Build and Initialise/Canine Physio DB MSc Assessment Init v1.sql`) derived from the existing finalised scripts, and synthetic seed data is loaded from a matching assessment seed script. No fresh schema is designed by the application code.
4. All Azure infrastructure is defined and deployed via Terraform modules committed to the repository, applied incrementally (data tier → container platform → object storage).
5. The application is developed in VS Code directly against the Azure MySQL connection string — there is no local Docker Compose / local MySQL stage.
6. No live personal or clinical data is used at any point; only synthetic data.
7. All assessment evidence (logs, metrics, scaling, cost, carbon, validation-block scenario) is captured and traceable to a requirement.
8. The solution can be rebuilt from the submitted artefacts (application source, Dockerfiles, Terraform modules, DB scripts) by another competent reader with only environment-specific changes.

### Step list (epic-level sequence)

1. Confirm analysis and scope.
2. Lock design alignment (wireframes, architecture diagram, storage layout).
3. Author the MSc assessment database initialisation and seed scripts (derived from the existing Canine Physio scripts).
4. Author the Terraform data-tier module (RG, Key Vault, MySQL Flexible Server).
5. Apply the data-tier Terraform; run the MSc init and seed scripts against Azure MySQL.
6. Build the application code in VS Code (admin web, API, PDF service) configured against the Azure MySQL connection string.
7. Containerise the three services for the cloud build (Dockerfiles only — no local Compose).
8. Author and apply the Terraform container-platform module (ACR, Log Analytics, App Insights, Container Apps environment, the three apps with ingress and scaling).
9. Build and push images; deploy internal services (API, PDF) and the public UI.
10. Author and apply the Terraform object-storage module (Storage Account, four Blob containers); seed exercise images/videos; wire the app to Blob and redeploy.
11. Run end-to-end validation in Azure.
12. Capture observability and assessment evidence (telemetry, scaling, validation block).
13. Capture cost and sustainability comparisons.
14. Assemble the assessment report and submission pack.

---

## Story group 1 — Analysis and scope

### US-01 Confirm assessed scope and out-of-scope list

- **As a** student, **I want** the assessed first-increment scope and explicit exclusions written down, **so that** every later story stays inside the assessment envelope.
- **Acceptance criteria**
  - Scope statement matches `Option C Architectural Brief and Requirements.md` (Assessed MVP Scope).
  - Out-of-scope items (owner portal, offline sync, full mobile, multi-tenant admin, advanced RBAC, automated owner delivery) are listed.
  - Distinction-level rubric targets are captured.
- **Done when** the scope summary is referenced from the report skeleton.

### US-02 Confirm first-increment workflow slice

- **As a** student, **I want** the practitioner workflow steps in scope identified by ID, **so that** build work targets the smallest defensible slice.
- **Acceptance criteria**
  - Steps P3–P11 and S1–S3 from `Designs/03i_mermaid_end_to_end_clinical_journey.md` are the in-scope slice.
  - The in-page PDF preview rule (no draft download, no draft print) is recorded as a requirement.
  - Linked screens: case detail, programme builder, in-page PDF preview, published programme.

---

## Story group 2 — Design alignment

### US-03 Finalise wireframes for the increment

- **As a** student, **I want** the four increment wireframes signed off, **so that** the build implements a single agreed design.
- **Acceptance criteria**
  - `04_case_detail.svg`, `05_programme_builder.svg`, `06_pdf_preview.svg`, `09_published_programme.svg` reflect the latest decisions (multi-select picker, drag-and-drop reorder with non-drag fallback, in-page preview).
  - Navigation across all wireframes matches the full IA.
- **Done when** all four wireframes validate with no errors.

### US-04 Lock target architecture diagram

- **As a** student, **I want** a single Azure target architecture diagram aligned to `Azure Production Architecture Diagram.md` and `Azure Architecture Diagram Specification.md`, **so that** the report shows a defensible managed-containers design.
- **Acceptance criteria**
  - Diagram shows Internet → UI Container App → internal API + PDF Container Apps → MySQL Flexible Server + Blob Storage + Key Vault → Azure Monitor / Log Analytics / Application Insights.
  - Public/internal ingress boundary is explicit.

### US-05 Confirm storage split (MySQL vs Blob)

- **As an** architect, **I want** the responsibilities of MySQL and Blob Storage written down, **so that** persistence decisions in code match the assessment narrative.
- **Acceptance criteria**
  - Aligned to `Azure Storage Layout Recommendation.md`: structured records, exercise text, video URLs, blob metadata in MySQL; PDFs/images/videos in Blob.
  - Container access levels recorded: `published-programmes` private, `exercise-images` private, `exercise-videos` public read, `internal-assets` private.

---

## Story group 3 — MSc assessment database scripts

The full Canine Physio schema is finalised in `Canine Physio Database/`. For the assessment, derive a focused init script and a focused seed script so the Azure database can be created and populated in two repeatable runs.

### US-06 Author the MSc assessment init script

- **As a** developer, **I want** `Canine Physio Database/Build and Initialise/Canine Physio DB MSc Assessment Init v1.sql` authored as a subset of the finalised v2.4 script, **so that** the Azure database contains exactly the tables and FKs needed for the increment.
- **Acceptance criteria**
  - Script is a derivative of `Canine Physio DB Day 1 Initialise v2.4.sql`, kept under the same `Build and Initialise/` folder.
  - Covers entities for the increment workflow (treatment cases, programmes, programme sessions, programme exercises, exercises) plus published-programme metadata (file path, version, generated-at, file id).
  - Is idempotent against an empty MySQL Flexible Server (drops/creates inside a transaction or guarded with `IF NOT EXISTS`).
  - Header comment cites the parent script and the assessment context.

### US-07 Author the MSc assessment seed script

- **As a** developer, **I want** a single seed script that loads synthetic owner/pet/case/programme/exercise data sufficient for the demo flow, **so that** the demonstration runs without any application-side seeding code.
- **Acceptance criteria**
  - Derived from the relevant `Canine Physio Database/User Journeys/` scripts (signup, initial review, first programme creation).
  - Produces at least one demo treatment case with owner, pet, practitioner, condition, goals, recent notes, draft programme, and an exercise catalogue large enough to populate AM/PM sessions.
  - Re-runnable cleanly against a freshly initialised database (truncate-then-insert pattern is acceptable).
  - All data is clearly synthetic and annotated as such.

---

## Story group 4 — Terraform data-tier module

### US-08 Author the data-tier Terraform module

- **As an** operator, **I want** a Terraform module that provisions the resource group, Key Vault, and MySQL Flexible Server, **so that** the database tier is deployed reproducibly before any application work begins.
- **Acceptance criteria**
  - Folder layout `Infrastructure/terraform/` with a `data-tier/` module and per-environment variable file.
  - Resources: `rg-hellobuddy-prod`, `kv-hellobuddy-prod`, `mysql-hellobuddy-prod` (Burstable B1ms) matching `Azure Resource Inventory.md`.
  - MySQL admin credentials are passed as Terraform variables (never committed) and written to Key Vault as a secret.
  - MySQL is created with a temporary firewall rule allowing the developer IP for the dev iteration phase (US-10/US-12); the rule is recorded as TODO to remove before final evidence capture (see US-29).

### US-09 Apply the data tier and initialise the database

- **As an** operator, **I want** the data-tier Terraform applied and the MSc init + seed scripts executed against Azure MySQL, **so that** application development can target a real cloud database immediately.
- **Acceptance criteria**
  - `terraform apply` completes; outputs include the MySQL FQDN and the Key Vault URI.
  - The MSc init script and the MSc seed script run cleanly against `mysql-hellobuddy-prod` (via Azure Cloud Shell, Workbench, or `mysql` CLI from the developer machine).
  - A verification query confirms expected tables and at least one demo case row exist.
  - Connection string is stored in Key Vault for later application use.

---

## Story group 5 — Build the application code (against Azure MySQL)

The app is developed in VS Code directly against the Azure MySQL connection string from US-09. No local Docker Compose stage.

### US-10 Admin web app shell

- **As a** practitioner, **I want** a consistent admin shell with branded layout, navigation, and an authentication placeholder, **so that** every increment page sits in the same frame.
- **Acceptance criteria**
  - ASP.NET Core server-rendered app with shared layout, theme tokens from `Standards/coding-standards.md`, and the full IA navigation.
  - Seeded single-practitioner login or simple local auth placeholder.
  - Health endpoint exposed for container probes.
  - Configuration reads the MySQL connection string from environment variable (locally) or Key Vault reference (in Azure).

### US-11 Case detail page

- **As a** practitioner, **I want** to open a seeded treatment case with Summary and Programme tabs, **so that** I can start the increment workflow from a realistic context.
- **Acceptance criteria**
  - Reads from the Azure MySQL database; renders owner, pet, condition, goals, recent notes.
  - Programme tab lists existing drafts and published versions and offers "Create draft programme".

### US-12 Exercise library data access

- **As a** developer, **I want** the API to expose the seeded exercise catalogue with default fields and (later) video URLs, **so that** the programme builder can populate the multi-select picker quickly.
- **Acceptance criteria**
  - Read endpoint returns exercises with title, default reps/sets/hold/frequency, notes, image URL placeholder, video URL placeholder.
  - URL fields tolerate null/empty values until the Blob module (US-22+) wires them up.

### US-13 Programme builder page

- **As a** practitioner, **I want** an editable programme builder with sessions, a multi-select exercise picker, and reorder, **so that** I can compose a programme.
- **Acceptance criteria**
  - AM/PM (or single) session structure supported.
  - Modal multi-select picker adds exercises in one action, prefilled from defaults.
  - Reorder via non-drag fallback (alt buttons) is implemented and accessible per WCAG 2.2 SC 2.5.7. Drag-and-drop is a stretch and may be deferred under the agreed time-box.
  - Edits persist to Azure MySQL.

### US-14 In-page PDF preview with validation panel

- **As a** practitioner, **I want** the draft preview to render inside the admin page with a validation panel, **so that** I can fix blockers before publish without producing a draft download.
- **Acceptance criteria**
  - Preview renders inside the page; no print or download action exists in the draft state.
  - Validation panel lists missing required fields; publish is blocked while blockers remain.
  - Same validation rule set used in preview and at publish.

### US-15 Publish workflow and version immutability

- **As a** practitioner, **I want** publish to produce an immutable versioned PDF, **so that** the issued owner document can never be silently overwritten.
- **Acceptance criteria**
  - On publish, the API creates a new programme version, calls the PDF service, persists the PDF (filesystem during US-15, Blob from US-23 onwards), and writes metadata to MySQL.
  - Previously published versions remain accessible and unchanged.
  - Audit log records `programme.published`, `pdf.generated`, `pdf.generation_failed`, `pdf.persisted`.

### US-16 PDF rendering service

- **As a** developer, **I want** a dedicated PDF service that converts a server-rendered HTML programme template into a branded A4 PDF, **so that** the second container has a meaningful boundary.
- **Acceptance criteria**
  - Internal-only HTTP endpoint accepts a programme payload (or rendered HTML) and returns a PDF stream.
  - Uses PuppeteerSharp or Playwright with headless Chromium.
  - Health endpoint exposed for container probes.
  - Output matches `Designs/owner_programme_pdf.svg` layout intent.

### US-17 Structured logging and observability hooks

- **As a** developer, **I want** structured logs and Application Insights telemetry wired into all three services from the start, **so that** the cloud observability stack receives real data on first deploy.
- **Acceptance criteria**
  - Request, dependency, and exception telemetry emitted from UI, API, and PDF.
  - Application events (publish requested, validation blocked, PDF generated, PDF failed) emitted as structured logs.
  - Connection string / instrumentation key sourced from configuration.

---

## Story group 6 — Containerise services for the cloud build

No local Compose. Dockerfiles exist solely to build images for ACR.

### US-18 Dockerfile for admin web service

- **As a** developer, **I want** a multi-stage Dockerfile for the ASP.NET Core admin web app, **so that** the UI image is reproducible.
- **Acceptance criteria**
  - Build/run stages separated; no secrets baked in.
  - Image starts on a configurable port and reads connection strings from environment variables / Key Vault references.

### US-19 Dockerfile for API service

- **As a** developer, **I want** a multi-stage Dockerfile for the API service, **so that** the API image is reproducible.
- **Acceptance criteria**
  - Same multi-stage pattern as the UI image.
  - Health endpoint exposed for container probes.

### US-20 Dockerfile for PDF service

- **As a** developer, **I want** a Dockerfile for the PuppeteerSharp/Playwright PDF service, **so that** the rendering container builds cleanly.
- **Acceptance criteria**
  - Headless browser dependencies installed.
  - Exposes a single internal endpoint for HTML → PDF conversion.

---

## Story group 7 — Terraform container-platform module

### US-21 Author the container-platform Terraform module

- **As an** operator, **I want** a Terraform module that provisions ACR, Log Analytics, Application Insights, the Container Apps environment, and the three Container Apps with ingress and scaling rules, **so that** the runtime is reproducible.
- **Acceptance criteria**
  - Folder `Infrastructure/terraform/container-platform/`.
  - Resources: `acrhellobuddyprod`, Log Analytics workspace, Application Insights, `cae-hellobuddy-prod`, `ca-hello-buddy-api` (internal), `ca-hello-buddy-pdf` (internal), `ca-hello-buddy-ui` (external HTTPS).
  - Scaling rules: UI min 1/max 3 HTTP concurrency; API min 0/max 3 HTTP+CPU; PDF min 0/max 2 low concurrency/CPU.
  - User-assigned managed identity created and granted ACR pull, Key Vault secret read, MySQL access.
  - Image references parameterised by tag so a new build triggers a new revision via re-apply.

### US-22 Build and push images, then deploy to Container Apps

- **As an** operator, **I want** images built and pushed to ACR and the container-platform Terraform applied, **so that** the three services are live in Azure.
- **Acceptance criteria**
  - All three images visible in ACR with semantic tags.
  - Terraform apply completes; UI is reachable over public HTTPS.
  - API and PDF are not publicly exposed.
  - UI → API and API → PDF calls succeed using internal ingress.
  - API → MySQL succeeds using the Key Vault-backed connection string.

---

## Story group 8 — Terraform object-storage module

### US-23 Author the object-storage Terraform module

- **As an** operator, **I want** a Terraform module that provisions the Storage Account and four Blob containers with the agreed access levels, **so that** the storage layout matches the design.
- **Acceptance criteria**
  - Folder `Infrastructure/terraform/object-storage/`.
  - Storage account `sthellobuddyprod` (GPv2, hot tier).
  - Containers: `published-programmes` (private), `exercise-images` (private), `exercise-videos` (public read), `internal-assets` (private), matching `Azure Storage Layout Recommendation.md`.
  - Role assignments grant the Container Apps managed identity least-privilege access (write to `published-programmes`, read on the rest as needed).

### US-24 Seed exercise images and videos into Blob

- **As a** developer, **I want** the synthetic exercise images and videos uploaded into the seeded Blob containers, **so that** programme PDFs and the builder UI render with realistic media.
- **Acceptance criteria**
  - A scripted upload (Azure CLI or PowerShell) places the synthetic assets into `exercise-images` and `exercise-videos`.
  - Asset URLs are recorded in MySQL exercise rows (small follow-up SQL update is acceptable).
  - All assets are clearly synthetic.

### US-25 Wire the application to Blob and redeploy

- **As a** developer, **I want** the API to write published PDFs to `published-programmes` and resolve image/video URLs from Blob, **so that** publish persistence matches the production design.
- **Acceptance criteria**
  - Publish writes the PDF to `published-programmes` with stable naming and stores the URL/metadata in MySQL.
  - Builder and PDF render resolve image and video URLs from Blob.
  - New images built and pushed; Terraform re-applied with new image tags to roll out the changed revisions.

---

## Story group 9 — End-to-end validation

### US-26 Cloud smoke test of the slice

- **As a** student, **I want** an end-to-end run of the increment workflow in Azure, **so that** the deployment is proven before evidence capture.
- **Acceptance criteria**
  - Public UI loads; seeded case opens; programme builder edits save; in-page PDF preview renders; publish completes; PDF appears in `published-programmes`; MySQL metadata updated.

### US-27 Validation-blocked scenario

- **As a** student, **I want** to trigger and capture a validation block before publish, **so that** the validation gate is evidenced.
- **Acceptance criteria**
  - Screenshot or recording of the in-page preview validation panel blocking publish.
  - Log entry showing the validation event.

### US-28 Failure-mode evidence

- **As a** student, **I want** to capture one diagnosed failure (e.g., bad secret, service crash), **so that** the recovery narrative is evidenced.
- **Acceptance criteria**
  - Failure scenario reproduced and resolved.
  - Logs/screenshots captured before and after.

### US-29 Lock down MySQL public access

- **As an** operator, **I want** the temporary developer-IP firewall rule on MySQL removed and private access confirmed, **so that** the final evidence reflects the production-grade security posture.
- **Acceptance criteria**
  - Firewall rule removed via Terraform variable change and re-apply.
  - Final smoke test passes with API → MySQL via private connectivity only.

---

## Story group 10 — Observability and evidence capture

### US-30 Confirm telemetry pipelines

- **As a** student, **I want** logs in Log Analytics and request telemetry in Application Insights, **so that** the report can cite real data.
- **Acceptance criteria**
  - One successful publish trace recorded end-to-end across UI → API → PDF → Blob → MySQL.
  - One failed scenario recorded with diagnostic evidence.

### US-31 Load test and scaling evidence

- **As a** student, **I want** a lightweight load test against the UI plus an Azure Monitor scaling screenshot, **so that** autoscaling behaviour is evidenced.
- **Acceptance criteria**
  - Scripted requests (PowerShell loop, `hey`, or `k6` if time allows).
  - Replica count before/after scaling captured for UI and, if possible, API.

---

## Story group 11 — Cost and sustainability

### US-32 Seven-day cost comparison

- **As a** student, **I want** a Container Apps vs equivalent VM seven-day cost comparison, **so that** the report defends the platform choice.
- **Acceptance criteria**
  - Workload, uptime, traffic, and storage assumptions stated.
  - Pricing-calculator outputs cited.

### US-33 Carbon impact comparison

- **As a** student, **I want** a defensible carbon comparison using a published method, **so that** the sustainability section is evidence-based.
- **Acceptance criteria**
  - Method cited (Microsoft sustainability guidance, Cloud Carbon Footprint, or equivalent).
  - Comparison aligned with the cost-comparison assumptions.

---

## Story group 12 — Report assembly and submission

### US-34 Draft architecture decision defences

- **As a** student, **I want** written defences for the four architecture decisions (Container Apps over VM, managed MySQL, separate PDF service, synthetic-data vertical slice), **so that** the report can answer the rubric directly.
- **Acceptance criteria**
  - Each decision has a defence and counter-argument paragraph aligned to `Option C Architectural Brief and Requirements.md`.

### US-35 Compile assessment report and submission pack

- **As a** student, **I want** the final report, diagrams, evidence screenshots, and reproducibility artefacts packaged together, **so that** submission is complete.
- **Acceptance criteria**
  - Report covers executive summary, scenario/requirements, design justification, architecture diagram, build/deploy, network/security/scaling, observability, cost, sustainability, conclusions.
  - Submission pack includes: application source, Dockerfiles, Terraform modules and tfvars (with secrets stripped), MSc init + seed SQL scripts, and evidence artefacts.
  - All evidence is traceable back to a story in this file.
  - All artefacts are reproducible from the repository with environment-specific changes only.

---

## Cross-cutting non-functional acceptance criteria

These apply to every story unless explicitly relaxed:

- Only synthetic data is used in code, screenshots, and the report.
- No secrets in source control or image layers.
- All Azure resources are created and updated through Terraform; portal changes are not part of the assessed deployment path.
- The Canine Physio Database scripts (specifically the MSc Assessment Init + seed scripts) are the single source of truth for schema and seed data — no parallel schema is created in the application code.
- The developer-IP MySQL firewall rule is temporary and removed before final evidence capture (US-29).
- Public surface area is limited to the UI ingress endpoint.
- All deployed services emit logs and metrics to the shared observability stack.

## Source documents

- `MSc Assessment/Option C Architectural Brief and Requirements.md`
- `MSc Assessment/Azure Deployment Order Checklist.md`
- `MSc Assessment/Azure Resource Inventory.md`
- `MSc Assessment/Azure Infrastructure and Cost Reference.md`
- `MSc Assessment/Azure Architecture Diagram Specification.md`
- `MSc Assessment/Azure Production Architecture Diagram.md`
- `MSc Assessment/Azure Storage Layout Recommendation.md`
- `MSc Assessment/First Increment Demo Scope and Workflow.md`
- `Canine Physio Database/Build and Initialise/Canine Physio DB Day 1 Initialise v2.4.sql` (parent of the MSc init script)
- `Canine Physio Database/User Journeys/` (source for the MSc seed script)
- `Designs/03i_mermaid_end_to_end_clinical_journey.md`
- `Designs/end_to_end_clinical_workflow_annotated.svg`
- `Designs/wireframes/` (04, 05, 06, 09 for the first increment)

