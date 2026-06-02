using HelloBuddy.Contracts;

namespace HelloBuddy.Ui.Models;

public sealed class OwnerEditorVm
{
    public ulong? OwnerId { get; set; }
    public SaveOwnerRequest Form { get; set; } = new();
}
