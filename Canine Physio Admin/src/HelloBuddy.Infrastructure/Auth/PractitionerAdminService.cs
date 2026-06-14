using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Data.Entities;
using HelloBuddy.Application.Auth;
using Microsoft.EntityFrameworkCore;

namespace HelloBuddy.Infrastructure.Auth;

public sealed class PractitionerAdminService : IPractitionerAdminService
{
    private readonly CaninePhysioDbContext _db;

    public PractitionerAdminService(CaninePhysioDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PractitionerSummary>> ListPractitionersAsync(CancellationToken ct)
    {
        return await _db.Practitioners
            .Include(p => p.PractitionerLogin)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Select(p => new PractitionerSummary(
                p.PractitionerId,
                p.FirstName,
                p.LastName,
                p.Email,
                p.PhoneNumber,
                p.IsActive ?? true,
                p.PractitionerLogin != null ? p.PractitionerLogin.Role : "physiotherapist"))
            .ToListAsync(ct);
    }

    public async Task<AdminPractitionerResult> AddPractitionerAsync(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        string role,
        string initialPassword,
        CancellationToken ct)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var emailExists = await _db.Practitioners.AnyAsync(p => p.Email == normalizedEmail, ct);
        if (emailExists)
        {
            return new AdminPractitionerResult(AdminPractitionerOutcome.EmailAlreadyInUse,
                "A practitioner with that email address already exists.");
        }

        var practitioner = new Practitioner
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            IsActive = true,
        };
        _db.Practitioners.Add(practitioner);
        await _db.SaveChangesAsync(ct);

        var login = new Practitionerlogin
        {
            PractitionerId = practitioner.PractitionerId,
            PasswordHash = LoginService.HashPassword(normalizedEmail, initialPassword),
            Role = role == "administrator" ? "administrator" : "physiotherapist",
            IsActive = true,
            MustChangePassword = true,
        };
        _db.Practitionerlogins.Add(login);
        await _db.SaveChangesAsync(ct);

        return new AdminPractitionerResult(
            AdminPractitionerOutcome.Success,
            Practitioner: new PractitionerSummary(
                practitioner.PractitionerId,
                practitioner.FirstName,
                practitioner.LastName,
                practitioner.Email,
                practitioner.PhoneNumber,
                true,
                login.Role));
    }

    public async Task<AdminPractitionerResult> RenamePractitionerAsync(
        ulong targetPractitionerId,
        string firstName,
        string lastName,
        string newEmail,
        string? phoneNumber,
        CancellationToken ct)
    {
        var practitioner = await _db.Practitioners
            .FirstOrDefaultAsync(p => p.PractitionerId == targetPractitionerId, ct);
        if (practitioner is null)
        {
            return new AdminPractitionerResult(AdminPractitionerOutcome.NotFound);
        }

        var normalizedEmail = newEmail.Trim().ToLowerInvariant();
        if (!string.Equals(practitioner.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            var emailTaken = await _db.Practitioners
                .AnyAsync(p => p.Email == normalizedEmail && p.PractitionerId != targetPractitionerId, ct);
            if (emailTaken)
            {
                return new AdminPractitionerResult(AdminPractitionerOutcome.EmailAlreadyInUse,
                    "That email address is already used by another practitioner.");
            }
        }

        // Update practitioner record only; historical attribution name snapshots are unchanged.
        practitioner.FirstName = firstName.Trim();
        practitioner.LastName = lastName.Trim();
        practitioner.Email = normalizedEmail;
        practitioner.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        await _db.SaveChangesAsync(ct);

        var login = await _db.Practitionerlogins
            .FirstOrDefaultAsync(l => l.PractitionerId == targetPractitionerId, ct);

        return new AdminPractitionerResult(
            AdminPractitionerOutcome.Success,
            Practitioner: new PractitionerSummary(
                practitioner.PractitionerId,
                practitioner.FirstName,
                practitioner.LastName,
                practitioner.Email,
                practitioner.PhoneNumber,
                practitioner.IsActive ?? true,
                login?.Role ?? "physiotherapist"));
    }

    public async Task<AdminPractitionerResult> SetPasswordAsync(
        ulong targetPractitionerId,
        string newPassword,
        CancellationToken ct)
    {
        var login = await _db.Practitionerlogins
            .Include(l => l.Practitioner)
            .FirstOrDefaultAsync(l => l.PractitionerId == targetPractitionerId, ct);
        if (login is null)
        {
            return new AdminPractitionerResult(AdminPractitionerOutcome.NotFound);
        }

        login.PasswordHash = LoginService.HashPassword(login.Practitioner.Email, newPassword);
        login.MustChangePassword = false;
        login.FailedAttemptCount = 0;
        login.LockedUntil = null;
        await _db.SaveChangesAsync(ct);

        return new AdminPractitionerResult(AdminPractitionerOutcome.Success);
    }

    public async Task<AdminPractitionerResult> ChangeOwnPasswordAsync(
        ulong adminPractitionerId,
        string currentPassword,
        string newPassword,
        CancellationToken ct)
    {
        var login = await _db.Practitionerlogins
            .Include(l => l.Practitioner)
            .FirstOrDefaultAsync(l => l.PractitionerId == adminPractitionerId, ct);
        if (login is null)
        {
            return new AdminPractitionerResult(AdminPractitionerOutcome.NotFound);
        }

        if (!LoginService.VerifyPassword(login.Practitioner.Email, login.PasswordHash, currentPassword))
        {
            return new AdminPractitionerResult(AdminPractitionerOutcome.NotFound,
                "Current password is incorrect.");
        }

        login.PasswordHash = LoginService.HashPassword(login.Practitioner.Email, newPassword);
        login.MustChangePassword = false;
        await _db.SaveChangesAsync(ct);

        return new AdminPractitionerResult(AdminPractitionerOutcome.Success);
    }

    public async Task<AdminPractitionerResult> DeactivatePractitionerAsync(
        ulong targetPractitionerId,
        ulong requestingPractitionerId,
        CancellationToken ct)
    {
        if (targetPractitionerId == requestingPractitionerId)
        {
            return new AdminPractitionerResult(AdminPractitionerOutcome.CannotTargetSelf,
                "You cannot deactivate your own account.");
        }

        var practitioner = await _db.Practitioners
            .Include(p => p.PractitionerLogin)
            .FirstOrDefaultAsync(p => p.PractitionerId == targetPractitionerId, ct);

        if (practitioner is null)
        {
            return new AdminPractitionerResult(AdminPractitionerOutcome.NotFound);
        }

        practitioner.IsActive = false;
        if (practitioner.PractitionerLogin is not null)
        {
            practitioner.PractitionerLogin.IsActive = false;
        }

        await _db.SaveChangesAsync(ct);
        return new AdminPractitionerResult(AdminPractitionerOutcome.Success);
    }

    public async Task<AdminPractitionerResult> ActivatePractitionerAsync(
        ulong targetPractitionerId,
        CancellationToken ct)
    {
        var practitioner = await _db.Practitioners
            .Include(p => p.PractitionerLogin)
            .FirstOrDefaultAsync(p => p.PractitionerId == targetPractitionerId, ct);

        if (practitioner is null)
        {
            return new AdminPractitionerResult(AdminPractitionerOutcome.NotFound);
        }

        practitioner.IsActive = true;
        if (practitioner.PractitionerLogin is not null)
        {
            practitioner.PractitionerLogin.IsActive = true;
        }

        await _db.SaveChangesAsync(ct);
        return new AdminPractitionerResult(AdminPractitionerOutcome.Success);
    }
}
