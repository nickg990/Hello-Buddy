using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Sessionskip
{
    public ulong SessionSkipId { get; set; }

    public ulong SessionOccurrenceId { get; set; }

    public ulong SessionSkipReasonId { get; set; }

    public string? Comments { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Sessionoccurrence SessionOccurrence { get; set; } = null!;

    public virtual Sessionskipreason SessionSkipReason { get; set; } = null!;
}
