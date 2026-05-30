using Azure.Identity;
using HelloBuddy.Ui.Services;
using HelloBuddy.Ui.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
}

builder.Services.AddSingleton<ITelemetryInitializer>(new CloudRoleNameInitializer("hello-buddy-ui"));
builder.Services.AddApplicationInsightsTelemetry();

// -----------------------------------------------------------------
// DataProtection keys persisted to Blob Storage so antiforgery /
// cookie auth survives revision swaps and multi-replica scale-out.
// Falls back to ephemeral on-disk keys when DataProtection:BlobUri
// is unset (local dev).
// -----------------------------------------------------------------
var dpBlobUri = builder.Configuration["DataProtection:BlobUri"];
if (!string.IsNullOrWhiteSpace(dpBlobUri))
{
    builder.Services.AddDataProtection()
        .PersistKeysToAzureBlobStorage(new Uri(dpBlobUri), new DefaultAzureCredential())
        .SetApplicationName("HelloBuddy.Ui");
}

// -----------------------------------------------------------------
// Typed API client + practitioner-header delegating handler.
// -----------------------------------------------------------------
var apiUri = builder.Configuration["Api:Uri"]
    ?? throw new InvalidOperationException("Api:Uri is not configured.");

builder.Services.AddTransient<PractitionerHeaderHandler>();
builder.Services.AddHttpClient<IAdminApiClient, AdminApiClient>(client =>
{
    client.BaseAddress = new Uri(apiUri);
    client.Timeout = TimeSpan.FromMinutes(2);
})
.AddHttpMessageHandler<PractitionerHeaderHandler>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
