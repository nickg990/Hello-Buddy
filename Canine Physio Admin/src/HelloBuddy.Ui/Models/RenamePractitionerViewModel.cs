using System.ComponentModel.DataAnnotations;

namespace HelloBuddy.Ui.Models;

/// <summary>View model for renaming/editing an existing practitioner.</summary>
public sealed class RenamePractitionerViewModel
{
    public ulong PractitionerId { get; set; }

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
}
