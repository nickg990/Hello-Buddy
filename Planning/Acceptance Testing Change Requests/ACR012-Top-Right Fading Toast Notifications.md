# ACR012 - Top-Right Fading Toast Notifications for All Pages

Date: 2026-06-10
Status: Implemented
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Shared layout / global feedback)

## Why this change

Success and error feedback is currently rendered as inline alert banners at the top of the page
content, which push the layout down and are easy to miss. Feedback should be presented as
non-intrusive popups in the top-right of the page that fade away automatically, applied
consistently across all pages that provide messages.

> Note: the original request referred to "tiny mce" for the messages. TinyMCE is a rich-text
> editor, not a notification library; the intended behaviour (fading top-right popups) is
> implemented with Bootstrap Toasts, which are already available in the project.

## Scope

In scope:
- Display success and error messages as toast popups in the top-right of the page.
- Toasts fade out automatically after a short delay and can be dismissed.
- Apply to all pages that currently produce success/error/published messages.
- Provide a client-side hook so asynchronous flows (e.g. the programme builder) raise the same
  toasts.

Out of scope:
- Persistent notification history / message centre.
- Per-message user preferences (e.g. disabling toasts).

## Acceptance criteria

1. Success messages appear as a top-right toast that fades automatically.
2. Error messages appear as a top-right toast (visually distinct from success) that fades.
3. The "published" message (with optional PDF download link) renders as a toast.
4. All message-producing pages use toasts (Home, Programmes, Cases, Owners, Case detail).
5. The programme builder's asynchronous save/feedback uses the same toast mechanism.
6. Implementation conforms to coding standards (no inline styles; styling via CSS classes).

## Implementation notes (as built)

- Layout: replaced the inline alert block in `Views/Shared/_Layout.cshtml` with a Bootstrap
  toast container positioned top-right, rendering `TempData["Saved"]`, `TempData["Error"]`, and
  `TempData["Published"]` (with optional `TempData["PublishedFile"]` download link).
- All five message-producing controllers funnel through the layout TempData keys, so they are
  covered automatically. `CaseDetailController` programme messages were converted from the
  bespoke `CaseProgrammeMessage` keys to the standard `Saved`/`Error` keys.
- JS: `wwwroot/js/site.js` initialises/shows server-rendered toasts on load and exposes
  `window.showToast(message, kind)` for client-side flows.
- Builder: `Views/Programmes/Builder.cshtml` feedback now calls `window.showToast` instead of
  injecting an inline alert.
- CSS: `wwwroot/css/site.css` adds `.app-toast-container` (z-index above the sticky navbar).

## Risks and mitigations

Risks:
- Toasts could overlap the sticky navigation.
- Important errors could fade before being read.

Mitigations:
- Toast container z-index sits above the navbar.
- Error toasts use a longer display delay than success toasts and remain dismissible.

## Verification

- UI smoke test confirms the toast container is present in the rendered layout.
- Manual verification of success/error/published toasts across pages.
- Full solution builds clean (warnings-as-errors); UI and API in-memory suites green.
