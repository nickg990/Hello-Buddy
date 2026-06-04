namespace HelloBuddy.Contracts;

public sealed class SaveExerciseRequest
{
    public ulong ExerciseCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ObjectiveSummary { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public ushort? DefaultReps { get; set; }
    public ushort? DefaultSets { get; set; }
    public ushort? DefaultHoldSeconds { get; set; }
    public bool IsActive { get; set; } = true;
    public List<InstructionStepInput> Instructions { get; set; } = [];

    public sealed class InstructionStepInput
    {
        public ushort StepNumber { get; set; }
        public string InstructionText { get; set; } = string.Empty;
    }
}
