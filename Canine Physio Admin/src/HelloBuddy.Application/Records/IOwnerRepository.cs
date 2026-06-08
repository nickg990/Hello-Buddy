using HelloBuddy.Contracts;

namespace HelloBuddy.Application.Records;

/// <summary>
/// Persistence boundary for <c>Owner</c> aggregate operations used by the Admin API.
/// </summary>
public interface IOwnerRepository
{
    /// <summary>Returns a summary list of owners; anonymised owners are excluded unless explicitly requested.</summary>
    Task<IReadOnlyList<OwnerListItem>> ListAsync(bool includeAnonymised, CancellationToken ct);

    /// <summary>Returns the full detail view-model for a single owner, or <c>null</c> if not found/visible.</summary>
    Task<OwnerDetailVm?> GetAsync(ulong ownerId, bool includeAnonymised, CancellationToken ct);

    /// <summary>Creates a new owner from the supplied request and returns the generated owner id.</summary>
    Task<ulong> CreateAsync(SaveOwnerRequest request, CancellationToken ct);

    /// <summary>Updates an existing owner; returns <c>false</c> if the owner does not exist.</summary>
    Task<bool> UpdateAsync(ulong ownerId, SaveOwnerRequest request, CancellationToken ct);

    /// <summary>Checks whether <paramref name="email"/> is already used by an owner other than <paramref name="excludedOwnerId"/>.</summary>
    Task<bool> EmailInUseAsync(string email, ulong? excludedOwnerId, CancellationToken ct);

    /// <summary>
    /// Applies right-to-be-forgotten data control for an owner visible to the supplied practitioner:
    /// hard-delete when no linked data exists, otherwise anonymise personal data while retaining linked clinical records.
    /// </summary>
    Task<OwnerDataControlResult> ApplyDataControlAsync(ulong ownerId, ulong practitionerId, CancellationToken ct);
}
