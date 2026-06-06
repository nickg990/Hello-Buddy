namespace HelloBuddy.Application.Programmes;

public enum ProgrammeStatusTransitionResult
{
    Updated,
    NotFound,
    InvalidTransition,
    BlockedByAnotherActiveProgramme,
}