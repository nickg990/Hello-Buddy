using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Exercisecompletion
{
    public ulong ExerciseCompletionId { get; set; }

    public ulong SessionOccurrenceId { get; set; }

    public ulong? SessionExerciseId { get; set; }

    public string ExerciseKeySnapshot { get; set; } = null!;

    public string? ExerciseTitleSnapshot { get; set; }

    public DateTime? CompletedDateTime { get; set; }

    public DateTime? DeviceRecordedDateTime { get; set; }

    public DateTime? SyncedDateTime { get; set; }

    public ushort? RepsCompleted { get; set; }

    public ushort? SetsCompleted { get; set; }

    public byte? PainScore { get; set; }

    public string? Comments { get; set; }

    public string CompletionStatus { get; set; } = null!;

    public virtual Sessionexercise? SessionExercise { get; set; }

    public virtual Sessionoccurrence SessionOccurrence { get; set; } = null!;
}
