using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Useraccount
{
    public ulong UserAccountId { get; set; }

    public ulong OwnerId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PasswordSalt { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual ICollection<Auditlog> Auditlogs { get; set; } = new List<Auditlog>();

    public virtual Notificationpreference? Notificationpreference { get; set; }

    public virtual Owner Owner { get; set; } = null!;

    public virtual ICollection<Passwordresetrequest> Passwordresetrequests { get; set; } = new List<Passwordresetrequest>();

    public virtual ICollection<Termsacceptance> Termsacceptances { get; set; } = new List<Termsacceptance>();
}
