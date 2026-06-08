# ADR-0003: Runbook-First Operations and Environment Configuration Matrix

- Status: Accepted
- Date: 2026-06-08

## Context

Increment 7 requires handover-ready operations: predictable environment config, backup/restore validation, and consistent release steps. Prior documentation existed but was not consolidated into a runbook-first operating model.

## Decision

Standardise operations around runbooks in `Infrastructure/runbooks`:

- Environment configuration matrix.
- Release runbook (local gate, deploy, post-deploy checks, rollback drill).
- Backup/restore runbook and restore helper script.

## Consequences

- Pros: clearer operator workflow and easier handoff.
- Pros: explicit mapping of required variables reduces config drift.
- Cons: requires runbook maintenance as settings evolve.
- Cons: scripts depend on Azure CLI and role permissions.
