using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Programmeversion
{
    public ulong ProgrammeVersionId { get; set; }

    public ulong ProgrammeId { get; set; }

    public uint VersionNumber { get; set; }

    public string VersionStatus { get; set; } = null!;

    public string PayloadJson { get; set; } = null!;

    public string PayloadSchemaVersion { get; set; } = null!;

    public string? ChangeSummary { get; set; }

    public ulong CreatedByPractitionerId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? PublishedDate { get; set; }

    public DateTime? SupersededDate { get; set; }

    public DateTime? RetiredDate { get; set; }

    public virtual Practitioner CreatedByPractitioner { get; set; } = null!;

    public virtual Programme Programme { get; set; } = null!;

    public virtual ICollection<Programme> Programmes { get; set; } = new List<Programme>();

    public virtual ICollection<Sessionoccurrence> Sessionoccurrences { get; set; } = new List<Sessionoccurrence>();
}
