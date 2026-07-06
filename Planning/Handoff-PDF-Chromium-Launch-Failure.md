# Handoff — PDF Preview Failure (Chromium launch) + Exercise Image Blob Library

Date: 2026-07-06
Author: Prior chat session (compact handoff for a fresh chat)
Status: **UNRESOLVED — root cause not conclusively proven.** A resilience fix is
written but NOT yet built/deployed/validated.

> The operator's instinct: "I feel we are missing something." Treat that as
> correct. The theories below were each proposed and then partially or fully
> disproven. Do not assume the last theory is right. Start from the evidence.

---

## 1. What the user was originally doing (context chain)

1. Original task: exercise images (Google Drive) not displaying on exercise/edit pages.
2. Pivoted to a **blob-storage image library** with a file-picker (retire Drive for
   images, keep videos on Drive untouched). This is the R2-S8 work.
3. Blob images render through a **managed proxy** because the `published-programmes`
   container is **private** (cannot hotlink raw blob URLs in `<img src>`).
4. That image work is essentially done and was being validated when the PDF issue surfaced.

### Image-library facts (working)
- Managed marker pattern: any URL containing `/exercise-media/` is resolved server-side.
  Helper: `HelloBuddy.Application/Media/ExerciseMediaKey.cs` (`Marker`, `Prefix`, `TryResolve`).
- UI proxy route: `GET /Exercises/MediaContent?key=...` (ExercisesController) streams private blob bytes.
- API routes: `GET /api/exercises/media/library` (list) and `GET /api/exercises/media/content?key=...` (stream).
- Settings: DB app-setting `FileStorage.ImageLibraryFolder` (default `exercise-media/images/`),
  editable in UI Settings — replaces the old `FileStorage.ImageLibraryUrl`.
- Fix made this session (image rendering in panes): Edit.cshtml current + selected panes and
  Details.cshtml now route `<img src>` through the `MediaContent` proxy instead of the raw blob URL.
  JS helper `toDisplayUrl()` rewrites any `/exercise-media/` URL to `/Exercises/MediaContent?key=...`.
  **This part works locally and is deployed (UI + API on commit `1217a59`).**
- Azure blob `published-programmes/exercise-media/images/` **confirmed to contain all 10 images.**
- API UAMI `uami-hellobuddy-api` **has** `Storage Blob Data Contributor` scoped to the
  `published-programmes` container (verified — permissions are fine).

---

## 2. The unresolved problem — PDF preview 500

Navigating to PDF preview
(`/Programmes/1/PreviewPdf`) returns the ASP.NET error page.

### Call chain
UI `ProgrammesController.PreviewPdf` (line ~269) → API `/api/programmes/1/preview-pdf`
→ PDF container `POST /render` → `PuppeteerPdfRenderer` launches Chromium → **fails**.

### The exception (PDF container, `ca-hello-buddy-pdf`)
```
PuppeteerSharp.ProcessException: Failed to launch browser!
[ERROR:dbus/bus.cc:405] Failed to connect to the bus:
Failed to connect to socket /run/dbus/system_bus_socket: No such file or directory
```
Chromium dies at launch. No PDF produced → API 500 → UI 500.

---

## 3. Theories proposed and their status (READ THIS — avoid repeating)

| # | Theory | Status | Evidence |
|---|--------|--------|----------|
| 1 | Image **size/bytes** in blob storage overloads PDF | **DISPROVEN** | It's a browser *launch* failure, before any HTML/image is rendered. |
| 2 | A **rollback** re-introduced a previously-fixed bug | **DISPROVEN** | git reflog + `git log -p` show `HeadlessMode.Shell` was ADDED in `5514cbe`, never reverted. Only 3 commits ever touched the renderer. No lost branches. |
| 3 | The **UI/API deploys** broke PDF | **DISPROVEN** | `-UiOnly`/`-ApiOnly` use the component-deploy path (`az containerapp update` on ONE app). They physically cannot touch the PDF container. PDF image unchanged (`5514cbe`, revision `0000006`, built 16:10 UTC). |
| 4 | **OOM / memory pressure** crashes Chromium | **DISPROVEN** | `WorkingSetBytes` peaked ~190 MB out of 1 GiB. Nowhere near limit. |
| 5 | The `HeadlessMode.Shell` fix is simply **insufficient** in ACA | **WEAK / contradicted** | User states it demonstrably worked before after a PDF redeploy. Same image shows BOTH success and failure in logs → intermittent, not a hard regression. |
| 6 | **Intermittent cold-start** Chromium launch failure (KEDA scaler cold-starts replicas; first launch under CPU pressure trips dbus) | **CURRENT LEADING THEORY, UNPROVEN** | System logs show a KEDA `external-push` scaler with `ScaledObjectCheckFailed`, replica reschedules, `ProbeFailed`. Logs show same revision both succeeding (524 KB PDF uploaded) and failing. But NOT conclusively tied to cold start. |
| 7 | **Recent Chromium change** (removal of old headless; `--headless=new` needs dbus; old path split into `chrome-headless-shell`) is the underlying cause | **PLAUSIBLE, UNVERIFIED** | Real, documented Chromium change (Chrome ~112→~132). But code already uses Shell mode, so either the Debian `chromium` apt package doesn't honour it cleanly OR a version bump changed behaviour. **Chromium version in the image was NOT checked.** |

### KEY UNRESOLVED CONTRADICTION (the "missing something")
- Code uses `HeadlessMode.Shell` (which should avoid dbus) **yet the dbus error still appears.**
- Same image/revision shows **both success and failure**.
- => Something environmental or version-specific is not understood. This is the gap.

---

## 4. Concrete evidence gathered (facts, not theories)

- Deployed PDF image: `acrhellobuddyprod.azurecr.io/hello-buddy-pdf:5514cbe`,
  revision `ca-hello-buddy-pdf--0000006`, created 2026-07-06T16:15Z, built in ACR 16:10Z.
- `git show 5514cbe:.../PuppeteerPdfRenderer.cs` → contains `HeadlessMode.Shell`. Fix IS in the deployed image.
- PDF container resources: **CPU 0.5, memory 1 GiB, minReplicas 1, maxReplicas 3.**
- PDF memory (WorkingSetBytes, last 6h): ~186–197 MB peak. No OOM.
- Logs (last ~6h) show BOTH: successful `POST /render → 200` + `Uploaded 524920 bytes to blob`
  AND `Failed to launch browser!` on the same revision.
- System logs: KEDA `external-push` scaler, `ScaledObjectCheckFailed`, `ProbeFailed`,
  replica reschedules — i.e. the PDF app scales/cold-starts.
- git reflog HEAD: `1217a59 (R2-S8)` ← `5514cbe (Image url issue fix)` ← `a89ca8d (R2-S7)`.
  No revert commits. Only branch is `main`.

### Dead-ends / gotchas encountered
- Log Analytics queries with broad `has_any` filters returned huge (50+ MB) result sets and
  hung the terminal. Keep KQL tight: `top 1 by TimeGenerated desc`, or `summarize`, narrow time.
- PowerShell string interpolation of log rows mangled output (concatenated timestamps). Prefer
  `--output table` server-side or write JSON to a temp file and read it.
- `az storage blob list --auth-mode login` failed (user lacks data-plane RBAC); use
  `--account-key` (from `az storage account keys list`) for ad-hoc listing.
- Only ONE PDF revision exists (`0000006`); older revisions purged → cannot diff against the
  "known-good" revision directly.

---

## 5. The fix that was WRITTEN this session (NOT built/deployed/validated)

File: `Canine Physio Admin/src/HelloBuddy.Admin.Pdf/PuppeteerPdfRenderer.cs`

Changes (resilience-first, root-cause-agnostic):
1. `RenderAsync` retries the whole render **once** if the browser drops mid-job
   (calls `ResetBrowserAsync` then retries).
2. `GetOrCreateBrowserAsync` now checks `_browser is { IsConnected: true }` and
   **relaunches** a dead/disconnected browser (self-heal). Was: cached forever, never checked.
3. Launch **retry loop** (3 attempts, 500ms×attempt backoff) around `Puppeteer.LaunchAsync`,
   catching `ProcessException`.
4. Added Chromium arg **`--no-zygote`** (plus existing `--no-sandbox`, `--disable-dev-shm-usage`,
   `--disable-gpu`). Rationale: the zygote helper is what reaches for the dbus system bus;
   disabling it removes the exact dependency producing the error.
5. Added `ResetBrowserAsync()` helper.

Status: edited, `get_errors` clean. **Build was SKIPPED by the user. Not compiled, not deployed.**

---

## 6. What to do next (recommended, in order)

1. **VERIFY THE GAP FIRST — do not skip.** Confirm the actual Chromium version and whether
   Shell mode is really being used in the deployed image:
   - Exec into a running/one-off PDF container: `chromium --version`.
   - Check whether `chrome-headless-shell` binary exists in the image or whether apt `chromium`
     is being used with `--headless` (shell) vs `--headless=new`.
   - This directly tests theory #7 and the KEY CONTRADICTION.
2. **Reproduce faithfully in a LOCAL Linux Docker container** (NOT Windows `dotnet run` — Windows
   Chrome has no dbus, so it cannot reproduce). Build `Dockerfile.pdf` locally, run it, POST a
   render, and confirm the failure + that `--no-zygote`/retry fixes it.
3. Consider Dockerfile options if version is the cause:
   - Pin/verify the Chromium package version.
   - Or install a real `chrome-headless-shell`.
   - Or (belt-and-braces) install `dbus` + provide a session bus, or set
     `DBUS_SESSION_BUS_ADDRESS=/dev/null`.
4. Build: `cd "Canine Physio Admin"; dotnet build HelloBuddy.Admin.sln` (must be 0/0).
5. Deploy PDF ONLY: `cd Infrastructure/terraform/container-tier; .\deploy.ps1 -PdfOnly`.
   (Component deploy = `az containerapp update` on `ca-hello-buddy-pdf` only. No DB, no terraform.)
6. Re-test PDF preview for programme 1. Pull logs (tight KQL) to confirm no launch failures.
7. **Also consider raising PDF resources** to 1.0 CPU / 2 GiB as a mitigation for cold-start
   CPU pressure (defined in terraform container-tier `main.tf`) — cheap insurance, not a proven fix.

---

## 7. Deploy / infra reference

- Build: `cd "C:\Projects\Hello Buddy\Canine Physio Admin"; dotnet build HelloBuddy.Admin.sln`
- Deploy PDF only: `cd Infrastructure/terraform/container-tier; .\deploy.ps1 -PdfOnly`
- Component deploys (`-UiOnly`/`-ApiOnly`/`-PdfOnly`) do a single `az containerapp update`.
  NO terraform, NO DB, NO migrations.
- `-AppsOnly` = terraform Phase B (all 3 apps + migrate JOB resource) but does **not** run
  migrations. Migrations only run with explicit `-RunMigrations` or manual `az containerapp job start`.
- **DB drop risk (see repo memory INCIDENT-2026-07-06-db-drop.md):** `0010_schema_fresh.sql`
  contains `DROP DATABASE` and is still in the forward chain. NEVER run the migrate job casually.
- Image tags derive from git short SHA. **Commit before deploying** or the tag is reused and
  Container Apps won't create a new revision (this bit us once this session — old views kept serving).
- Local stack: `Infrastructure/local-dev/run-local-admin-stack.ps1` (UI :5046, API :5080, PDF :5081,
  Azurite blob :10000). PDF runs via Windows `dotnet run` locally — CANNOT reproduce the dbus bug.
- Azure resources: RG `rg-hellobuddy-prod`, ACR `acrhellobuddyprod`, storage `sthellobuddyprod`,
  Log Analytics `log-hellobuddy-prod`, apps `ca-hello-buddy-{ui,api,pdf}`, migrate job `caj-hellobuddy-migrate`.

---

## 8. Constraints / preferences

- Do NOT touch video code (Drive video library stays as-is).
- Build must be 0 warnings / 0 errors.
- Do NOT run the migrate job.
- Operator runs tests (integration-first); agent should not auto-run tests.
- Do NOT create markdown docs unless asked (this handoff was explicitly requested).
