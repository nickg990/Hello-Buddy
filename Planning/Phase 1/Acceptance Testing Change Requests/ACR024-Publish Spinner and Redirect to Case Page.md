# ACR024 - Publish Spinner in the PDF Viewer Popup and Redirect to the Case Page

Date: 2026-06-12
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (PDF Viewer publish confirmation popup and post-publish navigation)

## Why this change

On the **PDF Viewer** page (`Views/Programmes/Preview.cshtml`), the publish confirmation popup
(`#publishConfirmModal`) submits the **Publish PDF** action. Publishing renders and stores a PDF
server-side, which takes a moment, but there is currently:

1. No busy indicator while publishing runs, so the practitioner may think nothing happened and
   click again (double-submit risk).
2. A post-publish redirect back to the **PDF Builder** page (`Publish` action currently does
   `RedirectToAction(nameof(Builder), ...)`). Once published the programme is locked for edit, so
   landing on the (now read-only) Builder is confusing.

This ACR adds a spinner inside the publish popup while publishing is in progress and changes the
post-publish redirect to return the user to the **Case page** (case detail), where the published
programme is listed.

## Scope

In scope:
- Add a spinner / busy state to the publish popup's **Publish PDF** button while the publish POST
  is in flight, and prevent duplicate submissions.
- Change the successful-publish redirect target from the Builder page to the Case detail page for
  the programme's treatment case.

Out of scope:
- The PDF generation logic, the API publish endpoint, and version-history behaviour — unchanged.
- The Download PDF spinner (ACR017) and the preview-iframe spinner (ACR014) — unaffected.
- Validation-failure behaviour beyond returning the user to a sensible page with the existing
  error toast (see acceptance criterion 5).

## Acceptance criteria

1. Clicking **Publish PDF** in the `#publishConfirmModal` popup immediately shows a visible
   spinner/busy indicator on (or beside) the Publish button, with accessible status text
   (e.g. `role="status"` "Publishing...").
2. While publishing is in progress, the Publish button is disabled (and the modal's Cancel/close
   guarded as appropriate) so the action cannot be submitted twice.
3. On successful publish, the user is redirected to the **Case page**
   (`CaseDetail` → `Index`) for the programme's treatment case, not back to the Builder.
4. The existing success feedback is preserved: the published-file toast/message
   (`TempData["Published"]`) still shows after the redirect on the Case page.
5. On publish **validation failure** (incomplete draft), the user is returned to a page where the
   error toast is shown (the PDF Viewer page is acceptable, since the programme is not yet
   locked); the spinner is cleared and the button re-enabled. The publish must not silently fail.
6. Implementation conforms to coding standards (no inline styles; spinner via Bootstrap
   `spinner-border` markup and CSS classes; script consistent with the existing ACR017 pattern);
   suites green.

## Implementation guidance

### 1) Spinner + double-submit guard (view: `src/HelloBuddy.Ui/Views/Programmes/Preview.cshtml`)

- The publish popup currently contains:
  ```html
  <form method="post" asp-action="Publish" asp-route-id="@Model.ProgrammeId" class="d-inline">
      @Html.AntiForgeryToken()
      <button type="submit" class="btn btn-primary">Publish PDF</button>
  </form>
  ```
- Give the form and button stable ids (e.g. `id="publish-form"` and `id="publish-submit"`), and
  add an inline spinner span inside the button that is hidden by default, mirroring the ACR017
  Download button pattern already in this view:
  ```html
  <button type="submit" id="publish-submit" class="btn btn-primary">
      <span class="d-inline-flex align-items-center gap-2">
          <span>Publish PDF</span>
          <span id="publish-spinner" class="d-none" role="status" aria-live="polite" aria-label="Publishing">
              <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
              <span>Publishing...</span>
          </span>
      </span>
  </button>
  ```
- In the existing `@section Scripts` block at the bottom of `Preview.cshtml`, add a small handler
  that, on `submit` of the publish form:
  - sets the button to busy: remove `d-none` from `#publish-spinner`, set the button `disabled`
    (and add `aria-disabled="true"`), and disable the modal Cancel button so the user cannot
    dismiss mid-publish;
  - guards against a second submit (ignore/`preventDefault` if already busy);
  - then allows the normal form POST to proceed (do not `preventDefault` the first submit — this
    is a full-page POST/redirect, so the spinner naturally disappears when the new page loads).
  - Because this is a standard non-AJAX POST that ends in a redirect, no completion polling is
    required (unlike the file-download case in ACR017); the navigation itself ends the busy state.
- Keep the spinner markup and classes consistent with `#preview-download-spinner` already in this
  file. Do not introduce inline styles.

### 2) Redirect to the Case page (controller: `src/HelloBuddy.Ui/Controllers/ProgrammesController.cs`)

- The `Publish` action currently ends with:
  ```csharp
  return RedirectToAction(nameof(Builder), new { id });
  ```
- On **success**, redirect to the case detail page instead. The treatment case id is available on
  the programme view-model (`ProgrammeVm.TreatmentCaseId`), and the case page route is
  `CaseDetail` → `Index` with `id = TreatmentCaseId` (see the Builder back-link
  `Url.Action("Index", "CaseDetail", new { id = programme.TreatmentCaseId })`).
- Obtain the `TreatmentCaseId`:
  - Preferred: use the `PublishResponse` if it already carries the case id; if it does not, fetch
    the programme via `await _api.GetProgrammeAsync(id, ct)` (the action already has `id`) and read
    `TreatmentCaseId`. Reuse the existing client method; do not add new API surface.
- After a successful publish, set the existing success TempData and redirect:
  ```csharp
  TempData["PublishedFile"] = resp.FileName;
  TempData["Published"] = $"Published {resp.FileName} ({resp.Bytes:N0} bytes).";
  // ...
  return RedirectToAction("Index", "CaseDetail", new { id = treatmentCaseId });
  ```
- On **validation failure** (`ApiValidationException`): keep setting `TempData["Error"]` as today,
  but redirect to the **PDF Viewer** (`Preview`) page for the programme (not the Builder), so the
  error toast is shown on a page that still offers Publish:
  ```csharp
  return RedirectToAction(nameof(Preview), new { id });
  ```
- Leave the unexpected-exception path (rethrow after logging) unchanged.
- `TempData["Published"]` / `TempData["Error"]` survive the redirect and are rendered by the
  existing toast mechanism (ACR012) on the destination page.

### 3) Tests

- Controller test (`tests/HelloBuddy.Ui.Tests/ProgrammesControllerTests.cs`):
  - On successful publish, assert the result is a `RedirectToActionResult` to
    `Index`/`CaseDetail` with the correct `id` (treatment case id), and that
    `TempData["Published"]` is set. Stub `IAdminApiClient.PublishProgrammeAsync` to return a
    `PublishResponse`, and stub `GetProgrammeAsync` to return a `ProgrammeVm` with a known
    `TreatmentCaseId` if the implementation reads it from there.
  - On `ApiValidationException`, assert redirect to `Preview` (PDF Viewer) and that
    `TempData["Error"]` is set (extend the existing
    `Publish_Post_WhenApiValidationException_...` test).
- UI smoke test (`tests/HelloBuddy.Ui.Tests/UiSmokeTests.cs`):
  - On the PDF Viewer page (draft), assert the publish popup contains the publish spinner markup
    (e.g. `id="publish-spinner"`) and the submit button id (e.g. `id="publish-submit"`).

## Risks and mitigations

Risks:
- A full-page POST that fails server-side could leave the spinner showing if the browser does not
  navigate. Mitigation: the failure paths redirect (PRG), so the destination page load clears the
  spinner; the spinner lives only on the in-flight Preview page instance.
- Reading `TreatmentCaseId` could add a query. Mitigation: prefer the value already returned by the
  publish flow / view-model; only fetch the programme if not otherwise available, and reuse the
  existing client method (no new endpoint).

## Verification

- Manual: open the PDF Viewer for a draft, click Publish PDF in the popup → spinner shows and the
  button is disabled → on completion the browser lands on the Case page with the "Published ..."
  toast, and the published programme shows in the case programme list with the PDF Builder entry
  disabled (per ACR023).
- Manual (validation failure): publish an incomplete draft → returns to the PDF Viewer with the
  error toast and a usable (re-enabled) Publish button.
- Full solution builds clean; suites green.
