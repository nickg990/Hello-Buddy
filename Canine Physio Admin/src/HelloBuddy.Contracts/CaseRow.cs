namespace HelloBuddy.Contracts;

public sealed record CaseRow(
    ulong TreatmentCaseId,
    string CaseTitle,
    string Status,
    DateOnly StartDate,
    string PetName,
    string OwnerName,
    string PractitionerName);
