using Azure.Identity;
using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Identity;
using HelloBuddy.Application.Auth;
using HelloBuddy.Ui.Models;
using HelloBuddy.Ui.Services;
using HelloBuddy.Ui.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

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

// Cookie authentication: signed-in users have claims for id/name/role.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("practitioner_role", "administrator"));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<PractitionerHeaderHandler>();
builder.Services.AddScoped<ICurrentPractitionerAccessor, UiPractitionerAccessor>();
builder.Services.AddHttpClient<IAdminApiClient, AdminApiClient>(client =>
{
    client.BaseAddress = new Uri(apiUri);
    client.Timeout = TimeSpan.FromMinutes(2);
})
.AddHttpMessageHandler<PractitionerHeaderHandler>();

builder.Services.Configure<MediaSearchOptions>(builder.Configuration.GetSection("MediaSearch"));

builder.Services.AddControllersWithViews();

// UI auth services currently use DB-backed implementations from Infrastructure.
// Keep opt-in to avoid forcing DbContext wiring in UI test host/bootstrap.
var useDbBackedUiAuth = builder.Configuration.GetValue<bool>("Auth:UseDbBackedServices");
if (useDbBackedUiAuth)
{
    var connectionString = builder.Configuration.GetConnectionString("CaninePhysioDb")
        ?? throw new InvalidOperationException(
            "Auth:UseDbBackedServices is true, but ConnectionStrings:CaninePhysioDb is not configured for the UI host.");

    builder.Services.AddDbContext<CaninePhysioDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

    builder.Services.AddUiAuthServices();
}
else
{
    builder.Services.AddScoped<ILoginService, NoOpLoginService>();
    builder.Services.AddScoped<IPractitionerAdminService, NoOpPractitionerAdminService>();
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Static assets must never require authentication — MapStaticAssets() creates endpoint-based
// routes that inherit the FallbackPolicy (RequireAuthenticatedUser). AllowAnonymous() exempts
// CSS, JS, images and other static files so unauthenticated pages (login, error) render correctly.
app.MapStaticAssets().AllowAnonymous();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program;
