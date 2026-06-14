using System.ComponentModel.DataAnnotations;

namespace HelloBuddy.Ui.Models;

/// <summary>View model for the login form (email + password).</summary>
public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
