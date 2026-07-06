# Phase 2 Handoff

## CHAT HANDOVER STATE

### Architecture State
- Component: Azure MySQL Flexible Server mysql-hellobuddy-prod -> private-only DB endpoint; VNet-integrated; no public/firewall path.
- Component: Container Apps Environment cae-hellobuddy-prod -> private network execution plane for app containers and migration job.
- Component: Planned migration job caj-hellobuddy-migrate -> manual trigger DB migration executor in Container Apps.
- Component: Terraform data-tier -> provisions DB shell + Key Vault secret; does not execute schema scripts.
- Component: Terraform container-tier -> hosts app resources and will host migration job + UAMI + KV access wiring.
- Component: deploy.ps1 in container-tier -> two-phase infra/app deploy flow; planned opt-in migration trigger path.
- Data Flow: operator starts job -> job reads ConnectionStrings--CaninePhysioDb from Key Vault via UAMI -> runner executes ordered sql files -> writes schema_migrations tracking rows -> exits success/failure.
- Release 2 status: R2-S1, R2-S3, R2-S5 implemented + local-tested; not deployed to Azure. R2-S2, R2-S6, R2-S7 required for go-live.

### Verified Changes
- File: [Planning/Phase 2/Release-2-Epic-and-Increment-Stories.md](Release-2-Epic-and-Increment-Stories.md)
	- Restored from git commit 36411e9 after empty merge state in d985e08.
	- File now contains full Release 2 epic and story content again.
- File: [Planning/Phase 2/Release-2-Database-Migration-Job-Epic-and-Story.md](Release-2-Database-Migration-Job-Epic-and-Story.md)
	- Added dedicated epic/story R2-S7 for migration job implementation.
	- Includes Sonnet-first execution guidance and Opus escalation point for first-adoption baseline decision.
- File: [Planning/Phase 2/Phase-2-Handoff.md](Phase-2-Handoff.md)
	- Added operational handoff context for fresh-chat continuation.
	- Updated with explicit release status: S1/S3/S5 local-tested not deployed; S2/S6/S7 go-live required.

### Blockers and Gotchas
- Private MySQL connectivity from Cloud Shell/local outside VNet fails by design; not a transient issue.
- MySQL networking mode immutable post-create; cannot toggle current server to public access.
- Terraform does not apply schema scripts; migration execution path required.
- First production migration run risk: historical scripts may already be applied; schema_migrations baseline seeding required before normal apply mode.
- Keep migration mechanism forward-only, idempotent, and secret-safe (no baked credentials in image).

### Next Step Prompt (Copy into next fresh chat)
Resume Phase 2 implementation from [Planning/Phase 2/Release-2-Database-Migration-Job-Epic-and-Story.md](Release-2-Database-Migration-Job-Epic-and-Story.md). Implement R2-S7 end-to-end in code: create tools/db-migrate/migrations with ordered sql files, implement runner with schema_migrations and SEED_BASELINE mode, add Dockerfile.migrate, add Terraform azurerm_container_app_job plus dedicated UAMI and Key Vault wiring in Infrastructure/terraform/container-tier/main.tf, update Infrastructure/terraform/container-tier/deploy.ps1 with migration image build and opt-in RunMigrations trigger, then validate locally against throwaway MySQL for fresh apply, no-op rerun, and baseline behavior. Keep existing container-tier conventions. Preserve release state: R2-S1/R2-S3/R2-S5 local-tested not deployed; R2-S2/R2-S6/R2-S7 needed for go-live.

---

## DEPLOYMENT LOG — 2026-07-06 (R2-S7 delivered + Phase 2 deployed)

### Outcome
- **Phase 2 application containers (UI, API, PDF) deployed to Azure.** UI live at `https://ca-hello-buddy-ui--iiudvb5.salmoncoast-c110d913.ukwest.azurecontainerapps.io`.
- **Database migrated successfully.** All 7 ordered scripts applied (`applied=7 skipped=0 errors=0`), including Release 2 scripts `0060_release2_exercise_audit.sql` and `0070_release2_app_settings.sql`.
- **Migration job** `caj-hellobuddy-migrate` created and working.

### Infrastructure reorganisation (important for new chats)
All migration/infrastructure code was moved OUT of the `Canine Physio Admin/` solution and INTO the `Infrastructure/` folder:
- Migration runner + SQL: `Infrastructure/tools/db-migrate/` (`migrate.sh`, `Dockerfile.migrate`, `migrations/0010..0070`).
- Old `Canine Physio Admin/Dockerfile.migrate` and `Canine Physio Admin/tools/` were deleted.
- `.dockerignore` added at repo root (defense-in-depth for build context packing).

### Deploy commands (verified working)
```powershell
cd Infrastructure\terraform\container-tier
# Full build + deploy + migrate:
.\deploy.ps1 -RunMigrations
# Reuse existing images, skip foundation, deploy apps + run migration:
.\deploy.ps1 -SkipBuild -AppsOnly -RunMigrations
```
- Image tag is derived from git short SHA (this deploy used `a89ca8d`).
- `-RunMigrations` is opt-in; migrations never run automatically.

### Three bugs found and FIXED during deployment
1. **Migrate image build context (Windows long-path).** Building from repo root made `az acr build` tar the whole repo and crash with `WinError 3` on deep Android build artifacts under `Canine Physio App/Archive/...`. FIX: migrate image now builds from its own folder (`Infrastructure/tools/db-migrate/`) with simple relative COPY paths; `deploy.ps1` sets `BuildContext` per image.
2. **MySQL SSL flag.** Debian `default-mysql-client` is the **MariaDB** client, which rejects Oracle's `--ssl-mode=REQUIRED` (`unknown variable 'ssl-mode=REQUIRED'`). FIX: `migrate.sh` now uses `--ssl`.
3. **Broken log-fetch in deploy.ps1.** `az containerapp job execution logs show` is not a valid command. FIX: replaced with a Log Analytics KQL query against `ContainerAppConsoleLogs_CL` (workspace `log-hellobuddy-prod`), with retry to absorb 1–3 min ingestion lag.

### ⚠️ INCIDENT — production DB dropped then rebuilt (resolved)
- **What happened:** First migration run executed `0010_schema_fresh.sql` against the existing `canine_physiotherapy` DB. Its first statement is `DROP DATABASE IF EXISTS canine_physiotherapy;`. Because no `schema_migrations` baseline existed, the runner treated every script as unapplied and started at `0010`, dropping the DB. The run then failed because `0010` also destroyed the `schema_migrations` table (it lived inside the app DB).
- **Impact:** DB was empty/disposable (confirmed with operator), so **no point-in-time restore was needed**. Azure MySQL Flexible Server PITR was available (retention 1 day) had it been required.
- **Root-cause FIX (critical):** `schema_migrations` now lives in a **dedicated `_migrations` metadata database**, not the app DB. `DROP DATABASE` on the app DB can no longer destroy migration tracking. Once a script is recorded it will not re-run.
- **Resolution:** After the fix, a normal run cleanly applied all 7 scripts and re-seeded the DB.

### Remaining design risk (know before next migration)
- `0010_schema_fresh.sql` still contains `DROP DATABASE` and is first in the chain. It is harmless now (recorded as applied; tracking survives in `_migrations`). BUT if this job is ever pointed at a **pre-existing, populated DB that was never baselined**, `0010` would drop it again.
- **Safe procedure for an existing populated DB:** run `SEED_BASELINE=true` FIRST (records `0010`–`0050` without executing), then a normal run applies only newer scripts. Consider excluding `0010` from the runtime migrations folder for extra safety.

### Useful diagnostics
```powershell
# Migration job execution status
az containerapp job execution list --resource-group rg-hellobuddy-prod --name caj-hellobuddy-migrate --output table

# Migration job logs (Log Analytics; customerId for log-hellobuddy-prod = 769fa3f9-50eb-49c2-8d92-23f2bcdb1a8d)
$q = "ContainerAppConsoleLogs_CL | where ContainerGroupName_s startswith 'caj-hellobuddy-migrate' | project TimeGenerated, Log_s | order by TimeGenerated asc | take 100"
az monitor log-analytics query --workspace 769fa3f9-50eb-49c2-8d92-23f2bcdb1a8d --analytics-query $q --output table

# Confirm images in ACR
az acr repository show-tags --name acrhellobuddyprod --repository hello-buddy-migrate --output table
```

### Release status after this deploy
- R2-S7 (migration job): **implemented, deployed, and used successfully.**
- UI/API/PDF containers: **deployed to Azure.**
- Database schema + seed + Release 2 changes: **applied.**

### Scheduled-scaling permission fix (Automation Account → Container Apps)
- **Symptom:** `Scale-ContainersUp`/`Down` runbook failed with `403 AuthorizationFailed` on `Microsoft.App/containerApps/write` for the Automation Account identity (`aa-hellobuddy-prod`, objectId `52d9fcbf-e2bd-4346-9cb8-cdeab544123f`).
- **Cause:** The scaling grant had been created manually and was **scoped to the container app resources**. The two-phase deploy destroys the apps in Phase A (`deploy_container_apps=false`) and recreates them in Phase B, which deleted the app-scoped grant. The Automation identity's MySQL Contributor survived (server not recreated) but its container-app access was lost.
- **Permanent fix (Option B — Terraform-managed, self-healing):** Added to `Infrastructure/terraform/container-tier/main.tf`:
  - `data "azurerm_automation_account" "scaling"` (reads existing account, no hardcoded GUID).
  - `azurerm_role_assignment.automation_ui_contributor` / `_api_` / `_pdf_` — Contributor scoped to each individual container app, count-guarded with `deploy_container_apps` so they are recreated automatically on every deploy.
  - New variable `automation_account_name` (default `aa-hellobuddy-prod`) in `variables.tf`.
- **Applied via targeted apply** (`-target` the 3 role assignments) to avoid touching anything else; apps stayed on the same revisions (no disruption). Verified: identity now holds Contributor on ui/api/pdf apps.
- **Note:** A normal full `deploy.ps1` will also reconcile benign pre-existing drift on the apps (`http_scale_rule` → `custom_scale_rule` schema; `workload_profile_name` cosmetic) — functionally identical scaler, expected, non-destructive.

### PDF service 500 — Chromium headless-mode regression (FIXED & verified)
- **Symptom:** After this deploy, the PDF viewer / Admin settings page returned HTTP 500. PDF service logs showed `PuppeteerSharp.ProcessException: Failed to launch browser!` with `Failed to connect to the bus … /run/dbus/system_bus_socket`.
- **Key diagnostic — NOT a code change:** Git history confirmed the PDF code did not change in this release. `PuppeteerPdfRenderer.cs` last changed 2026-05-28; `Dockerfile.pdf` last changed 2026-06-02. The demo ~1.5 weeks earlier ran on image `e993a5d` (built 2026-06-16), which is still in ACR and works.
- **Root cause — unpinned dependency drift:** `Dockerfile.pdf` installs `chromium` via `apt-get` **unpinned** and uses the floating `mcr.microsoft.com/dotnet/aspnet:9.0` base. Between 16 Jun and 6 Jul, Debian bookworm rolled Chromium forward. In PuppeteerSharp 20.x, `Headless = true` selects the **new** headless mode (`--headless=new`), which starts a full Chrome needing a D-Bus system bus + graphics stack — neither exists in the slim container — so launch crashes. Same code + newer Chromium = regression.
- **Proof of theory:** Rolled the PDF container back to `e993a5d` (`az containerapp update --image …:e993a5d`) → PDF rendered correctly. Confirmed the code was not at fault; the build-time Chromium was.
- **Permanent fix (code):** `PuppeteerPdfRenderer.cs` now sets `HeadlessMode = HeadlessMode.Shell` (classic `--headless`, container-proven, no D-Bus/GPU) and adds `--disable-gpu` to launch args. Immune to future Chromium drift regardless of which version apt pulls.
- **Deployed:** Built + pushed as tag `a89ca8d-headlessfix` and deployed to only the PDF app via `deploy.ps1 -PdfOnly -ImageTag 'a89ca8d-headlessfix'` (no Terraform apply → scaler role assignments untouched, re-verified intact). Live on revision `ca-hello-buddy-pdf--0000003`; PDF rendering confirmed working on the current (drifted) Chromium.
- **Deploy command (surgical, PDF-only):**
  ```powershell
  cd Infrastructure\terraform\container-tier
  .\deploy.ps1 -PdfOnly -ImageTag '<unique-tag>'
  ```
- **Follow-up hardening (optional, not yet done):** Pin the `chromium` apt package version and the base-image digest in `Dockerfile.pdf` for reproducible builds. The `HeadlessMode.Shell` fix already removes the functional exposure, so this is defense-in-depth only.

### Known issue — Exercise image not displaying (OPEN)
- On the Exercise edit screen, the **Current image** and **Selected image** panels do not display the image. Reported after the exercise image source was changed from local managed file lookup to a Google Drive URL.
- Status: **under investigation — not yet fixed.**

