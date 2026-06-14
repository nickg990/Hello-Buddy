using Azure.Identity;
using Azure.Storage.Blobs;
using HelloBuddy.Application.Records;
using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Identity;
using HelloBuddy.Admin.Pdf;
using HelloBuddy.Api.Endpoints;
using HelloBuddy.Api.Security;
using HelloBuddy.Api.Services;
using HelloBuddy.Api.Telemetry;
using HelloBuddy.Infrastructure.Auth;
using HelloBuddy.Infrastructure.Records;
using Microsoft.AspNetCore.Authentication;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

const string PractitionerHeader = "X-Practitioner-Id";

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------
// Configuration: layer Azure Key Vault on top of defaults when present.
// In Azure the Container App's managed identity is used.
// -----------------------------------------------------------------
var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
}

var allowedPractitionerIds = new HashSet<ulong>(
    builder.Configuration
        .GetSection("Security:AllowedPractitionerIds")
        .Get<ulong[]>() ?? Array.Empty<ulong>());

var connectionString = builder.Configuration.GetConnectionString("CaninePhysioDb")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:CaninePhysioDb is not configured.");

builder.Services.AddSingleton<AuditSaveChangesInterceptor>();
builder.Services.AddDbContext<CaninePhysioDbContext>((sp, options) =>
    options
        .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));
builder.Services.AddProblemDetails();
builder.Services.AddHelloBuddyApplication();
builder.Services.AddHelloBuddyInfrastructure();
builder.Services.AddSingleton<IProgrammePdfTemplate, RazorProgrammePdfTemplate>();

builder.Services
    .AddAuthentication(ApiAuthenticationSchemes.PractitionerHeader)
    .AddScheme<AuthenticationSchemeOptions, HeaderPractitionerAuthenticationHandler>(
        ApiAuthenticationSchemes.PractitionerHeader,
        static _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(ApiAuthorizationPolicies.AdminOnly, policy =>
    {
        policy.AddAuthenticationSchemes(ApiAuthenticationSchemes.PractitionerHeader);
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("practitioner_role", "administrator");
    });
});

// Practitioner identity is supplied per-request by the UI via the
// X-Practitioner-Id header (Release 1 service-to-service identity, TD-005).
builder.Services.AddScoped<ICurrentPractitionerAccessor, HeaderPractitionerAccessor>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<ITelemetryInitializer>(new CloudRoleNameInitializer("hello-buddy-api"));
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHostedService<ExerciseLibrarySeedHostedService>();
builder.Services.AddHostedService<PractitionerLoginSeedHostedService>();

// -----------------------------------------------------------------
// PDF rendering: remote PDF service via typed HttpClient.
// -----------------------------------------------------------------
var pdfServiceUri = builder.Configuration["PdfService:Uri"]
    ?? throw new InvalidOperationException("PdfService:Uri is not configured.");
builder.Services.AddHttpClient<IPdfRenderer, HttpPdfRenderer>(client =>
{
    client.BaseAddress = new Uri(pdfServiceUri);
    client.Timeout = TimeSpan.FromMinutes(2);
});

// -----------------------------------------------------------------
// IFileStore: Azurite-first in Development, Azure Blob in cloud,
// and explicit local filesystem fallback for emergency unblock.
// -----------------------------------------------------------------
var blobServiceUri = builder.Configuration["Storage:BlobServiceUri"];
var storageMode = ResolveStorageMode(builder.Configuration["Storage:Mode"], builder.Environment, blobServiceUri);
var storageConnectionString = builder.Configuration["Storage:ConnectionString"];
var publishedContainer = builder.Configuration["Storage:PublishedProgrammesContainer"] ?? "published-programmes";

ValidateExerciseMediaPolicy(builder.Configuration, storageMode);

switch (storageMode)
{
    case "Azurite":
        var azuriteConnectionString = storageConnectionString ?? "UseDevelopmentStorage=true";
        builder.Services.AddSingleton(_ => new BlobServiceClient(azuriteConnectionString));
        builder.Services.AddSingleton(sp =>
        {
            var service = sp.GetRequiredService<BlobServiceClient>();
            return service.GetBlobContainerClient(publishedContainer);
        });
        builder.Services.AddSingleton<IFileStore, AzureBlobFileStore>();
        break;

    case "Azure":
        if (string.IsNullOrWhiteSpace(blobServiceUri))
        {
            throw new InvalidOperationException(
                "Storage:BlobServiceUri must be configured when Storage:Mode is Azure.");
        }

        builder.Services.AddSingleton(_ => new BlobServiceClient(new Uri(blobServiceUri), new DefaultAzureCredential()));
        builder.Services.AddSingleton(sp =>
        {
            var service = sp.GetRequiredService<BlobServiceClient>();
            return service.GetBlobContainerClient(publishedContainer);
        });
        builder.Services.AddSingleton<IFileStore, AzureBlobFileStore>();
        break;

    case "FileSystem":
        builder.Services.AddSingleton<IFileStore>(sp =>
        {
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var logger = sp.GetRequiredService<ILogger<LocalFileStore>>();
            var root = Path.Combine(env.ContentRootPath, "published-programmes");
            return new LocalFileStore(root, logger);
        });
        break;

    default:
        throw new InvalidOperationException($"Unsupported storage mode '{storageMode}'.");
}

builder.Services.AddScoped<IExerciseMediaGovernanceService, ExerciseMediaGovernanceService>();

var scanMode = (builder.Configuration["Storage:ExerciseMediaMalwareScanMode"] ?? "StubAllowAll").Trim();
if (scanMode.Equals("Disabled", StringComparison.OrdinalIgnoreCase)
    || scanMode.Equals("StubAllowAll", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IExerciseMediaMalwareScanner, StubAllowAllExerciseMediaMalwareScanner>();
}

var app = builder.Build();

app.Logger.LogInformation("Storage mode: {StorageMode} (container: {Container})", storageMode, publishedContainer);

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

// -----------------------------------------------------------------
// X-Practitioner-Id gate: reject API calls without the header.
// Health probes are exempt.
// -----------------------------------------------------------------
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/api"))
    {
        if (!ctx.Request.Headers.TryGetValue(PractitionerHeader, out var raw) ||
            !ulong.TryParse(raw.ToString(), out var practitionerId))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsync($"Missing or invalid {PractitionerHeader} header.");
            return;
        }

        if (allowedPractitionerIds.Count > 0 && !allowedPractitionerIds.Contains(practitionerId))
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Response.WriteAsync($"{PractitionerHeader} is not allowed.");
            return;
        }
    }
    await next();
});

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.MapOwnerEndpoints();
app.MapPetEndpoints();
app.MapCaseEndpoints();
app.MapExerciseEndpoints();
app.MapProgrammeEndpoints();
app.MapAdminEndpoints();

// Non-production helper route: stream files from the local
// published-programmes folder when FileSystem mode is active.
// In Azure/Azurite blob modes, download URLs come from AzureBlobFileStore.
if (!app.Environment.IsProduction())
{
    app.MapGet("/dev-published/{fileName}", (string fileName, IFileStore fileStore) =>
    {
        if (fileStore is not LocalFileStore local) return Results.NotFound();
        if (!System.Text.RegularExpressions.Regex.IsMatch(fileName, @"^programme-\d+-\d{8}-\d{6}\.pdf$"))
            return Results.BadRequest();
        var stream = local.OpenReadOrNull(fileName);
        return stream is null ? Results.NotFound() : Results.File(stream, "application/pdf", fileName);
    });
}

app.Run();

static string ResolveStorageMode(string? configuredMode, IHostEnvironment environment, string? blobServiceUri)
{
    if (!string.IsNullOrWhiteSpace(configuredMode))
    {
        var normalized = configuredMode.Trim();
        if (!normalized.Equals("Azurite", StringComparison.OrdinalIgnoreCase)
            && !normalized.Equals("Azure", StringComparison.OrdinalIgnoreCase)
            && !normalized.Equals("FileSystem", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Storage:Mode must be one of: Azurite, Azure, FileSystem.");
        }

        if (normalized.Equals("Azurite", StringComparison.OrdinalIgnoreCase)) return "Azurite";
        if (normalized.Equals("Azure", StringComparison.OrdinalIgnoreCase)) return "Azure";
        return "FileSystem";
    }

    if (environment.IsDevelopment())
    {
        return "Azurite";
    }

    return string.IsNullOrWhiteSpace(blobServiceUri) ? "FileSystem" : "Azure";
}

static void ValidateExerciseMediaPolicy(IConfiguration configuration, string storageMode)
{
    var maxBytes = configuration.GetValue<long?>("Storage:ExerciseMediaMaxBytes") ?? 0;
    if (maxBytes <= 0)
    {
        throw new InvalidOperationException("Storage:ExerciseMediaMaxBytes must be configured and greater than zero.");
    }

    var orphanMode = (configuration["Storage:ExerciseMediaOrphanCleanupMode"] ?? string.Empty).Trim();
    if (!orphanMode.Equals("Retain", StringComparison.OrdinalIgnoreCase)
        && !orphanMode.Equals("DeleteManagedOrphans", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "Storage:ExerciseMediaOrphanCleanupMode must be either 'Retain' or 'DeleteManagedOrphans'.");
    }

    var scanMode = (configuration["Storage:ExerciseMediaMalwareScanMode"] ?? string.Empty).Trim();
    if (!scanMode.Equals("Disabled", StringComparison.OrdinalIgnoreCase)
        && !scanMode.Equals("StubAllowAll", StringComparison.OrdinalIgnoreCase)
        && !scanMode.Equals("Required", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "Storage:ExerciseMediaMalwareScanMode must be one of: Disabled, StubAllowAll, Required.");
    }

    if (scanMode.Equals("Required", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "Storage:ExerciseMediaMalwareScanMode=Required is not supported until a real scanner integration is configured.");
    }

    if (!storageMode.Equals("FileSystem", StringComparison.OrdinalIgnoreCase))
    {
        var baseUrl = (configuration["Storage:ExerciseMediaBaseUrl"] ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException(
                "Storage:ExerciseMediaBaseUrl must be an absolute URL when Storage:Mode is Azurite or Azure.");
        }
    }
}

public partial class Program;
