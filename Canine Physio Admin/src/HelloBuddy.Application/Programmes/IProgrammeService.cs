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

    /// <summary>Applies builder edits and returns the refreshed view-model, or <c>null</c> when the programme is not visible.</summary>
    Task<ProgrammeVm?> UpdateAsync(ulong programmeId, ProgrammeBuilderForm form, ulong practitionerId, CancellationToken ct);

    /// <summary>Renders the programme to PDF and stores it, returning the publish metadata; <c>null</c> when not visible.</summary>
    Task<PublishResponse?> PublishAsync(ulong programmeId, ulong practitionerId, CancellationToken ct);
}
