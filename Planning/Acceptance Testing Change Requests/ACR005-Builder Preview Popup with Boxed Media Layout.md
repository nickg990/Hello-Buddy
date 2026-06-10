# ACR005 - Builder Preview as a Dedicated In-App Page with Scrollable PDF and External Controls

Date: 2026-06-09
Updated: 2026-06-10
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Programme Builder preview)

## Why this change

The live preview on the Builder page is too compressed to be useful. Rather than an in-page
modal/popup (the original proposal), the preview should open as its own dedicated in-app page
(same tab, in-app navigation - not a new browser tab). The page should render the programme
preview as a PDF inside its own scrollable box, with Save PDF and navigation controls placed
outside the scrollable PDF area, and clear navigation back to the Builder.

This supersedes the earlier popup/modal approach in this ACR.

## Scope

In scope:
- Remove the inline (compressed) live preview from the Builder page.
- Add a Preview action on the Builder that navigates (same tab, in-app) to a dedicated preview
  page - not a new browser tab and not a modal/popup.
- On the preview page, render the programme preview as a PDF inside its own scrollable box.
- Place Save PDF and navigation controls outside the scrollable PDF box.
- Provide clear navigation back to the Builder (returning to the same programme/session context).
- Preserve the per-exercise media behaviour in the previewed PDF: each exercise shows its image
  thumbnail in a box to the left of the exercise information; the thumbnail is clickable and
  opens the exercise's video link in a new tab; where no video link exists, the user is informed
  that no video link information is available.

Out of scope:
- Changing the PDF document layout/content itself beyond the media/click-through behaviour above
  (this ACR is primarily about where and how the preview is shown).
- Editing exercise media from within the preview page.

## Acceptance criteria

1. The Builder page no longer shows the compressed inline live preview.
2. A Preview action navigates to a dedicated preview page in the same tab (not a new browser tab,
   not a modal popup).
3. The preview page displays the programme preview as a PDF within its own scrollable box.
4. The PDF box scrolls independently while the surrounding controls remain accessible.
5. A Save PDF control is available outside the scrollable PDF box and downloads/saves the PDF.
6. Navigation controls outside the PDF box allow returning to the Builder for the same programme.
7. Returning to the Builder restores the prior Builder context (programme/session in progress).
8. In the previewed PDF, each exercise shows its image thumbnail in a box to the left of the
   exercise information.
9. Clicking an exercise thumbnail that has a video link opens that link in a new tab.
10. For an exercise with no video link, the user is informed that no video link information is
    available (no broken/empty link is followed).

## Decision: no JavaScript popup for the no-video-link case

The preview uses the browser's native PDF viewer inside the iframe, so interactions within the
PDF (including link clicks) are handled by that viewer. We are deliberately not implementing a
separate JavaScript "no video link" OK popup. Instead, an exercise without a video link is shown
in the PDF as a non-linked thumbnail with a clear label indicating no video is available.

This avoids adding a custom PDF.js rendering layer purely for the popup. If the lack of an
explicit popup proves confusing in acceptance testing, revisit with a follow-up CR (for example
switch the preview to PDF.js to enable custom click handling).

## Technical feasibility and approach

Yes, this is technically possible.

- Dedicated page: Add a new admin route/page (for example `/builder/{programmeId}/preview`) and
  navigate to it from the Builder Preview button. This keeps it in the same tab and within the
  app's normal navigation, with a standard Back to Builder link/button.
- Displaying the PDF: The most reliable cross-browser way to embed a PDF in a scrollable box is
  to render the generated PDF in an `<iframe>` (or `<embed>`/`<object>`) whose `src` points at an
  endpoint that returns the PDF with `Content-Type: application/pdf` and an inline content
  disposition. Wrap the iframe in a container `div` with fixed/max height and `overflow: auto`
  so the PDF area scrolls independently while the Save PDF and navigation controls live outside
  that container.
  - An `<iframe>` is the recommended approach here. Browsers' built-in PDF viewers handle their
    own internal scrolling, but constraining the iframe inside a sized, scrollable box gives the
    requested "PDF in its own scrollable box" behavior and keeps external controls separate.
  - Alternative: a JavaScript PDF renderer (for example PDF.js) gives full control over rendering
    and avoids reliance on the browser's native viewer, at the cost of added complexity. Not
    required for this ACR unless native viewer inconsistencies are encountered.
- Save PDF: Provide a Save PDF button outside the iframe that triggers a download of the same
  generated PDF (content-disposition attachment), independent of the inline preview.
- Reuse existing generation: Point the preview iframe and the Save PDF action at the existing
  PDF generation path so preview and saved output stay identical.
- Media/click-through: The boxed left-thumbnail layout and clickable video links are part of the
  PDF's own content (the PDF already supports click-through video links per CR002-I3). In the
  embedded preview, clicking a thumbnail follows the PDF's link to open the video in a new tab.
  Exercises without a video link render a clear no-video-link indication rather than a live link.
  - Note: with a native-viewer iframe, the "no video link" feedback is expressed within the PDF
    content (for example a non-linked thumbnail with an explanatory label) rather than a separate
    JavaScript OK popup, since interaction inside the iframe is handled by the browser PDF viewer.

## Implementation notes (proposed)

- Replace the Builder's inline preview/modal trigger with navigation to the dedicated preview
  page (same tab).
- Add a preview page with: a header/control bar (Back to Builder, Save PDF) outside a sized,
  scrollable container holding the PDF iframe.
- Serve the PDF from an endpoint that supports inline (for the iframe) and attachment (for Save
  PDF) responses.
- Ensure Back to Builder preserves the in-progress programme/session context.

## Risks and mitigations

Risks:
- Native browser PDF viewers differ in toolbar/scroll behavior across browsers.
- Iframe sizing/scroll on smaller screens.
- Losing Builder in-progress state when navigating to the preview page.

Mitigations:
- Constrain the iframe in a sized `overflow: auto` container; consider PDF.js only if native
  viewers prove inconsistent.
- Use responsive height for the PDF container and verify on smaller viewports.
- Persist/restore Builder state (or pass programme context via the route) so Back to Builder
  returns cleanly.
