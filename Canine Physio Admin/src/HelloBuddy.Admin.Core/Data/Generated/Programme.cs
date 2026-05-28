using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Programme
{
    public ulong ProgrammeId { get; set; }

    public ulong TreatmentCaseId { get; set; }

    public ulong? ProgrammeTemplateId { get; set; }

    public string ProgrammeName { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Status { get; set; } = null!;

    public bool? IsCurrent { get; set; }

    public ulong? CurrentProgrammeVersionId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual Programmeversion? CurrentProgrammeVersion { get; set; }

    public virtual Programmetemplate? ProgrammeTemplate { get; set; }

    public virtual ICollection<Programmeversion> Programmeversions { get; set; } = new List<Programmeversion>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public virtual Treatmentcase TreatmentCase { get; set; } = null!;
}
