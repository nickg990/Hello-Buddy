# Hello Buddy â€” Cloud Admin

Containerised canine-physiotherapy admin running on Azure Container Apps: a public UI, an internal API, and an internal PDF service, backed by managed MySQL, Blob Storage, Key Vault, and Application Insights. Data is synthetic.

## Architecture at a glance

- `ca-hello-buddy-ui` â€” external ingress, the only public surface.
- `ca-hello-buddy-api` â€” internal-only, reached via the UI.
- `ca-hello-buddy-pdf` â€” internal-only, renders programme PDFs.
- **MySQL Flexible Server** (private), **Blob Storage** (`published-programmes`, SAS download), **Key Vault** (secrets via managed identity), **App Insights** (telemetry).

## Quick start

1.  **Prerequisites:** Azure CLI, Terraform, and an Azure subscription (Docker optional for local-only image workflows).
2.  **Provision** (in tier order): `terraform apply` for the network tier, then MySQL, then the container platform.
3.  **Build & push images:** `az acr build` for the `ui`, `api`, and `pdf` images to `acrhellobuddyprod`.
4.  **Seed & verify:** run the database seed, then warm the UI: `GET https://<ui-fqdn>/healthz` â†’ `{"status":"ok"}`.
5.  **Use it:** open the UI URL â†’ seeded case â†’ programme builder â†’ preview â†’ publish.

## Increment 7 operational runbooks

Release-ready operational assets are in `Infrastructure/runbooks`:

- `Environment-Configuration-Matrix.md` — required settings and validation checklist.
- `Release-Runbook.md` — local gate, deploy flow, post-deploy checks, rollback drill.
- `Backup-Restore-Runbook.md` — local reset and Azure point-in-time restore drill.
- `Invoke-ReleaseGate.ps1` — orchestrates local release-gate test lanes.
- `Invoke-ContainerAppRollback.ps1` — component rollback drill helper.
- `Invoke-MySqlPointInTimeRestore.ps1` — MySQL restore helper for recovery rehearsal.

## Local Azurite-first storage (CR003-I3)

Local development now defaults to Azurite for blob-backed functionality.

Local tooling canonical path: `Infrastructure/local-dev` is the only supported local tooling folder. The legacy `docal-dev` folder has been removed.

- Script: `Infrastructure/local-dev/run-local-admin-stack.ps1`
- Default behavior: starts Azurite container (`hellobuddy-azurite`) and then UI/API/PDF.
- API Development config uses:
	- `Storage:Mode=Azurite`
	- `Storage:ConnectionString=UseDevelopmentStorage=true`
	- `Storage:PublishedProgrammesContainer=published-programmes`

Exercise media governance policy (CR002-I3 hardening):

- `Storage:ExerciseMediaMalwareScanMode`:
	- `StubAllowAll` (default stub hook, logs scan call)
	- `Disabled`
	- `Required` (startup fails until a real scanner integration is added)
- `Storage:ExerciseMediaOrphanCleanupMode`:
	- `Retain`
	- `DeleteManagedOrphans` (delete previous managed `exercise-media/...` object when image URL changes)
- `Storage:ExerciseMediaMaxBytes` must be > 0.
- `Storage:ExerciseMediaBaseUrl` must be absolute when using `Azurite` or `Azure` storage mode.

Optional switches:

- `-SkipAzurite` to skip emulator startup (requires explicit filesystem mode config to avoid startup mismatch).
- `-ResetAzurite` to recreate the Azurite container from scratch.

Manual teardown/reset (clear local manual test data back to baseline seed):

```powershell
Set-Location "C:\Projects\Hello Buddy\Infrastructure\local-dev"
.\teardown-local-admin-data.ps1

use this command as a fallback if necessary: .\teardown-local-admin-data.ps1 -Password <local database password> -Force
```

Notes:

- Use `-Force` to skip confirmation prompt.
- Uses local `mysql.exe` when available, otherwise falls back to Docker mysql client.

## Container-tier deployment script

Use the combined deployment script at:

- `Infrastructure/terraform/container-tier/deploy.ps1`

Run from:

- `Infrastructure/terraform/container-tier`

### Command line syntax

```powershell
.\deploy.ps1 [-ImageTag <tag>] [-SubnetAppsId <subnet-resource-id>] [-SkipBuild] [-FoundationOnly] [-AppsOnly]
```

### Arguments

- `-ImageTag <tag>`
	- Overrides the image tag for all three images (`ui`, `api`, `pdf`).
	- Default: git short SHA from `git rev-parse --short HEAD`.
	- Fallback if git SHA is unavailable: timestamp `yyyyMMddHHmm`.

- `-SubnetAppsId <subnet-resource-id>`
	- Overrides `subnet_apps_id` passed to Terraform in both Phase A and Phase B.
	- Use when pinning to a specific delegated Container Apps subnet.

- `-SkipBuild`
	- Skips all `az acr build` image builds.
	- Terraform still runs and updates Container Apps to the image refs for the selected tag.

- `-FoundationOnly`
	- Runs only Phase A Terraform (`deploy_container_apps=false`), then exits.
	- Useful for foundation/resource setup without touching app revisions.

- `-AppsOnly`
	- Skips Phase A foundation apply.
	- Runs image build (unless `-SkipBuild`) and Phase B Terraform (`deploy_container_apps=true`).
	- Preferred for routine app redeploys when foundation already exists.

### Run options (copy/paste)

- Full two-phase deploy (foundation + build + apps):

```powershell
Set-Location "C:\Projects\Hello Buddy\Infrastructure\terraform\container-tier"
.\deploy.ps1
```

- Apps-only redeploy (skip foundation, rebuild images, update apps):

```powershell
Set-Location "C:\Projects\Hello Buddy\Infrastructure\terraform\container-tier"
.\deploy.ps1 -AppsOnly
```

- Apps-only using already-pushed images (no rebuild):

```powershell
Set-Location "C:\Projects\Hello Buddy\Infrastructure\terraform\container-tier"
.\deploy.ps1 -AppsOnly -SkipBuild
```

- Foundation-only apply:

```powershell
Set-Location "C:\Projects\Hello Buddy\Infrastructure\terraform\container-tier"
.\deploy.ps1 -FoundationOnly
```

- Deploy with explicit image tag:

```powershell
Set-Location "C:\Projects\Hello Buddy\Infrastructure\terraform\container-tier"
.\deploy.ps1 -AppsOnly -ImageTag v1
```

- Deploy with explicit subnet override:

```powershell
Set-Location "C:\Projects\Hello Buddy\Infrastructure\terraform\container-tier"
.\deploy.ps1 -AppsOnly -SubnetAppsId "/subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.Network/virtualNetworks/<vnet>/subnets/<subnet-apps>"
```

### Notes

- `-AppsOnly` and `-FoundationOnly` are mutually exclusive in intent; use one mode at a time.
- For normal output, do not pipe with `2>&1 | Select-Object ...` when running this script in PowerShell because stderr redirection can promote CLI warnings into terminating errors under `$ErrorActionPreference = "Stop"`.
- `az acr build` runs in ACR (remote build), so local Docker daemon is not required for this script path.

## Troubleshooting

- **Cold-start lag:** hit `/healthz` twice to warm the UI before a demo.
- **API returns 401:** requests need the `X-Practitioner-Id` header (the UI injects it; see DEC-010). Calling the API directly without it is rejected by design.
- **Case load returns 500:** usually an empty database or a bad `ConnectionStrings` secret â€” reseed, or fix the Key Vault value.
- **SAS download returns 403:** the link has expired (30-minute TTL by design, DEC-011) â€” republish the programme to regenerate a fresh link.

### Container App startup / hanging UI pages

When UI pages hang on every API-dependent screen (owners, pets, cases, exercise library), the UI is almost always waiting on a crash-looping API container. Use these three commands in order to triage:

1. `az containerapp revision list -g rg-hellobuddy-prod -n ca-hello-buddy-api -o table`
   Instantly tells you which app is unhealthy. `Activating` + `HealthState: None` = startup crash loop. `Healthy` + `Running` = app is fine, look elsewhere.
2. `az containerapp logs show -g rg-hellobuddy-prod -n ca-hello-buddy-api --revision <revision-name> --tail 100 --type console`
   Surfaces the actual exception from stdout â€” typically an unhandled `InvalidOperationException` from `Program.cs` startup validation.
3. `az containerapp revision show -g rg-hellobuddy-prod -n ca-hello-buddy-api --revision <revision-name> --query "properties.template.containers[0].env" -o table`
   Confirms whether config drift between the repo and the live revision is the cause. If a required env var is missing here, the Terraform definition wasn't applied (or was edited in the wrong file â€” the live container apps are defined in `container-tier/main.tf`, not in the unused `app-api/` / `app-ui/` / `app-pdf/` submodules).

## Cost estimate

A lean band of roughly **Â£18â€“Â£24/month**, with headroom to the ~Â£30/month design budget. Container Apps scale to zero / down when idle, so the bill tracks ~40 active demand-hours/week rather than the 168 idle hours an always-on VM would charge for. Scheduling MySQL to office-hours-only in development (DEC-002) cuts data-tier compute by ~60%.

## Carbon estimate

Directional only, using the Cloud Carbon Footprint methodology and Microsoftâ€™s Emissions Impact Dashboard guidance. The same property that makes the container model cheaper â€” not paying for idle compute â€” also lowers its energy draw versus an always-on VM serving the same intermittent demand. Exact emissions depend on the data-centre grid mix and PUE, which are not transparent at tenant level, so this is a directional claim, not an exact kilogram figure.

