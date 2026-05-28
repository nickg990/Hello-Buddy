using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Programmetemplate
{
    public ulong ProgrammeTemplateId { get; set; }

    public ulong PractitionerId { get; set; }

    public string TemplateName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual Practitioner Practitioner { get; set; } = null!;

    public virtual ICollection<Programme> Programmes { get; set; } = new List<Programme>();
}
