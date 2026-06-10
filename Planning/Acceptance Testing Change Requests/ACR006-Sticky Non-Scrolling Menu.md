# ACR006 - Sticky (Non-Scrolling) Main Menu

Date: 2026-06-09
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Navigation / layout)

## Why this change

The main menu currently scrolls away with page content. The menu must remain fixed (sticky) and
not scroll out of view, so navigation is always available regardless of page scroll position.

## Scope

In scope:
- Make the main menu sticky so it stays in place while page content scrolls.
- Ensure the menu does not scroll out of view on long pages.

Out of scope:
- Restructuring menu items or content (other than as covered by ACR004).
- Mobile-specific navigation redesign beyond keeping the menu accessible.

## Acceptance criteria

1. The main menu remains visible when the page content is scrolled.
2. The menu does not scroll out of view on long pages.
3. Page content is not obscured by the sticky menu (correct offset/spacing).
4. Behavior is consistent across the admin screens.

## Implementation notes (proposed)

- Apply sticky/fixed positioning to the menu container in the shared layout.
- Add appropriate content offset so the sticky menu does not overlap page content.
- Verify against long scrolling screens (for example case detail, builder).

## Risks and mitigations

Risks:
- Sticky menu overlapping content or anchored scroll targets.
- Z-index/stacking conflicts with modals/popups (see ACR005).

Mitigations:
- Set content padding/offset to match menu height.
- Ensure modal z-index sits above the sticky menu.
