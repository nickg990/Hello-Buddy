using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class PractitionerPet
{
    public ulong PractitionerPetId { get; set; }

    public ulong PractitionerId { get; set; }

    public ulong PetId { get; set; }

    public DateTime AssignedFrom { get; set; }

    public DateTime? AssignedTo { get; set; }

    public bool IsPrimary { get; set; }

    public string Status { get; set; } = null!;

    public string? ReferralSource { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Pet Pet { get; set; } = null!;

    public virtual Practitioner Practitioner { get; set; } = null!;
}
