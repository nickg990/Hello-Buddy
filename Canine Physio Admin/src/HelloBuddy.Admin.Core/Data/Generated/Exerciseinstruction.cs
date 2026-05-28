using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Exerciseinstruction
{
    public ulong ExerciseInstructionId { get; set; }

    public ulong ExerciseId { get; set; }

    public ushort StepNumber { get; set; }

    public string InstructionText { get; set; } = null!;

    public virtual Exercise Exercise { get; set; } = null!;
}
