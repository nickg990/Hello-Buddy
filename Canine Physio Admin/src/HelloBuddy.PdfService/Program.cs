using HelloBuddy.Admin.Pdf;
using HelloBuddy.Contracts;
using HelloBuddy.PdfService.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITelemetryInitializer>(new CloudRoleNameInitializer("hello-buddy-pdf"));
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<IPdfRenderer, PuppeteerPdfRenderer>();

var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.MapPost("/render", async (RenderRequest req, IPdfRenderer renderer, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.Html))
        return Results.BadRequest(new { error = "html is required" });

    var bytes = await renderer.RenderAsync(req.Html, ct);
    return Results.File(bytes, "application/pdf");
});

app.Run();
