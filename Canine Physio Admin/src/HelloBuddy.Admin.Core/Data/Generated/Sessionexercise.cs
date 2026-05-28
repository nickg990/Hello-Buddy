using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Sessionexercise
{
    public ulong SessionExerciseId { get; set; }

    public ulong SessionId { get; set; }

    public ulong ExerciseId { get; set; }

    public ushort? Reps { get; set; }

    public ushort? Sets { get; set; }

    public ushort? HoldSeconds { get; set; }

    public ushort SortOrder { get; set; }

    public string? Notes { get; set; }

    public virtual Exercise Exercise { get; set; } = null!;

    public virtual ICollection<Exercisecompletion> Exercisecompletions { get; set; } = new List<Exercisecompletion>();

    public virtual Session Session { get; set; } = null!;
}
