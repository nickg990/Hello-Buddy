# Increment 2 Kickoff Brief — From Assessment Slice to Working Prototype

**Created:** 2026-06-02
**Purpose:** Reference document for the next chat instance(s). The MSc COM712 cloud
assessment is **complete and submitted**. This brief frames the transition from the
assessment's thin admin slice into a fully working Release 1 prototype of the
Hello Buddy Canine Physiotherapy Admin system.
**Read first, then read:** [Technical Debt/TD-001 Admin Standards Deviations.md](../Technical%20Debt/TD-001%20Admin%20Standards%20Deviations.md),
[Standards/coding-standards.md](../Standards/coding-standards.md),
[Canine Physio Requirements/10_implementation_approach.md](../Canine%20Physio%20Requirements/10_implementation_approach.md).

---

## 1. Where we are now (as-built)

The assessment delivered a deliberately **minimal vertical slice** that exists to give the
Azure infrastructure a real workload. It is deployed, seeded and end-to-end working.

### Application (`Canine Physio Admin/`)

Three-container topology (DEC-009), six actual projects under `src/`:

| Project | Role |
| --- | --- |
| `HelloBuddy.Ui` | Public ASP.NET Core MVC + Razor views, thin controllers, calls API over HTTP |
| `HelloBuddy.Api` | Internal-only API: EF Core `CaninePhysioDbContext`, MySQL, Blob, PDF client. Owns persistence |
| `HelloBuddy.PdfService` | Internal-only PuppeteerSharp/Chromium renderer, `POST /render` |
| `HelloBuddy.Admin.Core` | Conflates Domain + Infrastructure (EF context, scaffolded entities, identity) |
| `HelloBuddy.Admin.Pdf` | HTML/PDF templating (`IPdfRenderer`) |
| `HelloBuddy.Contracts` | Shared DTOs across UI/API boundary |

### What actually works today

- Loads **one seeded treatment case** (Buddy, hind-limb rehab, practitioner id 1).
- **Programme builder** edits already-seeded session exercises (reps, sets, hold seconds, sort order, notes) via `PUT /api/programmes/{id}`.
- **Live preview** (UI calls PDF service for an HTML preview).
- **Publish**: UI → API → PDF service → private Blob Storage, returns a `PublishResponse`; download via 30-minute user-delegation SAS.
- Inter-service auth via `X-Practitioner-Id` header (DEC-010).

### What is NOT built yet (despite being Release 1 scope)

- **No create/edit CRUD** for owners, pets, treatment cases or case notes — only editing seeded rows.
- **No exercise library** management screens.
- **No draft-programme creation** — programmes are seeded, not authored.
- **No publish-time validation** (verified from source: the publish path has no guard — no "at least one exercise", no required-field, no video-link checks). A "blocked publish then fix" flow is currently impossible.
- **No versioning / immutable published history.**
- **No tests of any kind.**
- **No real authentication** (single seeded practitioner identity).

### Infrastructure (`Infrastructure/`)

- Three Terraform tiers: `vnet-tier/`, `data-tier/`, `container-tier/`. All UK West, RG `rg-hellobuddy-prod`.
- MySQL Flexible Server, private (VNet-delegated subnet + private DNS, no public endpoint), `nsg-subnet-mysql` allows 3306 only from `subnet-apps`.
- Key Vault read by API via managed identity (UAMI), **not** Container Apps `keyvaultref`.
- `Infrastructure/README.md` — quick-start, troubleshooting, cost & carbon. Kept after cleanup; the assessment-only `dod-test*` / `lockdown-check*` scripts were deleted.
- **Schema/seed is a manual step** — Terraform creates an empty database only. Seeding = run three SQL scripts from `Canine Physio Database/Build and Initialise/` (schema → reference data → demo seed).

---

## 2. The next goal

Turn the slice into the **Release 1 prototype** described in
[01_project_summary_and_release_scope.md](../Canine%20Physio%20Requirements/01_project_summary_and_release_scope.md):
a usable admin system where a practitioner can create owners/pets/cases/notes, maintain an
exercise library, author and validate a programme, and publish a branded versioned PDF —
all backed by the approved database, with tests and the standards-compliant architecture.

The requirements doc sequences this as increments 2–7. We are starting **Increment 2**.

---

## 3. Increment 2 — definition

Per [10_implementation_approach.md](../Canine%20Physio%20Requirements/10_implementation_approach.md),
Increment 2 = **core database-backed records**:

- Owner list / detail / create / edit
- Pet list / detail / create / edit
- Treatment case list / detail / create / edit
- Case note creation
- Basic validation
- Synthetic seed data
- Tests for core CRUD

**Done when:** a practitioner can create an owner, add a pet, create a treatment case and add a case note.

---

## 4. Architectural debt to pay down FIRST (Increment 2 blockers)

These come from [TD-001](../Technical%20Debt/TD-001%20Admin%20Standards%20Deviations.md). The TD file
states **no new feature work starts until the blockers clear**. Treat TD-001 as the refactor backlog.

| ID | Blocker | Action |
| --- | --- | --- |
| **D-02** | `Admin.Core` conflates Domain + Infrastructure | Split into `HelloBuddy.Domain` + `HelloBuddy.Infrastructure`; `Pdf` depends on Application contracts. Adopt the standards §2 layout (Domain/Application/Infrastructure/Pdf/Web) |
| **D-03** | No tests | Stand up integration + web smoke test projects on day one |
| **D-10** | Controllers inject `DbContext` and run inline LINQ | Introduce `ITreatmentCaseRepository`, `IProgrammeRepository`, query objects |
| **D-12** | No FluentValidation / `Result<T>` / ProblemDetails | Add validators in the Application layer alongside D-10 |
| **C7** | Two-phase Terraform `count`-gate workaround | Split `container-tier` into `foundation/` + `app/` root modules |

Quick-win companion commits (cheap, do early): **D-04** `Directory.Build.props`,
**D-05** `Directory.Packages.props`, **D-06** `.editorconfig`, **D-09** DI extension methods,
**D-18/D-19** Serilog + correlation-ID middleware, **D-24** stand up `docs/adr/`.

---

## 5. Carried production debt (not Increment 2, but keep architecture-ready)

| ID | Item |
| --- | --- |
| D-27 | Container App ingress is internet-facing without WAF/Front Door |
| TD-004 | Local Docker build blocked on DevBox; CI must use `az acr build` |
| TD-005 | Replace `X-Practitioner-Id` header with Entra workload-identity JWT |
| TD-007 | Replace per-request SAS minting with Front Door + signed URLs at scale |
| TD-008 | Move synchronous PDF generation onto a queue-backed worker |

Also keep future phases unblocked but unbuilt (YAGNI): MAUI owner app, JSON programme
snapshots, offline sync, XML export, analytics.

---

## 6. Standards reminders for the next chat

- **Database is the source of truth** — code conforms to the approved schema, not vice versa.
- **Vertical slices first** — a working end-to-end thin slice beats a perfect isolated layer.
- **Boundaries are contracts** — anything crossing a layer/process boundary is validated, versioned and explicit.
- **net9.0 stays** (D-07: amend the standard rather than downgrade). Re-pin Pomelo to GA when available (D-08).
- Every material decision gets an ADR in `docs/adr/` + a `DecisionLog.md` line.
- WCAG 2.2 AA is a real gate for new screens (D-22) — labels, one `<h1>`, `aria-live` status, visible focus, ≥4.5:1 contrast.

---

## 7. Confirmed kickoff decisions (Nick, 2026-06-02)

These are **fixed inputs** for the next chat — not up for re-litigation.

1. **Refactor in place** inside `Canine Physio Admin/`. A substantial rewrite across all three containers is acceptable if that is what standards compliance requires. Do **not** spin up a parallel solution and port across.
2. **Keep the three-container split** (UI / API / PDF) and extend it as Release 1 grows. The standards' "one web host, multiple projects" layout (§2) applies *per container* — each container gets its own layered project structure rather than collapsing back to a single host.
3. **Keep the live Azure deployment** as the prototype target. **Split the Terraform and PowerShell** into a more granular deployment solution so individual pieces can be applied/redeployed independently (closes C7 and the implicit "one-big-tier" debt in `Infrastructure/`).

## 8. Suggested first session plan

1. Re-read this brief + TD-001. Confirm Increment 2 scope, blockers-first.
2. Land the layered split (D-02) **per container** + `Directory.Build.props` / `Packages.props` / `.editorconfig` (D-04/05/06) at the solution root.
3. Stand up `tests/` with a `WebApplicationFactory` smoke test and a Testcontainers MySQL integration test (D-03).
4. Introduce repositories + FluentValidation for the first CRUD slice (D-10/D-12).
5. Build the **Owner** vertical slice (list/detail/create/edit) end-to-end through UI → API → MySQL as the template for pet/case/note.
6. **Split Terraform** — break `container-tier/` into `foundation/` (CAE, ACR, Storage, AppInsights, UAMIs, KV secrets) + per-app modules (`app-ui/`, `app-api/`, `app-pdf/`) so each Container App can be replanned/applied independently. Refactor PowerShell into matching small scripts (one purpose each).
7. Update `DecisionLog.md` + open ADRs in `docs/adr/` as decisions are made (record the three confirmed decisions above as ADR-001/002/003).

---

## 9. Key references

- [Technical Debt/TD-001 Admin Standards Deviations.md](../Technical%20Debt/TD-001%20Admin%20Standards%20Deviations.md) — refactor backlog (master)
- [Standards/coding-standards.md](../Standards/coding-standards.md) — target architecture
- [Canine Physio Requirements/10_implementation_approach.md](../Canine%20Physio%20Requirements/10_implementation_approach.md) — increment plan
- [Canine Physio Requirements/01_project_summary_and_release_scope.md](../Canine%20Physio%20Requirements/01_project_summary_and_release_scope.md) — Release 1 scope
- [Canine Physio Requirements/08_acceptance_criteria_and_tests.md](../Canine%20Physio%20Requirements/08_acceptance_criteria_and_tests.md) — acceptance criteria
- [MSc Assessment/DecisionLog.md](../MSc%20Assessment/DecisionLog.md) — DEC-001 … DEC-012
- [Designs/Admin System Planning Pack.md](../Designs/Admin%20System%20Planning%20Pack.md) — admin UX/flow
- [Infrastructure/README.md](../Infrastructure/README.md) — deploy/run quick-start

---

## 10. Operational guardrails

- Do **not** `terraform destroy` the assessment estate until MSc grades are released.
- Pandoc on this machine: `& "$env:LOCALAPPDATA\Pandoc\pandoc.exe"` (the `C:\Program Files\Pandoc\pandoc.exe` path fails).
- Local container builds are blocked on the DevBox (TD-004) — use `az acr build` for image work.
- Seeding MySQL is manual (Terraform creates an empty DB only). Use the three SQL scripts in `Canine Physio Database/Build and Initialise/` in order.

