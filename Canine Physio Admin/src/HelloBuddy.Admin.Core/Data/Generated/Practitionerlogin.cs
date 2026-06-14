using System;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Practitionerlogin
{
    public ulong PractitionerId { get; set; }

    public string PasswordHash { get; set; } = null!;

    /// <summary>physiotherapist | administrator</summary>
    public string Role { get; set; } = "physiotherapist";

    public bool IsActive { get; set; } = true;

    public bool MustChangePassword { get; set; }

    public byte FailedAttemptCount { get; set; }

    public DateTime? LockedUntil { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual Practitioner Practitioner { get; set; } = null!;
}
