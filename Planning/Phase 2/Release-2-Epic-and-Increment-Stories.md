# Release 2 Epic and Increment Stories

**Created:** 2026-06-28
**Status:** Draft for execution
**Scope:** Hello Buddy Canine Physiotherapy Admin (Release 2)
**Implementation model:** AI-led implementation (Sonnet, measured in minutes) + human testing (~1 hour per story to test, identify issues, and re-test). Estimates include a 20% contingency for bug fixes and rework. Azure deployments add an approximate cloud wait/provisioning allowance.

---

## Epic: Release 2 — Audit Transparency, Content Onboarding, and Repeatable Test Environments

Extend the stabilised Release 1 admin system with change-auditability for the exercise library, a repeatable bulk content-loading capability for exercise descriptions, a corrected video-selection source, and a one-shot disposable Azure test environment that mirrors production without scheduling.

### Epic outcomes

- Every exercise data change is captured as an auditable history (author, date, content changed) and is viewable in-app from an "Audit history" button.
- The clinic's authored exercise descriptions can be loaded into the library in bulk from a reviewed source file, with correct descriptions and auto-numbered steps.
- The exercise video-selection popup points practitioners at the correct Google Drive video library.
- A single global disclaimer, managed from an **admin-only Settings page** (same gating as RTBF), prints as small grey centre-aligned small-print at the bottom of every programme PDF (preview and published), without appearing in the builder.
- Non-administrators cannot access the Settings page or the disclaimer endpoint.
- A complete Azure **test** environment can be stood up from scratch with a single run (network, data, containers, schema, seed), exercised, and then manually torn down by deleting one resource group.

### Epic non-goals

- Owner/pet/case audit-history UI (only the exercise library is in scope for the audit-history viewer).
- Image loading during bulk content import.
- Automated start/stop scheduling for the test environment (deliberately excluded — the test environment is ephemeral).
- Production data backfill of historical exercise changes that predate the audit wiring.

---

## Delivery-wide guardrails

- Database schema remains the source of truth; production schema changes ship as idempotent, re-runnable update scripts.
- Integration-first: verify against a local database before deploying to Azure, then re-test in Azure.
- The logged-in practitioner identity already flows cookie → `X-Practitioner-*` headers → `ICurrentPractitionerAccessor`; reuse it, do not invent a parallel identity path.
- Reuse the existing `Auditlog` table and the existing `Exerciseinstruction` line-per-step model; do not add parallel structures.
- The test environment must never share Terraform state, naming, or secrets with production.

---

## Story Index

| ID | Title | Increment |
|----|-------|-----------|
| R2-S1 | Exercise library audit trail + Audit history viewer | Increment 10 |
| R2-S2 | Exercise content loader (descriptions → library) | Increment 11 |
| R2-S3 | Configurable Google Drive video-selection source (Settings page) | Increment 11 |
| R2-S5 | Global PDF disclaimer footer (embedded, code-managed) | Increment 11 |
| R2-S6 | Video still-image (thumbnail) generator script | Increment 11 |
| R2-S4 | Disposable Azure test environment package | Increment 12 |

---

## Increment 10: Exercise Library Audit Trail

### Story R2-S1: Exercise library audit trail + Audit history viewer

#### a) User story and brief for Sonnet

**User story**
As a practitioner, I want to view the full change history of an exercise (who changed it, when, and what content changed) from an "Audit history" button, so that exercise library edits are transparent and accountable.

**Brief for Sonnet**

Current state (verified in code):
- The `Auditlog` table already exists ([Canine Physio Admin/src/HelloBuddy.Admin.Core/Data/Generated/Auditlog.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Core/Data/Generated/Auditlog.cs)) with columns: `AuditLogId`, `PractitionerId`, `UserAccountId`, `EntityName`, `EntityId`, `ActionType`, `OldValuesJson`, `NewValuesJson`, `ActionDateTime`.
- Owner, Pet, Treatmentcase, Treatmentcasenote, and Programme already write to `Auditlog`; **Exercise does not**.
- Exercise create/update flows live in `ExerciseRepository` ([Canine Physio Admin/src/HelloBuddy.Infrastructure/Records/RecordRepositories.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Infrastructure/Records/RecordRepositories.cs)) — `CreateAsync`, `UpdateAsync`, `SetActiveAsync`, plus `ApplyInstructionRows`.
- Attribution columns (`CreatedByPractitionerId/Name`, `UpdatedByPractitionerId/Name`) and `CreatedDate`/`UpdatedDate` already populate via the accessor + `AuditSaveChangesInterceptor`.
- The logged-in name is available via `ICurrentPractitionerAccessor.PractitionerName` (API: `HeaderPractitionerAccessor`, fed by `X-Practitioner-Name`).

Implement:
1. **Wire Exercise into the existing `Auditlog` table** following the same pattern as Owner/Pet/Programme. On `CreateAsync`/`UpdateAsync`/`SetActiveAsync`, write an `Auditlog` row capturing `EntityName = "Exercise"`, `EntityId = ExerciseId`, `ActionType` (Create/Update/Activate/Deactivate), `OldValuesJson`/`NewValuesJson` for the changed fields (Title, Category, ObjectiveSummary, VideoUrl, ImageUrl, Default Reps/Sets/HoldSeconds, IsActive, and the ordered instruction steps), and `PractitionerId`/`ActionDateTime` from the accessor/clock. Record the **logged-in practitioner name** (resolve from accessor; persist via existing attribution columns and/or include the name in the serialized values so the viewer can show the author even after a rename).
2. **API:** add a read endpoint, e.g. `GET /api/exercises/{id}/audit`, returning the audit rows for that exercise ordered newest-first, projected to a DTO `{ author, actionDateTime, actionType, changes[] }` where `changes` is a field-by-field diff derived from `OldValuesJson`/`NewValuesJson`.
3. **UI:** add an **"Audit history"** button on the exercise Details (and/or Edit) view that opens a **popup/modal** showing a **table** with columns: **Author**, **Date**, **Content changed**. The "Content changed" column lists the changed fields with old → new values (steps shown as a readable list). Use the existing Bootstrap modal styling consistent with the rest of the UI.
4. **Database update script:** the existing `Auditlog` table should require **no schema change** (it already holds everything). Still provide an idempotent verification/update SQL script under `Canine Physio Database/Build and Initialise/` (e.g. `Canine Physio DB Scripts - Release 2 - Exercise Audit.sql`) that asserts the `Auditlog` table/columns exist and adds any index needed for `(EntityName, EntityId)` lookups — safe to run against production.
5. **Tests:** in-memory API tests asserting an audit row is written on create/update/activate/deactivate with the correct author and diff; a UI test asserting the Audit history button renders the modal table.

Constraints: do not duplicate the audit mechanism — reuse the `Auditlog` writer used by the other repositories. Diffing must not log unchanged fields. The viewer must remain correct after a practitioner is renamed (author name is a snapshot).

#### b) Estimate (AI implementation + human testing, incl. 20% contingency)

| Phase | Base | With 20% contingency |
|-------|------|----------------------|
| AI implementation (repo audit-write wiring, API endpoint, modal/table UI, SQL script, unit tests) | ~15 min | ~18 min |
| Human testing (local DB: create/edit/activate, verify modal diff + author name, identify issues, re-test) | 1.0 h | 1.2 h |
| Azure deploy (rebuild API+UI images, push, container revision) — hands-on | ~15 min | ~18 min |
| Azure provisioning/rollout wait (mostly unattended) | ~15 min | ~18 min |
| Azure re-test | ~20 min | ~24 min |
| **Total** | **~2.0 h** | **~2.4 h** |

*Azure wait note:* image build + ACR push + container app revision rollout typically adds ~10–20 min of unattended cloud wait per app on top of hands-on time.

---

## Increment 11: Exercise Content Onboarding and Video Source

### Story R2-S2: Exercise content loader (descriptions → library)

#### a) User story and brief for Sonnet

**User story**
As a clinic administrator, I want a one-time loader that imports the authored exercise descriptions from the `Content` folder into the exercise library — with each exercise's prose as its description and the steps auto-numbered — so that the library is populated quickly and consistently without manual re-keying.

**Brief for Sonnet**

Source data (reviewed):
- `Hello Buddy/Content/Exercise Descriptions (1).docx` (the `Content` folder now sits at the workspace root, `Hello Buddy/Content`) is freeform prose: each exercise is `Title – description paragraph(s)`. Some exercises contain inline sub-labelled actions (e.g. "Lateral forelimbs:", "Craniocaudal:") and bracketed practitioner notes. There are **no pre-numbered steps**.
- **Important (confirmed):** the application **auto-numbers steps from lines** — `ParseInstructions` in [Canine Physio Admin/src/HelloBuddy.Ui/Controllers/ExercisesController.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Controllers/ExercisesController.cs) splits the "Ordered instructions (one step per line)" textarea on newlines and assigns `StepNumber = 1..n`, persisting one `Exerciseinstruction` row per line. So the loader does **not** need to invent numbering — it must produce **one step per line**, and the app/numbering handles the rest.

Implement:
1. **Reformat the source to a reviewed Markdown file first.** Convert `Exercise Descriptions (1).docx` into a structured, human-reviewable `.md` (e.g. `Hello Buddy/Content/exercise-descriptions.md`) with a clear, parseable shape per exercise, for example:
   ```
   ## <Title>
   <Description / summary prose>

   ### Steps
   - <step line 1>
   - <step line 2>
   ```
   The `<Title>` becomes `Title`, the description prose maps to `ObjectiveSummary`, and each `Steps` bullet becomes one instruction line (one step per line, to match `ParseInstructions`). Keep sub-labelled actions and bracketed notes as their own step lines where that reads naturally. **Pause and confirm the .md structure with the user before bulk-loading** if any exercise is ambiguous.
2. **Loader script:** create a runnable loader (prefer a small console runner under `tools/` or an idempotent one-shot, consistent with `tools/db-migrate/`) that reads the reviewed `.md`, maps each exercise to a `SaveExerciseRequest` (Title, ObjectiveSummary, Instructions[] one-per-line, sensible Category default / "Uncategorised" fallback, `IsActive = true`), and inserts via the existing `IExerciseRepository.CreateAsync` (or the `POST /api/exercises` endpoint) so attribution/audit wiring (R2-S1) is honoured. **Idempotent:** skip or update by `ExerciseKey`/Title so re-running does not duplicate.
3. **Do not load images** and do not fabricate video URLs — leave `ImageUrl`/`VideoUrl` null.
4. **Tests:** a parse test (md → request objects) covering an exercise with sub-labelled lines; a load test asserting rows + correctly ordered `Exerciseinstruction` steps and that a re-run is idempotent.

Constraints: descriptions/steps must round-trip so the existing Details view renders numbered steps correctly. Ask clarifying questions before finalising if the description/step boundary for any exercise is unclear.

#### b) Estimate (AI implementation + human testing, incl. 20% contingency)

| Phase | Base | With 20% contingency |
|-------|------|----------------------|
| AI: docx → reviewed `.md` conversion + structure | ~15 min | ~18 min |
| Human review of `.md` content accuracy (clinical wording) | 0.75 h | 0.9 h |
| AI: loader script + idempotency + unit tests | ~15 min | ~18 min |
| Human testing (run loader on local DB, verify descriptions + numbered steps + no dupes on re-run, identify issues, re-test) | 1.0 h | 1.2 h |
| Azure deploy/run loader against Azure DB + re-test — hands-on | ~20 min | ~24 min |
| Azure job provisioning/run wait (mostly unattended) | ~10 min | ~12 min |
| **Total** | **~2.75 h** | **~3.3 h** |

*Azure wait note:* if the loader runs as a Container Apps Job against Azure MySQL (per existing seeding method), allow ~5–10 min job provisioning/run wait.

---

### Story R2-S3: Configurable Google Drive video-selection source (Settings page)

> **Depends on R2-S5** for the admin Settings page + global settings (`AppSetting`) infrastructure. Implement R2-S5 first (or together).

#### a) User story and brief for Sonnet

**User story**
As an administrator, I want to configure the Google Drive video-library URL from a new admin Settings page (rather than it being hardcoded in config), so that the video-selection popup always opens the correct shared library and the link can be changed without a redeploy.

**Brief for Sonnet**

Current state (verified):
- The exercise edit video-search popup lives in [Canine Physio Admin/src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml) and is populated from `Model.VideoSearchProviders`.
- The Google Drive URL is currently in [Canine Physio Admin/src/HelloBuddy.Ui/appsettings.json](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/appsettings.json) (around line 14) with a hardcoded fallback in [Canine Physio Admin/src/HelloBuddy.Ui/Controllers/ExercisesController.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Controllers/ExercisesController.cs) (around line 232). Old value: `https://drive.google.com/drive/u/1/folders/13mCIF8x8VNVfg30xbbnrAxKEFRh2QF9C`.

Implement:
1. **Make the Drive video-library URL a configurable global setting** stored in the same admin-managed settings store introduced by R2-S5 (e.g. an `AppSetting` row `VideoLibrary.GoogleDriveUrl`). Add a **field on the admin Settings page** (admin-only, same `AdminOnly` gating) to view/edit it, persisted via the admin-gated settings API. Write an `Auditlog` entry on change (consistent with R2-S1).
2. **Resolve the popup's Drive provider URL from the setting at request time** (the controller/VM that builds `VideoSearchProviders` reads the stored setting), so a change takes effect without redeploy. Keep `appsettings.json` as a **default/fallback only** when the setting is unset.
3. **Seed the setting's initial value** to the new folder: `https://drive.google.com/drive/folders/1FQXInuGCPdFP5ywFaNnO39Be0ffeZMGm` (also update the appsettings default + controller fallback to this value so an unconfigured environment still points at the right folder).
4. Verify the popup's provider dropdown opens the configured URL in a new tab; changing it on Settings updates the popup.
5. Tests: setting persists/reloads; popup uses the stored value; fallback used when unset; non-admin denied access to the setting (mirrors R2-S5).

**Amendment (2026-07-05):** The Google Drive URL input on the Settings page must render **blank** (empty value) when no setting has been saved to the DB yet — do not pre-populate with the appsettings fallback. The field must carry `placeholder="Please paste in video URL"` so the admin sees greyed-out prompt text. The fallback URL still applies silently at runtime for the popup; it is intentionally not surfaced in the UI field so an admin is prompted to set the correct value explicitly.

Constraints: change only the Drive provider source; do not alter YouTube/Vimeo/other providers or the URL-validation logic. Validate the URL is a well-formed `https://drive.google.com/...` link before saving. Reuse R2-S5's settings table/page/API — do not create a parallel settings mechanism.

#### b) Estimate (AI implementation + human testing, incl. 20% contingency)

| Phase | Base | With 20% contingency |
|-------|------|----------------------|
| AI implementation (settings field + resolve-at-request + seed/fallback + tests) | ~12 min | ~14 min |
| Human testing (edit URL on Settings, confirm popup opens new folder, fallback) | ~25 min | ~30 min |
| Azure deploy (UI image rebuild + revision) — hands-on + rollout wait | ~20 min | ~24 min |
| **Total** | **~1.0 h** | **~1.1 h** |

*Note:* R2-S3 ships in the same UI image as R2-S1/R2-S2/R2-S5, sharing one Increment 11 Azure deploy/wait cycle — its marginal cost is just the settings field + resolution wiring plus a few minutes of popup verification.

---

### Story R2-S6: Video still-image (thumbnail) generator script

#### a) User story and brief for Sonnet

**User story**
As an administrator, I want a script that generates a static still image (thumbnail) for each video in the Google Drive video library, so that exercises can show a representative poster image without manual screenshotting.

**Brief for Sonnet**

Feasibility (confirmed): downloading each MP4 and extracting a frame with **ffmpeg** is standard and reliable; the only real constraint is **Drive access** (the shared folder must be reachable via a Drive API key/service account, or synced locally). This is an **offline tooling script**, not application code.

Implement (place under `tools/` alongside `tools/db-migrate/`, e.g. `tools/video-thumbnails/`):
1. A script (PowerShell or Python — match existing tooling conventions) that:
   - Enumerates the videos in the configured Google Drive folder (`https://drive.google.com/drive/folders/1FQXInuGCPdFP5ywFaNnO39Be0ffeZMGm`). Support either Drive API (API key / service account) **or** a local synced/downloaded folder path as input, documented in a short README.
   - For each MP4, downloads it (if remote) to a temp location and runs **ffmpeg** to grab a representative frame (configurable timestamp, default ~1s or 10% in) and writes a `{videoName}.jpg` (configurable output size, e.g. width 640).
   - Is **idempotent** (skips a video whose thumbnail already exists unless `-Force`), logs a summary, and writes outputs to a chosen folder.
2. **Pre-flight checks:** verify `ffmpeg` is on PATH (clear error + install hint if not); handle Drive auth failure gracefully.
3. **No application/runtime change** and **no image upload** in Phase 2 — the script only **produces** JPGs to a local output folder for review. (Wiring thumbnails into exercises / blob is a Phase 3 concern — see note below.)
4. README documenting prerequisites (ffmpeg, Drive auth options), usage, and parameters.

Constraints: do not commit downloaded videos or generated images into the repo by default (gitignore the output/temp folders); do not embed secrets (Drive credentials via env var / local credential file, never hardcoded). Keep it a standalone one-shot tool.

> **Phase 3 link:** in Phase 3, exercise videos move to **managed MP4 in blob** (R3-S1). At that point this script should be **retargeted to run against blob** (deterministic keys, no Drive auth) and its output **uploaded as the exercise's managed image / video poster** used on-device. Phase 2 delivers the Drive-based generator; Phase 3 retargets + wires it in.

#### b) Estimate (AI implementation + human testing, incl. 20% contingency)

| Phase | Base | With 20% contingency |
|-------|------|----------------------|
| AI implementation (script: enumerate/download/ffmpeg-extract, idempotency, README) | ~20 min | ~24 min |
| Human testing (Drive auth/local-folder run, verify JPGs, idempotent re-run) | 1.0 h | 1.2 h |
| No Azure deploy (offline tooling) | — | — |
| **Total** | **~1.3 h** | **~1.6 h** |

*Note:* time depends on Drive access setup; if a local synced folder is used, the auth step is skipped and human testing drops by ~15–20 min.

---

### Story R2-S5: Global PDF disclaimer footer (embedded, code-managed)

#### a) User story and brief for Sonnet

**User story**
As a practitioner, I want every programme PDF to include a standard disclaimer so that owners receive appropriate terms of service and safety information with every exercise programme.

**Scope note (updated 2026-07-04):** The original brief called for an admin-only Settings page + database table to allow runtime editing of the disclaimer. Scope was simplified: the disclaimer wording is embedded as a private constant in `ProgrammeService`. Future wording changes require a code deployment. The Settings page / admin API / database table are deferred; if runtime editability is needed it can be added later without any other code changes (just swap the constant for a DB lookup).

**Brief for Sonnet**

Current state (verified):
- The programme PDF is produced from a single Razor template, `Templates/Programme.cshtml`, compiled by [Canine Physio Admin/src/HelloBuddy.Admin.Pdf/RazorProgrammePdfTemplate.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/RazorProgrammePdfTemplate.cs) and bound to `ProgrammeVm` ([Canine Physio Admin/src/HelloBuddy.Contracts/ProgrammeVm.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Contracts/ProgrammeVm.cs)). This **same template** renders both the **preview PDF** and the **published PDF**.
- The **builder live preview pane** is a separate HTML partial, `_BuilderPreviewPane` — the disclaimer must **NOT** render there (it is unaffected as it does not use `Programme.cshtml`).
- Disclaimer source text: `Hello Buddy/Content/Hello_Buddy_Terms_of_Service_Disclaimer.txt`, reworded for a programme/paper context ("this exercise programme" instead of "this app").

Implemented:
1. Added `public string? DisclaimerText { get; init; }` to `ProgrammeVm` (non-positional `init` property — fully backward-compatible with all existing positional constructors).
2. Added `private const string PdfDisclaimerText` to `ProgrammeService` with the PDF-reworded disclaimer text.
3. `BuildRenderVmAsync` sets `DisclaimerText = PdfDisclaimerText` — covers **preview and publish** render paths.
4. `RenderVersionPdfAsync` injects `DisclaimerText = PdfDisclaimerText` after deserialization — so historical version re-renders also include the disclaimer.
5. `Programme.cshtml` footer updated: disclaimer renders below the practitioner name/date line, same 8pt grey colour and centre-alignment as the existing footer, with `margin-top: 4mm` separating them. Guarded by `@if (!string.IsNullOrWhiteSpace(...))` so null is safe.
6. In-memory test asserts the serialised publish payload contains the disclaimer text.

No database script, no Settings page, no admin API required for this simplified scope.

#### b) Estimate (AI implementation + human testing, incl. 20% contingency)

| Phase | Base | With 20% contingency |
|-------|------|----------------------|
| AI implementation (VM property + service constant + template footer + test) | ~10 min | ~12 min |
| Human testing (preview + published PDF show grey centred footer; builder does NOT) | ~20 min | ~24 min |
| Azure deploy (API image rebuild + revision) — hands-on + rollout wait | ~15 min | ~18 min |
| **Total** | **~45 min** | **~54 min** |

*Note:* R2-S5 can share the Increment 11 UI/API image build with R2-S2/R2-S3, collapsing into one Azure deploy/wait cycle.

---

## Increment 12: Disposable Azure Test Environment

### Story R2-S4: Disposable Azure test environment package

#### a) User story and brief for Sonnet

**User story**
As a platform engineer, I want a single-run package that builds a complete Azure **test** environment from scratch (network, data, containers, schema, and seed) using `test` naming instead of `prod`, so that I can spin it up, test, and then tear it down by deleting one resource group.

**Brief for Sonnet**

Current state (verified):
- Production Terraform is under `Infrastructure/terraform/` in three tiers: `vnet-tier/`, `data-tier/`, `container-tier/` (with `foundation/` + `app-*` modules).
- Naming is hardcoded to `prod` in variable defaults and some literals, e.g.: `rg-hellobuddy-prod`, `vnet-hellobuddy-prod`, `kv-hellobuddy-prod`, `mysql-hellobuddy-prod`, `acrhellobuddyprod`, `sthellobuddyprod`, `log-hellobuddy-prod`, `appi-hellobuddy-prod`, `cae-hellobuddy-prod`, and the Automation Account `aa-hellobuddy-prod` (in [Infrastructure/terraform/data-tier/main.tf](../../Infrastructure/terraform/data-tier/main.tf)).
- DB seeding uses a Container Apps **Job** (mysql:8.0, base64 script env var) per [Infrastructure/terraform/data-tier/rebuild-azure-db.ps1](../../Infrastructure/terraform/data-tier/rebuild-azure-db.ps1), running these scripts in order from `Canine Physio Database/Build and Initialise/`: `Canine Physio DB Scripts v2.3 (fresh).sql`, `Canine Physio DB Day 1 Initialise v2.4.sql`, `Canine Physio DB Scripts - Increment 8 - Login and Attribution.sql`, `Canine Physio DB MSc Assessment Seed v1.sql`, plus the Increment 9 rollback script.
- Terraform state is **local** per tier (no azurerm backend).
- The Automation Account + runbooks + schedules (DEC-002 start/stop) live in `data-tier/main.tf` (~lines 119–260).

Implement (per the agreed approach — **copy tiers into a new `Infrastructure/terraform-test/` folder with local state and one orchestrator script**):
1. **Copy** `vnet-tier`, `data-tier`, and `container-tier` into `Infrastructure/terraform-test/` and re-point every `prod` name to `test`: `rg-hellobuddy-test`, `vnet-hellobuddy-test`, `kv-hellobuddy-test`, `mysql-hellobuddy-test`, `acrhellobuddytest`, `sthellobuddytest`, `log-hellobuddy-test`, `appi-hellobuddy-test`, `cae-hellobuddy-test`, container apps `ca-hello-buddy-{ui,api,pdf}-test` (respect Azure global-uniqueness/length rules for ACR/Storage/KV — no hyphens, ≤24 chars). Keep region **UK West**.
2. **Exclude scheduling/automation:** remove (or gate behind `deploy_automation = false`) the Automation Account, runbooks, schedules, and the automation→MySQL role assignment so the test environment has no start/stop scheduling.
3. **Single-run orchestrator:** add one PowerShell script (e.g. `Infrastructure/terraform-test/deploy-test-environment.ps1`) that runs end-to-end with one invocation: `terraform apply` vnet-tier → data-tier → container-tier foundation → `az acr build` the three images into the test ACR → `terraform apply` container apps → run the schema+seed Job (reuse the proven base64/Job seeding method against the test MySQL) → print the test UI URL and a "to delete, remove resource group `rg-hellobuddy-test`" instruction. Scheduling is intentionally skipped; seeding **is** included.
4. **Isolation:** separate local state directory, a non-committed `test.tfvars` (subscription id, test MySQL admin creds, test subnet/DNS ids), and a `.gitignore` covering `*.tfvars`/state. Must not touch any `prod` resource or `prod` state.
5. **Teardown doc:** a short README in `terraform-test/` stating the environment is ephemeral and is destroyed by deleting `rg-hellobuddy-test` (single resource group), with an optional `terraform destroy` note.

Constraints: this is a build-from-scratch, single-run activity producing a self-contained, deletable environment. Do not introduce a remote backend (keep local state for simplicity/isolation). Verify the API seed service env (`Seed__PractitionerLogin__Enabled=true`) is set so login rows are created at startup, matching prod.

#### b) Estimate (AI implementation + human testing, incl. 20% contingency)

| Phase | Base | With 20% contingency |
|-------|------|----------------------|
| AI: copy + re-prefix all tiers to `test`, strip automation, orchestrator script, tfvars/gitignore, README | ~30 min | ~36 min |
| Human testing (single-run spin-up, verify network/data/containers/schema/seed + UI reachable, identify issues, re-test) | 1.0 h | 1.2 h |
| Azure provisioning wait (first-time create: VNet/DNS, MySQL Flexible Server is the long pole, KV, ACR builds ×3, CAE, 3 container apps, seed Job) — largely unattended | ~1.5–2.5 h cloud wait | ~3.0 h |
| Verified teardown test (delete `rg-hellobuddy-test`, confirm clean) | ~20 min + ~15 min wait | ~42 min |
| **Total** | **~3.5–4.5 h** | **~5.5 h** |

*Azure wait note (significant for this story):* a from-scratch build is dominated by cloud provisioning. MySQL Flexible Server creation alone is commonly ~15–45 min; the Container Apps Environment ~5–15 min; three `az acr build` runs ~5–10 min each; container app revisions a few minutes each. Budget **~1.5–2.5 hours of mostly-unattended cloud wait** for a clean first build, plus ~10–20 min for resource-group deletion at teardown.

---

## Release 2 rollup estimate

| Story | With 20% contingency |
|-------|----------------------|
| R2-S1 Exercise audit trail + viewer | ~2.4 h |
| R2-S2 Exercise content loader | ~3.3 h |
| R2-S3 Configurable Google Drive video source (Settings) | ~1.1 h (shares a UI deploy) |
| R2-S5 Global PDF disclaimer footer (admin-only Settings) | ~2.4 h (shares an Increment 11 deploy) |
| R2-S6 Video thumbnail generator script | ~1.6 h (offline tooling, no deploy) |
| R2-S4 Disposable Azure test environment | ~5.5 h |
| **Release 2 total** | **~16.3 h** (of which ~2–3 h is unattended Azure provisioning wait, dominated by R2-S4; Increment 11 UI/API stories share a single deploy cycle) |

### Acceptance focus

- Exercise create/edit/activate writes auditable history; the **Audit history** button shows a popup table of **Author / Date / Content changed** using the logged-in practitioner name.
- Authored exercise descriptions load from a reviewed `.md` into the library with correct descriptions and auto-numbered (one-per-line) steps; re-running does not duplicate; no images loaded.
- The exercise edit video popup opens the Google Drive folder configured on the admin Settings page (seeded to `1FQXInuGCPdFP5ywFaNnO39Be0ffeZMGm`), changeable without redeploy.
- A tooling script generates a still-image thumbnail for each Drive video via ffmpeg (output JPGs for review; no upload in Phase 2).
- A standard disclaimer (embedded in code from the terms-of-service source text, reworded for a programme/paper context) renders as **small grey centre-aligned** small-print at the bottom of every programme PDF on **preview and published output**, and does **not** appear in the builder preview pane.
- A single run builds a complete `test`-prefixed Azure environment (including DB schema + seed, excluding scheduling); teardown is a single resource-group delete.
- Each story is verified locally first, then re-tested in Azure.
