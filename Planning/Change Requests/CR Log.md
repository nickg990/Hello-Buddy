# Change Request Log

Last Updated: 2026-06-06

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
| CR015-I5 | Exercise Video Search Provider Base URL Configuration | Implemented | Product / Release 1 planning | 2026-06-06 | 2026-06-06 | Implemented: exercise editor search provider list is now configuration-bound (description + base URL), rendered in the dropdown, wired through UI app settings and Azure container app Terraform variables, and validated by UI test coverage. | Validate end-to-end in local/Azure flow and close after sign-off. |
| CR001-I4 | Programme Builder Multi-Select Exercise Add | Proposed | Product / Release 1 planning | 2026-06-06 | 2026-06-06 | Proposed: add multi-select capability to each session's Add exercise control so practitioners can add multiple exercises in one action while preserving per-session duplicate filtering and AM/PM independence; no batch-size cap requested. | Confirm picker UX approach (native multi-select vs enhanced searchable picker), then approve and schedule implementation. |
| CR014-I4 | Case Detail Programme List: Bottom Status Message, Remove Id Column, Sort by Start Date | Implemented | Product / Release 1 planning | 2026-06-05 | 2026-06-05 | Implemented: programme status/delete messages now render at the bottom of the case detail page with scroll-to-bottom, programme id column removed, and programmes list ordered by start date. | Validate end-to-end in local flow and close after sign-off. |
| CR013-I4 | Per-Session Add-Exercise Dropdown Filtering and No-Scroll Add | Implemented | Product / Release 1 planning | 2026-06-05 | 2026-06-05 | Implemented: per-session add-exercise dropdown excludes exercises already in that session (re-added when removed) while remaining independent per AM/PM session; add/remove now reloads to the affected session card instead of the page top. | Validate end-to-end in local flow and close after sign-off. |
| CR012-I4 | Builder UX: Single Save Buttons, Editable Title, and Save Scroll Targets | Implemented | Product / Release 1 planning | 2026-06-05 | 2026-06-05 | Implemented: single page-level Save session edits button (change-aware), Save setup change-aware enablement, pencil-edit title flow, and post-save scroll behavior (setup=top, session edits=bottom). | Validate end-to-end in local flow and close after sign-off. |
| CR011-I4 | Single Active Programme Rule with Complete-and-Activate Flow | Implemented | Product / Release 1 planning | 2026-06-05 | 2026-06-05 | Implemented end-to-end: activate/complete endpoints, single-active guard by case, case UI status dropdown actions, and API/UI automated test coverage. | Validate in local/Azure smoke flows and close after client sign-off. |
| CR010-I4 | Delete Programme Action on Case Detail Rows (Bin Icon) | Implemented | Product / Release 1 planning | 2026-06-05 | 2026-06-05 | Implemented end-to-end: bin action in case rows, antiforgery delete flow, API delete endpoint, version-history guardrails, and UI/API test coverage. | Validate in local/Azure smoke flows and close after client sign-off. |
| CR002-I3 | Exercise Media Upload and PDF Click-Through Video | Implemented | Product / Release 1 planning | 2026-06-04 | 2026-06-04 | Core feature delivered: upload endpoint, UI flow, programme/PDF propagation, governance hooks, orphan policy, and tests passing. | Review with client and move to Closed after acceptance. |
| CR003-I3 | Azurite-First Local Blob Emulation for Whole Admin Application | Implemented | Platform / Release 1 engineering | 2026-06-04 | 2026-06-04 | Local Azurite-first mode implemented with tooling/docs consolidation and emulator-backed tests. | Validate team runbook usage and close when accepted. |
| CR004-I3 | Self-Hosted Malware Scanning for Exercise Media | Deferred | Platform / Security | 2026-06-04 | 2026-06-04 | CR defined for real scanner integration behind existing hook; no provider implementation started. | Client decision on self-hosted approach, then approve and schedule. |
| CR005-I3 | Live Image Preview on Exercise Edit Screen | In Progress | Product / Release 1 engineering | 2026-06-04 | 2026-06-04 | Side-by-side current/selected image panels with sitting-dog placeholder; view + partial implemented, pending build, test and deploy. | Build UI, run smoke tests, deploy and verify. |
| CR006-I3 | Deselect Pending Selected Exercise Image | Implemented | Product / Release 1 engineering | 2026-06-05 | 2026-06-05 | Right-click deselect implemented for selected image while preserving left-click open-in-new-tab behavior. | Deploy UI and validate in edit/create flows. |
| CR007-I3 | Video URL Live Preview with Select/Clear Workflow | Implemented | Product / Release 1 engineering | 2026-06-05 | 2026-06-05 | Current/selected video panels plus search helper popup implemented; selected video supports right-click deselect and left-click open. | Deploy UI and complete manual UX validation. |
| CR008-I3 | Exercise Editor Media/Layout Simplification and Reflow | Implemented | Product / Release 1 engineering | 2026-06-05 | 2026-06-05 | Removed image manual override/remove-image controls and delivered boxed horizontal media/default sections with requested ordering. | Deploy UI and validate layout on desktop/mobile. |
| CR009-I3 | Exercise Details Media Horizontal Click-Open Layout | Implemented | Product / Release 1 engineering | 2026-06-05 | 2026-06-05 | Details page media now renders horizontally with click-open tiles and no separate View/Open link labels. | Deploy UI and verify on local/prod exercise details pages. |

## Notes

- Update this file whenever a CR status changes.
- Keep each CR document as the detailed source of scope and acceptance criteria.

