# Increment 3 Summary and Increment 4 Handoff

**Date:** 2026-06-05
**Status:** Complete and handed off
**Scope:** Hello Buddy Canine Physiotherapy Admin — Release 1, Increment 3

## What Increment 3 delivered

Increment 3 established the Exercise Library as the editable content foundation for later programme-builder work. The implementation now covers:

- exercise list browsing with search/filtering;
- exercise create/edit lifecycle;
- ordered instruction-step management as first-class data;
- activate/deactivate controls;
- category lookup support;
- seeded canonical exercise content for local/dev/test consistency;
- per-component deployment flow;
- UI smoke tests and API integration coverage.

## Key technical outcomes

- Ordered instructions are stored in `ExerciseInstruction` and treated as the source of truth.
- Legacy `Exercise.InstructionsText` remains read-only for pre-existing data.
- Seeder logic is idempotent and keyed by `ExerciseKey`.
- Increment 3 deployment uses split component scripts (`UiOnly`, `ApiOnly`, `PdfOnly`) rather than a full monolithic redeploy.
- Exercise media UX was extended through follow-up CRs to support live previews, right-click deselect, and cleaner layout across edit/details screens.

## Change requests completed during/after Increment 3

- CR-002: Exercise media upload and PDF click-through video.
- CR-003: Azurite-first local blob emulation.
- CR-004: Self-hosted malware scanning deferred.
- CR-005: Live image preview on exercise edit screen.
- CR-006: Right-click deselect for pending selected image.
- CR-007: Video URL preview/search helper workflow.
- CR-008: Exercise editor media/layout simplification and reflow.
- CR-009: Exercise details media horizontal click-open layout.

## Validation status

- UI smoke tests: passing.
- UI build: passing.
- Local admin stack: starts successfully with Azurite, API, PDF, and UI.
- Deployment path: verified via component-only UI redeploys.

## What Increment 4 should consume

Increment 4 should use the existing exercise contracts and seeded content rather than introducing new exercise fixtures.

Recommended dependencies for the next chat:

- `ExerciseListItem` and `ExerciseListFilter` contracts remain stable.
- The exercise picker should default to active exercises only.
- The seeded exercise set should be treated as canonical test data.
- No new schema work is required for Increment 4’s first pass.

## Open items carried forward

- TD-002 remains deferred: domain split and AuthN/Z policy migration.
- Browser-level automation for media preview interactions remains deferred in TD-003.
- Any future enhancement to move legacy free-text instructions into step rows is still deferred.

## Handoff note

Increment 3 is complete enough for Increment 4 to begin. The next conversation should focus on programme builder entry points, picker behavior, and how it consumes the exercise contracts and seeded data without altering Increment 3’s public surface.
