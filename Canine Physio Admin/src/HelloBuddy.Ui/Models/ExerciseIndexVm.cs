using HelloBuddy.Contracts;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelloBuddy.Ui.Models;

public sealed class ExerciseIndexVm
{
    public ExerciseListFilter Filter { get; set; } = new();
    public IReadOnlyList<ExerciseListItem> Exercises { get; set; } = [];
    public IReadOnlyList<SelectListItem> CategoryOptions { get; set; } = [];
}
