using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelloBuddy.Ui.Models;

/// <summary>
/// Model for the reusable exercise-filter pane partial (<c>_ExerciseFilterPane</c>).
/// A single page can render several instances (e.g. one per programme-builder
/// session), so every instance is scoped by <see cref="InstanceKey"/> which is
/// used to build unique element ids and a per-instance persistence key.
/// </summary>
public sealed class ExerciseFilterPaneVm
{
    /// <summary>
    /// Unique suffix for this pane instance (e.g. the session id). Used for
    /// element ids and to namespace client-side persistence.
    /// </summary>
    public required string InstanceKey { get; init; }

    /// <summary>
    /// Persistence bucket shared by all panes that should remember the same
    /// filter (e.g. the programme id). Combined with <see cref="InstanceKey"/>
    /// so each session within a programme persists independently.
    /// </summary>
    public string PersistScope { get; init; } = string.Empty;

    /// <summary>Active exercise categories for the category dropdown.</summary>
    public IReadOnlyList<SelectListItem> CategoryOptions { get; init; } = [];

    /// <summary>
    /// CSS selector of the target <c>&lt;select&gt;</c> whose options this pane
    /// filters. The client script narrows the visible options in that element.
    /// </summary>
    public required string TargetSelectSelector { get; init; }
}
