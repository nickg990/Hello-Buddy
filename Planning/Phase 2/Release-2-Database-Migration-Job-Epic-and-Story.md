# Release 2 — Database Migration Job Epic and Story

**Created:** 2026-07-05
**Status:** Draft for execution
**Scope:** Hello Buddy Canine Physiotherapy Admin (Release 2 — infrastructure enablement)
**Implementation model:** AI-led implementation (Sonnet, measured in minutes) + human testing (~1 hour per story to test, identify issues, and re-test). Estimates include a 20% contingency for bug fixes and rework. Azure deployments add an approximate cloud wait/provisioning allowance.

---

## Which AI tool should implement this?

**Recommendation: Claude Sonnet (agent mode) is the right primary tool for this story.** The work is multi-file infrastructure scaffolding (Terraform resource, a small container image, an entrypoint script, `deploy.ps1` wiring, a runbook) against an existing, well-established set of conventions in this repo — exactly the kind of bounded, pattern-following implementation Sonnet does well and fast.

- **Use Sonnet (agent mode)** for the bulk of the implementation: it can read the existing `container-tier/main.tf`, `data-tier/main.tf`, the Dockerfiles, and `deploy.ps1`, then mirror the established patterns (UAMI + role assignment + Key Vault secret reference) with high reliability.
- **Escalate to Claude Opus** for the single deep design decision only: choosing the **first-adoption baseline strategy** so that the two Release 2 scripts (and the earlier canonical scripts already applied to production) are *not* re-run destructively on the first job execution. This is a judgement call about existing production state; a stronger reasoning model is worth using for that one decision, then hand back to Sonnet to implement.
- **Not suitable:** non-agentic / single-file assistants — this spans Terraform, Docker, shell, and PowerShell across multiple folders and needs to read existing files to match conventions.

---

## Epic: Repeatable, Private-Safe Database Migrations via a Container Apps Job

Establish a permanent, low-cost, production-grade mechanism for applying ordered database schema/data changes to the **private, VNet-integrated** Azure MySQL Flexible Server, without standing up jump VMs, VPN gateways, or exposing the database publicly. The mechanism runs *inside* the existing VNet-integrated Container Apps environment (which already has private line-of-sight to MySQL), reads its connection secret from Key Vault via managed identity, applies only the migrations not yet applied, and records what it did — then stops, leaving nothing running to bill.

### Why this is needed (verified constraints)

- The MySQL Flexible Server (`mysql-hellobuddy-prod`) is **private-access / VNet-integrated**. This networking mode is **immutable at creation**: there is no firewall-rule path, and it cannot be toggled to public.
- Azure Cloud Shell and the local dev machine sit **outside** the VNet and therefore cannot resolve or reach the private MySQL endpoint (`ERROR 2005 Unknown server host`).
- Terraform (`data-tier`) creates only an **empty database shell** — the schema and seed data live in ordered `.sql` scripts under `Canine Physio Database/Build and Initialise/` that must be executed from **inside** the VNet.
- The Container Apps environment (`cae-hellobuddy-prod`) is already VNet-integrated, so a **Container Apps Job** launched into it already has private database line-of-sight — no new networking required.

### Epic outcomes

- A single command (`az containerapp job start -g rg-hellobuddy-prod -n caj-hellobuddy-migrate`) applies any pending database migrations to the private MySQL and exits with a clear success/failure code.
- Migrations are **ordered** and **tracked**: a `schema_migrations` table records every applied script, so re-running the job is safe and applies only what is new.
- The job authenticates with **no secrets in the image**: it reads `ConnectionStrings--CaninePhysioDb` from Key Vault via a user-assigned managed identity, mirroring the existing API app wiring.
- The mechanism is **defined in Terraform** (`container-tier`) and integrated into `deploy.ps1`, so it is reproducible and versioned alongside the rest of the infrastructure.
- Nothing is left running after a migration — the job scales to zero, so ongoing cost is effectively nil.

### Epic non-goals

- Replacing local-development schema setup (`reset-local-admin-data.ps1` remains the local flow).
- Automatic rollback / down-migrations (out of scope; forward-only, idempotent scripts are the model — consistent with existing Release 2 scripts).
- Migrating away from raw `.sql` scripts to an ORM migration framework (EF migrations etc.).
- Data backfill or transformation beyond running the authored `.sql` scripts.

---

## Delivery-wide guardrails

- Database schema remains the source of truth; production schema changes ship as **idempotent, re-runnable** `.sql` scripts (consistent with the existing Release 2 scripts).
- The job must **never expose** the database publicly or add firewall rules; it runs inside the existing VNet-integrated Container Apps environment only.
- **No secrets in the image or in Terraform state as plaintext** beyond what already exists — the connection string is read from Key Vault at runtime via managed identity.
- The migration container must reuse the **existing** Key Vault secret name and identity pattern used by `ca-hello-buddy-api`; do not invent a parallel secret path.
- The test/production separation rule stands: the migration job in production must not share state, naming, or secrets with any test environment.
- First run against production must **not** destructively re-apply scripts already applied to the live database — the tracking table must be seeded/baselined to reflect current production state before applying only the genuinely new scripts.

---

## Story Index

| ID | Title | Increment |
|----|-------|-----------|
| R2-S7 | Database migration mechanism via Azure Container Apps Job | Increment 12 |

---

## Increment 12: Repeatable Private-Safe Database Migrations

### Story R2-S7: Database migration mechanism via Azure Container Apps Job

#### a) User story and brief for Sonnet

**User story**
As the person operating Hello Buddy, I want a single, repeatable command that safely applies pending database changes to the private Azure MySQL from inside the VNet, so that I can ship schema/data updates (now and in future phases) reliably and cheaply without VMs, VPNs, or exposing the database publicly.

**Brief for Sonnet**

> **Before implementing:** read the existing files named below so the new resources mirror the established conventions exactly (UAMI + role assignment + Key Vault secret reference, two-phase `deploy.ps1`, Dockerfile build context). Do **not** invent new patterns where an existing one applies.

Current state (verified):
- Infrastructure lives under `Infrastructure/terraform/` in three tiers: `vnet-tier/`, `data-tier/`, `container-tier/`.
- [Infrastructure/terraform/container-tier/main.tf](../../Infrastructure/terraform/container-tier/main.tf) defines three container apps (`ca-hello-buddy-ui/api/pdf`), their user-assigned managed identities (`uami-hellobuddy-ui/api/pdf`), role assignments, and Key Vault secret references. The API app already reads `ConnectionStrings--CaninePhysioDb` from Key Vault via its UAMI — **this is the exact wiring to mirror.**
- [Infrastructure/terraform/data-tier/main.tf](../../Infrastructure/terraform/data-tier/main.tf) creates the empty MySQL database, Key Vault, and the connection-string secret; it does **not** run any `.sql`.
- [Infrastructure/terraform/container-tier/deploy.ps1](../../Infrastructure/terraform/container-tier/deploy.ps1) does a two-phase apply (foundation + apps) and uses `az acr build`.
- Dockerfiles sit at the Admin solution root: `Canine Physio Admin/Dockerfile.api`, `Dockerfile.ui`, `Dockerfile.pdf` — the migration Dockerfile should follow the same location/naming (`Dockerfile.migrate`) and build-context conventions.
- Ordered SQL scripts live under `Canine Physio Database/Build and Initialise/`. The canonical order (from [Infrastructure/local-dev/reset-local-admin-data.ps1](../../Infrastructure/local-dev/reset-local-admin-data.ps1)) is: v2.3 fresh → Day 1 Initialise v2.4 → Increment 8 Login and Attribution → MSc Assessment Seed v1 → Increment 9 Rollback → Release 2 Exercise Audit → Release 2 App Settings. The two Release 2 scripts are idempotent.
- ACR is `acrhellobuddyprod`; resource group `rg-hellobuddy-prod`; Container Apps environment `cae-hellobuddy-prod`.

Implement:

1. **Migrations folder with ordered, numeric-prefixed scripts.** Create `tools/db-migrate/migrations/` containing the ordered `.sql` files renamed with a stable sortable numeric prefix (e.g. `0010_...sql`, `0020_...sql`, … `0060_release2_exercise_audit.sql`, `0070_release2_app_settings.sql`). Filenames are the tracking key. Copy (do not move/delete the originals) from `Canine Physio Database/Build and Initialise/`, preserving exact SQL content. Document the mapping in a short README.

2. **Migration runner + entrypoint.** Under `tools/db-migrate/`, create a small runner that:
   - Reads the MySQL connection string from the environment variable populated by Key Vault (same secret as the API app), never from a baked-in value.
   - Ensures a tracking table exists: `CREATE TABLE IF NOT EXISTS schema_migrations (filename VARCHAR(255) PRIMARY KEY, applied_at DATETIME NOT NULL);`.
   - Lists `migrations/*.sql` sorted by filename, and for each **not** already present in `schema_migrations`, applies it (via the mysql client) inside a transaction where feasible, then records `INSERT INTO schema_migrations(filename, applied_at) VALUES(?, NOW())`.
   - Logs each applied/skipped file and a final summary; exits non-zero on any failure so the Container Apps Job reports failure.
   - Supports a **`SEED_BASELINE` mode** (env flag / arg): on first adoption against an already-populated production database, it records the already-applied scripts into `schema_migrations` **without executing them**, so only genuinely new scripts run thereafter. *(This is the one decision to confirm with Opus/human before the first production run.)*

3. **`Dockerfile.migrate`.** A small image (e.g. `mcr.microsoft.com/mysql`/`debian-slim` + `mysql-client`, or a minimal image with the runner's runtime) that includes the runner and the `migrations/` folder and sets the runner as its entrypoint. Follow the same build-context/location convention as the existing Dockerfiles.

4. **Terraform: `azurerm_container_app_job` `caj-hellobuddy-migrate`** in `container-tier`:
   - `trigger_type = "Manual"`, `replica_timeout`, `replica_retry_limit`, `manual_trigger_config { parallelism = 1 }`.
   - A dedicated user-assigned identity `uami-hellobuddy-migrate` with a **Key Vault Get** role assignment on the Key Vault (mirror the API app's identity + role wiring exactly).
   - The container references the migration image from `acrhellobuddyprod`, with the connection string injected from the Key Vault secret `ConnectionStrings--CaninePhysioDb` via `secret` + `env` using the UAMI (same pattern as the API app).
   - Runs into `cae-hellobuddy-prod` so it inherits VNet integration and private DB line-of-sight.
   - Gate the resource behind the existing `deploy_container_apps`-style variable so plans stay consistent.

5. **`deploy.ps1` integration.** Extend `container-tier/deploy.ps1` to build/push the migration image (`az acr build` for `Dockerfile.migrate`) alongside the apps, and add an **opt-in** `-RunMigrations` switch that, after apply, runs `az containerapp job start -g rg-hellobuddy-prod -n caj-hellobuddy-migrate` and reports the result. Do not run migrations automatically on every deploy.

6. **Runbook.** Add a short migration runbook under `Infrastructure/runbooks/` covering: how to add a new migration (drop a new numbered `.sql`, rebuild image), how to run it (`az containerapp job start ...`), how to check status/logs (`az containerapp job execution list ...`), and the one-time `SEED_BASELINE` first-adoption procedure against existing production.

7. **Tests / validation.** Since this is infrastructure/tooling, validate by: running the runner locally against a throwaway/local MySQL to confirm (a) it creates `schema_migrations`, (b) applies all scripts on a fresh DB, (c) a second run applies nothing, (d) `SEED_BASELINE` records without executing. Document these validation steps in the README (no unit-test project required, but keep the runner logic small and testable).

Constraints:
- Forward-only, idempotent scripts; reuse the existing Key Vault secret and identity pattern — do **not** create a parallel secret path.
- Never expose the database publicly or add firewall rules.
- The first production run **must** be baselined so already-applied scripts are not destructively re-run.
- Keep the image minimal and the job scaled-to-zero when idle (no ongoing cost).

#### b) Estimate (AI implementation + human testing, incl. 20% contingency)

| Phase | Base | With 20% contingency |
|-------|------|----------------------|
| AI: migrations folder + numbered scripts + README | ~10 min | ~12 min |
| AI: runner + entrypoint (tracking table, ordered apply, SEED_BASELINE) | ~20 min | ~24 min |
| AI: `Dockerfile.migrate` | ~8 min | ~10 min |
| AI: Terraform `container_app_job` + UAMI + KV role + secret wiring | ~20 min | ~24 min |
| AI: `deploy.ps1` build/push + `-RunMigrations` switch + runbook | ~15 min | ~18 min |
| Human: local validation (fresh apply, no-op re-run, SEED_BASELINE) | 0.75 h | 0.9 h |
| Azure: build/push image, apply Terraform, first baseline run + verify — hands-on | ~25 min | ~30 min |
| Azure: job provisioning/run wait (mostly unattended) | ~10 min | ~12 min |
| **Total** | **~2.6 h** | **~3.1 h** |

*Azure wait note:* first-time image build + ACR push + Terraform apply of the new job/identity/role, then the initial baseline + migration run, adds ~10–20 min of mostly-unattended cloud wait on top of hands-on time.

---

## Sequencing note

R2-S7 is the delivery vehicle for the database changes introduced by **R2-S1** (Exercise Audit index) and **R2-S3/R2-S5** (`AppSetting`). Recommended order:

1. Implement and validate R2-S7 **locally** against a throwaway MySQL (fresh apply + no-op re-run + `SEED_BASELINE`).
2. Deploy the job to Azure and run the **one-time `SEED_BASELINE`** so `schema_migrations` reflects the scripts already applied to production.
3. Apply the two Release 2 migrations (`0060_release2_exercise_audit.sql`, `0070_release2_app_settings.sql`) as the job's first *real* run.
4. Deploy the R2 application images (API/UI) that depend on those schema changes.
