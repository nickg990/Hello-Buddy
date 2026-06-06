using HelloBuddy.Contracts;

namespace HelloBuddy.Ui.Models;

public sealed class ProgrammesBuilderPageVm
{
    public required ProgrammeVm Programme { get; init; }
    public required IReadOnlyList<ExerciseListItem> AvailableExercises { get; init; }

    public string SessionStructure
        => Programme.Sessions.Any(s => string.Equals(s.Period, ProgrammeDomainConstants.SessionPeriodAm, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(s.Period, ProgrammeDomainConstants.SessionPeriodPm, StringComparison.OrdinalIgnoreCase))
            ? ProgrammeDomainConstants.SessionStructureAmPm
            : ProgrammeDomainConstants.SessionStructureSingle;
}