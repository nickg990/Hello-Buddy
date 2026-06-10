# Acceptance Testing Error Log

Tracks defects and follow-up corrections raised during acceptance testing of the
Canine Physio Admin application (post ACR001-ACR009 implementation). These items are
distinct from increment build errors and capture user-observed issues during sign-off.

## ERR-AT-001: Copyright text still contains a "Privacy" link

**Date:** 2026-06-10  
**Status:** Open  
**Severity:** Low (branding / footer correctness)  
**Area:** Shared layout footer  
**Type:** UI content defect  
**Related:** ACR009 (Copyright Text Update)

### Symptom

The footer copyright still renders a trailing "- Privacy" link after the copyright/company text.

### Expected

Remove the "- Privacy" link from the copyright text so the footer shows only the copyright
symbol followed by "2026 Net Intentions Ltd".

### Notes

- Confirm whether the Privacy page should remain reachable via another route after the footer link is removed.

## ERR-AT-002: Navigation bar logo image too small

**Date:** 2026-06-10  
**Status:** Open  
**Severity:** Low (branding visibility)  
**Area:** Shared layout navbar (brand logo)  
**Type:** UI styling defect  
**Related:** ACR007 (Replace Text with Logo)

### Symptom

The navigation bar logo image renders smaller than desired after the ACR007 sizing adjustment.

### Expected

Increase the navigation bar logo image size by 50% while keeping the navbar height proportionate
and not so deep that it dominates the header.

### Notes

- Adjust the `.brand-logo` height in `src/HelloBuddy.Ui/wwwroot/css/site.css` (current value scaled up by 50%).

## ERR-AT-003: Add note button does not add type and note to the case notes box on submission

**Date:** 2026-06-10  
**Status:** Open  
**Severity:** High (core workflow defect)  
**Area:** Case detail notes  
**Type:** UI behavior defect  
**Related:** ACR002 (Add Note Persists to Case Notes Panel)

### Symptom

After clicking "Add note", the submitted note type and note text still do not appear in the case
notes box on submission. The note is not visibly added to the panel as expected.

### Expected

On submission, the new note (type and text) is immediately shown in the case notes panel and
persists on reopen of the case.

### Notes

- Re-verify the end-to-end path: case detail form post -> API `POST /api/cases/{id}/notes` ->
  repository persistence -> refreshed case notes render.
- Confirm the displayed notes collection is re-read and bound after a successful add.

## ERR-AT-004: Replace specific navigation links with a generic "Back" link

**Date:** 2026-06-10  
**Status:** Open  
**Severity:** Medium (navigation consistency / UX)  
**Area:** Global navigation (page-level back links)  
**Type:** UX change / defect

### Symptom

Pages use specific contextual links (for example "All cases") rather than a consistent way to
return to the previous page.

### Expected

Replace specific page navigation links (such as the "All cases" button) with a generic back link
in the top-left of each page that simply returns the user to the previous page.

### Notes

- Provide a single, consistent generic "Back" affordance per page (top-left).
- Behaviour should return to the previous page rather than a hard-coded destination.

## ERR-AT-005: Remove "Back to builder" button from the preview page

**Date:** 2026-06-10  
**Status:** Open  
**Severity:** Low (navigation consistency)  
**Area:** Programme preview page  
**Type:** UI cleanup  
**Related:** ERR-AT-004, ACR005 (Builder Preview)

### Symptom

The preview page shows a dedicated "Back to builder" button.

### Expected

Remove the "Back to builder" button from the preview page; navigation back will use the generic
top-left back link introduced in ERR-AT-004.

### Notes

- Keep the external "Save PDF" control; only remove the redundant back-to-builder button.

## ERR-AT-006: Image placeholder box and image-sourced video link not implemented in builder/preview

**Date:** 2026-06-10  
**Status:** Open  
**Severity:** High (missing required feature)  
**Area:** Programme builder and preview media layout  
**Type:** Missing implementation  
**Related:** ACR005 (Builder Preview with Boxed Media Layout)

### Symptom

The required image placeholder box on the left of the exercise rows has not been implemented on
the builder page or the preview page. There is also a separate video link rendered on the page.

### Expected

1. Implement an image placeholder box positioned to the left of each exercise on both the builder
   page and the preview page.
2. The exercise image renders inside that left-hand placeholder box.
3. The video link must come from the image itself (clicking the image opens the video) rather than
   being a separate link elsewhere on the page.
4. When no image/video exists, the placeholder box should still render consistently.

### Notes

- This requirement has been raised before and must not be skipped: the media layout is a boxed
  left-hand image, and the video is accessed by clicking that image (no separate text link).
- Apply consistently to both builder and preview surfaces.

## ERR-AT-007: Navigation bar is not sticky on all pages

**Date:** 2026-06-10  
**Status:** Resolved  
**Severity:** Medium (navigation consistency / UX)  
**Area:** Shared layout navbar  
**Type:** UI behavior defect  
**Related:** ACR006 (Sticky Non-Scrolling Menu)

### Symptom

Although a sticky navigation was introduced under ACR006, the navigation bar does not stay
fixed on all pages; on longer pages it scrolls out of view.

### Expected

The navigation bar must remain sticky (fixed at the top) on every page, regardless of page
length.

### Resolution

- Root cause: `position: sticky` was applied to `.site-navbar`, but its parent `<header>` was
  only as tall as the navbar, so the sticky element "unstuck" immediately once the header box
  scrolled past.
- Fix: made the `<header>` itself the sticky element by adding a `site-header` class in
  `src/HelloBuddy.Ui/Views/Shared/_Layout.cshtml` and a corresponding
  `.site-header { position: sticky; top: 0; z-index: 1030; }` rule in
  `src/HelloBuddy.Ui/wwwroot/css/site.css`. As `<header>` is a direct child of `<body>` (a tall
  containing block), the navigation now stays pinned for the full scroll on every page.
- The toast container (ACR012) z-index sits above the sticky header so notifications remain
  visible.

### Verification

- UI smoke test confirms the `site-header` class is present in the rendered layout.
- Full solution builds clean (warnings-as-errors); UI and API in-memory suites green.

