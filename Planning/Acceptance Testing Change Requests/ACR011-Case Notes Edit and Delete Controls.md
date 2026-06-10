# ACR011 - Case Notes Edit and Delete Controls (Per-Note Edit Popup and Confirmed Delete)

Date: 2026-06-10
Status: Implemented
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Case detail / Case notes)

## Why this change

Practitioners need to maintain case notes after they are created. Notes can contain typos or
need clinical revision, and incorrect notes must be removable. The case notes panel currently
only supports adding notes, so per-note edit and delete controls are required.

## Scope

In scope:
- Add an edit (pencil) icon and a delete (bin) icon against each case note.
- Delete prompts the user to confirm or cancel before removing the note.
- On confirm, the note is hard-deleted from the database and the page is refreshed.
- Edit opens a small popup (modal) allowing the note type and note text to be edited.
- Saving the edit updates the database, closes the popup, and refreshes the page.

Out of scope:
- Soft-delete / note version history.
- Rich-text or attachment support in notes.
- Bulk edit/delete of multiple notes.

## Acceptance criteria

1. Each case note shows an edit (pencil) icon and a delete (bin) icon.
2. Clicking delete presents a confirm/cancel prompt; cancel makes no change.
3. Confirming delete removes the note record from the database and refreshes the page so the
   note no longer appears.
4. Clicking edit opens a small popup pre-filled with the note's type and text.
5. Saving the popup updates the note in the database, closes the popup, and refreshes the page
   showing the updated values.
6. Editing or deleting a note belonging to another practitioner is not possible (authorisation
   scoping is preserved).
7. Implementation conforms to coding standards (no inline styles; styling via CSS classes).

## Implementation notes (as built)

- API: added `PUT /api/cases/{id}/notes/{noteId}` and `DELETE /api/cases/{id}/notes/{noteId}`
  in `src/HelloBuddy.Api/Endpoints/CaseEndpoints.cs`.
- Application/Infrastructure: added `UpdateNoteAsync` and `DeleteNoteAsync` to
  `ITreatmentCaseRepository` and implemented them in `RecordRepositories.cs`, scoped by the
  current practitioner with the standard anonymised-owner exclusion.
- UI client: added `UpdateCaseNoteAsync` and `DeleteCaseNoteAsync` to `IAdminApiClient` /
  `AdminApiClient`.
- UI controller: added `Notes/{noteId}/Update` and `Notes/{noteId}/Delete` POST actions in
  `CaseDetailController` using Post-Redirect-Get and TempData success/error messages.
- View: `Views/CaseDetail/Index.cshtml` renders the pencil/bin icons per note, a Bootstrap modal
  for editing, and a confirm-guarded delete form.

## Risks and mitigations

Risks:
- Accidental deletion of a note.
- Cross-practitioner data exposure via the new endpoints.

Mitigations:
- Confirm/cancel prompt before delete.
- Repository queries scope by `PractitionerId` and exclude anonymised owners.
- API and UI test coverage for update/delete (persist, 404, and authorisation behaviour).

## Verification

- UI smoke tests: edit/delete controls render, edit persists and refreshes, delete removes and
  refreshes.
- API in-memory tests: update persists new type/text, update missing note returns 404, delete
  removes the note, delete missing note returns 404.
- Full solution builds clean (warnings-as-errors); UI and API in-memory suites green.
