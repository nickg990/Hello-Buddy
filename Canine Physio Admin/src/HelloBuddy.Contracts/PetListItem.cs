namespace HelloBuddy.Contracts;

public sealed record PetListItem(
    ulong PetId,
    ulong OwnerId,
    string Name,
    string OwnerName,
    string? Breed,
    string Sex,
    bool IsActive,
    int CaseCount);
