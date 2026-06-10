# ACR013 - Pages Must Not Scroll After a Submit

Date: 2026-06-10
Status: Implemented
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Global post-submit behaviour)

## Why this change

After submitting a form, some pages scroll (for example jumping to the bottom to reveal an inline
message). With feedback now delivered via top-right toasts (see ACR012), pages should remain at
the top after a submit so the user keeps a stable, predictable view and sees the toast feedback.

## Scope

In scope:
- Ensure pages do not scroll after a submit.
- Remove post-submit scroll-to-message behaviour now that feedback is shown via toasts.

Out of scope:
- Intentional scroll-position preservation on asynchronous panel refreshes (e.g. the programme
  builder retaining the user's current scroll position during an in-place update) which is a
  deliberate UX behaviour, not a post-submit jump.

## Acceptance criteria

1. Submitting a form does not cause the page to scroll to a new position.
2. After a submit, the page is displayed from the top (via Post-Redirect-Get or top-of-page
   render).
3. Feedback is visible via the top-right toast rather than requiring a scroll (links ACR012).
4. The programme builder's deliberate scroll-position retention during async refresh is
   unaffected.

## Implementation notes (as built)

- Removed the post-submit scroll-to-bottom script from `Views/CaseDetail/Index.cshtml` (and the
  associated `#case-programme-message` inline block, superseded by toasts).
- Note add/edit/delete and programme actions on the case page use Post-Redirect-Get, so the page
  reloads at the top.
- Left intact the builder's intentional `window.scrollTo({ top: currentScroll })` call in
  `Views/Programmes/Builder.cshtml`, which preserves (not jumps) the user's scroll position
  during asynchronous panel refresh.

## Risks and mitigations

Risks:
- Users could miss feedback that previously required a scroll to view.

Mitigations:
- Feedback is delivered via the top-right toast (ACR012), visible without scrolling.

## Verification

- Manual verification that case-page submits (note add/edit/delete, programme actions) return to
  the top of the page.
- Full solution builds clean (warnings-as-errors); UI and API in-memory suites green.
