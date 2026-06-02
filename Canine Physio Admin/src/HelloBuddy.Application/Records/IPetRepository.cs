using HelloBuddy.Contracts;

namespace HelloBuddy.Application.Records;

/// <summary>
/// Persistence boundary for <c>Pet</c> aggregate operations used by the Admin API.
/// </summary>
public interface IPetRepository
{
    /// <summary>Returns a summary list of all pets across all owners.</summary>
    Task<IReadOnlyList<PetListItem>> ListAsync(CancellationToken ct);

    /// <summary>Returns the detail view-model for a single pet scoped to the requesting practitioner; <c>null</c> if not visible or not found.</summary>
    Task<PetDetailVm?> GetAsync(ulong petId, ulong practitionerId, CancellationToken ct);

    /// <summary>Creates a new pet for the supplied practitioner and returns the generated pet id.</summary>
    Task<ulong> CreateAsync(SavePetRequest request, ulong practitionerId, CancellationToken ct);

    /// <summary>Updates an existing pet scoped to the practitioner; returns <c>false</c> if not found.</summary>
    Task<bool> UpdateAsync(ulong petId, SavePetRequest request, ulong practitionerId, CancellationToken ct);

    /// <summary>Returns <c>true</c> when the owner exists and can be assigned as a pet's owner.</summary>
    Task<bool> OwnerExistsAsync(ulong ownerId, CancellationToken ct);

    /// <summary>Returns <c>true</c> when the pet with <paramref name="petId"/> exists.</summary>
    Task<bool> ExistsAsync(ulong petId, CancellationToken ct);
}
