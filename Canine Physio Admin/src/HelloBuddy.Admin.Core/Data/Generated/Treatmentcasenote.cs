using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Treatmentcasenote
{
    public ulong TreatmentCaseNoteId { get; set; }

    public ulong TreatmentCaseId { get; set; }

    public ulong PractitionerId { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? NoteType { get; set; }

    public string NoteText { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual Practitioner Practitioner { get; set; } = null!;

    public virtual Treatmentcase TreatmentCase { get; set; } = null!;
}
