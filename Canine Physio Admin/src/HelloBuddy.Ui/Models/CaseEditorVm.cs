using HelloBuddy.Contracts;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelloBuddy.Ui.Models;

public sealed class CaseEditorVm
{
    public ulong? TreatmentCaseId { get; set; }
    public SaveTreatmentCaseRequest Form { get; set; } = new();
    public IReadOnlyList<SelectListItem> PetOptions { get; set; } = [];
}
