# Submission Checklist — COM712 Assessment 3 (Option C)

**Deadline:** Monday 2 June 2026
**Owner:** Nick
**Last updated:** 28 May 2026

This checklist drives the remaining work between the Day 4b "all working" state and submission. Tick boxes as you go; commit this file at the end so the submission package shows a clean ledger.

Cross-references:

- [DecisionLog.md](DecisionLog.md) — DEC-001 … DEC-012
- [Technical Debt/TD-001 Admin Standards Deviations.md](../Technical%20Debt/TD-001%20Admin%20Standards%20Deviations.md)
- [Azure Architecture Diagram Specification.md](Azure%20Architecture%20Diagram%20Specification.md) — canonical labels and boundary rules
- [Azure Resource Inventory.md](Azure%20Resource%20Inventory.md)
- [Azure Production Architecture Diagram.md](Azure%20Production%20Architecture%20Diagram.md)
- [Demo Video Filming Script.md](Demo%20Video%20Filming%20Script.md)

---

## 0. Pre-flight (do first, ~5 min)

- [ ] Warm the UI: `GET https://<ui-fqdn>/healthz` → expect `{"status":"ok"}`. If cold-start latency > 3 s, hit it again.
- [ ] Confirm both API and PDF replicas are up: `az containerapp replica list -g rg-hellobuddy-prod -n ca-hello-buddy-api -o table` and same for `-pdf`.
- [ ] Confirm `terraform plan` is clean from `Infrastructure/terraform/container-platform/`: zero changes expected.
- [ ] Create the evidence folder: `MSc Assessment/Evidence/` (it does not exist yet).

---

## 1. Evidence screenshots (~45 min in one sweep)

Save under `MSc Assessment/Evidence/` with the numeric filenames below. PNG, full Portal chrome visible (resource group breadcrumb in the top bar is good context for the marker).

| #   | Filename                                    | Where to capture                                                                                                                                                                                                                                         | What must be visible                                                                                                                                     |
| --- | ------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 01  | `01-resource-group-manifest.png`            | Portal → Resource groups → `rg-hellobuddy-prod` → Overview                                                                                                                                                                                               | Full resource list (Container Apps Env, 3× Container Apps, ACR, KV, Storage, App Insights, Log Analytics, 3× UAMI, MySQL Flexible)                       |
| 02  | `02-container-app-ui.png`                   | Portal → `ca-hello-buddy-ui` → Overview                                                                                                                                                                                                                  | Running revision, ingress URL, replica count                                                                                                             |
| 03  | `03-container-app-api.png`                  | Portal → `ca-hello-buddy-api` → Overview                                                                                                                                                                                                                 | Running revision, **no public URL**, replica count                                                                                                       |
| 04  | `04-container-app-pdf.png`                  | Portal → `ca-hello-buddy-pdf` → Overview                                                                                                                                                                                                                 | Running revision, **no public URL**, fixed 1 replica                                                                                                     |
| 05  | `05-ingress-ui-external.png`                | `ca-hello-buddy-ui` → Ingress                                                                                                                                                                                                                            | "Accepting traffic from anywhere" = **on**, target port 8080                                                                                             |
| 06  | `06-ingress-api-internal.png`               | `ca-hello-buddy-api` → Ingress                                                                                                                                                                                                                           | "Accepting traffic from anywhere" = **off** (internal only), target port 8080                                                                            |
| 07  | `07-ingress-pdf-internal.png`               | `ca-hello-buddy-pdf` → Ingress                                                                                                                                                                                                                           | "Accepting traffic from anywhere" = **off** (internal only), target port 8080                                                                            |
| 08  | `08-appinsights-end-to-end-transaction.png` | App Insights → Transaction search → click a recent publish operation → End-to-end transaction view                                                                                                                                                       | **Centrepiece.** All four hops under one OperationId: browser → ui → api → pdf, plus api → blob. Span durations visible.                                 |
| 09  | `09-appinsights-application-map.png`        | App Insights → Application map                                                                                                                                                                                                                           | Three nodes (`hello-buddy-ui`, `hello-buddy-api`, `hello-buddy-pdf`) plus blob dependency                                                                |
| 10  | `10-replicas-during-load.png`               | `ca-hello-buddy-ui` → Revisions and replicas → during a quick load (open the UI in 3 tabs, hit publish in one)                                                                                                                                           | More than 1 UI replica if scale rule fired; otherwise capture during normal traffic                                                                      |
| 11  | `11-keyvault-secrets.png`                   | `kv-hellobuddy-prod` → Secrets                                                                                                                                                                                                                           | `ConnectionStrings--CaninePhysioDb`, `ApplicationInsights--ConnectionString`                                                                             |
| 12  | `12-containerapp-secrets-pane.png`          | `ca-hello-buddy-api` → Secrets                                                                                                                                                                                                                           | KV-referenced secrets shown as `keyvaultref:...`                                                                                                         |
| 13  | `13-iam-storage-published-programmes.png`   | Storage account → Containers → `published-programmes` → Access Control (IAM) → Role assignments                                                                                                                                                          | `uami-hellobuddy-api` with Storage Blob Data Contributor at container scope                                                                              |
| 14  | `14-iam-storage-dataprotection-keys.png`    | Storage account → Containers → `dataprotection-keys` → Access Control (IAM) → Role assignments                                                                                                                                                           | `uami-hellobuddy-ui` with Storage Blob Data Contributor at container scope                                                                               |
| 15  | `15-iam-keyvault.png`                       | `kv-hellobuddy-prod` → Access Control (IAM)                                                                                                                                                                                                              | `uami-hellobuddy-api` with Key Vault Secrets User                                                                                                        |
| 16  | `16-iam-acr.png`                            | `acrhellobuddyprod` → Access Control (IAM)                                                                                                                                                                                                               | All three UAMIs with AcrPull                                                                                                                             |
| 17  | `17-log-analytics-multi-app-query.png`      | Log Analytics workspace → Logs → run: `ContainerAppConsoleLogs_CL \| where ContainerAppName_s in ("ca-hello-buddy-ui","ca-hello-buddy-api","ca-hello-buddy-pdf") \| summarize count() by ContainerAppName_s, bin(TimeGenerated, 5m) \| render timechart` | Three series, one per app                                                                                                                                |
| 18  | `18-blob-with-sas-working.png`              | Browser tab showing a working SAS download URL (paste address bar) plus the PDF rendering                                                                                                                                                                | Full URL with `sv=`, `se=`, `sig=`; rendered PDF visible                                                                                                 |
| 19  | `19-cost-analysis-7-days.png`               | Subscription → Cost Management → Cost analysis → scope to `rg-hellobuddy-prod` → last 7 days, group by Service                                                                                                                                           | Cost bars by service. Capture **before any teardown**.                                                                                                   |
| 20  | `20-terraform-plan-clean.png`               | Terminal screenshot from `Infrastructure/terraform/container-platform/` running `terraform plan`                                                                                                                                                         | "No changes. Your infrastructure matches the configuration."                                                                                             |
| 21  | `21-sas-expired-403.png`                    | Take the working SAS URL from #18, hand-edit `se=` to a past timestamp (e.g. yesterday), paste into a fresh browser tab                                                                                                                                  | HTTP 403 / `AuthenticationFailed` `Signed expiry time ... must be after signed start time` or similar. Evidences DEC-011 server-side expiry enforcement. |
| 22  | `22-blob-anonymous-409.png`                 | Take the working blob URL from #18, strip the entire `?sv=...` SAS query string, paste into a fresh browser tab                                                                                                                                          | HTTP 409 `PublicAccessNotPermitted`. Evidences private-container posture.                                                                                |
| 23  | `23-api-401-no-header.png`                  | From inside the UI container (`az containerapp exec -g rg-hellobuddy-prod -n ca-hello-buddy-ui --command "/bin/sh"`), `curl -i http://ca-hello-buddy-api/cases` with **no** `X-Practitioner-Id`, then again **with** the header                          | First call returns 401, second returns 200. Evidences DEC-010 auth model.                                                                                |
| 24  | `24-api-internal-boundary.png`              | From your laptop (outside the Container Apps Env), `curl -v https://ca-hello-buddy-api.<envsuffix>.uksouth.azurecontainerapps.io/healthz`                                                                                                                | DNS resolution failure **or** connection refused / timeout. Evidences the internal-ingress boundary is real, not just labelled.                          |
| 25  | `25-rbac-ui-uami-list.png`                  | Terminal: `az role assignment list --assignee <ui-uami-principal-id> --all -o table` (get principalId from `az identity show -g rg-hellobuddy-prod -n uami-hellobuddy-ui --query principalId -o tsv`)                                                    | Exactly two rows: AcrPull (ACR scope) + Storage Blob Data Contributor (dataprotection-keys container scope). No other grants.                            |
| 26  | `26-mysql-firewall-private.png`             | Portal → MySQL Flexible Server → Networking                                                                                                                                                                                                              | Public access **disabled** / Private endpoint only / no allow-all firewall rule. Evidences data-tier network posture and Day 4b cleanup.                 |

**Quality bar:** every screenshot must show enough surrounding chrome (resource name in breadcrumb, blade title) that the marker can identify what they're looking at without your narration.

**Distinction-tier rationale:** items 21–26 evidence _behaviour under conditions_ (expired credential rejected, anonymous access blocked, missing auth header rejected, network boundary holds, RBAC genuinely least-privilege, data tier privately networked). Items 01–20 evidence _configuration_; items 21–26 evidence that the configuration actually does what the report claims.

---

## 1b. Stretch evidence (distinction tier — if time allows)

These add depth but are not required. Pick up only after Sections 1, 2 are complete.

| #   | Filename                                 | Where to capture                                                                                                                                                                                          | What must be visible                                                                                                                 |
| --- | ---------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| 27  | `27-revision-history.png`                | `ca-hello-buddy-ui` → Revision management                                                                                                                                                                 | v1 and v2 revisions visible, traffic split (100% to v2), shows zero-downtime swap capability. Ties to DEC-012 (antiforgery context). |
| 28  | `28-loganalytics-single-operationid.png` | Log Analytics → run: `union AppRequests, AppDependencies \| where OperationId == "<paste id from #08>" \| project TimeGenerated, AppRoleName, Name, DurationMs, ResultCode \| order by TimeGenerated asc` | Rows from all three apps under one OperationId. Proves correlation at the data layer, not just the visualisation in #08.             |
| 29  | `29-appinsights-failures-pane.png`       | Force a 500 (e.g. temporarily set API env var `ConnectionStrings__CaninePhysioDb` to garbage in a throwaway revision), hit publish, capture App Insights → Failures, then revert                          | Failed request + exception detail captured. Evidences observability catches problems. **Revert the env var before filming.**         |
| 30  | `30-cost-by-resource.png`                | Cost Management → Cost analysis → scope `rg-hellobuddy-prod` → group by **Resource** (not Service)                                                                                                        | Per-Container-App cost breakdown. Pairs with #19 to show component-level cost awareness.                                             |

---

## 2. Diagram & inventory update (~30 min total)

### 2a. Azure Production Architecture Diagram

- [ ] Open [Azure Production Architecture Diagram.md](Azure%20Production%20Architecture%20Diagram.md)
- [ ] Cross-check every label against [Azure Architecture Diagram Specification.md](Azure%20Architecture%20Diagram%20Specification.md) — labels must match **verbatim**
- [ ] Confirm three Container App nodes are drawn (UI / API / PDF), not one
- [ ] Confirm internal-only boundary is drawn around API + PDF
- [ ] Confirm three UAMI nodes with arrows to their respective RBAC scopes
- [ ] Confirm the `dataprotection-keys` blob container appears (added in DEC-012)
- [ ] Render the diagram and eyeball it

### 2b. Azure Resource Inventory

- [ ] Open [Azure Resource Inventory.md](Azure%20Resource%20Inventory.md)
- [ ] Verify all three Container Apps listed (`ca-hello-buddy-ui`, `-api`, `-pdf`)
- [ ] Verify all three UAMIs listed (`uami-hellobuddy-ui`, `-api`, `-pdf`) with their RBAC grants
- [ ] Verify both blob containers listed (`published-programmes`, `dataprotection-keys`)
- [ ] Verify ACR image list reflects `hello-buddy-ui:v2`, `hello-buddy-api:v1`, `hello-buddy-pdf:v1`

---

## 3. Demo video (per filming script)

- [ ] Re-read [Demo Video Filming Script.md](Demo%20Video%20Filming%20Script.md) end-to-end before pressing record
- [ ] **Warm the UI** with a `/healthz` hit immediately before filming each take
- [ ] During the App Insights segment: trigger the publish action, then narrate for **at least 60 seconds** before switching to the trace view (App Insights ingestion lag)
- [ ] Film once end-to-end, review, then re-film weak sections only
- [ ] Export, check audio levels, check screen resolution legibility
- [ ] Save final video alongside this checklist (under `MSc Assessment/` or `MSc Assessment/Evidence/`)

---

## 4. Submission package

- [ ] All evidence files committed
- [ ] Final architecture diagram committed
- [ ] Final Resource Inventory committed
- [ ] [DecisionLog.md](DecisionLog.md) committed (DEC-001 … DEC-012)
- [ ] [Technical Debt/TD-001 Admin Standards Deviations.md](../Technical%20Debt/TD-001%20Admin%20Standards%20Deviations.md) committed
- [ ] This checklist ticked through and committed
- [ ] Report document embeds the diagram + key screenshots
- [ ] Repo tag: `git tag msc-submission-v1 -m "COM712 Assessment 3 submission"` then `git push origin msc-submission-v1`
- [ ] Submit per assessment portal instructions

---

## 5. Post-submission

- [ ] 2 June: Re-read TD-001 review checkpoint line; confirm C1–C6 closed, C7 still open
- [ ] Decide retention window for the live Azure environment (cost vs. marker re-check)
- [ ] If teardown chosen: `terraform destroy` from `Infrastructure/terraform/container-platform/` then `data-platform/`. **Do not destroy until grades are released.**
