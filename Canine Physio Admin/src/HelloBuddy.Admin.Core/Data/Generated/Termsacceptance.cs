using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Termsacceptance
{
    public ulong TermsAcceptanceId { get; set; }

    public ulong UserAccountId { get; set; }

    public ulong TermsDocumentId { get; set; }

    public DateTime AcceptedDateTime { get; set; }

    public string? AcceptanceMethod { get; set; }

    public string? AcceptedVersionText { get; set; }

    public virtual Termsdocument TermsDocument { get; set; } = null!;

    public virtual Useraccount UserAccount { get; set; } = null!;
}
