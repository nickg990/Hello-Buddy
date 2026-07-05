using HelloBuddy.Contracts;

namespace HelloBuddy.Ui.Models;

public sealed class ExerciseDetailPageVm
{
    public required ExerciseDetailVm Exercise { get; init; }
    public IReadOnlyList<ExerciseAuditEntryVm> AuditHistory { get; init; } = [];
}
