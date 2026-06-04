# Change Request Log

Last Updated: 2026-06-04

## Purpose

Single place to track Change Request status, progress, and next actions.

## Status legend

- Proposed: documented but not approved.
- Approved: agreed and queued for implementation.
- In Progress: actively being implemented.
- Implemented: code complete, pending final sign-off/release.
- Deferred: intentionally paused for future decision.
- Closed: completed and signed off.

## CR register

| CR | Title | Status | Owner | Date Opened | Date Updated | Progress Summary | Next Action |
|---|---|---|---|---|---|---|---|
| CR-002 | Exercise Media Upload and PDF Click-Through Video | Implemented | Product / Release 1 planning | 2026-06-04 | 2026-06-04 | Core feature delivered: upload endpoint, UI flow, programme/PDF propagation, governance hooks, orphan policy, and tests passing. | Review with client and move to Closed after acceptance. |
| CR-003 | Azurite-First Local Blob Emulation for Whole Admin Application | Implemented | Platform / Release 1 engineering | 2026-06-04 | 2026-06-04 | Local Azurite-first mode implemented with tooling/docs consolidation and emulator-backed tests. | Validate team runbook usage and close when accepted. |
| CR-004 | Self-Hosted Malware Scanning for Exercise Media | Deferred | Platform / Security | 2026-06-04 | 2026-06-04 | CR defined for real scanner integration behind existing hook; no provider implementation started. | Client decision on self-hosted approach, then approve and schedule. |

## Notes

- Update this file whenever a CR status changes.
- Keep each CR document as the detailed source of scope and acceptance criteria.
