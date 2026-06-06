# CR013-I4 - Per-Session Add-Exercise Dropdown Filtering and No-Scroll Add

Date: 2026-06-05
Status: Implemented
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Programme Builder UX)

## Why this change

When building a programme, the "Add exercise" dropdown lists every active exercise even
if it has already been added to the session. This makes accidental duplicate adds easy.
Separately, adding an exercise triggers a full page reload that scrolls back to the top,
forcing the practitioner to scroll down again to continue working in the same session.

## Scope

In scope:
- Remove an exercise from a session's "Add exercise" dropdown once it has been added to
  that session, preventing accidental duplicates.
- Add the exercise back to the dropdown if it is later removed from the session.
- Keep filtering per-session: an exercise added to the AM session must still be selectable
  in the PM session (and vice versa), so the same exercise can appear in both.
- After clicking Add (and Remove), keep the page positioned at the affected session card
  instead of jumping to the top of the page.

Out of scope:
- AJAX/partial-update of the session without a page reload.
- Changes to the underlying duplicate-prevention validation in the API.
- Reordering or multi-select add.

## Acceptance criteria

1. After an exercise is added to a session, it no longer appears in that session's
   "Add exercise" dropdown.
2. If that exercise is removed from the session, it reappears in the dropdown.
3. Filtering is independent per session: adding an exercise to AM does not remove it from
   the PM "Add exercise" dropdown.
4. Clicking Add reloads to the same session card position, not the top of the page.
5. Clicking Remove reloads to the same session card position, not the top of the page.

## Implementation notes

- Dropdown filtering is performed per session in the Builder view by excluding exercises
  whose `ExerciseId` already appears in `session.Exercises`.
- Each session card carries an `id="session-card-{SessionId}"` anchor.
- `AddExercise`/`RemoveExercise` actions set a `BuilderScrollSessionId` temp flag with the
  affected session id; the Builder view scrolls that session card into view on load.

## Risks and mitigations

Risks:
- Scroll anchor missing if the session is removed/renumbered.

Mitigations:
- Scroll logic is defensive (no-op when the target card is not found), falling back to the
  existing top/bottom scroll behavior.
