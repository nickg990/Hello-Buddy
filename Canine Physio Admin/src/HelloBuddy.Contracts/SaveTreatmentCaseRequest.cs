using System.ComponentModel.DataAnnotations;

namespace HelloBuddy.Contracts;

public sealed class SaveTreatmentCaseRequest
{
    [Range(typeof(ulong), "1", "18446744073709551615")]
    public ulong PetId { get; set; }

    [Required]
    [StringLength(255)]
    public string CaseTitle { get; set; } = string.Empty;
    public string? ClinicalSummary { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "planned";
}
