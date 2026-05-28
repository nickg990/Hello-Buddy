using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Exercise
{
    public ulong ExerciseId { get; set; }

    public ulong? ExerciseCategoryId { get; set; }

    public string ExerciseKey { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? ObjectiveSummary { get; set; }

    public string? InstructionsText { get; set; }

    public ushort? DefaultReps { get; set; }

    public ushort? DefaultSets { get; set; }

    public ushort? DefaultHoldSeconds { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public bool? IsActive { get; set; }

    public string? ImageUrl { get; set; }

    public string? VideoUrl { get; set; }

    public virtual Exercisecategory? ExerciseCategory { get; set; }

    public virtual ICollection<Exerciseinstruction> Exerciseinstructions { get; set; } = new List<Exerciseinstruction>();

    public virtual ICollection<Sessionexercise> Sessionexercises { get; set; } = new List<Sessionexercise>();
}
