namespace HelloBuddy.Admin.Models;

public sealed record ProgrammeVm(
    ulong ProgrammeId,
    ulong TreatmentCaseId,
    string ProgrammeName,
    string Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Notes,
    string CaseTitle,
    string PetName,
    string OwnerName,
    IReadOnlyList<ProgrammeVm.SessionRow> Sessions)
{
    public sealed record SessionRow(
        ulong SessionId,
        string Period,
        string? Objective,
        string Status,
        byte SortOrder,
        IReadOnlyList<SessionExerciseRow> Exercises);

    public sealed record SessionExerciseRow(
        ulong SessionExerciseId,
        ulong ExerciseId,
        string ExerciseTitle,
        string? ObjectiveSummary,
        ushort? Reps,
        ushort? Sets,
        ushort? HoldSeconds,
        ushort SortOrder,
        string? Notes);
}
