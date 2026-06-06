namespace HelloBuddy.Contracts;

public sealed class ProgrammeStructureForm
{
    public ulong ProgrammeId { get; set; }
    public string ProgrammeName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    // Supported values: "single", "am-pm".
    public string SessionStructure { get; set; } = "single";
}