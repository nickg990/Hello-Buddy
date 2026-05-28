namespace HelloBuddy.Contracts;

public sealed record CaseDetailVm(
    ulong TreatmentCaseId,
    string CaseTitle,
    string Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? ClinicalSummary,
    string PetName,
    string? PetBreed,
    string? PetSex,
    decimal? PetWeight,
    byte? PetAge,
    string OwnerName,
    string OwnerEmail,
    IReadOnlyList<CaseDetailVm.ProgrammeRow> Programmes)
{
    public sealed record ProgrammeRow(
        ulong ProgrammeId,
        string ProgrammeName,
        string Status,
        DateOnly StartDate,
        DateOnly? EndDate,
        int SessionCount,
        int ExerciseCount);
}
