using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Registrationcode
{
    public ulong RegistrationCodeId { get; set; }

    public ulong PractitionerId { get; set; }

    public ulong? PetId { get; set; }

    public string Code { get; set; } = null!;

    public DateTime IssuedDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public DateTime? UsedDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public virtual Pet? Pet { get; set; }

    public virtual Practitioner Practitioner { get; set; } = null!;
}
