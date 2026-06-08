# Backup and Restore Runbook

This runbook covers local reset and Azure point-in-time restore drill steps.

## 1. Local Reset and Seed Validation

From repo root:

```powershell
./Infrastructure/local-dev/reset-local-admin-data.ps1 -Force
```

Expected result:

- Schema rebuild succeeds.
- Baseline scripts are re-applied.
- Known seeded entities are available to integration tests.

## 2. Local Teardown Shortcut

```powershell
./Infrastructure/local-dev/teardown-local-admin-data.ps1 -Force
```

This delegates to the same reset flow and is useful as an operator-friendly command.

## 3. Azure Point-in-Time Restore Drill

Use helper script:

```powershell
./Infrastructure/runbooks/Invoke-MySqlPointInTimeRestore.ps1 \
  -ResourceGroupName rg-hellobuddy-prod \
  -ServerName mysql-hellobuddy-prod \
  -RestoreServerName mysql-hellobuddy-restore-drill \
  -RestoreTimeUtc "2026-06-08T08:30:00Z"
```

Notes:

- Restore creates a new server from backup history.
- Validate connectivity and seeded workflow checks against the restored server.
- Remove the drill server after verification to control cost.

## 4. Drill Exit Criteria

- Restore command succeeds.
- Connection string to restored instance works.
- Core API integration smoke paths pass against restored data.
- Outcome and timing recorded in release evidence.
