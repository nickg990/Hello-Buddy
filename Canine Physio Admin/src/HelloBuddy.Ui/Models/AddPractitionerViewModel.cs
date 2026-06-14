using System.ComponentModel.DataAnnotations;

namespace HelloBuddy.Ui.Models;

/// <summary>View model for adding a new practitioner from the Admin area.</summary>
public sealed class AddPractitionerViewModel
{
    [Required]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "physiotherapist";

    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    [Display(Name = "Initial password")]
    public string Password { get; set; } = string.Empty;
}
