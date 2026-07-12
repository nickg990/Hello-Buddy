# Database Migration Runbook

**Mechanism:** Azure Container Apps Job `caj-hellobuddy-migrate`  
**Resource group:** `rg-hellobuddy-prod`  
**Story:** R2-S7

---

## How it works

The migration job runs `tools/db-migrate/migrate.sh` inside the VNet-integrated
Container Apps environment (`cae-hellobuddy-prod`), which has private
line-of-sight to the MySQL Flexible Server. It reads its connection string from
Key Vault at runtime via `uami-hellobuddy-migrate` (Key Vault Secrets `Get`).

No credentials are baked into the image. Nothing runs automatically on every
deploy — migrations are triggered explicitly.

---

## Adding a new migration

1. Author the SQL file in `Canine Physio Database/Build and Initialise/` as usual.
2. Copy it to `Canine Physio Admin/tools/db-migrate/migrations/` with the next
   available numeric prefix (e.g. `0080_my_change.sql`).
3. Update `Canine Physio Admin/tools/db-migrate/migrations/README.md` with the mapping.
4. Rebuild and push the image:

   ```powershell
   cd Infrastructure/terraform/container-tier
   .\deploy.ps1 -MigrateOnly
   ```

5. Run the migration (see below).

---

## Running a migration (normal)

```powershell
# Option A — via deploy.ps1 (builds + applies Terraform + runs migrations)
.\deploy.ps1 -RunMigrations

# Option B — trigger manually (image already pushed, Terraform already applied)
az containerapp job start `
    --resource-group rg-hellobuddy-prod `
    --name caj-hellobuddy-migrate
```

> **Note — standalone `-RunMigrations`:** when `-RunMigrations` is passed on its
> own (no build/deploy switches such as `-ApiOnly`, `-AppsOnly`, `-FoundationOnly`
> or `-ExerciseImport`), `deploy.ps1` now **skips the Terraform apply and image
> builds entirely** and just starts + tails the existing job. This is the fast
> "run migrations now" path. To force a full build + two-phase apply before
> running, combine it with a deploy switch (e.g. `-MigrateOnly -RunMigrations`).

---

## Checking status and logs

```bash
# List recent executions
az containerapp job execution list \
    --resource-group rg-hellobuddy-prod \
    --name caj-hellobuddy-migrate \
    --output table

# Show status of a specific execution
az containerapp job execution show \
    --resource-group rg-hellobuddy-prod \
    --name caj-hellobuddy-migrate \
    --job-execution-name <execution-name>

# Tail logs for a specific execution
az containerapp job execution logs show \
    --resource-group rg-hellobuddy-prod \
    --name caj-hellobuddy-migrate \
    --job-execution-name <execution-name>
```

A successful run ends with:

```
==> Migration run complete: applied=N skipped=M errors=0
```

A failed run exits non-zero. The Container Apps Job status shows `Failed`.
Check logs for the `ERROR applying <filename>` line to identify the failing script.

---

## First-adoption baseline (one-time, production only)

**Context:** The production database (`mysql-hellobuddy-prod`) was seeded using
the canonical scripts before `schema_migrations` tracking was introduced. The
first job run must record those already-applied scripts **without re-executing
them**, then apply only the genuinely new Release 2 scripts.

### Decision: confirmed baseline approach

Scripts `0010` through `0050` (schema fresh → MSc seed → Inc 9 rollback) are
already applied to production. Scripts `0060_release2_exercise_audit.sql` and
`0070_release2_app_settings.sql` are **not yet** applied.

### Steps

1. **Before applying**: verify the existing schema matches the baseline.

2. Set `SEED_BASELINE=true` for a one-time job run to record `0010`–`0050`
   without executing them:

   ```powershell
   # Option A — set the Terraform variable, apply, run job, then reset
   cd Infrastructure/terraform/container-tier
   .\deploy.ps1 -SkipBuild -RunMigrations
   # But first: terraform apply with -var migrate_seed_baseline=true
   terraform apply -auto-approve `
       -var "deploy_container_apps=true" `
       -var "migrate_seed_baseline=true" `
       -var "migrate_app_image=acrhellobuddyprod.azurecr.io/hello-buddy-migrate:<tag>"

   az containerapp job start `
       --resource-group rg-hellobuddy-prod `
       --name caj-hellobuddy-migrate
   ```

   Expected output: 5 × `MARK` lines (0010–0050), then exit 0.

3. **Reset** `migrate_seed_baseline` back to `"false"` and re-apply:

   ```powershell
   terraform apply -auto-approve `
       -var "deploy_container_apps=true" `
       -var "migrate_seed_baseline=false" `
       -var "migrate_app_image=acrhellobuddyprod.azurecr.io/hello-buddy-migrate:<tag>"
   ```

4. **Run again** (normal mode) to apply the Release 2 scripts:

   ```powershell
   az containerapp job start `
       --resource-group rg-hellobuddy-prod `
       --name caj-hellobuddy-migrate
   ```

   Expected output: 5 × `SKIP`, 2 × `APPLY` (0060, 0070), exit 0.

---

## Validate `schema_migrations` content

Connect via a Container Apps exec shell (or temporary container app):

```sql
SELECT filename, applied_at FROM schema_migrations ORDER BY filename;
```

Expected after baseline + Release 2 apply:

| filename | applied_at |
|----------|------------|
| 0010_schema_fresh.sql | ... |
| 0020_day1_initialise.sql | ... |
| 0030_increment8_login_attribution.sql | ... |
| 0040_msc_assessment_seed.sql | ... |
| 0050_increment9_rollback_email_audit.sql | ... |
| 0060_release2_exercise_audit.sql | ... |
| 0070_release2_app_settings.sql | ... |

---

## Exercise library import

The exercise library (categories, exercises, per-step instructions) is loaded
through the **same migrate job** via a re-runnable import mode, driven from
`deploy.ps1 -ExerciseImport`. It is separate from the numbered forward-only
migrations: the source of truth is
`Infrastructure/tools/db-migrate/exercise-import/exercises.md`, which is compiled
to `exercise-import.sql` and baked into the migrate image.

### Modes

| `-ImportMode` | Behaviour |
|---------------|-----------|
| `update` (default) | Upsert only. Categories upsert on `CategoryKey`, exercises on `ExerciseKey`, instructions rebuilt per exercise. **Nothing is deleted.** |
| `replace` | Deletes every `Exercise` **not referenced** by any `SessionExercise` row, then applies the upsert. Exercises currently in use by a programme are retained and overwritten (FK-safe guarded delete — a blanket delete would fail on `FK ... ON DELETE RESTRICT`). |

### What `deploy.ps1 -ExerciseImport` does

1. Regenerates `exercise-import.sql` from `exercises.md`.
2. Rebuilds and pushes the migrate image (unless `-SkipBuild`).
3. `terraform apply` with `exercise_import_mode=<update|replace>`, pinning the
   web apps to their **live** images so they are never blanked.
4. Starts + tails the job.
5. `terraform apply` again to reset `exercise_import_mode=off`, so a later
   `-RunMigrations` is unaffected. This reset runs even if the job fails.

### Commands

```powershell
cd Infrastructure/terraform/container-tier

# Upsert (safe, additive) — nothing removed
.\deploy.ps1 -ExerciseImport -ImportMode update

# Replace — remove unreferenced exercises, then upsert
.\deploy.ps1 -ExerciseImport -ImportMode replace
```

The job log reports the `Exercise` row count before and after the import.

---

## Rollback

There is **no automated rollback**. Scripts are forward-only and idempotent.

If a migration causes a regression:
1. Author a corrective SQL script (e.g. `0080_fix_...sql`).
2. Follow the "Adding a new migration" steps above.
3. Do not delete or rename already-applied files.
