# Release Runbook (Increment 7)

This runbook defines the Release 1 prototype operating flow.

## 1. Preflight

1. Confirm Azure CLI login and subscription context.
2. Confirm required configuration values using the environment matrix.
3. Confirm local dependencies:
   - .NET SDK
   - Docker (for Testcontainers and optional Azurite lane)
   - Terraform (for infra apply mode)

## 2. Local Release Gate

From repo root:

```powershell
./Infrastructure/runbooks/Invoke-ReleaseGate.ps1
```

Optional flags:

```powershell
./Infrastructure/runbooks/Invoke-ReleaseGate.ps1 -SkipIntegration
./Infrastructure/runbooks/Invoke-ReleaseGate.ps1 -RunAzuriteLane
```

Gate policy:

- No cloud deploy unless mandatory lanes pass.

## 3. Deploy

### Full deploy (foundation + build + apps)

```powershell
Set-Location "Infrastructure/terraform/container-tier"
./deploy.ps1
```

### Apps-only deploy

```powershell
Set-Location "Infrastructure/terraform/container-tier"
./deploy.ps1 -AppsOnly
```

### Single-component deploy drill

```powershell
Set-Location "Infrastructure/terraform/container-tier"
./deploy.ps1 -ApiOnly
```

## 4. Post-Deploy Verification

1. Verify revision health:

```powershell
az containerapp revision list -g rg-hellobuddy-prod -n ca-hello-buddy-api -o table
```

2. Verify endpoint health:

- UI: `https://<ui-fqdn>/healthz`
- API: `https://<api-internal-fqdn>/healthz` (internal check path)
- PDF: `https://<pdf-internal-fqdn>/healthz` (internal check path)

3. Re-run cloud regression checks (manual or automated lane) and store evidence.

## 5. Rollback Drill

Use helper script to pin a prior revision.

```powershell
./Infrastructure/runbooks/Invoke-ContainerAppRollback.ps1 -Component api
```

Or explicit revision:

```powershell
./Infrastructure/runbooks/Invoke-ContainerAppRollback.ps1 -Component ui -TargetRevision ca-hello-buddy-ui--abc123
```

After rollback:

1. Re-check revision health.
2. Re-run smoke checks.
3. Capture drill evidence and outcome notes.
