using HelloBuddy.Contracts;

namespace HelloBuddy.Ui.Models;

public sealed class HomePrivacyVm
{
    public required IReadOnlyList<OwnerListItem> Owners { get; init; }
}
