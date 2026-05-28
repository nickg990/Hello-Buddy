using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Identity;
using HelloBuddy.Admin.Pdf;
using HelloBuddy.Admin.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CaninePhysioDb")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:CaninePhysioDb is not configured.");

builder.Services.AddDbContext<CaninePhysioDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<ICurrentPractitionerAccessor, SeededPractitionerAccessor>();

// Application Insights — connection string read from configuration. Empty in
// Development is a no-op; populated from Key Vault in the Azure deployment.
builder.Services.AddApplicationInsightsTelemetry();

// PDF + view-to-string rendering for the publish flow.
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IPdfRenderer, PuppeteerPdfRenderer>();
builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
