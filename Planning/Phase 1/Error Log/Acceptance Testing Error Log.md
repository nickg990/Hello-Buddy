# Acceptance Testing Error Log

Tracks defects and follow-up corrections raised during acceptance testing of the
Canine Physio Admin application (post ACR001-ACR009 implementation). These items are
distinct from increment build errors and capture user-observed issues during sign-off.

## Iteration 3 defects (ERR-AT-011 to ERR-AT-014) — recommended implementation order

Raised 2026-06-14 from acceptance testing of the iteration-2 build (ACR025-ACR027,
ERR-AT-009/010). Recommended order for implementation, chosen to front-load the
highest-impact usability fix and to group the two PDF-template changes into one pass:

| Priority | Item | Severity | Rationale for order |
|----------|------|----------|---------------------|
| 1 | **ERR-AT-012** Login button white text | High | Blocks the primary call to action on the login page; smallest, isolated CSS change; do first. |
| 2 | **ERR-AT-014** Reduce breadcrumb H4 to normal text | Low | Quick, isolated view-markup change across the ACR027 pages; clears a visible polish issue early. |
| 3 | **ERR-AT-011** Add logo to PDF header | Medium | First of the two PDF-template changes; embeds the logo asset + header markup. |
| 4 | **ERR-AT-013** PDF AM/PM continuous flow | Medium | Second PDF-template change (CSS `break-inside`); do alongside/after ERR-AT-011 since both edit `Programme.cshtml` — one coordinated pass avoids touching the template twice. |

Notes:
- ERR-AT-011 and ERR-AT-013 both edit `src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml`; do
  them as one coordinated PDF pass (logo + page-break) and run the PDF suite once.
- ERR-AT-012 and ERR-AT-014 are independent UI changes and can ship together or separately.
- Apply the same gate discipline as the previous round: strict build (warnings-as-errors) +
  full test suite green per change; update/extend tests where markup assertions change.

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

## ERR-AT-008: All styling lost after I89 code-review security fix (static assets blocked by auth fallback policy)

**Date:** 2026-06-12  
**Status:** Open  
**Severity:** Critical (completely broken UI styling for all unauthenticated pages)  
**Area:** `HelloBuddy.Ui` middleware pipeline — `Program.cs`, static assets  
**Type:** Regression introduced by Increment 8/9 code-review fix I89-CR-003  

### Symptom

After the I89-CR-003 code-review fix introduced a deny-by-default fallback authorization policy,
the entire application loses all CSS and JavaScript styling. The login page renders as an unstyled
HTML document. All navigation bars, layouts, Bootstrap styles, and site-specific styles are absent.

### Root cause

`app.MapStaticAssets()` in .NET 9 is an **endpoint-based** API, unlike the older
`app.UseStaticFiles()` middleware. Because it is called *after* `app.UseAuthorization()` in the
current middleware pipeline, every static file endpoint (`/css/site.css`,
`/lib/bootstrap/bootstrap.min.css`, `/js/site.js`, favicon, etc.) is subject to the fallback
authorization policy (`RequireAuthenticatedUser`).

When an unauthenticated browser fetches a CSS or JS file, the authorization middleware intercepts
the request, determines the user is not authenticated, and issues a `302 Found` redirect to
`/Account/Login?returnUrl=/css/site.css`. The browser receives an HTML document (the login page)
instead of stylesheet bytes, so no styles are applied.

This breaks the login page specifically — it is marked `[AllowAnonymous]` at the controller
action level so its HTML renders, but every subsequent browser request for CSS/JS is redirected,
leaving the page completely unstyled. Authenticated users are largely unaffected because the
cookie is sent with same-origin asset requests, but the login page and any other anonymous-marked
page lose all styling entirely.

```
Unauthenticated GET /css/site.css
  → UseAuthorization middleware: no principal
  → Fallback policy: RequireAuthenticatedUser
  → CookieAuthenticationHandler: challenge
  → 302 Location: /Account/Login?returnUrl=%2Fcss%2Fsite.css
  → Browser uses HTML instead of CSS
  → Styling absent
```

### Affected files

- `src/HelloBuddy.Ui/Program.cs` — `app.MapStaticAssets()` placement after `app.UseAuthorization()`

### Proposed fix

Chain `.AllowAnonymous()` onto the `MapStaticAssets()` call. This exempts all static file
endpoints from the fallback policy while leaving all controller/page endpoints still protected:

```csharp
// Before (broken):
app.MapStaticAssets();

// After (correct):
app.MapStaticAssets().AllowAnonymous();
```

The call on the controller route `MapControllerRoute(...).WithStaticAssets()` is unrelated —
`WithStaticAssets()` only enables asset fingerprinting/caching hints and does not register
additional authorization-checked endpoints. It does not need to change.

### Secondary consideration

If any future page-level static assets are served via a separate `MapStaticAssets()` call (e.g.
scoped to a Razor Pages group), each such call must also carry `.AllowAnonymous()` for the same
reason.

### Verification steps (post-fix)

1. Run the local stack (`run-local-admin-stack.ps1`).
2. Open the app in a private/incognito window (no auth cookie).
3. Confirm the login page renders with full Bootstrap and Hello Buddy styling.
4. Log in and confirm all authenticated pages continue to render correctly.
5. Run the full test suite: `dotnet test HelloBuddy.Admin.sln` — all three projects must be green.

## ERR-AT-009: Replace history-based "Back" links with stable, explicit back navigation

**Date:** 2026-06-14
**Status:** Open
**Severity:** High (core navigation defect — users get lost / land on the wrong page)
**Area:** Global navigation (page-level back links) — `_BackLink.cshtml`, `site.js`, page views
**Type:** UX defect / navigation correctness
**Related:** ERR-AT-004 (generic back link, now superseded), ERR-AT-005, ACR021

### Symptom

The current "Back" affordance uses the browser history stack (`window.history.back()`, wired in
`src/HelloBuddy.Ui/wwwroot/js/site.js` against `[data-back-link]` elements, with a fallback href in
`src/HelloBuddy.Ui/Views/Shared/_BackLink.cshtml`). Because it depends on whatever the user did
before, it is unpredictable: it can return to an unexpected page, re-trigger a stale POST page, or
go nowhere useful when a page is opened directly or after a redirect. Acceptance testers found the
back behaviour confusing.

### Expected

Remove the history-stack back link and replace it with a **stable, explicit back button** on each
page that always navigates to a fixed, known parent — never the browser history. The button is a
**secondary** button (styled per ACR022 `btn-outline-secondary`, brand-coloured per ACR025) placed
where the current back link sits.

Required back-navigation targets:

| Page                          | Back button | Target page                                                                 |
| ----------------------------- | ----------- | --------------------------------------------------------------------------- |
| Owners (list)                 | **None**    | No back button on this page.                                                |
| Pets (list)                   | **None**    | No back button on this page.                                                |
| Pet Details                   | Yes         | Pets (list) — `Pets/Index`.                                                 |
| Treatment Case (case detail)  | Yes         | Pet Details of the **selected pet** — `Pets/Details` for the case's `PetId`.|
| Exercise Programme Builder     | Yes         | Treatment Case for the **selected pet** — `CaseDetail/Index` for `TreatmentCaseId`. |
| PDF Viewer (Preview)          | Yes         | Treatment Case for the pet — `CaseDetail/Index` for the programme's `TreatmentCaseId`. |
| PDF History                   | Yes         | Treatment Case for the pet — `CaseDetail/Index` for the programme's `TreatmentCaseId`. |

### Root cause

`_BackLink.cshtml` renders a `data-back-link` anchor whose click handler in `site.js` calls
`window.history.back()` whenever `window.history.length > 1`, only falling back to the model href
when there is no history. So the destination is the previous browser entry, not the logical parent.

### Affected files

- `src/HelloBuddy.Ui/Views/Shared/_BackLink.cshtml` — replace the history-driven partial with an
  explicit-target secondary back button (or retire it in favour of per-page buttons).
- `src/HelloBuddy.Ui/wwwroot/js/site.js` — remove the `[data-back-link]` `history.back()` handler.
- `src/HelloBuddy.Ui/Views/Pets/Details.cshtml` — back to `Pets/Index`.
- `src/HelloBuddy.Ui/Views/CaseDetail/Index.cshtml` — back to `Pets/Details` for `Model.Case.PetId`
  (currently points to `Cases/Index` via the history partial).
- `src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml` — back to `CaseDetail/Index` for
  `programme.TreatmentCaseId` (target already correct; convert to a stable explicit button, not
  history).
- `src/HelloBuddy.Ui/Views/Programmes/Preview.cshtml` — back to `CaseDetail/Index` for
  `Model.TreatmentCaseId` (currently points to the Builder via the history partial).
- `src/HelloBuddy.Ui/Views/Programmes/History.cshtml` — back to `CaseDetail/Index` for
  `Model.TreatmentCaseId` (currently points to the Builder via the history partial).
- Any other view currently using `<partial name="_BackLink" ... />` (e.g. Owner Details, Case Edit)
  — audit and either point at the correct explicit parent or remove if not in the list.

### Proposed fix

1. Replace `_BackLink.cshtml` with a small partial that renders an **explicit-href secondary back
   button** from a required model value, with no JavaScript history behaviour. For example:

   ```cshtml
   @model (string Href, string Label)
   <a href="@Model.Href" class="btn btn-outline-secondary mb-3">&larr; @Model.Label</a>
   ```

   (Or keep the existing partial signature but drop the `data-back-link` attribute so it is a plain
   link, and remove the JS handler.)

2. Remove the `[data-back-link]` click handler from `site.js` entirely so no page can fall back to
   `history.back()`.

3. Update each page to render the back button with its fixed target using tag-helper routing,
   binding ids from the view model:
   - Pet Details → `asp-controller="Pets" asp-action="Index"`.
   - Treatment Case → `asp-controller="Pets" asp-action="Details" asp-route-id="@Model.Case.PetId"`.
   - Builder → `asp-controller="CaseDetail" asp-action="Index" asp-route-id="@programme.TreatmentCaseId"`.
   - PDF Viewer → `asp-controller="CaseDetail" asp-action="Index" asp-route-id="@Model.TreatmentCaseId"`.
   - PDF History → `asp-controller="CaseDetail" asp-action="Index" asp-route-id="@Model.TreatmentCaseId"`.

4. Remove the back button from the **Owners** and **Pets** list pages.

5. Label the buttons clearly for the destination (e.g. "Back to Pets", "Back to pet",
   "Back to treatment case") so the target is obvious; styling is secondary (`btn-outline-secondary`).

### Verification steps (post-fix)

1. From each page, click Back and confirm it lands on the exact target above, regardless of how the
   page was reached (direct URL, after a redirect, after a POST).
2. Confirm Owners and Pets list pages have **no** back button.
3. Confirm there is no remaining `history.back()` behaviour anywhere (search for `data-back-link`).
4. Confirm the back buttons are styled as secondary (ACR022/ACR025) and meet the touch-target rule.
5. Update/extend UI smoke tests for the new explicit targets; remove assertions tied to the old
   history link. Full solution builds clean (warnings-as-errors); suites green.

## ERR-AT-010: Pages must not jump/scroll on submit or refresh (full sweep)

**Date:** 2026-06-14
**Status:** Open
**Severity:** High (pervasive UX defect across the application)
**Area:** All pages — form submit and page refresh behaviour
**Type:** UX defect / scroll-position stability
**Related:** ACR013 (No Page Scroll After Submit), ACR012 (toasts), TD-004 (Builder scroll anchor)

### Symptom

On several pages the viewport "jumps" (scrolls to an unexpected position) when a form is submitted
or when the page refreshes. ACR013 addressed the case-detail page specifically, but acceptance
testing found the problem is broader. This is a high-priority, thorough sweep: **every** page must
hold a stable position on submit and refresh.

### Expected

1. Submitting any form does not cause the page to jump to a new scroll position.
2. After a submit, the page is shown from a stable, predictable position (top of page via
   Post-Redirect-Get for full-page submits), with feedback delivered via the top-right toast
   (ACR012) rather than requiring a scroll.
3. Refreshing any page does not cause a visible jump; the page renders from a stable position.
4. Deliberate scroll-position retention (the programme builder's in-place async panel refresh,
   ACR013 / TD-004) is preserved — that is intentional retention, not a jump, and must not regress.

### Root cause (to confirm during the sweep)

Likely contributors to investigate page-by-page:

- Full-page form posts that re-render the same view (return `View(...)`) instead of using
  Post-Redirect-Get, leaving the browser at a scrolled position or re-posting on refresh.
- In-page anchor/`#fragment` navigation or `scrollIntoView`/`window.scrollTo` calls left over from
  the pre-toast inline-message era (ACR013 removed one such script on the case page; others may
  remain).
- Autofocus on a control low on the page causing the browser to scroll to it on load.
- Modal show/hide or validation re-render shifting layout and moving the scroll anchor.

### Affected files (sweep — confirm each)

Audit every view with a form and every controller POST action that returns a view rather than a
redirect:

- `src/HelloBuddy.Ui/Views/**/*.cshtml` — every page containing a `<form>` (Owners, Pets,
  Pet Details, Cases create/edit, Case detail notes + programme actions, Exercises create/edit/
  media, Programme Builder, PDF Viewer publish, PDF History actions, Account login/change password,
  Admin practitioners/data control).
- `src/HelloBuddy.Ui/Controllers/*.cs` — every `[HttpPost]` action; prefer `RedirectToAction`
  (Post-Redirect-Get) on success so a refresh re-issues a GET, not a re-POST, and the page renders
  from the top.
- `src/HelloBuddy.Ui/wwwroot/js/site.js` — audit for any residual scroll calls or fragment
  handling that could move the viewport on load/submit.

Known exception (do not change): the Builder's intentional async scroll retention in
`src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml` (ACR013 / TD-004).

### Proposed fix

1. **Post-Redirect-Get everywhere for full-page submits.** Every controller POST that currently
   returns `View(...)` on success should `RedirectToAction(...)` to the relevant GET, so refresh is
   safe and the page renders from the top. Validation failures may re-render the view, but should
   render from the top (not scroll to the error); rely on the toast/summary plus inline field
   validation rather than scrolling.
2. **Remove stray scroll triggers.** Search the views and `site.js` for `scrollIntoView`,
   `window.scrollTo`, `location.hash`, `#`-only anchors used as buttons, and `autofocus` on
   low-page controls; remove or neutralise any that cause a jump on load/submit. Preserve only the
   documented Builder retention.
3. **Feedback via toasts.** Confirm all submit feedback uses the ACR012 top-right toasts (visible
   without scrolling) so there is no reason to scroll to an inline message.
4. **Stable layout on validation re-render.** Where a form re-renders with validation errors, keep
   the page anchored at the top and surface errors via the validation summary + toast.

### Verification steps (post-fix)

For **every** page with a form, and for general refresh behaviour:

1. Scroll down, submit the form, and confirm the page does not jump to an unexpected position
   (it returns to the top via PRG for full-page submits).
2. After a successful submit, refresh (F5) and confirm no re-POST prompt and no jump.
3. Confirm feedback appears in the top-right toast without needing to scroll.
4. Confirm the Builder's deliberate async scroll retention still works (no regression).
5. Document each page checked (a short checklist in the change set) so the sweep is demonstrably
   thorough. Full solution builds clean (warnings-as-errors); UI and API in-memory suites green.

### Notes

- This is explicitly a thorough, application-wide sweep, not a single-page fix. Treat ACR013 as the
  pattern to generalise to all pages.

## ERR-AT-011: Add the Hello Buddy logo to the PDF header (replace the "HB" placeholder)

**Date:** 2026-06-14
**Status:** Open
**Severity:** Medium (branding correctness on the client-facing PDF)
**Area:** Programme PDF template — `HelloBuddy.Admin.Pdf`
**Type:** Branding / missing asset
**Related:** ACR026 (PDF visual restyle), ACR007 (logo in navbar)

### Symptom

The generated programme PDF header renders a plain text "HB" inside a coloured disc as a
placeholder (`<div class="pdf-header-disc"><span>HB</span></div>` in
`src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml`). The actual Hello Buddy logo is not shown.

### Expected

1. Display the real Hello Buddy logo in the PDF header where the "HB" placeholder disc currently
   sits.
2. Increase the logo size so its height matches the visual height of the header text block (the
   title line plus the two sub-lines) — i.e. the logo should read as a proper brand mark, not a
   small token. It should also visually balance with the footer text height.
3. The logo must render reliably in the headless-Chromium (PuppeteerSharp) PDF pipeline, which
   renders **self-contained HTML in a separate process** — it cannot reach the UI project's
   `wwwroot`.

### Root cause

The PDF template was authored with a literal "HB" text disc placeholder (ACR026) because the PDF
service (`HelloBuddy.Admin.Pdf` / `HelloBuddy.PdfService`) is a separate process and the template
is compiled from an **embedded resource**; it has no access to the UI project's static assets at
`src/HelloBuddy.Ui/wwwroot/img/logo-disc.svg`.

### Affected files

- `src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml` — the `.pdf-header-disc` block and its
  `.pdf-header-disc` / `.pdf-header` CSS.
- `src/HelloBuddy.Admin.Pdf/HelloBuddy.Admin.Pdf.csproj` — currently embeds only
  `Templates\Programme.cshtml`; a logo asset must also be embedded.
- `src/HelloBuddy.Admin.Pdf/RazorProgrammePdfTemplate.cs` — loads the template by manifest resource
  name; the logo can be loaded the same way and injected.
- Source asset: `src/HelloBuddy.Ui/wwwroot/img/logo-disc.svg` (an SVG that itself embeds a base64
  PNG of the dog mark; ~large file).

### Proposed fix

1. Add the logo as an **embedded resource** in `HelloBuddy.Admin.Pdf` (copy `logo-disc.svg` into
   e.g. `src/HelloBuddy.Admin.Pdf/Templates/logo-disc.svg` and add an `<EmbeddedResource>` entry
   in the `.csproj`). Embedding keeps the PDF service self-contained (no cross-project file or
   network dependency at render time).
2. Inline the logo into the rendered HTML so the headless browser needs no external fetch. Options
   (pick the simplest that renders reliably):
   - Read the embedded SVG bytes in `RazorProgrammePdfTemplate`, base64-encode, and expose to the
     template as a `data:image/svg+xml;base64,...` URI rendered in an `<img>`; **or**
   - Inline the SVG markup directly into the header.
   Prefer a data-URI `<img>` for predictable sizing.
3. Replace the `.pdf-header-disc`/`<span>HB</span>` placeholder with the logo image. Size it so its
   height matches the header text block (roughly the title 17pt + two 9pt sub-lines ≈ a ~48-64px
   tall mark; tune to match). Keep it circular/branded and vertically centred in the dark header
   band. Ensure it also balances visually with the footer.
4. Confirm the existing PDF tests still pass (they assert on `<div class="ex-media">` and the
   image-as-video-link markup — unrelated to the header — so they should be unaffected). Add a
   small assertion that the header no longer contains the literal `>HB<` placeholder and that an
   `<img`/inline `<svg` is present in the header, if a cheap assertion is feasible.

### Verification steps (post-fix)

1. Publish/preview a programme PDF and confirm the Hello Buddy logo renders in the header in place
   of the "HB" text.
2. Confirm the logo height visually matches the header text block and balances with the footer.
3. Confirm the logo renders in the actual PuppeteerSharp output (not just a browser preview) — i.e.
   the asset is inlined and not fetched from a URL.
4. Full solution builds clean (warnings-as-errors); PDF and related suites green.

## ERR-AT-012: Login (Sign In) button text not visible — must be white on brand primary

**Date:** 2026-06-14
**Status:** Open
**Severity:** High (login page usability — primary call to action is unreadable)
**Area:** `HelloBuddy.Ui` button styling — login page and navbar Sign In
**Type:** CSS regression / contrast defect
**Related:** ACR025 (brand button colours)

### Symptom

On the login page the "Sign In" button label is not visible (appears to render dark or same-colour
text on the brand background). The button should use the brand **primary** colours with **white**
label text, exactly like every other primary button.

### Expected

The Sign In button (login form and the navbar) renders as a solid brand-primary (`#4A7A96`) button
with **white** label text at rest, and `#28404F` on hover — consistent with the ACR025 primary
button styling used across the app.

### Root cause (to confirm)

Both Sign In buttons already use `btn btn-primary`:

- `src/HelloBuddy.Ui/Views/Account/Login.cshtml` (line ~33): `class="btn btn-primary w-100"`.
- `src/HelloBuddy.Ui/Views/Shared/_Layout.cshtml` (line ~70): `class="btn btn-primary btn-sm"`.

ACR025 restyles `.btn-primary` in `src/HelloBuddy.Ui/wwwroot/css/site.css` purely via Bootstrap 5
CSS custom properties (`--bs-btn-color: #fff; --bs-btn-bg: var(--hb-brand-primary); ...`). The most
likely cause of the invisible label is that the **text colour is not resolving to white** in the
rendered page — candidates to confirm in the running stack:

- A CSS **load-order / specificity** issue: the per-view scoped bundle (`HelloBuddy.Ui.styles.css`)
  loads after `site.css`; if any scoped rule sets `.btn-primary` background but not colour, the
  label can inherit a dark colour.
- A **CSS-variable resolution** edge case where `--bs-btn-color` is not applied (older cached CSS,
  or a residual override) leaving the default link/body colour on the label.
- Stale cached CSS from before ACR025 (verify with a hard refresh / `asp-append-version`).

### Affected files

- `src/HelloBuddy.Ui/wwwroot/css/site.css` — the ACR025 `.btn-primary` block (≈ line 330).
- `src/HelloBuddy.Ui/Views/Account/Login.cshtml` — Sign In submit button.
- `src/HelloBuddy.Ui/Views/Shared/_Layout.cshtml` — navbar Sign In link-button.
- `src/HelloBuddy.Ui/Views/Shared/_Layout.cshtml.css` — confirm no residual `.btn-primary` rule
  (the ACR025 change removed the old `#1b6ec2` block; verify nothing reintroduced it).

### Proposed fix

1. Harden the ACR025 `.btn-primary` rule so the label is **guaranteed white** regardless of
   CSS-variable resolution or scoped-bundle ordering: in addition to the `--bs-btn-*` variables,
   add explicit literal declarations on `.btn-primary` (and `:hover`/`:focus`/`:active`):
   `color: #fff;` with `background-color: var(--hb-brand-primary);` (and `#28404F` on hover/active).
   Keep the brand hex as the single source of truth via the `--hb-*` tokens where possible.
2. Verify there is no conflicting `.btn-primary` rule in any scoped `*.cshtml.css` that sets a
   background without a matching white `color`.
3. Confirm the brand `--hb-brand-primary` token is defined at `:root` in `site.css` (it is) and
   that `site.css` loads after Bootstrap (it does, per `_Layout.cshtml`).

### Verification steps (post-fix)

1. Load the login page (incognito / hard refresh to defeat CSS caching) and confirm the "Sign In"
   label is white and clearly legible on the brand-teal button.
2. Confirm the navbar Sign In button (signed-out state) is identical brand-primary with white text.
3. Confirm hover/active states show `#28404F` with white text.
4. Confirm WCAG AA contrast for the label (white on `#4A7A96` ≈ 4.5:1; on `#28404F` ≈ 9:1).
5. Full solution builds clean; UI suite green.

## ERR-AT-013: PDF AM/PM sessions should flow continuously, not force a page break

**Date:** 2026-06-14
**Status:** Open
**Severity:** Medium (PDF whitespace / layout efficiency)
**Area:** Programme PDF template — `HelloBuddy.Admin.Pdf`
**Type:** Print-layout defect
**Related:** ACR026 (PDF visual restyle), ERR-AT-011

### Symptom

When a programme has AM and PM sessions, the PM session is pushed onto a second page even when
only a few exercises exist, leaving large empty space at the bottom of page 1. Sessions should
flow continuously down the page and only break across pages when the content genuinely overflows.

### Expected

1. AM and PM (and any further) sessions render **continuously** in the document flow; a new session
   does not force a page break.
2. Page breaks happen naturally only when content overflows the page.
3. Individual exercise rows should still avoid being split mid-row across a page boundary
   (keep `break-inside: avoid` on `.ex-row`), but a whole **session** must not reserve a fresh page.

### Root cause

In `src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml` the `.session` block carries
`break-inside: avoid`:

```css
.session { break-inside: avoid; margin-bottom: 6mm; }
```

`break-inside: avoid` on the whole session tells the renderer to keep the entire session
(heading + all its exercise rows) together on one page; when the AM session leaves insufficient
room, the PM session is moved wholesale to page 2, creating the trailing whitespace.

### Affected files

- `src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml` — the `.session` CSS rule (and confirm
  `.ex-row` retains `break-inside: avoid`).

### Proposed fix

1. Remove `break-inside: avoid` from `.session` so sessions flow continuously and can split across
   a page boundary when needed.
2. Keep `break-inside: avoid` on `.ex-row` so an individual exercise (media box + details) is not
   split across pages.
3. Optionally add `break-inside: avoid` to the `.session-heading` (and keep the heading with at
   least the first row via `break-after: avoid` on the heading) so a session title does not strand
   at the very bottom of a page — but do **not** reapply it to the whole `.session`.

### Verification steps (post-fix)

1. Render a programme with AM + PM sessions each containing only a few exercises; confirm both
   sessions appear on page 1 with no large trailing gap.
2. Render a programme with many exercises; confirm content flows onto subsequent pages naturally
   and no exercise row is split across a page boundary.
3. Full solution builds clean; PDF suite green.

## ERR-AT-014: Breadcrumb subtitle (H4) is too dominant — reduce to normal text size

**Date:** 2026-06-14
**Status:** Open
**Severity:** Low (visual hierarchy / polish)
**Area:** `HelloBuddy.Ui` page headers — breadcrumb subtitle
**Type:** UI styling refinement
**Related:** ACR027 (page title H2 + breadcrumb subtitle)

### Symptom

The breadcrumb trail rendered under each page's H2 title uses an `<h4>` element, which is visually
too large and competes with / dominates the page. It should read as a normal-size breadcrumb, not a
heading.

### Expected

The breadcrumb trail renders at **normal body text size** (subordinate to the H2 page title) while
keeping the `" . "` separators, the navigable links, and the muted styling from ACR027.

### Root cause

ACR027 rendered the breadcrumb in an `<h4 class="text-muted">` element. The `<h4>` size is too
prominent for a subtitle/breadcrumb.

### Affected files (all use `<h4 class="text-muted">` for the breadcrumb)

- `src/HelloBuddy.Ui/Views/Owners/Details.cshtml`
- `src/HelloBuddy.Ui/Views/Pets/Index.cshtml` (subtitle "All registered pets")
- `src/HelloBuddy.Ui/Views/Pets/Details.cshtml`
- `src/HelloBuddy.Ui/Views/CaseDetail/Index.cshtml`
- `src/HelloBuddy.Ui/Views/Cases/Edit.cshtml` (two `<h4>` — edit and new-case variants)
- `src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml`
- `src/HelloBuddy.Ui/Views/Programmes/Preview.cshtml`
- `src/HelloBuddy.Ui/Views/Programmes/History.cshtml`

(Do **not** change `src/HelloBuddy.Ui/Views/Home/AccessDenied.cshtml`, whose `<h4 class="alert-heading">`
is an unrelated alert heading.)

### Proposed fix

1. Replace the breadcrumb `<h4 class="text-muted">` wrapper with a normal-size element — e.g.
   `<p class="text-muted mb-2 page-breadcrumb">…</p>` (or a `<nav>`/`<div>`), keeping the inner
   link markup and `" . "` separators unchanged.
2. Ensure the page **title** stays an `<h2>` and remains the visually dominant heading; the
   breadcrumb is plain body-size muted text beneath it.
3. Keep the heading-outline accessibility correct (the H2 is the page heading; the breadcrumb is no
   longer a heading element, which is fine for a breadcrumb).
4. Apply consistently across all listed views so the breadcrumb looks identical everywhere.

### Verification steps (post-fix)

1. Visit each listed page; confirm the breadcrumb renders at normal text size and the H2 title is
   clearly dominant.
2. Confirm the `" . "` separators and clickable ancestor links from ACR027 still work.
3. Confirm no heading-hierarchy regression.
4. Full solution builds clean; UI suite green (update any smoke assertions that match the old
   `<h4>` breadcrumb markup).

## ERR-AT-015: Edit Exercise saves a blank image over the current image

**Date:** 2026-06-15
**Status:** Open
**Severity:** High (data loss — existing exercise image is destroyed on save)
**Area:** `HelloBuddy.Ui` Exercise editor (Edit Exercise)
**Type:** UI/binding defect causing data loss
**Related:** Exercise media editing

### Symptom

When editing an existing exercise, the "Current image" panel correctly shows the saved image, but
the "Selected image (pending save)" panel is blank ("No image selected"). If the practitioner edits
any other field and clicks **Save exercise** *without* re-uploading the image, the save action wipes
the exercise's image — the previously-saved image is replaced with a blank/empty value.

### Steps to reproduce

1. Open an exercise that already has an image: `/Exercises/{id}/Edit`.
2. Observe "Current image" shows the image; "Selected image (pending save)" shows "No image selected".
3. Change any unrelated field (e.g. the title or instructions) and do **not** choose a new image.
4. Click **Save exercise**.
5. Reopen the exercise — the image is now gone.

### Expected

Saving an exercise without choosing a new image (and without explicitly removing it) must **preserve**
the existing image. The behaviour should mirror the video field, which round-trips correctly and is
not lost on save.

### Root cause

The Edit form has no field that posts the current image URL back to the server:

- The image is shown read-only via `Url.Action("Image", "Exercises", new { id = Model.ExerciseId })`,
  and the "Selected image" panel is a client-side preview only (it is only populated when the user
  picks a file). There is **no** `asp-for="Form.ImageUrl"` input in the form
  (see [src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml#L73-L100)).
- By contrast the video URL **is** a posted input
  (`<input asp-for="Form.VideoUrl" ... />`, [Edit.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml#L60-L67)),
  which is exactly why the video survives a save but the image does not.
- On POST, `vm.Form.ImageUrl` therefore binds to `null`. In
  [ExercisesController.ApplyImageSelectionAsync](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Controllers/ExercisesController.cs#L302-L321),
  when no new file is uploaded and `RemoveImage` is false, `vm.Form.ImageUrl` is left as the bound
  value (`null`). `UpdateExerciseAsync` then persists the null/blank image, overwriting the saved one.

### Proposed fix

Make the current image URL round-trip on save, mirroring how the video URL is preserved (this is the
"set the selected image to the current image" behaviour the user described). Preferred minimal change:

1. Add a hidden input to the Edit form so the current image URL is posted back:
   `<input type="hidden" asp-for="Form.ImageUrl" />`.
2. Confirm `ApplyImageSelectionAsync` order of precedence still holds:
   - a newly uploaded file overrides `Form.ImageUrl` with the uploaded URL (unchanged);
   - `RemoveImage = true` clears it (unchanged);
   - otherwise the posted `Form.ImageUrl` is preserved (now non-null).
3. (Optional UI polish) Populate the "Selected image (pending save)" panel with the current image on
   load so the panel is not misleadingly blank in edit mode.

### Affected files

- [src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml) — add the hidden `Form.ImageUrl` input (and optional selected-image init).
- [src/HelloBuddy.Ui/Controllers/ExercisesController.cs](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Controllers/ExercisesController.cs) — verify `ApplyImageSelectionAsync` precedence (likely no change needed).

### Verification steps (post-fix)

1. Edit an exercise with an image, change the title only, save — image is preserved.
2. Edit an exercise and upload a new image, save — new image replaces the old one.
3. Edit an exercise, tick the remove-image option, save — image is cleared.
4. Create a new exercise with an image — image is saved.
5. Full solution builds clean; UI suite green (add a UI/integration assertion that saving without a
   new file preserves `ImageUrl`).

## ERR-AT-016: Exercise Library "Apply" always forces Active-only back on — deactivated exercises cannot be viewed

**Date:** 2026-06-15
**Status:** Open
**Severity:** Medium (cannot view/manage deactivated exercises)
**Area:** `HelloBuddy.Ui` Exercise Library (Index filter)
**Type:** Filter binding defect (unchecked checkbox defaults to true)
**Related:** Exercise Library filtering

### Symptom

On the Exercise Library page, unticking the **Active only** checkbox and clicking **Apply** does not
show deactivated exercises — the filter re-applies "active only" every time, so deactivated exercises
can never be viewed.

### Steps to reproduce

1. Open `/Exercises` (Exercise Library).
2. Untick **Active only**.
3. Click **Apply**.
4. The list still excludes deactivated exercises, and the **Active only** checkbox reappears ticked.

### Expected

When **Active only** is unticked and **Apply** is clicked, the list includes deactivated (inactive)
exercises so they can be viewed and managed.

### Root cause

Classic "unchecked checkbox posts nothing" problem on a GET filter form:

- The checkbox is `<input type="checkbox" id="activeOnly" name="activeOnly" value="true" checked="@Model.Filter.ActiveOnly" />`
  ([src/HelloBuddy.Ui/Views/Exercises/Index.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Exercises/Index.cshtml#L38-L43)).
  When unticked, the browser omits `activeOnly` from the query string entirely.
- The controller action defaults the missing value to `true`:
  `bool activeOnly = true` in
  [ExercisesController.Index](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Controllers/ExercisesController.cs#L22-L36).
  So an absent `activeOnly` (unchecked) is read as `true`, forcing active-only back on.
- The same default exists downstream (`ExerciseListFilter.ActiveOnly = true`,
  `ExerciseEndpoints` `activeOnly ?? true`), which is correct as a "no filter specified" API default but
  reinforces the UI bug.

### Proposed fix

Make an unticked checkbox post an explicit `false`, the standard MVC pattern. Either:

- **Option A (view only, preferred):** add a hidden field immediately before the checkbox so the form
  always posts a value:
  `<input type="hidden" name="activeOnly" value="false" />`
  `<input type="checkbox" name="activeOnly" value="true" ... />`
  (Model binding takes the last value; ticked posts `false,true` → `true`, unticked posts `false`.)
- **Option B:** change the action parameter to `bool? activeOnly` and only treat a *present* value,
  defaulting to `true` solely on first load — but Option A is simpler and keeps existing defaults.

Keep the API/`ExerciseListFilter` default of `true` (correct for unspecified API calls).

### Affected files

- [src/HelloBuddy.Ui/Views/Exercises/Index.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Exercises/Index.cshtml) — add the hidden `activeOnly=false` companion field.
- [src/HelloBuddy.Ui/Controllers/ExercisesController.cs](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Controllers/ExercisesController.cs) — only if Option B is chosen.

### Verification steps (post-fix)

1. Open the Exercise Library — defaults to Active only ticked, only active exercises shown.
2. Untick **Active only**, click **Apply** — deactivated exercises now appear and the checkbox stays
   unticked.
3. Re-tick **Active only**, **Apply** — list returns to active-only.
4. Confirm other filters (search, category, has-video) still combine correctly with the active toggle.
5. Full solution builds clean; UI suite green (add an assertion that unticked active-only returns
   inactive exercises).

## ERR-AT-017: Adding an exercise in the Programme Builder wipes unsaved session edits

**Date:** 2026-06-18
**Status:** Fixed
**Severity:** High (core workflow defect — data loss)
**Area:** `HelloBuddy.Ui` Programme Builder (session editing + Add exercise)
**Type:** UI behaviour defect (unsaved form state discarded on partial submit + refresh)
**Related:** ACR003 (Builder Session Purpose Summary), AC-011 (Add exercise to session)

### Symptom

When creating or editing an exercise programme, clicking either **Add** (the AM "Add exercise"
button or the PM one) causes other unsaved session content to be lost after the panel refreshes.
Reps, sets, hold, notes and the session purpose summary that had been typed but not yet saved are
reverted to their last-persisted values.

### Steps to reproduce

1. Open a draft programme in the Builder (`/Programmes/{id}/Builder`) with AM/PM sessions.
2. Type a **Session purpose summary** and edit **reps/sets/notes** on existing exercises, but do
   **not** click **Save session edits**.
3. Click **Add** under either the AM or PM "Add exercise" control.
4. The new exercise is added, but the unsaved purpose/reps/sets/notes are gone.

### Root cause

The Builder splits editing into independent HTML forms:

- A single hidden form `session-edits-form` (posts to the `Builder` PUT action) carries every
  session purpose summary and every exercise's reps/sets/hold/notes/sort. Those inputs are attached
  to it via the `form="session-edits-form"` attribute and are only persisted by the **Save session
  edits** button.
- Each session has its **own** "Add exercise" form that posts only `exerciseId` to the `AddExercise`
  action.

In [src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml),
the async submit handler posts just the submitted form and then calls `refreshPanels()`, which
re-fetches the editor partial from the **server (DB state)**. Because the unsaved
`session-edits-form` fields were never submitted, the refresh overwrites them with the
last-persisted values — i.e. the edits are discarded. Both the AM and PM Add buttons behave the same
way.

### Fix

Treat each **Add** action as a "Save session edits" first, so current state is persisted as part of
the same action (no extra button, fully behind the scenes):

- **Client** — [Builder.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml):
  the generic async submit handler now, when the submitted form carries
  `data-include-session-edits="true"`, folds all `[form="session-edits-form"]` field values
  (SessionId/Objective + Exercises[*] reps/sets/hold/notes/sort + ProgrammeId) into the posted
  `FormData` before sending.
- **Markup** — [_BuilderEditor.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Programmes/_BuilderEditor.cshtml):
  both AM and PM "Add exercise" forms are tagged `data-include-session-edits="true"`.
- **Server** — [ProgrammesController.AddExercise](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Controllers/ProgrammesController.cs):
  the action now also binds `ProgrammeBuilderForm`. When the form contains edits for the matching
  programme, it calls `UpdateProgrammeAsync` to persist them **before** `AddSessionExerciseAsync`,
  honouring NotFound/Blocked outcomes (published programmes stay immutable). The subsequent refresh
  then reflects both the saved edits and the new exercise.

Safe because the save path tolerates an empty objective; the mandatory purpose-summary rule is only
enforced at publish, so pre-saving partial edits is never blocked.

### Affected files

- [src/HelloBuddy.Ui/Controllers/ProgrammesController.cs](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Controllers/ProgrammesController.cs) — `AddExercise` pre-saves session edits.
- [src/HelloBuddy.Ui/Views/Programmes/_BuilderEditor.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Programmes/_BuilderEditor.cshtml) — Add forms carry `data-include-session-edits`.
- [src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml) — submit handler merges session-edit fields.
- [tests/HelloBuddy.Ui.Tests/ProgrammesControllerTests.cs](Canine%20Physio%20Admin/tests/HelloBuddy.Ui.Tests/ProgrammesControllerTests.cs) — new/updated tests.

### Tests

- `AddExercise_Post_PersistsSessionEditsBeforeAddingExercise` — asserts (via `Received.InOrder`) that
  `UpdateProgrammeAsync` is called with the AM + PM purposes and reps/sets/hold/notes **before**
  `AddSessionExerciseAsync`.
- `AddExercise_Post_WithEmptyForm_DoesNotCallUpdateProgramme` — no redundant save when there are no edits.
- `AddExercise_Post_WhenPreSaveBlocked_ReturnsErrorAndDoesNotAddExercise` — published programmes stay immutable.
- `AddExercise_Post_AjaxDuplicate_ReturnsJsonErrorPayload` — updated for the new method signature.

### Verification

- `dotnet test tests/HelloBuddy.Ui.Tests` — **48 passed, 0 failed** (2026-06-18).

### Follow-up fix (2026-06-18)

Initial fix surfaced a runtime error on Add ("Unable to update programme builder. Please retry.").
Cause: the client merge loop copied the `session-edits-form` hidden `__RequestVerificationToken`
into the Add form's `FormData`, which already carries its own token — duplicate antiforgery tokens
fail validation. Fixed in
[Builder.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml) by
skipping `__RequestVerificationToken` when folding in the session-edit hidden fields.
