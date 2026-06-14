using HelloBuddy.Application.Auth;

namespace HelloBuddy.Ui.Services;

/// <summary>
/// Fallback auth services for UI-only host/test startup when DB-backed
/// auth services are not enabled. These prevent DI startup failures.
/// </summary>
public sealed class NoOpLoginService : ILoginService
{
    public Task<LoginResult> AuthenticateAsync(string email, string password, bool honeypotTriggered, CancellationToken ct)
        => Task.FromResult(new LoginResult(LoginOutcome.InvalidCredentials));

    public Task<bool> ForceChangePasswordAsync(ulong practitionerId, string newPassword, CancellationToken ct)
        => Task.FromResult(false);
}

public sealed class NoOpPractitionerAdminService : IPractitionerAdminService
{
    public Task<IReadOnlyList<PractitionerSummary>> ListPractitionersAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<PractitionerSummary>>(Array.Empty<PractitionerSummary>());

    public Task<AdminPractitionerResult> AddPractitionerAsync(string firstName, string lastName, string email, string? phoneNumber, string role, string initialPassword, CancellationToken ct)
        => Task.FromResult(new AdminPractitionerResult(AdminPractitionerOutcome.NotFound, "DB-backed admin services are disabled."));

    public Task<AdminPractitionerResult> RenamePractitionerAsync(ulong targetPractitionerId, string firstName, string lastName, string newEmail, string? phoneNumber, CancellationToken ct)
        => Task.FromResult(new AdminPractitionerResult(AdminPractitionerOutcome.NotFound, "DB-backed admin services are disabled."));

    public Task<AdminPractitionerResult> SetPasswordAsync(ulong targetPractitionerId, string newPassword, CancellationToken ct)
        => Task.FromResult(new AdminPractitionerResult(AdminPractitionerOutcome.NotFound, "DB-backed admin services are disabled."));

    public Task<AdminPractitionerResult> ChangeOwnPasswordAsync(ulong adminPractitionerId, string currentPassword, string newPassword, CancellationToken ct)
        => Task.FromResult(new AdminPractitionerResult(AdminPractitionerOutcome.NotFound, "DB-backed admin services are disabled."));

    public Task<AdminPractitionerResult> DeactivatePractitionerAsync(ulong targetPractitionerId, ulong requestingPractitionerId, CancellationToken ct)
        => Task.FromResult(new AdminPractitionerResult(AdminPractitionerOutcome.NotFound, "DB-backed admin services are disabled."));

    public Task<AdminPractitionerResult> ActivatePractitionerAsync(ulong targetPractitionerId, CancellationToken ct)
        => Task.FromResult(new AdminPractitionerResult(AdminPractitionerOutcome.NotFound, "DB-backed admin services are disabled."));
}
