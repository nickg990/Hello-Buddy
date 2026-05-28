using System;
using System.Collections.Generic;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Notificationpreference
{
    public ulong NotificationPreferenceId { get; set; }

    public ulong UserAccountId { get; set; }

    public bool? NotificationsEnabled { get; set; }

    public TimeOnly? NotificationTime { get; set; }

    public bool DownloadVideosEnabled { get; set; }

    public bool OfflineCachingEnabled { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual Useraccount UserAccount { get; set; } = null!;
}
