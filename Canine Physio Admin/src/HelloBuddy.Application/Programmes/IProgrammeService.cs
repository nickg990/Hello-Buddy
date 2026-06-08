using HelloBuddy.Contracts;

namespace HelloBuddy.Application.Programmes;

/// <summary>
/// Application-level orchestration for programme read, update and publish use-cases.
/// Endpoints depend on this interface and remain transport adapters only.
/// </summary>
public interface IProgrammeService
{
    /// <summary>Returns the programme view-model for the supplied practitioner, or <c>null</c> if not visible.</summary>
    Task<ProgrammeVm?> GetAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Creates a draft programme from a treatment case for the supplied practitioner, or <c>null</c> when not visible.</summary>
    Task<ProgrammeVm?> CreateDraftAsync(ulong treatmentCaseId, ulong practitionerId, CancellationToken ct);

    /// <summary>Returns <c>true</c> when the programme has publish history and should be treated as immutable.</summary>
    Task<bool> IsLockedForEditAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Returns version history for a visible programme, or <c>null</c> when not visible.</summary>
    Task<ProgrammeVersionHistoryVm?> GetVersionHistoryAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>
    /// Creates a new editable draft based on the supplied programme's published history.
    /// Returns <c>null</c> when programme is not visible or has no published history.
    /// </summary>
    Task<ProgrammeVm?> CreateDraftFromPublishedAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Applies builder edits and returns the refreshed view-model, or <c>null</c> when the programme is not visible.</summary>
    Task<ProgrammeVm?> UpdateAsync(ulong programmeId, ProgrammeBuilderForm form, ulong practitionerId, CancellationToken ct);

    /// <summary>Deletes the programme when visible and eligible; returns a result describing outcome.</summary>
    Task<DeleteProgrammeResult> DeleteAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Updates programme dates and session structure for a visible draft programme.</summary>
    Task<ProgrammeStructureUpdateResult> UpdateStructureAsync(ulong programmeId, ProgrammeStructureForm form, ulong practitionerId, CancellationToken ct);

    /// <summary>Adds an exercise to the supplied session in a visible programme.</summary>
    Task<AddSessionExerciseResult> AddSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong exerciseId, ulong practitionerId, CancellationToken ct);

    /// <summary>Removes an exercise row from the supplied session in a visible programme.</summary>
    Task<RemoveSessionExerciseResult> RemoveSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong sessionExerciseId, ulong practitionerId, CancellationToken ct);

    /// <summary>Activates a planned programme when no other programme in the same case is currently active.</summary>
    Task<ProgrammeStatusTransitionResult> ActivateAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Completes an active programme.</summary>
    Task<ProgrammeStatusTransitionResult> CompleteAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Returns publish-blocking draft validation errors keyed by field.</summary>
    Task<Dictionary<string, string[]>> ValidateDraftForPublishAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Renders the programme to PDF and stores it, returning the publish metadata; <c>null</c> when not visible.</summary>
    Task<PublishResponse?> PublishAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);
}
