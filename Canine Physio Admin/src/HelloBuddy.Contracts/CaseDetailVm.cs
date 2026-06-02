namespace HelloBuddy.Contracts;

public sealed record CaseDetailVm(
    ulong TreatmentCaseId,
    string CaseTitle,
    string Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? ClinicalSummary,
    ulong PetId,
    string PetName,
    string? PetBreed,
    string? PetSex,
    decimal? PetWeight,
    byte? PetAge,
    string OwnerName,
    string OwnerEmail,
    IReadOnlyList<CaseDetailVm.NoteRow> Notes,
    IReadOnlyList<CaseDetailVm.ProgrammeRow> Programmes)
{
    public sealed record NoteRow(
        ulong TreatmentCaseNoteId,
        DateTime CreatedDate,
        string? NoteType,
        string NoteText);

    public sealed record ProgrammeRow(
        ulong ProgrammeId,
        string ProgrammeName,
        string Status,
        DateOnly StartDate,
        DateOnly? EndDate,
        int SessionCount,
        int ExerciseCount);
}
