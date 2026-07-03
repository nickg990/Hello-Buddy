# Increment 3 — Exercise Library Delivery Plan

**Created:** 2026-06-03
**Status:** Approved for execution
**Scope:** Hello Buddy Canine Physiotherapy Admin — Release 1, Increment 3
**Parent plan:** [Release-1-Prototype-Epic-and-Increment-Stories.md](Release-1-Prototype-Epic-and-Increment-Stories.md) (Increment 3 section)
**Acceptance criteria covered:** AC-007, AC-008, AC-009 — see [08_acceptance_criteria_and_tests.md](../Canine%20Physio%20Requirements/08_acceptance_criteria_and_tests.md)
**Workflow reference:** [03b_mermaid_exercise_library_management.md](../Canine%20Physio%20Requirements/03b_mermaid_exercise_library_management.md)
**Wireframe:** [07_exercise_library.svg](../Designs/wireframes/07_exercise_library.svg)

---

## 1. Increment goal

Deliver a standalone, fully-tested **Exercise Library** feature in the admin app that lets a practitioner list, search/filter, create, edit, and activate/deactivate exercises — including the ordered instruction steps that downstream PDF and mobile JSON outputs depend on.

The library is the data foundation for the Increment 4 programme builder. Increment 3 must leave its contracts, seeded data, and repository surface in a shape Increment 4 can consume without rework.

---

## 2. Scope summary

### In scope

- Exercise list with category filter, active/inactive toggle, has-video filter, and free-text search across **title and instruction text**.
- Exercise create/edit form including:
  - title, category, summary, image URL, video URL;
  - default reps / sets / hold seconds;
  - active flag;
  - **ordered instruction steps** managed as a first-class child collection (add / remove / reorder), persisted via `ExerciseInstruction` rows.
- Exercise activate/deactivate action (preserves history, hides from new draft pickers in Increment 4).
- Exercise category lookup endpoint for the form dropdown.
- Idempotent app-start **exercise seeder** for Local/Development that owns the canonical exercise content (see §6).
- Integration tests (in-memory + Testcontainer) and UI smoke tests for the above.
- Per-component deployment scripts (`-UiOnly`, `-ApiOnly`, `-PdfOnly`) — closes Increment 3 story I3-S7.

### Out of scope (handed off)

- Programme builder picker integration — **Increment 4** (I4-S3, I4-S5).
- Publish/version interactions and "published programmes remain unchanged" enforcement at storage level — **Increment 5/6**. AC-009 is verified in this increment only by integration test that asserts a library edit does not mutate any existing `ProgrammeVersion` snapshot or stored PDF artefact.
- AuthN/Z policy switch — deferred under [TD-002](../Technical%20Debt/TD-002%20Increment%202%20Deferred.md) item #2.
- Explicit Domain layer split — deferred under [TD-002](../Technical%20Debt/TD-002%20Increment%202%20Deferred.md) item #1.
- Migration of legacy `Exercise.InstructionsText` free-text into step rows. The field is read-only in the editor for any pre-existing data and is not written by the new flow. A future "Convert to steps" action may be added if/when needed.
- UI styling refinement beyond the wireframe baseline.

---

## 3. Data model use

No schema changes. All required tables are already scaffolded in `HelloBuddy.Admin.Core/Data/Generated`:

| Table | Role in Increment 3 |
|---|---|
| `Exercise` | Master record. Editor writes title, category, summary, media URLs, defaults, `IsActive`, `UpdatedDate`. `ExerciseKey` set on create only. |
| `ExerciseCategory` | Lookup for the form dropdown and list filter. |
| `ExerciseInstruction` | Ordered step rows `(ExerciseId, StepNumber, InstructionText)`. Replace-all on save inside the same transaction as the parent update. |
| `Exercise.InstructionsText` | Legacy free-text. **Read-only** in the editor. Not written by new flow. |

### Why `ExerciseInstruction` matters end-to-end

- The publish stored procedure ([v5 SQL](../Canine%20Physio%20Database/User%20Journeys/Canine%20Physio%20-%20Generate%20and%20Publish%20Programme%20JSON%20from%20DB%20v5.sql)) emits each exercise's `instructions` JSON array by selecting `InstructionText` from `ExerciseInstruction` ordered by `StepNumber`.
- The PDF programme requirement [06_pdf_programme_requirements.md](../Canine%20Physio%20Requirements/06_pdf_programme_requirements.md) lists "ordered instructions" as a required field per exercise card.
- The admin wireframe and page-flow spec ([04_admin_page_flow_and_layout.md](../Canine%20Physio%20Requirements/04_admin_page_flow_and_layout.md)) require search across instruction text.

Treating `ExerciseInstruction` as the source of truth and `Exercise.InstructionsText` as legacy is therefore essential for the JSON/PDF pipelines in Increments 5+.

---

## 4. Architecture and code structure

Mirrors the Owner/Pet/Case pattern delivered in Increment 2. No new project boundaries.

| Project | New artefacts |
|---|---|
| `HelloBuddy.Contracts` | `ExerciseListItem`, `ExerciseListFilter` (`Search?`, `CategoryId?`, `HasVideo?`, `ActiveOnly`), `ExerciseDetailVm`, `SaveExerciseRequest` (including `IReadOnlyList<InstructionStep>` with `StepNumber`, `Text`), `ExerciseCategoryListItem`. |
| `HelloBuddy.Application` | `IExerciseRepository` (list/get/create/update/setActive/listCategories), `SaveExerciseRequestValidator` (FluentValidation: required title, ≤200 chars; category required; URL format on media fields; at least one non-empty instruction; step text ≤1000 chars). |
| `HelloBuddy.Infrastructure` | `Exercises/ExerciseRepository.cs` registered in `Records/RecordRepositories.cs`. Save method runs UPDATE + `DELETE FROM ExerciseInstruction WHERE ExerciseId = @id` + INSERT new rows in a single EF transaction. List query supports filter composition via `IQueryable` before paging. |
| `HelloBuddy.Api` | `Endpoints/ExerciseEndpoints.cs` with `MapGroup("/api/exercises")` — `GET /` (filter via query string), `GET /{id}`, `POST /`, `PUT /{id}`, `POST /{id}/activate`, `POST /{id}/deactivate`. Plus `GET /api/exercise-categories`. ProblemDetails on validation failures, matching Increment 2 conventions. |
| `HelloBuddy.Ui` | `Controllers/ExercisesController.cs`, views `Views/Exercises/Index.cshtml`, `Details.cshtml`, `Edit.cshtml` (shared create/edit). Add **Exercise library** nav entry in `_Layout.cshtml`. Instruction-step editor: server-rendered repeater with JS-free add/remove/reorder (up/down buttons) to stay consistent with Increment 2 progressive-enhancement style. |
| Seeder (new) | See §6. Lives in `HelloBuddy.Infrastructure/Seeding/ExerciseLibrarySeeder.cs`, registered as a hosted startup task and gated by `Seed:ExerciseLibrary:Enabled` config flag. |

---

## 5. Story breakdown (maps to Release-1 plan I3-S1…S7)

| Story | Deliverable | Notes |
|---|---|---|
| **I3-S1** Exercise list/search/filter | List view, filter chips, search box, paging, repository list query. | Search MUST match title and `InstructionText` rows. |
| **I3-S2** Exercise create/edit lifecycle | Create + edit views, validator, repository upsert, ordered-step editor, transactional save. | `ExerciseKey` auto-generated from title on create (slug + collision suffix); immutable thereafter. |
| **I3-S3** Active/inactive controls | Activate/deactivate API + UI button + list filter. | Inactive rows excluded from default list view but visible with toggle. |
| **I3-S4** Contracts alignment | Contracts package surface + validators; ensure Increment 4 picker can consume `ExerciseListItem` unchanged. | Document contract in this file (§4). |
| **I3-S5** Integration tests (local DB) | In-memory + Testcontainer suites — see §7. | Includes AC-009 stability test. |
| **I3-S6** Azure deployment + re-test | Deploy and re-run smoke pack against Azure. | Use new per-component scripts from I3-S7. |
| **I3-S7** Per-component deployment scripts | Split `Infrastructure/terraform/container-tier/deploy.ps1` into switches: `-UiOnly`, `-ApiOnly`, `-PdfOnly`, `-All` (default). | Each switch builds + pushes only the relevant image tag and updates only the matching Container App revision. Foundation/infra path untouched. |

---

## 6. Seeder design (agreed approach: **Option A — upsert by `ExerciseKey`**)

### Rationale

The existing [MSc Assessment Seed SQL](../Canine%20Physio%20Database/Build%20and%20Initialise/Canine%20Physio%20DB%20MSc%20Assessment%20Seed%20v1.sql) already inserts `SessionExercise` rows referencing exercises by `ExerciseKey` (`stepUp`, `baitedBackStretch`, …). A blind wipe-and-reseed of `Exercise` rows would either FK-fail or destroy Increment 2 programme data. Upsert by natural key avoids both.

### Behaviour

- Runs on **every application start** when `Seed:ExerciseLibrary:Enabled = true`.
  - **Local / Development**: default **enabled**.
  - **Test / Testcontainer**: default **enabled** (so prod and test share one code path).
  - **Production / Azure**: default **disabled** — opt-in only.
- Seeds a deterministic set of:
  - ~4 categories by name (upsert).
  - ~20 exercises by `ExerciseKey` (upsert all editable fields). The set must include every `ExerciseKey` referenced by the MSc baseline SQL seed so that script remains valid.
  - 3–5 `ExerciseInstruction` rows per exercise: `DELETE WHERE ExerciseId = @id` then `INSERT` with sequential `StepNumber`.
- Mix designed to exercise the test matrix: active + inactive rows; with-video and without-video rows; with-image and without-image rows; instruction text containing distinctive keywords for search assertions.
- Whole operation wrapped in a single EF transaction.

### Safety

- Never deletes rows it didn't author (i.e. never `DELETE FROM Exercise` unconditionally).
- If a seeded `Exercise` is found to be referenced by a `SessionExercise` row (Increment 4+), the seeder still safely updates the parent and replaces only that exercise's instruction rows — no cascade impact.

---

## 7. Test matrix

Two existing test projects extended; one optional UI smoke addition.

### `HelloBuddy.Api.InMemoryTests`
- List filter combinations: search text (matches title vs instruction-text-only), category, hasVideo, activeOnly.
- Create exercise with N instruction steps → round-trip GET returns ordered steps.
- Edit exercise: change steps from `[a,b,c]` to `[x,y]` → only new steps present, sequential `StepNumber`.
- Validator rejects: missing title, no instructions, malformed URL.
- Activate/deactivate flips `IsActive` and is reflected in default list.

### `HelloBuddy.Api.IntegrationTests` (Testcontainer MySQL)
- Same scenarios as above against the real schema.
- **AC-009 stability test**: pre-seed a `ProgrammeVersion` row and stored snapshot blob referencing an exercise; perform a library edit on that exercise; assert the `ProgrammeVersion` row hash/blob is byte-identical before and after.
- Seeder idempotency: run seeder twice; assert row counts and content stable.
- Seeder coexistence: run MSc baseline SQL seed → run app seeder → assert all `SessionExercise.ExerciseId` references still resolve.

### `HelloBuddy.Ui.Tests`
- Smoke: `/Exercises`, `/Exercises/Create`, `/Exercises/{id}`, `/Exercises/{id}/Edit` return 200 for an authenticated practitioner.
- Form post round-trip via TestServer asserts redirect-to-detail and persisted instruction steps.

### Local-first → Azure re-test gate
Per the Release 1 plan: all integration suites must pass locally before deploy; the smoke + AC-009 stability scenarios are re-run against the deployed environment via the per-component deploy scripts.

---

## 8. Open questions / dependencies

| # | Topic | Status |
|---|---|---|
| 1 | Should the editor offer a "Convert legacy `InstructionsText` to steps" action? | **Deferred** — not blocking. Add later only if real legacy content emerges. |
| 2 | `ExerciseKey` collision strategy on create from title slug | Decided: slug + numeric suffix on collision; key immutable after create. |
| 3 | TD-002 deferred items (Domain split, AuthN/Z) | Not blocking Increment 3. Re-flag for Increment 5 entry. |
| 4 | Image/video upload vs. URL-only | URL-only for Release 1 (matches schema and wireframe). Upload deferred. |

---

## 9. Handoff to Increment 4

Increment 4 (programme builder) should consume:

- `ExerciseListItem` and `ExerciseListFilter` contracts unchanged for the picker.
- `IExerciseRepository.ListAsync(filter, ct)` with `ActiveOnly = true` by default in the picker.
- Seeded data set as the canonical test fixture — Increment 4 tests must not introduce their own exercise inserts; they should rely on seeded `ExerciseKey` values.
- Per-component deploy scripts from I3-S7 are the deployment mechanism for I4 builds.

Any contract or seeded-key change required by Increment 4 must be agreed and back-ported into this plan to keep the Increment 3 test baseline valid.
