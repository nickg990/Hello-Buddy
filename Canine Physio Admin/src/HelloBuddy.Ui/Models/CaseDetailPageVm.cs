using HelloBuddy.Contracts;

namespace HelloBuddy.Ui.Models;

public sealed class CaseDetailPageVm
{
    public required CaseDetailVm Case { get; init; }
    public CreateCaseNoteRequest NewNote { get; init; } = new();
}
