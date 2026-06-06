# Infrastructure Error Log

Last Updated: 2026-06-06

## Status legend

- Open: issue is reproducible and unresolved.
- In Progress: investigation or fix is underway.
- Resolved: fix verified.
- Closed: signed off after validation.

## Error register

| Error ID | Area | Title | Status | Environment | Date Logged |
| --- | --- | --- | --- | --- | --- |
| INF-ERR-001 | Container Tier (Terraform) | Container apps pinned at min replica 1 caused constant idle cost | Resolved | Azure | 2026-06-06 |

## Details

### INF-ERR-001 - Container apps pinned at min replica 1 caused constant idle cost

- Severity: High (cost-impacting)

- Problem statement:
Container app scaling configuration implemented a minimum of 1 replica, which prevented services from scaling down to zero and incurred constant idle costs.

- Required target configuration:
1. Min replicas: 0
2. Max replicas: 1
3. Cooldown period: 120
4. Polling interval: 30

- Root cause:
Terraform container-tier definitions used fixed min replica values of 1 (and max values above 1 for some services), preventing scale-to-zero behavior.

- Fix applied:
1. Updated container-tier root and app modules (API, PDF, UI) to enforce:
   - `min_replicas = 0`
   - `max_replicas = 1`
   - scaler metadata `cooldownPeriod = 120`
   - scaler metadata `pollingInterval = 30`
2. Added validation guardrails in Terraform variables with explicit error messages that fail planning if min replicas is not 0.
3. Added explicit regression error text:
   - "Container configuration previously used a minimum of 1 replica, which prevented containers from scaling down to zero and incurred constant idle costs. Set min replicas to 0."

- Validation evidence:
1. `terraform validate` passed for:
   - `Infrastructure/terraform/container-tier`
   - `Infrastructure/terraform/container-tier/app-api`
   - `Infrastructure/terraform/container-tier/app-pdf`
   - `Infrastructure/terraform/container-tier/app-ui`

- Notes:
The current AzureRM provider schema in this workspace does not expose cooldown/polling directly on `http_scale_rule`; configuration was applied using `custom_scale_rule` metadata compatible with the deployed scaler behavior.
