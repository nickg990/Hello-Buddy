using HelloBuddy.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelloBuddy.Ui.Models;

public sealed class ExerciseEditorVm
{
    public ulong? ExerciseId { get; set; }
    public SaveExerciseRequest Form { get; set; } = new();
    public IReadOnlyList<SelectListItem> CategoryOptions { get; set; } = [];
    public IReadOnlyList<VideoSearchProviderVm> VideoSearchProviders { get; set; } = [];
    public string ImageLibraryUrl { get; set; } = string.Empty;
    public string InstructionsText { get; set; } = string.Empty;
    public string? LegacyInstructionsText { get; set; }
}
