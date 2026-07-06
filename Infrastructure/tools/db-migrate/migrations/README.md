# Migration Files

Each file is a numbered copy of the canonical SQL script from
`Canine Physio Database/Build and Initialise/`. The numeric prefix controls
execution order. Filenames are the tracking key in `schema_migrations`.

**Do not rename files after they have been applied to any environment.**

## File Mapping

| File | Source |
|------|--------|
| `0010_schema_fresh.sql` | `Canine Physio DB Scripts v2.3 (fresh).sql` |
| `0020_day1_initialise.sql` | `Canine Physio DB Day 1 Initialise v2.4.sql` |
| `0030_increment8_login_attribution.sql` | `Canine Physio DB Scripts - Increment 8 - Login and Attribution.sql` |
| `0040_msc_assessment_seed.sql` | `Canine Physio DB MSc Assessment Seed v1.sql` |
| `0050_increment9_rollback_email_audit.sql` | `Canine Physio DB Scripts - Increment 9 Rollback - Remove Programme Email Send Audit.sql` |
| `0060_release2_exercise_audit.sql` | `Canine Physio DB Scripts - Release 2 - Exercise Audit.sql` |
| `0070_release2_app_settings.sql` | `Canine Physio DB Scripts - Release 2 - App Settings.sql` |

## Adding a New Migration

1. Create the SQL file in `Canine Physio Database/Build and Initialise/` as usual.
2. Copy it here with the next available prefix (e.g. `0080_...sql`).
3. Rebuild the migration image: `cd Infrastructure/terraform/container-tier && .\deploy.ps1 -MigrateOnly`
4. Run the job: `az containerapp job start -g rg-hellobuddy-prod -n caj-hellobuddy-migrate`

See [../../../Infrastructure/runbooks/migration-runbook.md](../../../Infrastructure/runbooks/migration-runbook.md) for full operations guidance.
