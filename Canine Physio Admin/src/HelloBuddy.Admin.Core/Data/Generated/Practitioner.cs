using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Practitioner
{
    public ulong PractitionerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual ICollection<Auditlog> Auditlogs { get; set; } = new List<Auditlog>();

    public virtual ICollection<PractitionerPet> PractitionerPets { get; set; } = new List<PractitionerPet>();

    public virtual ICollection<Programmetemplate> Programmetemplates { get; set; } = new List<Programmetemplate>();

    public virtual ICollection<Programmeversion> Programmeversions { get; set; } = new List<Programmeversion>();

    public virtual ICollection<Registrationcode> Registrationcodes { get; set; } = new List<Registrationcode>();

    public virtual ICollection<Treatmentcasenote> Treatmentcasenotes { get; set; } = new List<Treatmentcasenote>();

    public virtual ICollection<Treatmentcase> Treatmentcases { get; set; } = new List<Treatmentcase>();
}
