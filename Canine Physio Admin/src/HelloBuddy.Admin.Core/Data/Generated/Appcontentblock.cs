using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Appcontentblock
{
    public ulong AppContentBlockId { get; set; }

    public string ContentGroup { get; set; } = null!;

    public string ContentKey { get; set; } = null!;

    public string? HeaderText { get; set; }

    public string? BodyText { get; set; }

    public ulong? LinkedTermsDocumentId { get; set; }

    public ushort SortOrder { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual Termsdocument? LinkedTermsDocument { get; set; }
}
