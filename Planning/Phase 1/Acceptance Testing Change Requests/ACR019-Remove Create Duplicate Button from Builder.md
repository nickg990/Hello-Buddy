# ACR019 - Remove "Create Duplicate" Button from the PDF Builder Page

Date: 2026-06-12
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Programme Builder page)

## Why this change

The Builder page (renamed to **PDF Builder** in ACR021) has a **Create duplicate** button that
creates a new editable draft from the published version. This is confusing on the Builder page and
should be removed from the Builder, along with its associated Builder-page code (the modal and the
button). The underlying "create a new draft from published" capability must remain available
elsewhere (it lives on the version history / **PDF History** page as "New draft from published"),
so only the Builder-page entry point is removed.

## Scope

In scope:
- Remove the **Create duplicate** button and its confirmation modal from the Builder view.
- Remove any Builder-page-only script/markup that exists solely to support that button.

Out of scope:
- The `CreateDraftFromPublished` controller action and API endpoint — these stay (used by the
  PDF History page).
- The equivalent button on the History / PDF History page — that stays.

## Acceptance criteria

1. The Builder (PDF Builder) page no longer shows a **Create duplicate** button.
2. The associated **Create Duplicate** confirmation modal markup is removed from the Builder view.
3. The `CreateDraftFromPublished` action/endpoint and the History-page button remain functional
   (the capability is unchanged elsewhere).
4. No dead code remains in the Builder view related to the removed button (no orphaned modal,
   ids, or handlers).
5. Implementation conforms to coding standards; suites green.

## Implementation guidance

- In `src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml`:
  - Remove the button:
    `<button type="button" class="btn btn-outline-primary" data-bs-toggle="modal" data-bs-target="#createDuplicateModal">Create duplicate</button>`
  - Remove the entire `#createDuplicateModal` modal block (the "Create duplicate draft?" modal
    that posts to `CreateDraftFromPublished`).
- Do **not** remove the `CreateDraftFromPublished` action in
  `src/HelloBuddy.Ui/Controllers/ProgrammesController.cs` or the API endpoint — the History /
  PDF History page still uses this capability.
- Confirm no other Builder markup/script references `createDuplicateModal`.

## Risks and mitigations

Risks:
- Accidentally removing the shared `CreateDraftFromPublished` capability used by History.

Mitigations:
- Scope the removal to the Builder view only; verify the History page button still works.

## Verification

- Manual: Builder page has no Create duplicate button; PDF History still offers
  "New draft from published" and it works.
- UI smoke: update/remove any assertion referencing the Builder duplicate button/modal.
- Full solution builds clean; suites green.
