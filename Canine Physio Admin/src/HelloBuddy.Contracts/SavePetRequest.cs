using System.ComponentModel.DataAnnotations;

namespace HelloBuddy.Contracts;

public sealed class SavePetRequest
{
    [Range(typeof(ulong), "1", "18446744073709551615")]
    public ulong OwnerId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, byte.MaxValue)]
    public byte? Age { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    [StringLength(100)]
    public string? Breed { get; set; }

    [Required]
    [StringLength(20)]
    public string Sex { get; set; } = "unknown";

    [Range(typeof(decimal), "0", "999.99")]
    public decimal? Weight { get; set; }
    public bool IsActive { get; set; } = true;
}
