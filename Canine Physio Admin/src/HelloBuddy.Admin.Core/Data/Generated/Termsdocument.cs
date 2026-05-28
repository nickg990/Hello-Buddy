using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Termsdocument
{
    public ulong TermsDocumentId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string VersionNumber { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string ContentText { get; set; } = null!;

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<Appcontentblock> Appcontentblocks { get; set; } = new List<Appcontentblock>();

    public virtual ICollection<Termsacceptance> Termsacceptances { get; set; } = new List<Termsacceptance>();
}
