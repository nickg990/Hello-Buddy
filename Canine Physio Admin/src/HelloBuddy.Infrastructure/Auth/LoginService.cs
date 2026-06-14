using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Data.Entities;
using HelloBuddy.Application.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelloBuddy.Infrastructure.Auth;

public sealed class LoginService : ILoginService
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly PasswordHasher<string> Hasher = new();

    private readonly CaninePhysioDbContext _db;

    public LoginService(CaninePhysioDbContext db)
    {
        _db = db;
    }

    public async Task<LoginResult> AuthenticateAsync(
        string email,
        string password,
        bool honeypotTriggered,
        CancellationToken ct)
    {
        if (honeypotTriggered)
        {
            return new LoginResult(LoginOutcome.HoneypotTriggered);
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new LoginResult(LoginOutcome.InvalidCredentials);
        }

        var practitioner = await _db.Practitioners
            .Include(p => p.PractitionerLogin)
            .FirstOrDefaultAsync(p => p.Email == email.Trim().ToLowerInvariant(), ct);

        if (practitioner?.PractitionerLogin is null)
        {
            return new LoginResult(LoginOutcome.InvalidCredentials);
        }

        var login = practitioner.PractitionerLogin;

        if (!login.IsActive || practitioner.IsActive != true)
        {
            return new LoginResult(LoginOutcome.AccountInactive);
        }

        if (login.LockedUntil.HasValue && login.LockedUntil.Value > DateTime.UtcNow)
        {
            return new LoginResult(LoginOutcome.AccountLocked);
        }

        var verifyResult = Hasher.VerifyHashedPassword(email, login.PasswordHash, password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            login.FailedAttemptCount++;
            if (login.FailedAttemptCount >= MaxFailedAttempts)
            {
                login.LockedUntil = DateTime.UtcNow.Add(LockoutDuration);
            }
            await _db.SaveChangesAsync(ct);
            return new LoginResult(LoginOutcome.InvalidCredentials);
        }

        // Success — reset lockout state
        login.FailedAttemptCount = 0;
        login.LockedUntil = null;
        login.LastLoginDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var fullName = $"{practitioner.FirstName} {practitioner.LastName}";

        if (login.MustChangePassword)
        {
            return new LoginResult(
                LoginOutcome.MustChangePassword,
                practitioner.PractitionerId,
                fullName,
                login.Role);
        }

        return new LoginResult(
            LoginOutcome.Success,
            practitioner.PractitionerId,
            fullName,
            login.Role);
    }

    public async Task<bool> ForceChangePasswordAsync(
        ulong practitionerId,
        string newPassword,
        CancellationToken ct)
    {
        var login = await _db.Practitionerlogins
            .FirstOrDefaultAsync(l => l.PractitionerId == practitionerId, ct);
        if (login is null)
        {
            return false;
        }

        login.PasswordHash = Hasher.HashPassword(
            login.Practitioner?.Email ?? practitionerId.ToString(),
            newPassword);
        login.MustChangePassword = false;
        login.FailedAttemptCount = 0;
        login.LockedUntil = null;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Creates a PBKDF2-SHA256 hash for a given password (used by seeding and admin ops).</summary>
    public static string HashPassword(string emailAsKey, string password)
        => Hasher.HashPassword(emailAsKey, password);

    /// <summary>Verifies a stored hash against a password attempt.</summary>
    public static bool VerifyPassword(string emailAsKey, string hash, string password)
        => Hasher.VerifyHashedPassword(emailAsKey, hash, password) != PasswordVerificationResult.Failed;
}
