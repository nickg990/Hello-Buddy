using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Sessioncontenttype
{
    public ulong SessionContentTypeId { get; set; }

    public string ContentKey { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string MobileDescription { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
