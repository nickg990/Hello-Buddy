using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Session
{
    public ulong SessionId { get; set; }

    public ulong ProgrammeId { get; set; }

    public ulong? SessionContentTypeId { get; set; }

    public DateTime CreatedDate { get; set; }

    public string Period { get; set; } = null!;

    public string? Objective { get; set; }

    public string Status { get; set; } = null!;

    public byte SortOrder { get; set; }

    public virtual Programme Programme { get; set; } = null!;

    public virtual Sessioncontenttype? SessionContentType { get; set; }

    public virtual ICollection<Sessionexercise> Sessionexercises { get; set; } = new List<Sessionexercise>();

    public virtual ICollection<Sessionoccurrence> Sessionoccurrences { get; set; } = new List<Sessionoccurrence>();
}
