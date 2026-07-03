# CR014-I4 - Case Detail Programme List: Bottom Status Message, Remove ID Column, Sort by Start Date

Date: 2026-06-05
Status: Implemented
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Case detail / Programmes list)

## Why this change

On the selected case screen, programme status messages (for example "Programme completed")
render at the top of the page via the shared layout. Because the programmes list sits at the
bottom of the page, the practitioner is bounced to the top to read a message about an action
they just performed at the bottom, then has to scroll back down.

The programmes list also shows the raw programme id (`#`) next to the name, which is noise for
practitioners, and the list is not ordered by start date.

## Scope

In scope:
- Show programme status/delete messages for the case detail screen at the bottom of the page
  (near the programmes list) instead of the top.
- After a programme status change or delete, keep the screen scrolled to the bottom.
- Remove the `#` (programme id) column from the programmes list.
- Sort the programmes list by start date.

Out of scope:
- Changing the global top-of-page flash behavior for other screens.
- Case note message placement (notes live mid-page and are unaffected).
- Programme creation message (redirects to the Builder page).

## Acceptance criteria

1. Activating or completing a programme shows the resulting message at the bottom of the case
   detail page, not the top.
2. After a status change, the page remains scrolled to the bottom.
3. Deleting a programme shows its message at the bottom and keeps the page at the bottom.
4. The programmes list no longer shows the numeric id column next to the name.
5. The programmes list is ordered by start date.

## Implementation notes

- `UpdateProgrammeStatus` and `DeleteProgramme` use dedicated `CaseProgrammeMessage` /
  `CaseProgrammeMessageType` temp keys so the shared layout's top alert is bypassed.
- The case detail view renders that message after the programmes card and scrolls to the
  bottom when a message is present.
- Programme rows are ordered with `OrderBy(p => p.StartDate)` and the `#`/id column is removed.

## Risks and mitigations

Risks:
- Divergent message styling between top and bottom alerts.

Mitigations:
- Bottom alert reuses the same Bootstrap `alert` classes; message type maps to
  `alert-success` / `alert-danger`.
