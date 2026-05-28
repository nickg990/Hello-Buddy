using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Pet
{
    public ulong PetId { get; set; }

    public ulong OwnerId { get; set; }

    public string Name { get; set; } = null!;

    public byte? Age { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Breed { get; set; }

    public string Sex { get; set; } = null!;

    public decimal? Weight { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual Owner Owner { get; set; } = null!;

    public virtual ICollection<PractitionerPet> PractitionerPets { get; set; } = new List<PractitionerPet>();

    public virtual ICollection<Registrationcode> Registrationcodes { get; set; } = new List<Registrationcode>();

    public virtual ICollection<Sessionoccurrence> Sessionoccurrences { get; set; } = new List<Sessionoccurrence>();

    public virtual ICollection<Treatmentcase> Treatmentcases { get; set; } = new List<Treatmentcase>();
}
