using Azure.Identity;
using Azure.Storage.Blobs;
using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Identity;
using HelloBuddy.Admin.Pdf;
using HelloBuddy.Api.Endpoints;
using HelloBuddy.Api.Services;
using Microsoft.EntityFrameworkCore;

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

var connectionString = builder.Configuration.GetConnectionString("CaninePhysioDb")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:CaninePhysioDb is not configured.");

builder.Services.AddDbContext<CaninePhysioDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Practitioner identity is supplied per-request by the UI via the
// X-Practitioner-Id header (Release 1 service-to-service identity, TD-005).
builder.Services.AddScoped<ICurrentPractitionerAccessor, HeaderPractitionerAccessor>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddApplicationInsightsTelemetry();

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
// IFileStore: Azure Blob in cloud, local filesystem in Development.
// -----------------------------------------------------------------
var blobServiceUri = builder.Configuration["Storage:BlobServiceUri"];
var publishedContainer = builder.Configuration["Storage:PublishedProgrammesContainer"] ?? "published-programmes";
if (!string.IsNullOrWhiteSpace(blobServiceUri))
{
    builder.Services.AddSingleton(_ => new BlobServiceClient(new Uri(blobServiceUri), new DefaultAzureCredential()));
    builder.Services.AddSingleton(sp =>
    {
        var service = sp.GetRequiredService<BlobServiceClient>();
        return service.GetBlobContainerClient(publishedContainer);
    });
    builder.Services.AddSingleton<IFileStore, AzureBlobFileStore>();
}
else
{
    builder.Services.AddSingleton<IFileStore>(sp =>
    {
        var env = sp.GetRequiredService<IWebHostEnvironment>();
        var logger = sp.GetRequiredService<ILogger<LocalFileStore>>();
        var root = Path.Combine(env.ContentRootPath, "published-programmes");
        return new LocalFileStore(root, logger);
    });
}

var app = builder.Build();

// -----------------------------------------------------------------
// X-Practitioner-Id gate: reject API calls without the header.
// Health probes are exempt.
// -----------------------------------------------------------------
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/api"))
    {
        if (!ctx.Request.Headers.TryGetValue(PractitionerHeader, out var raw) ||
            !ulong.TryParse(raw.ToString(), out _))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsync($"Missing or invalid {PractitionerHeader} header.");
            return;
        }
    }
    await next();
});

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.MapCaseEndpoints();
app.MapProgrammeEndpoints();

// Dev-only: stream files from the local published-programmes folder so the
// UI's "Download PDF" link works end-to-end in Development. In Azure the
// equivalent path is a user-delegation SAS URL minted by AzureBlobFileStore.
if (app.Environment.IsDevelopment())
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
