namespace HelloBuddy.Application.Auth;

/// <summary>
/// Authenticates practitioners and manages credential state.
/// Cookie sign-in/out is handled in the UI layer; this service handles
/// the underlying credential verification and lockout logic.
/// </summary>
public interface ILoginService
{
    /// <summary>
    /// Validates an email + password credential pair (including honeypot, lockout,
    /// and inactive checks). Updates FailedAttemptCount, LockedUntil, and
    /// LastLoginDate as appropriate.
    /// </summary>
    Task<LoginResult> AuthenticateAsync(
        string email,
        string password,
        bool honeypotTriggered,
        CancellationToken ct);

    /// <summary>
    /// Forces a password change for the given practitioner (clears MustChangePassword flag).
    /// The caller must already be authenticated.
    /// </summary>
    Task<bool> ForceChangePasswordAsync(
        ulong practitionerId,
        string newPassword,
        CancellationToken ct);
}
