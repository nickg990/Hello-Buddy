namespace HelloBuddy.Admin.Core.Data.Entities;

/// <summary>
/// Attribution columns added in Increment 8. Partial extension of the scaffolded entity.
/// </summary>
public partial class Owner
{
    public ulong? CreatedByPractitionerId { get; set; }
    public string? CreatedByPractitionerName { get; set; }
    public ulong? UpdatedByPractitionerId { get; set; }
    public string? UpdatedByPractitionerName { get; set; }
}

public partial class Pet
{
    public ulong? CreatedByPractitionerId { get; set; }
    public string? CreatedByPractitionerName { get; set; }
    public ulong? UpdatedByPractitionerId { get; set; }
    public string? UpdatedByPractitionerName { get; set; }
}

public partial class Treatmentcase
{
    public ulong? CreatedByPractitionerId { get; set; }
    public string? CreatedByPractitionerName { get; set; }
    public ulong? UpdatedByPractitionerId { get; set; }
    public string? UpdatedByPractitionerName { get; set; }
}

public partial class Treatmentcasenote
{
    public string? CreatedByPractitionerName { get; set; }
}

public partial class Programme
{
    public ulong? CreatedByPractitionerId { get; set; }
    public string? CreatedByPractitionerName { get; set; }
    public ulong? UpdatedByPractitionerId { get; set; }
    public string? UpdatedByPractitionerName { get; set; }
}

public partial class Exercise
{
    public ulong? CreatedByPractitionerId { get; set; }
    public string? CreatedByPractitionerName { get; set; }
    public ulong? UpdatedByPractitionerId { get; set; }
    public string? UpdatedByPractitionerName { get; set; }
}

public partial class Programmeversion
{
    public string? CreatedByPractitionerName { get; set; }
}
