# Increment 4 — Programme Builder Persistence Plan

**Created:** 2026-06-05
**Status:** Ready for execution
**Scope:** Hello Buddy Canine Physiotherapy Admin — Release 1, Increment 4
**Parent plan:** [Release-1-Prototype-Epic-and-Increment-Stories.md](../Release-1-Prototype-Epic-and-Increment-Stories.md)
**Depends on:** Increment 3 handoff in [Increment-3-Summary-and-Handoff.md](Increment-3-Summary-and-Handoff.md)

---

## 1. Increment goal

Turn the existing programme view/edit surface into a real draft-builder entry point for treatment cases, using the Increment 3 exercise contracts and seeded exercise data as the content foundation.

Increment 4 is the first release-1 slice that should let a practitioner start from a treatment case, create a draft programme, adjust the programme structure, and prepare it for later preview/publish work.

---

## 2. Scope summary

### In scope

- Draft programme creation from a treatment case.
- Programme name, status, and default session structure derived from the case.
- Builder entry point from the case detail page.
- Session-exercise edit persistence for prescription fields and ordering.
- Increment 3 exercise contracts and seeded exercise data reused as-is.
- Integration tests for the create-and-edit flow, plus UI smoke coverage for the new entry point.

### Out of scope

- Preview/publish validation rules beyond the current builder shape.
- PDF rendering changes.
- Immutable version history.
- New exercise fixtures or schema changes.
- Styling refresh beyond the existing admin baseline.

---

## 3. Story mapping

| Story | Increment 4 treatment |
|---|---|
| I4-S1: Create draft programme from case | Implemented as a new case-driven create action that inserts a draft programme and redirects to the builder. |
| I4-S2: Configure dates and single/AM/PM structure | Use the case dates as the initial programme dates and seed a default session row for the draft. |
| I4-S3: Add/remove exercises in sessions | Preserve the current builder persistence surface for session exercises; exercise picker work stays queued. |
| I4-S4: Prescription editing and notes | Continue supporting reps/sets/hold seconds/notes edits on session exercises. |
| I4-S5: Exercise ordering persistence | Preserve `SortOrder` in the existing builder save path and verify it in tests. |
| I4-S6: Draft validation before preview/publish | Leave the existing publish path unchanged for now; document the missing validation as follow-on work. |
| I4-S7: Integration tests for draft builder | Add integration coverage for case-to-draft creation and builder redirect. |
| I4-S8: Azure deployment and re-test | Re-run the new create flow after deployment to verify parity. |
| I4-S9: Granular config/secret validation | Keep the current deployment wiring; no new infra secrets are required for this slice. |

---

## 4. Implementation slice

The first implementation slice should do three things:

1. Add a programme create method to the application and repository boundaries.
2. Insert a draft programme plus one default planned session for a case-owned practitioner.
3. Expose the action through the case detail page and verify the flow with tests.

This is intentionally small. It gives Increment 4 a concrete user entry point without pretending the full builder backlog is already finished.

---

## 5. Test strategy

- API integration test: create a draft programme from a treatment case and assert the returned programme is linked to the case.
- UI smoke test: the case detail page exposes a create-draft button for the practitioner.
- Builder persistence test: existing session-exercise edit persistence remains intact after the create flow lands.

---

## 6. Handoff notes

- Reuse the seeded exercise keys and exercise contracts from Increment 3.
- Keep the programme builder surface focused on case-driven drafts, not on publish validation.
- Any later validation or preview work should build on this draft-programme entry point instead of adding a parallel route.