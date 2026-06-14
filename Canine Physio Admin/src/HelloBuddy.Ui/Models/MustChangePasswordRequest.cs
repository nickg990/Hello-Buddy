using System.ComponentModel.DataAnnotations;

namespace HelloBuddy.Ui.Models;

/// <summary>View model for the forced first-login password change.</summary>
public sealed class MustChangePasswordRequest
{
    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(NewPassword), ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
