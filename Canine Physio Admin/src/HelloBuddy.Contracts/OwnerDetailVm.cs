namespace HelloBuddy.Contracts;

public sealed record OwnerDetailVm(
    ulong OwnerId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? AddressLine1,
    string? AddressLine2,
    string? Town,
    string? Postcode,
    IReadOnlyList<OwnerDetailVm.PetRow> Pets,
    bool IsAnonymised)
{
    public string FullName => $"{FirstName} {LastName}".Trim();

    public sealed record PetRow(
        ulong PetId,
        string Name,
        string? Breed,
        string Sex,
        bool IsActive,
        int CaseCount);
}
