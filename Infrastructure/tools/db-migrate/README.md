# db-migrate — Database Migration Runner

Applies ordered SQL migrations to the private Azure MySQL Flexible Server from
inside the VNet-integrated Container Apps environment.

**Location:** `Infrastructure/tools/db-migrate/` (infrastructure-related code)

## Structure

```
Infrastructure/tools/db-migrate/
  Dockerfile.migrate  Container image definition (build context: repo root)
  migrate.sh          Shell runner (entrypoint for the container image)
  migrations/         Numbered .sql files applied in filename-sort order
    README.md         File mapping + "how to add a new migration"
```

## How it works

1. Reads `DB_CONNECTION_STRING` (ADO.NET format) from the environment — injected
   at runtime from Key Vault via the job's managed identity; no credentials in
   the image.
2. Creates `schema_migrations` if absent.
3. Applies each `migrations/*.sql` not already recorded, then inserts a tracking
   row. Exits non-zero on any failure.

## Local validation

Requires Docker + a local MySQL (or Docker MySQL container).

```bash
# Start a throwaway MySQL
docker run -d --name mysql-test -e MYSQL_ROOT_PASSWORD=test -p 3306:3306 mysql:8.0

# Build the image (build context = repo root)
docker build -f "Infrastructure/tools/db-migrate/Dockerfile.migrate" \
  -t hello-buddy-migrate:local .

# (a) Fresh apply — should apply all 7 scripts, exit 0
docker run --rm \
  -e "DB_CONNECTION_STRING=Server=host.docker.internal;Port=3306;Database=canine_physiotherapy;Uid=root;Pwd=test" \
  hello-buddy-migrate:local

# (b) No-op re-run — should skip all 7 scripts, exit 0
docker run --rm \
  -e "DB_CONNECTION_STRING=Server=host.docker.internal;Port=3306;Database=canine_physiotherapy;Uid=root;Pwd=test" \
  hello-buddy-migrate:local

# (c) SEED_BASELINE — on a blank tracking table, records without executing
#     (reset tracking first: docker exec mysql-test mysql -uroot -ptest canine_physiotherapy -e "TRUNCATE schema_migrations;")
docker run --rm \
  -e "DB_CONNECTION_STRING=Server=host.docker.internal;Port=3306;Database=canine_physiotherapy;Uid=root;Pwd=test" \
  -e SEED_BASELINE=true \
  hello-buddy-migrate:local

# Clean up
docker rm -f mysql-test
```

Expected results:
- **(a)**: `applied=7 skipped=0 errors=0`
- **(b)**: `applied=0 skipped=7 errors=0`
- **(c)**: `applied=7 skipped=0 errors=0` with "MARK" lines (not "APPLY")

## Azure operations

See [../../Infrastructure/runbooks/migration-runbook.md](../../Infrastructure/runbooks/migration-runbook.md).
