# Change Request Log

Last Updated: 2026-06-05

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
| CR-005 | Live Image Preview on Exercise Edit Screen | In Progress | Product / Release 1 engineering | 2026-06-04 | 2026-06-04 | Side-by-side current/selected image panels with sitting-dog placeholder; view + partial implemented, pending build, test and deploy. | Build UI, run smoke tests, deploy and verify. |
| CR-006 | Deselect Pending Selected Exercise Image | Implemented | Product / Release 1 engineering | 2026-06-05 | 2026-06-05 | Right-click deselect implemented for selected image while preserving left-click open-in-new-tab behavior. | Deploy UI and validate in edit/create flows. |
| CR-007 | Video URL Live Preview with Select/Clear Workflow | Implemented | Product / Release 1 engineering | 2026-06-05 | 2026-06-05 | Current/selected video panels plus search helper popup implemented; selected video supports right-click deselect and left-click open. | Deploy UI and complete manual UX validation. |
| CR-008 | Exercise Editor Media/Layout Simplification and Reflow | Implemented | Product / Release 1 engineering | 2026-06-05 | 2026-06-05 | Removed image manual override/remove-image controls and delivered boxed horizontal media/default sections with requested ordering. | Deploy UI and validate layout on desktop/mobile. |
| CR-009 | Exercise Details Media Horizontal Click-Open Layout | Implemented | Product / Release 1 engineering | 2026-06-05 | 2026-06-05 | Details page media now renders horizontally with click-open tiles and no separate View/Open link labels. | Deploy UI and verify on local/prod exercise details pages. |

## Notes

- Update this file whenever a CR status changes.
- Keep each CR document as the detailed source of scope and acceptance criteria.
