# CR-001 Increment 4 - Programme Builder Multi-Select Exercise Add

Date: 2026-06-06
Scope reviewed: Canine Physio Admin Programme Builder exercise selection flow
Reference documents: `Planning/Change Requests/CR Log.md`, `Technical Debt/*.md`
Reviewer: GitHub Copilot (GPT-5.3-Codex)

## Outcome
A new change request is proposed to add multi-select capability to the Programme Builder "Add exercise" dropdown so practitioners can add multiple exercises to a session in one action.

A duplicate scan of the Technical Debt folder found no existing request for this capability.

## Current behavior (evidence)
In `src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml` each session currently renders:

- a single-select `<select name="exerciseId">` for add;
- one `Add` submit action posting one `exerciseId` value;
- per-session filtering that prevents duplicates within the same session.

This means practitioners must repeat the add action one exercise at a time.

## Requested change
Add multi-select support to the Programme Builder exercise selection control so multiple exercises can be selected and added to the same session in one submit.

## Proposed implementation scope

### UI
- Change the add control from single-select to multi-select for each session (or equivalent tokenized multi-picker).
- Preserve existing per-session filtering rules:
  - selected/added exercises are removed from that session's add options;
  - exercises remain selectable in other sessions (for AM/PM independence).
- Keep existing session-anchor scroll behavior after add.

### Contracts/API/UI client
- Add a batch add request shape (for example `exerciseIds[]`) and accept multiple ids in one call.
- Preserve existing anti-forgery and case/session ownership checks.

### Application/Infrastructure
- Add a batch insert path for session exercises.
- Apply duplicate protection per session for all submitted ids.
- Return clear outcomes for partial/invalid selections (for example: added count, duplicates skipped).

## Acceptance criteria
1. A practitioner can select multiple exercises in a session add control and submit once.
2. All selected exercises are added to the target session in one operation.
3. Duplicate selections are not inserted into the same session.
4. The same exercise can still be added to a different session (for example AM and PM).
5. After add, the page remains at the affected session card.
6. Existing single-exercise add behavior remains functionally compatible if only one item is selected.

## Risks and mitigations
- Risk: larger post payloads for big selections.
  Mitigation: no product cap is applied; use efficient batch handling and return clear validation messages for invalid ids/session-state issues.
- Risk: partial failure handling ambiguity.
  Mitigation: define deterministic response semantics (added/skipped/not-found) and surface summary in UI.
- Risk: UX regression on mobile/smaller screens with native multi-select controls.
  Mitigation: evaluate native multi-select versus enhanced accessible picker before implementation.

## Clarifications confirmed
1. Existing duplicate-prevention behavior must persist: once an exercise is selected/added in a session, it must be removed from that same session's add list to reduce accidental duplicates.
2. No maximum batch-add count is currently required.
3. Multi-select control style (native HTML multi-select vs enhanced searchable picker) is intentionally left open for a separate UX discussion.
