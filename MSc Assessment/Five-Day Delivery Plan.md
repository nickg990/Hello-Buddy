# Hello Buddy Cloud Admin

## Five-Day Delivery Plan — COM712 Assessment 3 (Option C)

This is the day-by-day execution plan for the MSc assessment, paired with [Epic and User Stories.md](Epic%20and%20User%20Stories.md). Use it as the daily heartbeat and the go/no-go decision tool. If a checkpoint slips, follow the cut-scope guidance the same evening rather than borrowing time from the report.

Start date: Tuesday. Hard deadline: COB Sunday.

---

## Day-by-day plan

| Day | Story groups     | Headline goal                                                                                                             |
| --- | ---------------- | ------------------------------------------------------------------------------------------------------------------------- |
| Tue | 3, 4             | MSc init + seed scripts authored. Data-tier Terraform applied. Azure MySQL initialised and seeded.                        |
| Wed | 5                | App built in VS Code against Azure MySQL — case detail, builder, in-page preview, publish to filesystem, PDF service.     |
| Thu | 6, 7             | Dockerfiles, container-platform Terraform, push + deploy all three Container Apps to Azure.                               |
| Fri | 8, 9             | Object-storage Terraform, Blob seeding, wire app to Blob, redeploy. End-to-end smoke + validation block + MySQL lockdown. |
| Sat | 10, 11, start 12 | Telemetry, load test, scaling evidence. Cost + carbon comparisons. Start report draft.                                    |
| Sun | 12               | Finish report. Package submission pack.                                                                                   |

---

## End-of-day go/no-go checkpoints

Each checkpoint must be verifiable, not aspirational. If any fail, apply the contingency cut listed below the same evening.

### Tuesday COB

- `terraform apply` on the data-tier module is green.
- `Canine Physio DB MSc Assessment Init v1.sql` and the MSc seed script have run cleanly against `mysql-hellobuddy-prod`.
- Verification query returns the demo treatment case and at least one programme row.
- MySQL connection string is in Key Vault.

### Wednesday COB

- VS Code app boots against the Azure MySQL connection string.
- Case detail, programme builder, and in-page preview pages render.
- Publish flow produces a PDF written to the local filesystem (Blob wiring is deferred to Friday).
- Application Insights telemetry is wired (configuration-driven), even if the resource is not yet provisioned.

### Thursday COB

- All three Docker images built and pushed to `acrhellobuddyprod` with semantic tags.
- Container-platform Terraform applied; UI reachable on public HTTPS; API and PDF on internal ingress only.
- API → MySQL succeeds in Azure using the Key Vault-backed secret.
- UI → API → PDF round-trip succeeds end-to-end (PDF still written to ephemeral container storage is acceptable here).

### Friday COB

- Object-storage Terraform applied; the four Blob containers exist with the correct access levels.
- Synthetic exercise images and videos seeded.
- App rewired so publish writes the PDF to `published-programmes` and resolves image/video URLs from Blob.
- Cloud smoke test green: UI → builder → in-page preview → publish → PDF in Blob → metadata in MySQL.
- Validation-blocked scenario captured (screenshot + log).
- One diagnosed failure captured (e.g., bad secret).
- MySQL developer-IP firewall rule removed; final smoke test still green.

### Saturday COB

- Telemetry evidence captured: one full publish trace and one failed-scenario trace.
- Load test run; replica scaling evidence captured for UI (and API if possible).
- Seven-day cost comparison drafted with stated assumptions.
- Carbon comparison drafted using a cited method.
- Report draft covers at least: executive summary, scenario/requirements, design justification, architecture diagram, build/deploy.

### Sunday COB

- Report sections complete: network/security/scaling, observability, cost, sustainability, conclusions.
- Decision defences written for the four architecture decisions.
- Submission pack assembled: application source, Dockerfiles, Terraform modules and tfvars (secrets stripped), MSc SQL scripts, evidence artefacts, report.
- Repository can be cloned and rebuilt by another reader with environment-specific changes only.

---

## Contingency cuts (apply same evening if a checkpoint slips)

Apply in order. Each cut is small, defensible in the report, and aligned to the rubric.

1. **Drop drag-and-drop reorder.** Ship the non-drag fallback (alt buttons) only. Reorder is a stretch; the WCAG-compliant fallback is sufficient.
2. **Keep PDFs on ephemeral Container App storage** if Friday slips. Document this as a known limitation and defer Blob wiring (US-25) to Saturday. The architecture narrative still defends the design.
3. **Replace k6 with a scripted PowerShell or curl loop** for the load test. Azure Monitor scaling screenshots are the actual evidence; the load generator is interchangeable.
4. **Cite Microsoft sustainability guidance** for carbon comparison rather than running Cloud Carbon Footprint end-to-end. State assumptions explicitly.
5. **One Azure region only.** Pick the cheapest with Container Apps + MySQL Burstable availability. No comparison shopping.
6. **Seeded single-practitioner auth placeholder.** No Entra ID, no real identity provider work.
7. **Defer broader admin surfaces:** owners/pets CRUD, dashboard analytics, exercise library authoring, full mobile responsiveness beyond what falls out of the layout.

---

## Daily rhythm

- **Morning (30 min):** Read yesterday's checkpoint outcome. Confirm today's headline goal. Update story status in [Epic and User Stories.md](Epic%20and%20User%20Stories.md).
- **End of day (30 min):** Walk the checkpoint list. Decide go / no-go. Apply contingency cuts before going to bed if anything is amber or red. Do not optimistically defer.
- **Report drafting:** Start Wednesday in parallel. Saturday is too late.

---

## Risk register

| Risk                                                                 | Likely day | Mitigation                                                                                       |
| -------------------------------------------------------------------- | ---------- | ------------------------------------------------------------------------------------------------ |
| PuppeteerSharp/Playwright Linux container issues (fonts, deps)       | Wed/Thu    | Build PDF service image first thing Wednesday; smoke test render before wiring the publish flow. |
| MySQL Flexible Server private endpoint and VNet integration          | Tue/Fri    | Start with developer-IP firewall rule; switch to private only on Friday (US-29).                 |
| Terraform Container Apps resource verbosity needing multiple applies | Thu        | Budget two full apply cycles in Thursday's plan; do not start chasing scale rule edge cases.     |
| Report writing eats Sunday                                           | Sat/Sun    | Start the report Wednesday evening; lock decision defences on Saturday.                          |
| Azure region/quota surprise                                          | Tue        | Confirm region availability for MySQL Burstable B1ms and Container Apps before applying.         |

---

## Cross-references

- Backlog: [Epic and User Stories.md](Epic%20and%20User%20Stories.md)
- Architecture: [Option C Architectural Brief and Requirements.md](Option%20C%20Architectural%20Brief%20and%20Requirements.md)
- Resource inventory: [Azure Resource Inventory.md](Azure%20Resource%20Inventory.md)
- Storage layout: [Azure Storage Layout Recommendation.md](Azure%20Storage%20Layout%20Recommendation.md)
- Deployment phases: [Azure Deployment Order Checklist.md](Azure%20Deployment%20Order%20Checklist.md)
