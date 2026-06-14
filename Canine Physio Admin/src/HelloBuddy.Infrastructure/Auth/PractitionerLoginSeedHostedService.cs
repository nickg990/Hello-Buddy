using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HelloBuddy.Infrastructure.Auth;

/// <summary>
/// Seeds initial PractitionerLogin rows at API startup if none exist.
/// Creates login records for the two seeded physiotherapists (Amelia Carter,
/// James Holloway) and an initial administrator account.
/// Seed credentials are read from configuration when seeding is enabled.
/// This service is idempotent and safe to run repeatedly.
/// </summary>
public sealed class PractitionerLoginSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PractitionerLoginSeedHostedService> _logger;

    public PractitionerLoginSeedHostedService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<PractitionerLoginSeedHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var enabled = _configuration.GetValue<bool>("Seed:PractitionerLogin:Enabled");
        if (!enabled)
        {
            _logger.LogInformation("Practitioner login seeding is disabled.");
            return;
        }

        var initialPassword = _configuration["Seed:PractitionerLogin:InitialPassword"];
        if (string.IsNullOrWhiteSpace(initialPassword))
        {
            throw new InvalidOperationException(
                "Seed:PractitionerLogin:InitialPassword must be configured when Seed:PractitionerLogin:Enabled is true.");
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CaninePhysioDbContext>();

        await SeedLoginRowsAsync(db, initialPassword, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedLoginRowsAsync(CaninePhysioDbContext db, string initialPassword, CancellationToken ct)
    {
        // Seed logins for existing physiotherapists (Amelia Carter id=1, James Holloway id=2)
        // Skip if login already exists to avoid overwriting manual password changes.
        var existingIds = await db.Practitionerlogins
            .Select(l => l.PractitionerId)
            .ToListAsync(ct);

        var practitioners = await db.Practitioners
            .Where(p => !existingIds.Contains(p.PractitionerId))
            .ToListAsync(ct);

        foreach (var p in practitioners)
        {
            db.Practitionerlogins.Add(new Practitionerlogin
            {
                PractitionerId = p.PractitionerId,
                PasswordHash = LoginService.HashPassword(p.Email, initialPassword),
                Role = "physiotherapist",
                IsActive = true,
                MustChangePassword = false,
            });
            _logger.LogInformation("Seeded PractitionerLogin for practitioner {PractitionerId} (physiotherapist).", p.PractitionerId);
        }

        // Ensure an administrator account exists
        var adminExists = await db.Practitionerlogins
            .AnyAsync(l => l.Role == "administrator", ct);

        if (!adminExists)
        {
            const string adminEmail = "admin@caninephysio.local";
            var adminPractitioner = await db.Practitioners
                .FirstOrDefaultAsync(p => p.Email == adminEmail, ct);

            if (adminPractitioner is null)
            {
                adminPractitioner = new Practitioner
                {
                    FirstName = "System",
                    LastName = "Admin",
                    Email = adminEmail,
                    IsActive = true,
                };
                db.Practitioners.Add(adminPractitioner);
                await db.SaveChangesAsync(ct);
            }

            var existingAdminLogin = await db.Practitionerlogins
                .FirstOrDefaultAsync(l => l.PractitionerId == adminPractitioner.PractitionerId, ct);

            if (existingAdminLogin is null)
            {
                db.Practitionerlogins.Add(new Practitionerlogin
                {
                    PractitionerId = adminPractitioner.PractitionerId,
                    PasswordHash = LoginService.HashPassword(adminEmail, initialPassword),
                    Role = "administrator",
                    IsActive = true,
                    MustChangePassword = false,
                });
                _logger.LogInformation(
                    "Seeded administrator account for practitioner {PractitionerId}. MustChangePassword=false.",
                    adminPractitioner.PractitionerId);
            }
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(ct);
        }
    }
}
