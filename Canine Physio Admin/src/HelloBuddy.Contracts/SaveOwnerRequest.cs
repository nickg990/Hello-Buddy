using System.ComponentModel.DataAnnotations;

namespace HelloBuddy.Contracts;

public sealed class SaveOwnerRequest
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    public string? PhoneNumber { get; set; }

    [StringLength(255)]
    public string? AddressLine1 { get; set; }

    [StringLength(255)]
    public string? AddressLine2 { get; set; }

    [StringLength(100)]
    public string? Town { get; set; }

    [StringLength(20)]
    public string? Postcode { get; set; }
}
