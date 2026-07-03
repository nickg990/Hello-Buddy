# ACR017 - Loading Spinner on PDF Download

Date: 2026-06-12
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Programme PDF Viewer — Download PDF button)

## Why this change

On the PDF Viewer page (currently "Preview PDF"), clicking **Download PDF** generates the PDF
server-side, which can take a moment. There is no feedback during this time, so the user may think
nothing happened and click again. A spinner should appear when Download is clicked and finish once
the download has completed (the file has started downloading).

## Scope

In scope:
- Show a spinner / busy state on (or beside) the Download PDF button when it is clicked.
- Clear the spinner once the download has been delivered to the browser.

Out of scope:
- Changes to PDF generation logic or performance.
- The existing preview-iframe loading spinner (ACR014) — unaffected.

## Acceptance criteria

1. Clicking **Download PDF** immediately shows a visible spinner/busy indicator (e.g. on the
   button) with accessible status text.
2. The Download button is disabled (or otherwise guarded) while the download is in progress to
   prevent duplicate clicks.
3. The spinner is cleared and the button re-enabled once the download has been delivered.
4. The download itself continues to work exactly as today (same file content).
5. Implementation conforms to coding standards (no inline styles; styling via CSS classes; logic
   in `site.js` or a small page script consistent with the existing preview spinner pattern).

## Implementation guidance

- The Download link in `src/HelloBuddy.Ui/Views/Programmes/Preview.cshtml` currently points at
  `PreviewPdf` with `download = true` (an anchor that triggers a file download, not a navigation).
- Because a file download does not raise a page `load`/`unload` event reliably, use a
  **cookie-based download-complete signal** pattern: the server sets a short-lived response cookie
  (e.g. `pdf-download-complete=<token>`) on the file response; client JS records a token before
  starting the download, shows the spinner, then polls for the matching cookie and hides the
  spinner when it appears, with a sensible timeout fallback (e.g. 30s) to avoid a stuck spinner.
  - Server: in the `PreviewPdf` action (download branch) in
    `src/HelloBuddy.Ui/Controllers/ProgrammesController.cs`, append the response cookie when a
    download token query parameter is supplied.
  - Client: a small script (page-level or `site.js`) wires the Download button to generate a
    token, append it to the URL, show the spinner, and poll `document.cookie`.
- Reuse Bootstrap `spinner-border` markup and an accessible `role="status"` text label,
  consistent with the ACR014 preview spinner.

## Risks and mitigations

Risks:
- A file download has no native completion event, so a naive approach leaves the spinner stuck.

Mitigations:
- Use the cookie-token completion signal plus a timeout fallback that always clears the spinner.

## Verification

- Manual: clicking Download shows the spinner, which clears when the file save dialog/download
  begins; the button cannot be double-submitted mid-download.
- UI smoke: the Download button renders the spinner markup/attributes and the download endpoint
  still returns the PDF.
- Full solution builds clean; suites green.
