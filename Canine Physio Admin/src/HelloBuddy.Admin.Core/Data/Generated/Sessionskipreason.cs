using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Sessionskipreason
{
    public ulong SessionSkipReasonId { get; set; }

    public string ReasonName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Sessionskip> Sessionskips { get; set; } = new List<Sessionskip>();
}
