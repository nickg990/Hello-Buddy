# Increment 4 Summary and Increment 5 Handoff

**Date:** 2026-06-06
**Status:** Complete and handed off
**Scope:** Hello Buddy Canine Physiotherapy Admin — Release 1, Increment 4

## What Increment 4 delivered

Increment 4 turned the programme view/edit surface into a real draft-builder entry point driven from treatment cases, reusing the Increment 3 exercise contracts and seeded content. The implementation now covers:

- draft programme creation from a treatment case;
- programme name, dates, and default session structure derived from the case;
- builder entry point from the case detail page;
- session-exercise add/remove with per-session duplicate prevention;
- prescription editing (reps/sets/hold seconds/notes) and exercise ordering persistence;
- single active-programme rule with complete-and-activate flow;
- programme delete from the case detail rows with version-history guardrails;
- builder UX refinements (single change-aware save buttons, editable title, tuned scroll behavior);
- integration tests for the create-and-edit flow, plus UI smoke coverage for the new entry points.

## Key technical outcomes

- Draft programmes persist with `Status = "planned"` (a valid `programme` enum value); the `draft` concept stays on `ProgrammeVersion.VersionStatus`, not `Programme.Status`.
- Programme structure save extends through Contracts/API/Infrastructure to include `ProgrammeName` and rejects blank names.
- Single-active-programme rule is enforced in the repository via a transactional guard scoped by `TreatmentCaseId`; activate/complete endpoints back the case UI status dropdown.
- The builder uses one hidden form with HTML `form`-attribute binding for all session edit inputs, with client-side change-aware enablement.
- Per-session add-exercise dropdowns filter out exercises already in that session while remaining independent across AM/PM sessions; add/remove reloads to the affected session card rather than the page top.
- Case detail programme messages render at the bottom of the page (near the programmes list) with scroll-to-bottom, the id column is removed, and the list is ordered by start date.
- Local PDF publish wiring corrected: the API's `PdfService:Uri` now matches the PDF service's local port (`5081`).

## Change requests completed during/after Increment 4

- CR010-I4: Delete programme action on case detail rows (bin icon).
- CR011-I4: Single active programme rule with complete-and-activate flow.
- CR012-I4: Builder UX — single save buttons, editable title, and save scroll targets.
- CR013-I4: Per-session add-exercise dropdown filtering and no-scroll add.
- CR014-I4: Case detail programme list — bottom status message, remove id column, sort by start date.

## Defects raised and resolved in Increment 4

- ERR-I4-001: 500 on "Create draft programme" — `Programme.Status` was set to `"draft"` (not a valid enum member); fixed to `"planned"`. Resolved.
- ERR-I4-002: Redundant title pencil edit control (operator error) — pencil removed; title made directly editable in the setup card. Resolved.
- ERR-I4-003: "Save session edits" button rendered at top of page — relocated to the bottom of the builder with scroll-to-bottom on save. Resolved.
- ERR-I4-004: 500 on "Publish PDF" — local `PdfService:Uri` pointed at port `5068` while the PDF service listens on `5081`; corrected. Storage confirmed Azurite-only in local (no real Azure access). Resolved.

## Validation status

- UI smoke tests: passing.
- API in-memory and integration tests: passing for the create-and-edit and activate/complete flows.
- UI build: passing.
- Local admin stack: starts successfully with Azurite, API, PDF, and UI (PDF on `5081`).
- Deployment path: component-only redeploys remain the standard flow.

## What Increment 5 should consume

Increment 5 should build on the draft-programme entry point rather than adding a parallel route.

Recommended dependencies for the next chat:

- The case-driven draft create action and builder entry point remain the canonical start.
- Programme status lifecycle (`planned` → `active` → `completed`) and the single-active guard are stable and should be respected by any new status transitions.
- The session-exercise edit surface (hidden form + `form`-attribute binding) is the supported persistence path.
- Increment 3 exercise contracts and seeded data remain canonical; no new exercise fixtures.
- The PDF publish path is now reachable locally; preview/publish validation rules are the natural next slice.

## Open items carried forward

- Draft validation before preview/publish (I4-S6) remains documented as follow-on work; the publish path is otherwise unchanged.
- API publish endpoint still surfaces an unreachable PDF service (or storage error) as a bare 500; wrapping it in a structured 503/400 response is a recommended hardening follow-up.
- TD-002 remains deferred: domain split and AuthN/Z policy migration.
- Browser-level automation for media preview interactions remains deferred in TD-003.
- Immutable version history and the exercise-picker backlog remain queued.

## Handoff note

Increment 4 is complete enough for Increment 5 to begin. The next conversation should focus on draft validation and the preview/publish slice, building on the case-driven draft-programme entry point and the now-working local PDF publish wiring, without altering Increment 4's public surface.
