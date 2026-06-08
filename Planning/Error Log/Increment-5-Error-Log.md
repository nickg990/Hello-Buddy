# Increment 5 — Error Log

## ERR-I5-001: Missing automated coverage for async builder JSON contract and publish validation messaging

**Date:** 2026-06-06  
**Status:** Fixed  
**Severity:** High (regression-risking)  
**Area:** Programme builder (Increment 5)  
**Type:** Test coverage gap

### Symptom

Increment 5 introduced async builder mutations and in-place preview refreshes, but automated tests only covered page rendering and a browser happy-path flow. There was no direct automated verification of:

- AJAX JSON response contracts for builder save, structure save, add exercise, and remove exercise.
- Publish error-message aggregation when API validation fails (dedupe + omission of empty values).
- Publish success TempData contract (`PublishedFile`, `Published`) used by the post-redirect UI.

This created a risk of silent regressions where the front-end script receives unexpected payload shapes or where publish validation feedback is degraded.

### Root cause

Increment 5 behavior was implemented primarily in controller action branches keyed on `X-Requested-With: XMLHttpRequest`, but the existing UI suite focused on rendered HTML smoke checks and one browser-interaction test. Action-branch contracts were not unit tested directly.

### Fix

Added `ProgrammesControllerTests` in the UI test project with focused unit tests for the missing branches:

1. `Builder_Post_AjaxRequest_ReturnsJsonSuccessPayload`
2. `Structure_Post_AjaxInvalid_ReturnsJsonErrorPayload`
3. `AddExercise_Post_AjaxDuplicate_ReturnsJsonErrorPayload`
4. `RemoveExercise_Post_AjaxNotFound_ReturnsJsonErrorPayload`
5. `Publish_Post_WhenApiValidationException_AggregatesDistinctErrorsIntoTempData`
6. `Publish_Post_WhenApiPublishes_SetsPublishedTempData`

These tests use a controller-level harness (`NSubstitute` + `DefaultHttpContext` + `TempDataDictionary`) to assert payload/message behavior without requiring browser orchestration.

### Validation

- UI test project now passes with the new tests included.
- Full unit/integration test run executed after this change (see execution summary in this session response).

### Follow-up

- Keep browser E2E focused on end-user interactions and keep controller contract verification in unit tests for speed and determinism.
- If async payload schemas evolve, update controller unit tests in the same change to preserve contract safety.

## ERR-I5-002: Sort-order move landed one slot later than requested

**Date:** 2026-06-06  
**Status:** Fixed  
**Severity:** High (builder editing defect)  
**Area:** Programme builder sort renumbering  
**Type:** Ordering logic defect

### Symptom

When moving an exercise upward by editing its sort order, the moved exercise could land one slot later than requested.

Reproductions:

- Changing number 6 to number 4 resulted in the moved exercise becoming number 5.
- Changing number 6 to number 1 resulted in the moved exercise becoming number 2.

### Root cause

The initial renumbering implementation treated sort edits as a plain sort on the requested `SortOrder`, with ties broken by the existing persisted order.

That meant when a moved item requested an already-occupied slot, the current occupant for that slot stayed ahead of the moved item during ordering, so the moved item was renumbered into the next position instead of the requested one.

In practice, the logic behaved like:

- sort by requested sort value
- if two items now target the same slot, keep the original occupant first

This is not the required interaction model. Builder sort changes need **insert/reflow semantics**, not simple tie-broken sorting.

### Fix

Updated `ProgrammeRepository.UpdateSessionExercisesAsync` to use insertion-based move handling per touched session:

1. Start from the current session order.
2. Identify rows whose requested sort differs from their current sort.
3. Apply those moves by removing the item from the current list and inserting it into the requested target index.
4. Renumber the entire session sequentially from 1..N.

This ensures the moved exercise lands in the requested slot and intervening exercises shift around it consistently.

### Validation

Added focused API in-memory regression coverage for the exact defect:

1. Moving item 6 to position 4 results in order `1,2,3,6,4,5`.
2. Moving item 6 to position 1 results in order `6,1,2,3,4,5`.

### Follow-up

- Keep sort-order tests focused on user-visible final ordering, not just contiguous numeric renumbering.
- Add more move-direction regression cases if drag/drop or multi-session reordering is introduced later.

## ERR-I5-003: Multi-session sort edit inconsistent on Session 2

**Date:** 2026-06-06  
**Status:** Fixed  
**Severity:** High (data-entry correctness)  
**Area:** Programme builder session exercise ordering (multi-session)  
**Type:** Session-scoped ordering regression risk

### Symptom

Manual retest reported that sort reordering behaved correctly in Session 1 but not in Session 2 for AM/PM programmes.

### Root cause

The ordering logic was previously corrected for single-session insertion/reflow behavior, but regression protection did not explicitly validate multi-session behavior where both sessions are submitted in one builder update. This left a gap around proving that:

- Session-scoped reordering remains isolated per session.
- Session 1 ordering remains unchanged when only Session 2 ordering is edited.

### Fix

Strengthened regression coverage to lock in both single-session and multi-session semantics using API integration-style in-memory tests:

1. **Single-session move-up** remains covered with explicit requested-slot assertions.
2. **New multi-session test** validates that moving the last item to position 1 in Session 2:
	- lands in the requested slot for Session 2,
	- renumbers Session 2 contiguously,
	- does not alter Session 1 ordering.

### Validation

- API in-memory test project passes with new single + multi-session coverage.
- Full solution test run passes.

### Follow-up

- Keep session-aware ordering assertions in place for every future sort-order algorithm change.
- If UI drag/drop ordering is introduced, add browser automation for Session 2 AM/PM reorder parity.

## ERR-I5-004: Publish validation is not the prescribed reusable validator and omits required rules

**Date:** 2026-06-08  
**Status:** Open (code review finding)  
**Severity:** High (standards deviation + acceptance-criteria gap)  
**Area:** Programme publish validation  
**Type:** Standards conformance / validation completeness

### Symptom

Publish/preview validation is hand-rolled inside the Application service as a `Dictionary<string, string[]>` rather than implemented as the reusable FluentValidation validator the standards mandate, and several required rules are missing.

- Location: `ProgrammeService.ValidateDraftForPublishAsync` (`src/HelloBuddy.Application/Programmes/ProgrammeService.cs`).
- Coding standards §6 require: *"Implement once, in `PublishProgrammeValidator`, reused by preview and publish"* covering owner exists, pet exists, case exists, title or date range, start + end dates, ≥1 session, each session ≥1 exercise, each exercise has prescription, missing video link = warning not error.
- Current implementation only checks: ≥1 session, ≥1 total exercise, reps/sets > 0, hold-seconds > 0 when provided. It does not validate title/date-range or start + end dates.

### Root cause

Validation was implemented inline at the service boundary during the Increment 5 publish slice rather than as a dedicated FluentValidation validator, so the rule set drifted from the standards-defined publish ruleset.

### Recommended fix

1. Introduce `PublishProgrammeValidator` (FluentValidation) in `HelloBuddy.Application`, co-located with the publish command/DTO.
2. Implement the full §6 rule set, including title/date-range and start + end date checks, and treat a missing video link as a warning rather than a blocking error.
3. Reuse the same validator for both preview and publish to guarantee parity.
4. Keep typed error codes/keys so the existing `Results.ValidationProblem` response shape is preserved.

### Validation

- Add unit tests for each rule (pass/fail) in the Application test project.
- Confirm publish and preview surface identical validation outcomes for the same draft.

### Deferral tracking

- Deferred by decision pending manual end-to-end publish verification; tracked in [Technical Debt/TD-006 Publish Validator End-to-End Deferral.md](Technical%20Debt/TD-006%20Publish%20Validator%20End-to-End%20Deferral.md).

## ERR-I5-005: Publish version persistence is not atomic

**Date:** 2026-06-08  
**Status:** Fixed  
**Severity:** Medium (data-consistency risk on the canonical release path)  
**Area:** Programme publish persistence  
**Type:** Transactional integrity

### Symptom

`ProgrammeRepository.PersistPublishedVersionAsync` (`src/HelloBuddy.Infrastructure/Programmes/ProgrammeRepository.cs`) supersedes the current version, inserts the new published version, and updates `CurrentProgrammeVersionId` across three separate `SaveChangesAsync` calls with no surrounding transaction.

A process failure mid-sequence can leave inconsistent state: a version superseded with no committed successor, or a published version row that the programme does not point at.

### Root cause

The publish persistence routine was implemented as sequential saves without wrapping the multi-step state change in a single unit of work, despite publish being the canonical release path (§5 unit-of-work).

### Recommended fix

- Wrap the supersede + insert + current-pointer update in a single provider-safe transaction, reusing the pattern already established in `ProgrammeRepository.ActivateAsync` (`IDbContextTransaction` guarded by `_db.Database.IsRelational()` with try/finally dispose), so in-memory test providers remain supported.

### Implementation update

- Implemented a provider-safe transaction in `PersistPublishedVersionAsync` so supersede/insert/current-pointer updates are committed atomically.

### Validation

- Add an integration test (Testcontainers MySQL) asserting that a forced mid-publish failure leaves no partial version state.
- Confirm existing publish-success tests still pass.

## ERR-I5-006: Builder update repository method trusts caller for ownership

**Date:** 2026-06-08  
**Status:** Fixed  
**Severity:** Low (defense-in-depth / consistency)  
**Area:** Programme builder persistence  
**Type:** Authorization scoping consistency

### Symptom

`ProgrammeRepository.UpdateSessionExercisesAsync` filters only by `ProgrammeId` and relies on `ProgrammeService.UpdateAsync` having called `OwnsAsync` first. It is the only mutating method on this repository boundary without a `practitionerId` parameter, unlike its siblings.

### Root cause

Ownership enforcement for the builder save path was placed in the service layer only, leaving the repository write method dependent on the caller remembering to pre-check ownership.

### Recommended fix

- Add `practitionerId` to `UpdateSessionExercisesAsync` and scope the queried rows by `se.Session.Programme.TreatmentCase.PractitionerId == practitionerId`, consistent with the other mutating repository methods.

### Implementation update

- Updated `UpdateSessionExercisesAsync` signature to include `practitionerId` and applied practitioner scoping in edited/touched session queries.

### Validation

- Add a regression test confirming a non-owning practitioner cannot mutate another practitioner's session exercises.

## ERR-I5-007: Parameter-transposition risk across builder add-exercise boundary

**Date:** 2026-06-08  
**Status:** Fixed  
**Severity:** Low (maintainability / latent defect risk)  
**Area:** Programme builder add exercise  
**Type:** API ergonomics

### Symptom

`ProgrammeService.AddSessionExerciseAsync` has signature `(programmeId, sessionId, exerciseId, practitionerId)` but calls the repository as `(programmeId, practitionerId, sessionId, exerciseId)`. All arguments are `ulong`, so any future reorder mistake compiles silently.

### Root cause

The service and repository argument orders diverge, and the uniform `ulong` types remove compile-time protection against transposition.

### Recommended fix

- Align the argument order between the service and repository methods, or pass arguments by name at the call site, to remove the silent-transposition trap.

### Implementation update

- Updated the service-to-repository call to named arguments, eliminating silent transposition risk while preserving API signatures.

### Validation

- Confirm existing add-exercise tests still pass after the signature alignment.

## ERR-I5-008: Async builder responses use untyped anonymous JSON contracts

**Date:** 2026-06-08  
**Status:** Fixed  
**Severity:** Low (contract clarity)  
**Area:** Programme builder async endpoints  
**Type:** Boundary contract robustness

### Symptom

The builder/structure AJAX branches in `ProgrammesController` return anonymous objects (e.g. `Json(new { ok = true, message = ... })`). The shape is only pinned by tests, not by a typed contract (§1.6 boundaries are contracts).

### Root cause

The async response payloads were introduced as inline anonymous objects during the Increment 5 async builder work.

### Recommended fix

- Introduce a small typed response record for the builder AJAX result and return it from the relevant actions, keeping the existing JSON shape for the front-end script.

### Implementation update

- Introduced typed `BuilderAjaxResponse` and replaced anonymous AJAX payloads across builder/structure/add/remove branches while preserving lower-case `ok/message/error` JSON keys.

### Validation

- Confirm the existing `ProgrammesControllerTests` AJAX contract assertions still pass against the typed record.
