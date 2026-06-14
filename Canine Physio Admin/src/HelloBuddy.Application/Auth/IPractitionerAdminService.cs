namespace HelloBuddy.Application.Auth;

/// <summary>
/// Administrator operations: practitioner management (add, rename, soft-delete,
/// set/reset password, change admin password). UI-side role enforcement only
/// (API endpoints trust the X-Practitioner-Role header — TD-005).
/// </summary>
public interface IPractitionerAdminService
{
    Task<IReadOnlyList<PractitionerSummary>> ListPractitionersAsync(CancellationToken ct);

    Task<AdminPractitionerResult> AddPractitionerAsync(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        string role,
        string initialPassword,
        CancellationToken ct);

    /// <summary>
    /// Renames a practitioner and optionally changes their login email.
    /// Historical attribution snapshots are NOT rewritten.
    /// </summary>
    Task<AdminPractitionerResult> RenamePractitionerAsync(
        ulong targetPractitionerId,
        string firstName,
        string lastName,
        string newEmail,
        string? phoneNumber,
        CancellationToken ct);

    Task<AdminPractitionerResult> SetPasswordAsync(
        ulong targetPractitionerId,
        string newPassword,
        CancellationToken ct);

    /// <summary>
    /// Changes the admin's own password, requiring the current password for verification.
    /// </summary>
    Task<AdminPractitionerResult> ChangeOwnPasswordAsync(
        ulong adminPractitionerId,
        string currentPassword,
        string newPassword,
        CancellationToken ct);

    /// <summary>
    /// Soft-deletes (deactivates) a practitioner. Cannot target the calling admin.
    /// </summary>
    Task<AdminPractitionerResult> DeactivatePractitionerAsync(
        ulong targetPractitionerId,
        ulong requestingPractitionerId,
        CancellationToken ct);

    /// <summary>
    /// Re-activates a previously deactivated practitioner.
    /// </summary>
    Task<AdminPractitionerResult> ActivatePractitionerAsync(
        ulong targetPractitionerId,
        CancellationToken ct);
}
