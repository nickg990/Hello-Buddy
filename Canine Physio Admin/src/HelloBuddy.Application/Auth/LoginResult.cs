namespace HelloBuddy.Application.Auth;

public enum LoginOutcome
{
    Success,
    InvalidCredentials,
    AccountLocked,
    AccountInactive,
    HoneypotTriggered,
    MustChangePassword,
}

public sealed record LoginResult(
    LoginOutcome Outcome,
    ulong PractitionerId = 0,
    string PractitionerName = "",
    string PractitionerRole = "");
