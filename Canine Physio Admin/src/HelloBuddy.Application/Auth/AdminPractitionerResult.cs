namespace HelloBuddy.Application.Auth;

public enum AdminPractitionerOutcome
{
    Success,
    NotFound,
    EmailAlreadyInUse,
    CannotTargetSelf,
}

public sealed record PractitionerSummary(
    ulong PractitionerId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    string Role);

public sealed record AdminPractitionerResult(
    AdminPractitionerOutcome Outcome,
    string? Message = null,
    PractitionerSummary? Practitioner = null);
