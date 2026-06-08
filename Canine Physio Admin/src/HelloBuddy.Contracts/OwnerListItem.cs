namespace HelloBuddy.Contracts;

public sealed record OwnerListItem(
    ulong OwnerId,
    string FullName,
    string Email,
    string? PhoneNumber,
    int PetCount,
    bool IsAnonymised);
