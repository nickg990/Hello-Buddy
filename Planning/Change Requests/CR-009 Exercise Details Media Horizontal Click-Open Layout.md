# CR-009 - Exercise Details Media Horizontal Click-Open Layout

Date: 2026-06-05
Status: Implemented
Owner: Product / Release 1 engineering
Scope: Canine Physio Admin UI (Exercise Library details/open screen)

## Why this change

On the exercise details screen (Exercise Library -> Open), media links and previews are currently stacked and include separate "View image" / "Open video" link text.

This creates visual inconsistency versus the edit screen and introduces unnecessary extra link text around media.

Business and UX benefit:
- consistent media presentation between details and edit screens;
- cleaner details view with direct click targets;
- faster user interaction with media content.

## Requested behavior

- Show media horizontally on details screen.
- Remove explicit "View image" and "Open video" link labels.
- Allow left click directly on media tiles/previews to open in a new tab, matching edit-screen interaction style.

## Proposed implementation direction

- Replace stacked media link rows in `Details.cshtml` with horizontal media tiles.
- Current image tile:
  - image preview when available;
  - placeholder when unavailable;
  - entire tile clickable to open image in new tab when available.
- Current video tile:
  - still-frame/thumbnail preview when possible (YouTube/Vimeo/direct video);
  - fallback play-card visual when provider preview unavailable;
  - entire tile clickable to open video in new tab when available.
- Preserve responsive wrapping behavior on narrow screens.

## Acceptance criteria

1. Details page media area renders image and video side by side on desktop.
2. No separate "View image" or "Open video" text links are shown.
3. Left click on image tile opens full image in a new tab.
4. Left click on video tile opens video URL in a new tab.
5. Layout remains usable on narrow screens (tiles may wrap).
6. Existing UI test lane remains green.

## Status

- 2026-06-05: Logged from user request.
- 2026-06-05: Implemented in details view with horizontal image/video tiles, click-open behavior, provider-aware video still previews (YouTube/Vimeo/direct video fallback), and no separate View/Open link labels. UI tests passing.
