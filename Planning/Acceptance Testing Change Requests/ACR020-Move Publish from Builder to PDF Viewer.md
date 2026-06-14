# ACR020 - Move Publish Button and Functionality from Builder to PDF Viewer

Date: 2026-06-12
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Programme Builder and PDF Viewer pages)

## Why this change

Publishing finalises a programme into a locked, version-controlled PDF. It belongs on the
**PDF Viewer** page (currently "Preview PDF"), where the practitioner can see exactly what they are
about to publish, rather than on the **PDF Builder** (currently "Builder") editing page. The
Publish button and its confirmation flow should move from the Builder to the PDF Viewer.

> Sequencing note: this ACR pairs with ACR021 (rename Preview → PDF Viewer) and ACR023 (disable
> the PDF Builder button once published). Implement them together to keep labels consistent.

## Scope

In scope:
- Remove the **Publish PDF** button and its confirmation modal from the Builder view.
- Add the **Publish** button + confirmation flow to the PDF Viewer page.

Out of scope:
- The `Publish` controller action and API endpoint logic — unchanged; only the entry point moves.
- The Download PDF / preview behaviour on the Viewer page — unchanged.

## Acceptance criteria

1. The Builder (PDF Builder) page no longer shows a **Publish** button or its modal.
2. The PDF Viewer page shows a **Publish** button with the same confirmation message currently
   used (publishing generates and saves a locked PDF version; further changes require a new draft).
3. Publishing from the PDF Viewer behaves exactly as before (same `Publish` action/endpoint,
   same success/error feedback via toasts, same redirect/PRG behaviour).
4. The Publish button is **not shown** (or is disabled) on the PDF Viewer when the programme is
   already published/locked for edit (use the existing locked-for-edit signal — see guidance).
5. Implementation conforms to coding standards; suites green.

## Implementation guidance

- Remove from `src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml`:
  - the `<button ... data-bs-target="#publishConfirmModal">Publish PDF</button>` button, and
  - the entire `#publishConfirmModal` modal block (which posts to the `Publish` action).
- Add to `src/HelloBuddy.Ui/Views/Programmes/Preview.cshtml` (the PDF Viewer page) a **Publish**
  button + confirmation modal that posts to the existing
  `asp-action="Publish" asp-route-id="@Model.ProgrammeId"` with an antiforgery token, mirroring
  the markup removed from the Builder.
- Determine published/locked state for criterion 4 using the existing immutability signal
  (`IProgrammeService.IsLockedForEditAsync` / the corresponding API-client method). The Preview
  page currently binds `ProgrammeVm` only; surface a `bool` (e.g. `IsLockedForEdit`) to the view
  — either via the controller fetching it for the Preview action, or by adding the flag to a
  Preview page view-model. Do not duplicate immutability logic; reuse the existing check.
- Keep the `Publish` action in `ProgrammesController.cs` and the API endpoint as-is.

## Risks and mitigations

Risks:
- Showing Publish on an already-published programme would error or create confusion.

Mitigations:
- Gate the button on the locked-for-edit signal (criterion 4) and keep the server-side
  immutability guard as the backstop.

## Verification

- Manual: Builder has no Publish button; PDF Viewer publishes a draft successfully and hides the
  Publish button once published.
- UI smoke: assert Publish control is present on the Viewer page (draft) and absent on the
  Builder page; update any tests that asserted Publish on the Builder.
- Full solution builds clean; suites green.
