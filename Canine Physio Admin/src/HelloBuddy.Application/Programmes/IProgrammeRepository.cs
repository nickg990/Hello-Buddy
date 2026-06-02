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

    /// <summary>Returns <c>true</c> when the practitioner owns (via case) the programme.</summary>
    Task<bool> OwnsAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);

    /// <summary>Applies edits to session-exercise rows belonging to the supplied programme.</summary>
    Task UpdateSessionExercisesAsync(ulong programmeId, IReadOnlyList<ProgrammeBuilderForm.SessionExerciseEdit> edits, CancellationToken ct);
}
