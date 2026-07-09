using System.ComponentModel.DataAnnotations;

namespace HelloBuddy.Contracts;

/// <summary>
/// Form-bound payload for the programme builder edit submission.
/// </summary>
public sealed class ProgrammeBuilderForm
{
    public ulong ProgrammeId { get; set; }
    public List<SessionEdit> Sessions { get; set; } = new();
    public List<SessionExerciseEdit> Exercises { get; set; } = new();

    public sealed class SessionEdit
    {
        public ulong SessionId { get; set; }
        public string? Objective { get; set; }
    }

    public sealed class SessionExerciseEdit
    {
        public ulong SessionExerciseId { get; set; }
        public ushort? Reps { get; set; }
        public ushort? Sets { get; set; }
        public ushort? HoldSeconds { get; set; }
        public ushort SortOrder { get; set; }
        [StringLength(60)]
        public string? Notes { get; set; }
    }
}
