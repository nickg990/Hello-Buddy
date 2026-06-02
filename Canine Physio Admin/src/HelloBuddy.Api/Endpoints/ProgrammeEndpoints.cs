using HelloBuddy.Admin.Core.Identity;
using HelloBuddy.Admin.Pdf;
using HelloBuddy.Application.Programmes;
using HelloBuddy.Contracts;
using System.Text.RegularExpressions;

namespace HelloBuddy.Api.Endpoints;

public static class ProgrammeEndpoints
{
    private static readonly Regex PublishedFileNamePattern =
        new(@"^programme-\d+-\d{8}-\d{6}\.pdf$", RegexOptions.Compiled);

    public static IEndpointRouteBuilder MapProgrammeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/programmes/{id:long}", async (
            long id,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var vm = await programmes.GetAsync((ulong)id, practitioner.PractitionerId, ct);
            return vm is null ? Results.NotFound() : Results.Ok(vm);
        });

        app.MapPut("/api/programmes/{id:long}", async (
            long id,
            ProgrammeBuilderForm form,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var key = (ulong)id;
            if (form.ProgrammeId != key) return Results.BadRequest("ProgrammeId mismatch.");

            var updated = await programmes.UpdateAsync(key, form, practitioner.PractitionerId, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        app.MapPost("/api/programmes/{id:long}/publish", async (
            long id,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var result = await programmes.PublishAsync((ulong)id, practitioner.PractitionerId, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        app.MapGet("/api/programmes/published/{fileName}/download-url", async (
            string fileName,
            IFileStore fileStore,
            CancellationToken ct) =>
        {
            // Only the deterministic naming pattern produced by PublishAsync is accepted.
            if (!PublishedFileNamePattern.IsMatch(fileName))
                return Results.BadRequest();
            var url = await fileStore.GetReadUrlAsync(fileName, TimeSpan.FromMinutes(30), ct);
            return Results.Ok(new DownloadUrlResponse(url.ToString()));
        });

        return app;
    }
}
