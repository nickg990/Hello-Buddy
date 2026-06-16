namespace HelloBuddy.Contracts;

public sealed record ProgrammeVm(
    ulong ProgrammeId,
    ulong TreatmentCaseId,
    ulong PetId,
    ulong OwnerId,
    string ProgrammeName,
    string Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Notes,
    string CaseTitle,
    string PetName,
    string OwnerName,
    string PractitionerName,
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
        string? ImageUrl,
        string? VideoUrl,
        ushort? Reps,
        ushort? Sets,
        ushort? HoldSeconds,
        ushort SortOrder,
        string? Notes,
        IReadOnlyList<ProgrammeVm.InstructionStep> Instructions);

    public sealed record InstructionStep(ushort StepNumber, string InstructionText);
}
