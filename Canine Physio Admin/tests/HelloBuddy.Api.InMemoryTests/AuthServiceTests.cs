using FluentAssertions;
using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Data.Entities;
using HelloBuddy.Application.Auth;
using HelloBuddy.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HelloBuddy.Api.InMemoryTests;

/// <summary>
/// Behaviour-focused unit tests for the Increment 8 authentication and
/// practitioner-administration services. These exercise the real
/// <see cref="LoginService"/> and <see cref="PractitionerAdminService"/>
/// logic (lockout, honeypot, inactivity, duplicate-email, self-deactivation,
/// password verification) which the UI flow tests only cover via stubs.
/// </summary>
public sealed class AuthServiceTests
{
    private const string ValidPassword = "Password12345!";

    private static CaninePhysioDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<CaninePhysioDbContext>()
            .UseInMemoryDatabase($"auth-{Guid.NewGuid():N}")
            .Options;
        return new CaninePhysioDbContext(options);
    }

    private static async Task<Practitioner> SeedPractitionerAsync(
        CaninePhysioDbContext db,
        string email,
        string password = ValidPassword,
        string role = "physiotherapist",
        bool loginActive = true,
        bool practitionerActive = true,
        bool mustChangePassword = false)
    {
        var practitioner = new Practitioner
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            IsActive = practitionerActive,
        };
        db.Practitioners.Add(practitioner);
        await db.SaveChangesAsync();

        db.Practitionerlogins.Add(new Practitionerlogin
        {
            PractitionerId = practitioner.PractitionerId,
            PasswordHash = LoginService.HashPassword(email, password),
            Role = role,
            IsActive = loginActive,
            MustChangePassword = mustChangePassword,
        });
        await db.SaveChangesAsync();

        return practitioner;
    }

    // ---------------- LoginService ----------------

    [Fact]
    public async Task AuthenticateAsync_WhenHoneypotTriggered_ReturnsHoneypot()
    {
        await using var db = NewContext();
        var sut = new LoginService(db);

        var result = await sut.AuthenticateAsync("anyone@example.test", "irrelevant", honeypotTriggered: true, CancellationToken.None);

        result.Outcome.Should().Be(LoginOutcome.HoneypotTriggered);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenEmailUnknown_ReturnsInvalidCredentials()
    {
        await using var db = NewContext();
        var sut = new LoginService(db);

        var result = await sut.AuthenticateAsync("missing@example.test", ValidPassword, honeypotTriggered: false, CancellationToken.None);

        result.Outcome.Should().Be(LoginOutcome.InvalidCredentials);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenWrongPassword_IncrementsFailedCount()
    {
        await using var db = NewContext();
        var practitioner = await SeedPractitionerAsync(db, "user@example.test");
        var sut = new LoginService(db);

        var result = await sut.AuthenticateAsync("user@example.test", "wrong-password", honeypotTriggered: false, CancellationToken.None);

        result.Outcome.Should().Be(LoginOutcome.InvalidCredentials);
        var login = await db.Practitionerlogins.SingleAsync(l => l.PractitionerId == practitioner.PractitionerId);
        login.FailedAttemptCount.Should().Be(1);
        login.LockedUntil.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_AfterFiveFailures_LocksAccount()
    {
        await using var db = NewContext();
        var practitioner = await SeedPractitionerAsync(db, "user@example.test");
        var sut = new LoginService(db);

        for (var i = 0; i < 5; i++)
        {
            await sut.AuthenticateAsync("user@example.test", "wrong-password", honeypotTriggered: false, CancellationToken.None);
        }

        var login = await db.Practitionerlogins.SingleAsync(l => l.PractitionerId == practitioner.PractitionerId);
        login.FailedAttemptCount.Should().BeGreaterThanOrEqualTo(5);
        login.LockedUntil.Should().NotBeNull();
        login.LockedUntil!.Value.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenLocked_ReturnsAccountLocked_EvenWithValidPassword()
    {
        await using var db = NewContext();
        var practitioner = await SeedPractitionerAsync(db, "user@example.test");
        var login = await db.Practitionerlogins.SingleAsync(l => l.PractitionerId == practitioner.PractitionerId);
        login.LockedUntil = DateTime.UtcNow.AddMinutes(10);
        await db.SaveChangesAsync();
        var sut = new LoginService(db);

        var result = await sut.AuthenticateAsync("user@example.test", ValidPassword, honeypotTriggered: false, CancellationToken.None);

        result.Outcome.Should().Be(LoginOutcome.AccountLocked);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenLoginInactive_ReturnsAccountInactive()
    {
        await using var db = NewContext();
        await SeedPractitionerAsync(db, "user@example.test", loginActive: false);
        var sut = new LoginService(db);

        var result = await sut.AuthenticateAsync("user@example.test", ValidPassword, honeypotTriggered: false, CancellationToken.None);

        result.Outcome.Should().Be(LoginOutcome.AccountInactive);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsSuccess_AndResetsState()
    {
        await using var db = NewContext();
        var practitioner = await SeedPractitionerAsync(db, "user@example.test");
        var login = await db.Practitionerlogins.SingleAsync(l => l.PractitionerId == practitioner.PractitionerId);
        login.FailedAttemptCount = 3;
        await db.SaveChangesAsync();
        var sut = new LoginService(db);

        var result = await sut.AuthenticateAsync("user@example.test", ValidPassword, honeypotTriggered: false, CancellationToken.None);

        result.Outcome.Should().Be(LoginOutcome.Success);
        result.PractitionerId.Should().Be(practitioner.PractitionerId);
        result.PractitionerName.Should().Be("Test User");

        var reloaded = await db.Practitionerlogins.SingleAsync(l => l.PractitionerId == practitioner.PractitionerId);
        reloaded.FailedAttemptCount.Should().Be(0);
        reloaded.LockedUntil.Should().BeNull();
        reloaded.LastLoginDate.Should().NotBeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenEmailCasingDiffers_StillAuthenticates()
    {
        await using var db = NewContext();
        await SeedPractitionerAsync(db, "user@example.test");
        var sut = new LoginService(db);

        var result = await sut.AuthenticateAsync("USER@Example.TEST", ValidPassword, honeypotTriggered: false, CancellationToken.None);

        result.Outcome.Should().Be(LoginOutcome.Success);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenMustChangePassword_ReturnsMustChangePassword()
    {
        await using var db = NewContext();
        await SeedPractitionerAsync(db, "user@example.test", mustChangePassword: true);
        var sut = new LoginService(db);

        var result = await sut.AuthenticateAsync("user@example.test", ValidPassword, honeypotTriggered: false, CancellationToken.None);

        result.Outcome.Should().Be(LoginOutcome.MustChangePassword);
    }

    [Fact]
    public async Task ForceChangePasswordAsync_ClearsFlagAndAllowsNewPassword()
    {
        await using var db = NewContext();
        var practitioner = await SeedPractitionerAsync(db, "user@example.test", mustChangePassword: true);
        var sut = new LoginService(db);

        var changed = await sut.ForceChangePasswordAsync(practitioner.PractitionerId, "BrandNewPass99!", CancellationToken.None);

        changed.Should().BeTrue();
        var login = await db.Practitionerlogins.SingleAsync(l => l.PractitionerId == practitioner.PractitionerId);
        login.MustChangePassword.Should().BeFalse();
        LoginService.VerifyPassword("user@example.test", login.PasswordHash, "BrandNewPass99!").Should().BeTrue();
    }

    // ---------------- PractitionerAdminService ----------------

    [Fact]
    public async Task AddPractitionerAsync_WithDuplicateEmail_ReturnsEmailAlreadyInUse()
    {
        await using var db = NewContext();
        await SeedPractitionerAsync(db, "existing@example.test");
        var sut = new PractitionerAdminService(db);

        var result = await sut.AddPractitionerAsync(
            "New", "Person", "EXISTING@example.test", null, "physiotherapist", ValidPassword, CancellationToken.None);

        result.Outcome.Should().Be(AdminPractitionerOutcome.EmailAlreadyInUse);
    }

    [Fact]
    public async Task AddPractitionerAsync_Success_HashesPassword_DefaultsRole_AndForcesPasswordChange()
    {
        await using var db = NewContext();
        var sut = new PractitionerAdminService(db);

        var result = await sut.AddPractitionerAsync(
            "Jamie", "Lee", "jamie.lee@example.test", "07123 456789", "unknown-role", ValidPassword, CancellationToken.None);

        result.Outcome.Should().Be(AdminPractitionerOutcome.Success);

        var login = await db.Practitionerlogins
            .Include(l => l.Practitioner)
            .SingleAsync(l => l.Practitioner.Email == "jamie.lee@example.test");
        login.Role.Should().Be("physiotherapist");
        login.MustChangePassword.Should().BeTrue();
        login.PasswordHash.Should().NotBe(ValidPassword);
        LoginService.VerifyPassword("jamie.lee@example.test", login.PasswordHash, ValidPassword).Should().BeTrue();
    }

    [Fact]
    public async Task RenamePractitionerAsync_WithConflictingEmail_ReturnsEmailAlreadyInUse()
    {
        await using var db = NewContext();
        await SeedPractitionerAsync(db, "taken@example.test");
        var target = await SeedPractitionerAsync(db, "target@example.test");
        var sut = new PractitionerAdminService(db);

        var result = await sut.RenamePractitionerAsync(
            target.PractitionerId, "New", "Name", "taken@example.test", null, CancellationToken.None);

        result.Outcome.Should().Be(AdminPractitionerOutcome.EmailAlreadyInUse);
    }

    [Fact]
    public async Task RenamePractitionerAsync_Success_UpdatesRecord()
    {
        await using var db = NewContext();
        var target = await SeedPractitionerAsync(db, "target@example.test");
        var sut = new PractitionerAdminService(db);

        var result = await sut.RenamePractitionerAsync(
            target.PractitionerId, "Renamed", "Practitioner", "renamed@example.test", "07000 000000", CancellationToken.None);

        result.Outcome.Should().Be(AdminPractitionerOutcome.Success);
        var reloaded = await db.Practitioners.SingleAsync(p => p.PractitionerId == target.PractitionerId);
        reloaded.FirstName.Should().Be("Renamed");
        reloaded.Email.Should().Be("renamed@example.test");
    }

    [Fact]
    public async Task SetPasswordAsync_UpdatesHashAndClearsLockout()
    {
        await using var db = NewContext();
        var target = await SeedPractitionerAsync(db, "target@example.test");
        var login = await db.Practitionerlogins.SingleAsync(l => l.PractitionerId == target.PractitionerId);
        login.FailedAttemptCount = 4;
        login.LockedUntil = DateTime.UtcNow.AddMinutes(5);
        await db.SaveChangesAsync();
        var sut = new PractitionerAdminService(db);

        var result = await sut.SetPasswordAsync(target.PractitionerId, "ResetPass123!", CancellationToken.None);

        result.Outcome.Should().Be(AdminPractitionerOutcome.Success);
        var reloaded = await db.Practitionerlogins.SingleAsync(l => l.PractitionerId == target.PractitionerId);
        reloaded.FailedAttemptCount.Should().Be(0);
        reloaded.LockedUntil.Should().BeNull();
        LoginService.VerifyPassword("target@example.test", reloaded.PasswordHash, "ResetPass123!").Should().BeTrue();
    }

    [Fact]
    public async Task ChangeOwnPasswordAsync_WithWrongCurrentPassword_IsRejected()
    {
        await using var db = NewContext();
        var admin = await SeedPractitionerAsync(db, "admin@example.test", role: "administrator");
        var sut = new PractitionerAdminService(db);

        var result = await sut.ChangeOwnPasswordAsync(
            admin.PractitionerId, "not-the-current-password", "NewAdminPass99!", CancellationToken.None);

        result.Outcome.Should().NotBe(AdminPractitionerOutcome.Success);
        var login = await db.Practitionerlogins.SingleAsync(l => l.PractitionerId == admin.PractitionerId);
        LoginService.VerifyPassword("admin@example.test", login.PasswordHash, ValidPassword).Should().BeTrue();
    }

    [Fact]
    public async Task ChangeOwnPasswordAsync_WithCorrectCurrentPassword_UpdatesHash()
    {
        await using var db = NewContext();
        var admin = await SeedPractitionerAsync(db, "admin@example.test", role: "administrator");
        var sut = new PractitionerAdminService(db);

        var result = await sut.ChangeOwnPasswordAsync(
            admin.PractitionerId, ValidPassword, "NewAdminPass99!", CancellationToken.None);

        result.Outcome.Should().Be(AdminPractitionerOutcome.Success);
        var login = await db.Practitionerlogins.SingleAsync(l => l.PractitionerId == admin.PractitionerId);
        LoginService.VerifyPassword("admin@example.test", login.PasswordHash, "NewAdminPass99!").Should().BeTrue();
    }

    [Fact]
    public async Task DeactivatePractitionerAsync_WhenTargetingSelf_ReturnsCannotTargetSelf()
    {
        await using var db = NewContext();
        var admin = await SeedPractitionerAsync(db, "admin@example.test", role: "administrator");
        var sut = new PractitionerAdminService(db);

        var result = await sut.DeactivatePractitionerAsync(admin.PractitionerId, admin.PractitionerId, CancellationToken.None);

        result.Outcome.Should().Be(AdminPractitionerOutcome.CannotTargetSelf);
    }

    [Fact]
    public async Task DeactivatePractitionerAsync_Success_DeactivatesPractitionerAndLogin()
    {
        await using var db = NewContext();
        var admin = await SeedPractitionerAsync(db, "admin@example.test", role: "administrator");
        var target = await SeedPractitionerAsync(db, "target@example.test");
        var sut = new PractitionerAdminService(db);

        var result = await sut.DeactivatePractitionerAsync(target.PractitionerId, admin.PractitionerId, CancellationToken.None);

        result.Outcome.Should().Be(AdminPractitionerOutcome.Success);
        var reloadedPractitioner = await db.Practitioners.SingleAsync(p => p.PractitionerId == target.PractitionerId);
        var reloadedLogin = await db.Practitionerlogins.SingleAsync(l => l.PractitionerId == target.PractitionerId);
        reloadedPractitioner.IsActive.Should().BeFalse();
        reloadedLogin.IsActive.Should().BeFalse();
    }
}
