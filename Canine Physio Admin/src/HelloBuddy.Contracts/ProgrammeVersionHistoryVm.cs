namespace HelloBuddy.Contracts;

public sealed record ProgrammeVersionHistoryVm(
    ulong ProgrammeId,
    string ProgrammeName,
    ulong TreatmentCaseId,
    ulong OwnerId,
    ulong PetId,
    string OwnerName,
    string PetName,
    string CaseTitle,
    IReadOnlyList<ProgrammeVersionHistoryVm.VersionRow> Versions)
{
    public sealed record VersionRow(
        ulong ProgrammeVersionId,
        uint VersionNumber,
        string VersionStatus,
        string? ChangeSummary,
        ulong CreatedByPractitionerId,
        string CreatedByPractitionerName,
        DateTime CreatedDate,
        DateTime? PublishedDate,
        DateTime? SupersededDate,
        DateTime? RetiredDate);
}