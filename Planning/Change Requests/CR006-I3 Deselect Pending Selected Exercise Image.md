# CR006-I3 - Deselect Pending Selected Exercise Image

Date: 2026-06-05
Status: Implemented
Owner: Product / Release 1 engineering
Scope: Canine Physio Admin UI (Exercise editor create/edit view)

## Why this change

The current exercise image selection flow allows choosing and replacing files, but does not provide an explicit user action to clear a pending image selection before save.

This creates friction when users pick an image by mistake and want to revert back to a no-pending-selection state without saving.

Business and UX benefit:
- reduces accidental image updates;
- improves confidence before saving;
- keeps image-edit behavior predictable and reversible.

## Requested behavior

- User can deselect a pending selected image prior to save.
- Left click on selected image continues to open larger image in a new tab.
- Right click on selected image should present a deselect option.

## Proposed implementation direction (for approval)

- Keep left click behavior for open-in-new-tab.
- Add a custom right-click context menu on the selected image with action: "Deselect image".
- On deselect/clear:
  - reset file input value;
  - revoke object URL;
  - hide selected preview;
  - show placeholder again.

## Acceptance criteria

1. After selecting an image, user can clear it before save.
2. Clearing selected image restores "No image selected" placeholder in the selected panel.
3. Clearing does not affect the current stored image panel.
4. Saving after clearing does not upload a new image.
5. Behavior works on both create and edit modes (shared view).
6. Left click still opens selected image larger in a new tab.
7. Right click on selected image offers deselect option.
8. Existing UI smoke tests continue to pass.

## Risks and mitigations

- Risk: Browser native context-menu behavior differs across devices and touch input has no right-click equivalent.
  - Mitigation: scope is intentionally mouse-first for this workflow and can be revisited if user testing indicates a need.
- Risk: object URL leak if clear path misses cleanup.
  - Mitigation: centralize clear logic and always revoke existing object URL on clear/change/unload.

## Status

- 2026-06-05: Logged from user request.
- 2026-06-05: Implemented in shared exercise editor view with right-click deselect for selected image and preserved left-click open behavior (no fallback clear button by request). Pending deployment validation.

