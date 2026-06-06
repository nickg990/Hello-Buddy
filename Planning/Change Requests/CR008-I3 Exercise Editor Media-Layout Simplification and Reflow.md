# CR008-I3 - Exercise Editor Media/Layout Simplification and Reflow

Date: 2026-06-05
Status: Implemented
Owner: Product / Release 1 engineering
Scope: Canine Physio Admin UI (Exercise editor create/edit view)

## Why this change

The current exercise editor has media controls spread across separate sections and includes legacy image fields that no longer match the preferred clinician workflow.

This CR simplifies media handling, removes obsolete controls, and reorganizes layout to present media and defaults in clearer grouped sections.

Business and UX benefit:
- less clutter and fewer contradictory image controls;
- clearer separation between media selection and numeric defaults;
- faster scan and editing flow for clinicians.

## Requested changes

1. Remove Image URL manual override field from exercise editor.
2. Remove "Remove current image on save" checkbox.
3. Replace removed area with video functionality aligned to CR007-I3 workflow.
4. Present media panels horizontally in one grouped section:
   - Current image
   - Selected image
   - Current video
   - Selected video
5. Place defaults/flags in a second grouped horizontal section below media:
   - Is active (left-most)
   - Default reps
   - Default sets
   - Default hold seconds
6. Draw visual box/border around media section to separate it from other form areas.
7. Draw visual box/border around is-active/defaults section to separate it from other form areas.

## Proposed implementation direction (for approval)

- Shared view remains `Edit.cshtml` for both create and edit modes.
- Edit mode:
  - show Current image + Selected image + Current video + Selected video.
- Create mode:
  - show Selected image + Selected video only (no current media).
- Use responsive horizontal flex/grid layout that wraps on narrow screens while preserving left-to-right ordering.
- Remove image manual-override model binding field from rendered UI (leave backend contract unchanged unless separately approved).
- Keep data submission semantics unchanged for fields that remain.

## Acceptance criteria

1. Image URL manual override input is no longer rendered.
2. Remove current image checkbox is no longer rendered.
3. Media area is visually boxed and contains horizontal media panels in requested order.
4. Defaults area is visually boxed and appears below media area.
5. Is active appears at the left of defaults row.
6. Reps, sets, and hold seconds appear horizontally to the right of Is active.
7. Layout remains usable on desktop and narrow viewport widths.
8. Existing build/test lanes remain green.

## Risks and mitigations

- Risk: hiding manual image URL controls may reduce emergency override capability.
  - Mitigation: retain backend support and reintroduce admin-only override if needed in a future controlled CR.
- Risk: four-media-panel horizontal layout may become cramped on smaller screens.
  - Mitigation: use wrapping responsive layout with consistent panel sizing.

## Status

- 2026-06-05: Logged from user request.
- 2026-06-05: Implemented in shared exercise editor view: removed image manual override and remove-image controls, delivered boxed horizontal media/default sections, and aligned create/edit presentation with current/selected panel model. Pending deployment validation.

