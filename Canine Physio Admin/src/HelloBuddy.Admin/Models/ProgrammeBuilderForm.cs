namespace HelloBuddy.Admin.Models;

/// <summary>
/// Form-bound payload for the programme builder edit submission.
/// </summary>
public sealed class ProgrammeBuilderForm
{
    public ulong ProgrammeId { get; set; }
    public List<SessionExerciseEdit> Exercises { get; set; } = new();

    public sealed class SessionExerciseEdit
    {
        public ulong SessionExerciseId { get; set; }
        public ushort? Reps { get; set; }
        public ushort? Sets { get; set; }
        public ushort? HoldSeconds { get; set; }
        public ushort SortOrder { get; set; }
        public string? Notes { get; set; }
    }
}
