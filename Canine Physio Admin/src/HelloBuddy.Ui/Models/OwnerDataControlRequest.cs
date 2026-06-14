using System.ComponentModel.DataAnnotations;

namespace HelloBuddy.Ui.Models;

/// <summary>Form request for owner GDPR data-control submission from the Admin area.</summary>
public sealed class OwnerDataControlRequest
{
    [Range(typeof(ulong), "1", "18446744073709551615", ErrorMessage = "Select an owner before continuing.")]
    public ulong OwnerId { get; init; }
}
