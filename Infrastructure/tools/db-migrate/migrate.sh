#!/bin/sh
# migrate.sh — Hello Buddy database migration runner
#
# Reads DB_CONNECTION_STRING (ADO.NET format injected from Key Vault at runtime)
# to connect to MySQL, then applies any pending *.sql files under migrations/
# in filename-sort order.
#
# Modes:
#   Normal (default) — applies each migration not yet recorded in schema_migrations.
#   SEED_BASELINE=true — records all migrations as applied WITHOUT executing them.
#                        Use once against an already-populated production database
#                        to establish the baseline before applying only new scripts.
#
# Environment variables:
#   DB_CONNECTION_STRING  Required. ADO.NET/MySQL connection string from Key Vault.
#                         Expected keys: Server, Port (optional, default 3306),
#                         Database, Uid, Pwd, SslMode (optional).
#   SEED_BASELINE         Optional. Set to "true" to run in baseline mode.
#   MIGRATIONS_DIR        Optional. Override path to migrations folder (default: ./migrations).

set -e

MIGRATIONS_DIR="${MIGRATIONS_DIR:-$(dirname "$0")/migrations}"
SEED_BASELINE="${SEED_BASELINE:-false}"
# RESET_TRACKING=true drops the metadata database so ALL migrations are treated
# as unapplied and re-run from scratch. Because 0010_schema_fresh.sql performs
# DROP DATABASE + CREATE DATABASE on the application DB, a reset run rebuilds the
# entire schema cleanly. Destructive: only use on a disposable/seed database.
RESET_TRACKING="${RESET_TRACKING:-false}"

# ---------------------------------------------------------------------------
# Parse an ADO.NET-style connection string key (case-insensitive).
# Usage: get_param KEY
# ---------------------------------------------------------------------------
get_param() {
    key="$1"
    echo "$DB_CONNECTION_STRING" \
        | tr ';' '\n' \
        | grep -i "^${key}=" \
        | head -1 \
        | cut -d'=' -f2-
}

# ---------------------------------------------------------------------------
# Validate required env var
# ---------------------------------------------------------------------------
if [ -z "$DB_CONNECTION_STRING" ]; then
    echo "ERROR: DB_CONNECTION_STRING is not set. Set it via Key Vault secret injection." >&2
    exit 1
fi

MYSQL_HOST=$(get_param "Server")
MYSQL_DB=$(get_param "Database")
MYSQL_USER=$(get_param "Uid")
MYSQL_PASS=$(get_param "Pwd")
MYSQL_PORT=$(get_param "Port")
MYSQL_PORT="${MYSQL_PORT:-3306}"

if [ -z "$MYSQL_HOST" ] || [ -z "$MYSQL_DB" ] || [ -z "$MYSQL_USER" ] || [ -z "$MYSQL_PASS" ]; then
    echo "ERROR: DB_CONNECTION_STRING must contain Server, Database, Uid, and Pwd." >&2
    echo "       Received keys: $(echo "$DB_CONNECTION_STRING" | tr ';' '\n' | grep -o '^[^=]*')" >&2
    exit 1
fi

# Use MYSQL_PWD env var to avoid password in process arguments (visible in ps).
export MYSQL_PWD="$MYSQL_PASS"

# Base mysql command. The image ships the MariaDB client (Debian's
# default-mysql-client), which uses --ssl (not Oracle's --ssl-mode) to require
# TLS. Azure MySQL Flexible Server mandates TLS in transit; --ssl enables an
# encrypted connection.
MYSQL="mysql -h $MYSQL_HOST -P $MYSQL_PORT -u $MYSQL_USER --ssl"

# Migration progress is tracked in a DEDICATED metadata database, NOT the
# application database. This is deliberate: some migrations (e.g. a fresh-schema
# script) run DROP DATABASE against the app DB, which would otherwise destroy the
# tracking table mid-run. Keeping it separate means progress always survives.
META_DB="${META_DB:-_migrations}"

echo "==> Connecting to $MYSQL_HOST:$MYSQL_PORT database=$MYSQL_DB user=$MYSQL_USER"

# ---------------------------------------------------------------------------
# Exercise library import mode (independent of schema migrations).
#
# EXERCISE_IMPORT selects a re-runnable data load from exercise-import.sql
# (baked into the image), NOT a tracked migration:
#   off     — do nothing here; fall through to the normal migration loop.
#   update  — upsert exercises/categories; existing rows are overwritten,
#             missing rows are added. Nothing is deleted.
#   replace — first delete exercises NOT referenced by any SessionExercise
#             (their ExerciseInstruction rows cascade), then apply the import.
#             In-use exercises are RESTRICT-protected, so they are retained and
#             overwritten in place by the upsert. This is the FK-safe form of
#             "delete everything and re-add".
# ---------------------------------------------------------------------------
EXERCISE_IMPORT="${EXERCISE_IMPORT:-off}"
IMPORT_SQL="${IMPORT_SQL:-$(dirname "$0")/exercise-import/exercise-import.sql}"

if [ "$EXERCISE_IMPORT" != "off" ]; then
    echo "==> EXERCISE_IMPORT mode: $EXERCISE_IMPORT"

    if [ "$EXERCISE_IMPORT" != "update" ] && [ "$EXERCISE_IMPORT" != "replace" ]; then
        echo "ERROR: EXERCISE_IMPORT must be 'off', 'update' or 'replace' (got '$EXERCISE_IMPORT')." >&2
        exit 1
    fi

    if [ ! -f "$IMPORT_SQL" ]; then
        echo "ERROR: exercise import SQL not found at $IMPORT_SQL" >&2
        exit 1
    fi

    TOTAL_BEFORE=$($MYSQL "$MYSQL_DB" -N -e "SELECT COUNT(*) FROM Exercise;" 2>/dev/null)
    IN_USE=$($MYSQL "$MYSQL_DB" -N -e "SELECT COUNT(DISTINCT e.ExerciseId) FROM Exercise e JOIN SessionExercise se ON se.ExerciseId = e.ExerciseId;" 2>/dev/null)
    echo "    Exercises before: ${TOTAL_BEFORE:-?} (referenced by a session: ${IN_USE:-?})"

    if [ "$EXERCISE_IMPORT" = "replace" ]; then
        echo "    replace: deleting exercises not referenced by any SessionExercise"
        if ! $MYSQL "$MYSQL_DB" -e "DELETE e FROM Exercise e WHERE NOT EXISTS (SELECT 1 FROM SessionExercise se WHERE se.ExerciseId = e.ExerciseId);"; then
            echo "ERROR deleting unreferenced exercises — aborting" >&2
            exit 1
        fi
        echo "    retained ${IN_USE:-0} in-use exercise(s); they will be overwritten by the import"
    fi

    echo "==> Applying exercise import: $IMPORT_SQL"
    if $MYSQL "$MYSQL_DB" < "$IMPORT_SQL"; then
        TOTAL_AFTER=$($MYSQL "$MYSQL_DB" -N -e "SELECT COUNT(*) FROM Exercise;" 2>/dev/null)
        echo "==> Exercise import complete: mode=$EXERCISE_IMPORT exercises=${TOTAL_AFTER:-?}"
        exit 0
    else
        echo "ERROR applying exercise import — aborting" >&2
        exit 1
    fi
fi

# ---------------------------------------------------------------------------
# Optional: reset migration tracking so every script re-runs from scratch.
# Dropping the metadata DB clears all recorded filenames; the next loop then
# re-applies 0010 (DROP + CREATE app DB) through the latest script.
# ---------------------------------------------------------------------------
if [ "$RESET_TRACKING" = "true" ]; then
    echo "==> RESET_TRACKING=true — dropping metadata database '$META_DB' to force full re-run"
    $MYSQL -e "DROP DATABASE IF EXISTS \`$META_DB\`;"
fi

# ---------------------------------------------------------------------------
# Ensure metadata database + schema_migrations tracking table exist
# ---------------------------------------------------------------------------
echo "==> Ensuring metadata database '$META_DB' and schema_migrations table"
$MYSQL -e "CREATE DATABASE IF NOT EXISTS \`$META_DB\`;"
$MYSQL "$META_DB" -e "
CREATE TABLE IF NOT EXISTS schema_migrations (
    filename   VARCHAR(255) NOT NULL,
    applied_at DATETIME     NOT NULL,
    PRIMARY KEY (filename)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
"

if [ "$SEED_BASELINE" = "true" ]; then
    echo "==> Running in SEED_BASELINE mode — recording without executing"
fi

# ---------------------------------------------------------------------------
# Process each migration file in sorted filename order
# ---------------------------------------------------------------------------
APPLIED=0
SKIPPED=0
ERRORS=0

for sql_file in "$MIGRATIONS_DIR"/*.sql; do
    if [ ! -f "$sql_file" ]; then
        echo "==> No migration files found in $MIGRATIONS_DIR" >&2
        exit 1
    fi

    filename=$(basename "$sql_file")

    # Check whether this migration has already been applied / baselined.
    row_count=$($MYSQL "$META_DB" -N -e "SELECT COUNT(*) FROM schema_migrations WHERE filename='$filename';" 2>/dev/null)

    if [ "$row_count" -gt 0 ] 2>/dev/null; then
        echo "  SKIP  $filename (already recorded)"
        SKIPPED=$((SKIPPED + 1))
        continue
    fi

    if [ "$SEED_BASELINE" = "true" ]; then
        # Baseline mode: record without executing.
        $MYSQL "$META_DB" -e "INSERT INTO schema_migrations (filename, applied_at) VALUES ('$filename', NOW());"
        echo "  MARK  $filename"
        APPLIED=$((APPLIED + 1))
    else
        # Normal mode: apply then record.
        echo "  APPLY $filename"
        if $MYSQL "$MYSQL_DB" < "$sql_file"; then
            $MYSQL "$META_DB" -e "INSERT INTO schema_migrations (filename, applied_at) VALUES ('$filename', NOW());"
            APPLIED=$((APPLIED + 1))
        else
            echo "  ERROR applying $filename — aborting" >&2
            ERRORS=$((ERRORS + 1))
            exit 1
        fi
    fi
done

echo ""
echo "==> Migration run complete: applied=$APPLIED skipped=$SKIPPED errors=$ERRORS"

if [ "$ERRORS" -gt 0 ]; then
    exit 1
fi
