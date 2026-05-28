using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Owner
{
    public ulong OwnerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? Town { get; set; }

    public string? Postcode { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();

    public virtual ICollection<Useraccount> Useraccounts { get; set; } = new List<Useraccount>();
}
