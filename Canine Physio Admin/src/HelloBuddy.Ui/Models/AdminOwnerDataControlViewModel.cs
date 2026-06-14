using HelloBuddy.Contracts;

namespace HelloBuddy.Ui.Models;

/// <summary>View model for administrator-initiated owner GDPR data-control actions.</summary>
public sealed class AdminOwnerDataControlViewModel
{
    public ulong? OwnerId { get; init; }
    public required IReadOnlyList<OwnerListItem> Owners { get; init; }
}
