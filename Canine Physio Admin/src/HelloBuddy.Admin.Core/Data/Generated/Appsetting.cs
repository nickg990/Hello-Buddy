using System;

namespace HelloBuddy.Admin.Core.Data.Entities;

public partial class Appsetting
{
    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public DateTime UpdatedDate { get; set; }

    public ulong? UpdatedByPractitionerId { get; set; }
}
