using HelloBuddy.Contracts;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelloBuddy.Ui.Models;

public sealed class PetEditorVm
{
    public ulong? PetId { get; set; }
    public SavePetRequest Form { get; set; } = new();
    public IReadOnlyList<SelectListItem> OwnerOptions { get; set; } = [];
}
