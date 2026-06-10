using HelloBuddy.Contracts;

namespace HelloBuddy.Application.Records;

/// <summary>
/// Persistence boundary for <c>TreatmentCase</c> and associated <c>CaseNote</c> operations.
/// </summary>
public interface ITreatmentCaseRepository
{
    /// <summary>Lists treatment cases visible to the supplied practitioner.</summary>
    Task<IReadOnlyList<CaseRow>> ListAsync(ulong practitionerId, CancellationToken ct);

    /// <summary>Returns the detail view-model for a single case scoped to the practitioner; <c>null</c> if not visible or not found.</summary>
    Task<CaseDetailVm?> GetAsync(ulong treatmentCaseId, ulong practitionerId, CancellationToken ct);

    /// <summary>Creates a new treatment case and returns the generated id.</summary>
    Task<ulong> CreateAsync(SaveTreatmentCaseRequest request, ulong practitionerId, CancellationToken ct);

    /// <summary>Updates an existing treatment case; returns <c>false</c> when not found or not visible to the practitioner.</summary>
    Task<bool> UpdateAsync(ulong treatmentCaseId, SaveTreatmentCaseRequest request, ulong practitionerId, CancellationToken ct);

    /// <summary>Appends a note to a treatment case; returns the persisted row or <c>null</c> if the case is not visible.</summary>
    Task<CaseDetailVm.NoteRow?> AddNoteAsync(ulong treatmentCaseId, CreateCaseNoteRequest request, ulong practitionerId, CancellationToken ct);

    /// <summary>Updates an existing note on a treatment case; returns the persisted row or <c>null</c> if the case/note is not visible.</summary>
    Task<CaseDetailVm.NoteRow?> UpdateNoteAsync(ulong treatmentCaseId, ulong noteId, CreateCaseNoteRequest request, ulong practitionerId, CancellationToken ct);

    /// <summary>Permanently deletes a note from a treatment case; returns <c>false</c> when the case/note is not visible or not found.</summary>
    Task<bool> DeleteNoteAsync(ulong treatmentCaseId, ulong noteId, ulong practitionerId, CancellationToken ct);
}
