using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Passwordresetrequest
{
    public ulong PasswordResetRequestId { get; set; }

    public ulong UserAccountId { get; set; }

    public string ResetToken { get; set; } = null!;

    public DateTime RequestedDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public DateTime? ConsumedDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Useraccount UserAccount { get; set; } = null!;
}
