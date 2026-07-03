# ACR029 – Add Owner and Pet Delete Actions

**Date:** 2026-06-18
**Status:** Implemented

---

## Change Title

Add owner and pet delete actions

---

## Reason for Change

Practitioners need to be able to delete individual owners and individual pets directly from the Owners and Pets list views. Previously, owner deletion was only accessible via the GDPR Data Control page (Admin → Data Control), which is inconvenient for routine record management. There was no pet-level deletion at all. This change adds a trash/bin icon per row on both list views with a confirmation dialog, providing a faster, in-context deletion workflow.

---

## Scope

| Layer | Change |
|---|---|
| Application | Added `PetDeleteResult` enum; added `DeleteAsync` to `IPetRepository` |
| Infrastructure | Implemented `PetRepository.DeleteAsync` with full cascade delete (programmes → PDF blobs → sessions → case notes → treatment cases → registration codes → practitioner-pet assignments → pet); injected `IFileStore` into `PetRepository` |
| Contracts | Added `PetDeleteResponse` record |
| API | Added `DELETE /api/pets/{id}` endpoint in `PetEndpoints.cs` |
| UI – Services | Added `PetDeleteClientOutcome`, `PetDeleteClientResult` types and `DeletePetAsync` to `IAdminApiClient`; implemented in `AdminApiClient` |
| UI – Controllers | Added `POST /Owners/{id}/Delete` to `OwnersController` (reuses `ApplyOwnerDataControlAsync`); added `POST /Pets/{id}/Delete` to `PetsController` |
| UI – Views | Updated `Owners/Index.cshtml` and `Pets/Index.cshtml` with delete button (Bootstrap trash icon SVG) per row, confirmation modal (Bootstrap), and post-delete success toast |
| Tests | Updated both UI test mocks; added 4 UI smoke tests; added 3 API in-memory tests |

---

## Functional Requirements

1. **Owners list view** – A delete/bin icon button appears on each owner row.
2. **Owner deletion** – Clicking the icon opens a confirmation modal naming the owner and warning of irreversible deletion. Confirming posts to `POST /Owners/{id}/Delete`, which reuses the existing RTBF (`ApplyOwnerDataControlAsync`) to delete the owner, all their pets, case notes, treatment cases, exercise programmes, sessions, programme PDF blobs, and all related records.
3. **Pets list view** – A delete/bin icon button appears on each pet row.
4. **Pet deletion** – Clicking the icon opens a confirmation modal naming the pet and its owner and warning of irreversible deletion. Confirming posts to `POST /Pets/{id}/Delete`, which calls `DELETE /api/pets/{id}` to delete the pet, all case notes, treatment cases, exercise programmes, sessions, programme PDF blobs, and all related records. The owner record is **preserved**.
5. **Cancellation** – Clicking Cancel or the modal close button (×) dismisses the modal without any deletion.
6. **Post-delete feedback** – A success toast is shown on the list page after deletion.
7. **UI refresh** – After deletion the list page reloads, showing the updated list without the deleted record.

---

## Acceptance Criteria

| # | Criterion | Pass |
|---|---|---|
| AC1 | An owner can be deleted from the Owners list after confirmation | — |
| AC2 | Deleting an owner removes the owner, pets, case notes, programmes, sessions, and related data | — |
| AC3 | A pet can be deleted from the Pets list after confirmation | — |
| AC4 | Deleting a pet preserves the owner but removes that pet's case notes, programmes, sessions, and related data | — |
| AC5 | Cancel on the owner confirmation popup performs no deletion | — |
| AC6 | Cancel on the pet confirmation popup performs no deletion | — |
| AC7 | The Owners list refreshes correctly after owner deletion (deleted owner no longer visible) | — |
| AC8 | The Pets list refreshes correctly after pet deletion (deleted pet no longer visible) | — |
| AC9 | A success toast is shown after deletion | — |
| AC10 | `DELETE /api/pets/{id}` returns 404 for a non-existent pet | — |

---

## Testing Evidence Required

- [ ] Screenshot: Owners list showing trash icon on each row
- [ ] Screenshot: Owner deletion confirmation modal (showing owner name + irreversible warning)
- [ ] Screenshot: Owners list after owner deletion (owner row absent, success toast shown)
- [ ] Screenshot: Pets list showing trash icon on each row
- [ ] Screenshot: Pet deletion confirmation modal (showing pet name, owner name + irreversible warning + "owner record will be preserved")
- [ ] Screenshot: Pets list after pet deletion (pet row absent, owner still visible in Owners list)
- [ ] Test results: 3 API in-memory tests passing (`PetDelete_WithNoLinkedData_DeletesPet`, `PetDelete_WithLinkedCaseAndNotes_DeletesPetAndPreservesOwner`, `PetDelete_NonExistentPet_ReturnsNotFound`)
- [ ] Test results: 4 UI smoke tests passing (`OwnersIndex_RendersDeleteButtonAndConfirmationModal`, `OwnersDelete_WhenPosted_RedirectsToIndex`, `PetsIndex_RendersDeleteButtonAndConfirmationModal`, `PetsDelete_WhenPosted_RedirectsToIndex`)

---

## Risks / Notes

> **Irreversible deletion.** Both owner and pet delete are permanent and cannot be undone. The confirmation modal explicitly warns the user. No soft-delete or recovery mechanism is provided (consistent with existing RTBF behaviour).

> **Owner delete reuses RTBF.** The Owners list delete action calls the same `ApplyOwnerDataControlAsync` / `POST /api/owners/{id}/data-control` that the Admin Data Control GDPR page uses. This means owner deletion from the list also writes a GDPR audit log entry (`gdpr-deletion` action type), which is appropriate since the deletion semantics are identical.

> **Pet delete does not write a GDPR audit log.** Only owner deletions write GDPR audit entries. If a compliance audit trail is required for pet deletions in future, an audit log entry should be added.

> **No authorisation gating beyond existing practitioner scope.** All delete endpoints are subject to the existing `X-Practitioner-Id` header gate. Role-based gating (e.g. admin-only) is not added in this change; if required it should be tracked as a separate work item.

> **In-memory tests only for pet delete.** The Testcontainer integration test suite (`ApiTestcontainerIntegrationTests`) was not extended in this change. A Testcontainer test for pet delete with full blob-delete verification can be added if needed.
