using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Auditlog
{
    public ulong AuditLogId { get; set; }

    public ulong? PractitionerId { get; set; }

    public ulong? UserAccountId { get; set; }

    public string EntityName { get; set; } = null!;

    public ulong EntityId { get; set; }

    public string ActionType { get; set; } = null!;

    public string? OldValuesJson { get; set; }

    public string? NewValuesJson { get; set; }

    public DateTime ActionDateTime { get; set; }

    public virtual Practitioner? Practitioner { get; set; }

    public virtual Useraccount? UserAccount { get; set; }
}
