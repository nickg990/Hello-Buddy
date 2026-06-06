using HelloBuddy.Contracts;

namespace HelloBuddy.Application.Programmes;

/// <summary>
/// Persistence boundary for <c>Programme</c> aggregate read/write operations
/// used by the Admin API. Implementation lives in the Infrastructure layer.
/// </summary>
public interface IProgrammeRepository
{
    /// <summary>Loads the full <see cref="ProgrammeVm"/> for the supplied programme, scoped to the practitioner; returns <c>null</c> when not visible.</summary>
    Task<ProgrammeVm?> GetVmAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Creates a draft programme for the supplied treatment case and returns the refreshed view-model; returns <c>null</c> when not visible.</summary>
    Task<ProgrammeVm?> CreateDraftAsync(ulong treatmentCaseId, ulong practitionerId, CancellationToken ct);

    /// <summary>Returns <c>true</c> when the practitioner owns (via case) the programme.</summary>
    Task<bool> OwnsAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Deletes the supplied programme when visible and eligible according to business guardrails.</summary>
    Task<DeleteProgrammeResult> DeleteAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Updates dates and session structure for a draft programme.</summary>
    Task<ProgrammeStructureUpdateResult> UpdateStructureAsync(ulong programmeId, ulong practitionerId, ProgrammeStructureForm form, CancellationToken ct);

    /// <summary>Adds an exercise row to a session in the supplied programme.</summary>
    Task<AddSessionExerciseResult> AddSessionExerciseAsync(ulong programmeId, ulong practitionerId, ulong sessionId, ulong exerciseId, CancellationToken ct);

    /// <summary>Removes a session-exercise row from the supplied programme.</summary>
    Task<RemoveSessionExerciseResult> RemoveSessionExerciseAsync(ulong programmeId, ulong practitionerId, ulong sessionId, ulong sessionExerciseId, CancellationToken ct);

    /// <summary>Transitions a planned programme to active when no other programme in the same case is active.</summary>
    Task<ProgrammeStatusTransitionResult> ActivateAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Transitions an active programme to completed.</summary>
    Task<ProgrammeStatusTransitionResult> CompleteAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Applies edits to session-exercise rows belonging to the supplied programme.</summary>
    Task UpdateSessionExercisesAsync(ulong programmeId, IReadOnlyList<ProgrammeBuilderForm.SessionExerciseEdit> edits, CancellationToken ct);
}
