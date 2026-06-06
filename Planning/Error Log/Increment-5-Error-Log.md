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
