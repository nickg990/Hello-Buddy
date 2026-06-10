# ACR014 - Realtime Loading Spinner on Preview PDF Page

Date: 2026-06-10
Status: Implemented
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Programme Preview PDF page)

## Why this change

The Preview PDF page embeds the generated programme PDF in an iframe. Generating and loading the
PDF can take a moment, during which the user sees a blank frame with no indication that anything
is happening. A realtime loading spinner is needed to show the preview is being prepared.

## Scope

In scope:
- Show a loading spinner over the PDF preview frame while the PDF is generating/loading.
- Hide the spinner automatically once the preview has finished loading.

Out of scope:
- Spinner/progress indicators on other pages.
- Changes to PDF generation logic or performance.

## Acceptance criteria

1. When the Preview PDF page opens, a spinner is displayed over the preview frame area.
2. The spinner includes accessible status text (e.g. "Loading PDF preview…").
3. The spinner is removed automatically when the PDF preview has loaded.
4. Implementation conforms to coding standards (no inline styles; styling via CSS classes).

## Implementation notes (as built)

- View: `Views/Programmes/Preview.cshtml` adds a Bootstrap `spinner-border` overlay over the
  preview frame with accessible status text, and a small script that hides the overlay on the
  iframe's `load` event (with a `readyState` safety net for already-loaded frames).
- CSS: `wwwroot/css/site.css` adds `.programme-preview-loading` (centred overlay) and makes the
  preview frame wrap `position: relative` so the overlay is positioned correctly.

## Risks and mitigations

Risks:
- Spinner could persist if the iframe load event does not fire.

Mitigations:
- A `readyState === 'complete'` safety net hides the spinner if the frame loaded before the
  handler was bound.

## Verification

- UI smoke test confirms the spinner overlay and preview frame markup render on the Preview page.
- Full solution builds clean (warnings-as-errors); UI suite green.
