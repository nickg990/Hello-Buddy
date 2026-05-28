using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Treatmentcase
{
    public ulong TreatmentCaseId { get; set; }

    public ulong PetId { get; set; }

    public ulong PractitionerId { get; set; }

    public string CaseTitle { get; set; } = null!;

    public string? ClinicalSummary { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual Pet Pet { get; set; } = null!;

    public virtual Practitioner Practitioner { get; set; } = null!;

    public virtual ICollection<Programme> Programmes { get; set; } = new List<Programme>();

    public virtual ICollection<Treatmentcasenote> Treatmentcasenotes { get; set; } = new List<Treatmentcasenote>();
}
