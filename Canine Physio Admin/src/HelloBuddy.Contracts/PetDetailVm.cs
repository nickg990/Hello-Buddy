namespace HelloBuddy.Contracts;

public sealed record PetDetailVm(
    ulong PetId,
    ulong OwnerId,
    string Name,
    byte? Age,
    DateOnly? DateOfBirth,
    string? Breed,
    string Sex,
    decimal? Weight,
    bool IsActive,
    string OwnerName,
    string OwnerEmail,
    IReadOnlyList<PetDetailVm.CaseRow> Cases)
{
    public sealed record CaseRow(
        ulong TreatmentCaseId,
        string CaseTitle,
        string Status,
        DateOnly StartDate);
}
