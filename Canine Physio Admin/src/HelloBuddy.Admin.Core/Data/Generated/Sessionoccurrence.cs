using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Sessionoccurrence
{
    public ulong SessionOccurrenceId { get; set; }

    public ulong ProgrammeVersionId { get; set; }

    public ulong? SessionId { get; set; }

    public ulong PetId { get; set; }

    public DateOnly ScheduledDate { get; set; }

    public string Period { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? DeviceRecordedDateTime { get; set; }

    public DateTime? StartedDateTime { get; set; }

    public DateTime? CompletedDateTime { get; set; }

    public DateTime? SkippedDateTime { get; set; }

    public DateTime? SyncedDateTime { get; set; }

    public string? Comments { get; set; }

    public virtual ICollection<Exercisecompletion> Exercisecompletions { get; set; } = new List<Exercisecompletion>();

    public virtual Pet Pet { get; set; } = null!;

    public virtual Programmeversion ProgrammeVersion { get; set; } = null!;

    public virtual Session? Session { get; set; }

    public virtual Sessionskip? Sessionskip { get; set; }
}
