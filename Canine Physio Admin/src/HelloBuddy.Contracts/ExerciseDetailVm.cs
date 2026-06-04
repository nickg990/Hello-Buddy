namespace HelloBuddy.Contracts;

public sealed record ExerciseDetailVm(
    ulong ExerciseId,
    ulong? ExerciseCategoryId,
    string? CategoryName,
    string ExerciseKey,
    string Title,
    string? ObjectiveSummary,
    string? ImageUrl,
    string? VideoUrl,
    ushort? DefaultReps,
    ushort? DefaultSets,
    ushort? DefaultHoldSeconds,
    bool IsActive,
    string? LegacyInstructionsText,
    DateTime UpdatedDate,
    IReadOnlyList<ExerciseDetailVm.InstructionStepVm> Instructions)
{
    public sealed record InstructionStepVm(
        ushort StepNumber,
        string InstructionText);
}
