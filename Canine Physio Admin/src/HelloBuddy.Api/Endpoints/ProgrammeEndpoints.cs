using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Identity;
using HelloBuddy.Admin.Pdf;
using HelloBuddy.Api.Services;
using HelloBuddy.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace HelloBuddy.Api.Endpoints;

public static class ProgrammeEndpoints
{
    public static IEndpointRouteBuilder MapProgrammeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/programmes/{id:long}", async (
            long id,
            CaninePhysioDbContext db,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var vm = await LoadProgrammeVmAsync(db, practitioner, (ulong)id, ct);
            return vm is null ? Results.NotFound() : Results.Ok(vm);
        });

        app.MapPut("/api/programmes/{id:long}", async (
            long id,
            ProgrammeBuilderForm form,
            CaninePhysioDbContext db,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var key = (ulong)id;
            if (form.ProgrammeId != key) return Results.BadRequest("ProgrammeId mismatch.");

            var ownsProgramme = await db.Programmes
                .AnyAsync(p => p.ProgrammeId == key && p.TreatmentCase.PractitionerId == practitioner.PractitionerId, ct);
            if (!ownsProgramme) return Results.NotFound();

            var ids = form.Exercises.Select(e => e.SessionExerciseId).ToList();
            var entities = await db.Sessionexercises
                .Where(se => ids.Contains(se.SessionExerciseId) && se.Session.ProgrammeId == key)
                .ToListAsync(ct);

            foreach (var edit in form.Exercises)
            {
                var entity = entities.FirstOrDefault(e => e.SessionExerciseId == edit.SessionExerciseId);
                if (entity is null) continue;
                entity.Reps = edit.Reps;
                entity.Sets = edit.Sets;
                entity.HoldSeconds = edit.HoldSeconds;
                entity.SortOrder = edit.SortOrder;
                entity.Notes = edit.Notes;
            }

            await db.SaveChangesAsync(ct);

            var updated = await LoadProgrammeVmAsync(db, practitioner, key, ct);
            return Results.Ok(updated);
        });

        app.MapPost("/api/programmes/{id:long}/publish", async (
            long id,
            CaninePhysioDbContext db,
            ICurrentPractitionerAccessor practitioner,
            IPdfRenderer pdfRenderer,
            IFileStore fileStore,
            ILogger<Program> logger,
            CancellationToken ct) =>
        {
            var key = (ulong)id;
            var vm = await LoadProgrammeVmAsync(db, practitioner, key, ct);
            if (vm is null) return Results.NotFound();

            var html = ProgrammePdfTemplate.Render(vm);
            var pdf = await pdfRenderer.RenderAsync(html, ct);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var fileName = $"programme-{key}-{timestamp}.pdf";
            var uri = await fileStore.WriteAsync(fileName, pdf, ct);
            logger.LogInformation("Published programme {ProgrammeId} as {FileName} ({Bytes} bytes)", key, fileName, pdf.Length);

            return Results.Ok(new PublishResponse(uri.ToString(), fileName, pdf.Length));
        });

        app.MapGet("/api/programmes/published/{fileName}/download-url", async (
            string fileName,
            IFileStore fileStore,
            CancellationToken ct) =>
        {
            // Guard: only the deterministic naming pattern produced by the
            // publish handler is allowed through; anything else is a probe.
            if (!Regex.IsMatch(fileName, @"^programme-\d+-\d{8}-\d{6}\.pdf$"))
                return Results.BadRequest();
            var url = await fileStore.GetReadUrlAsync(fileName, TimeSpan.FromMinutes(30), ct);
            return Results.Ok(new DownloadUrlResponse(url.ToString()));
        });

        return app;
    }

    private static async Task<ProgrammeVm?> LoadProgrammeVmAsync(
        CaninePhysioDbContext db,
        ICurrentPractitionerAccessor practitioner,
        ulong id,
        CancellationToken ct)
    {
        var data = await db.Programmes
            .Where(p => p.ProgrammeId == id && p.TreatmentCase.PractitionerId == practitioner.PractitionerId)
            .Select(p => new
            {
                p.ProgrammeId,
                p.TreatmentCaseId,
                p.ProgrammeName,
                p.Status,
                p.StartDate,
                p.EndDate,
                p.Notes,
                CaseTitle = p.TreatmentCase.CaseTitle,
                PetName = p.TreatmentCase.Pet.Name,
                OwnerFirst = p.TreatmentCase.Pet.Owner.FirstName,
                OwnerLast = p.TreatmentCase.Pet.Owner.LastName,
                Sessions = p.Sessions
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new
                    {
                        s.SessionId,
                        s.Period,
                        s.Objective,
                        s.Status,
                        s.SortOrder,
                        Exercises = s.Sessionexercises
                            .OrderBy(se => se.SortOrder)
                            .Select(se => new ProgrammeVm.SessionExerciseRow(
                                se.SessionExerciseId,
                                se.ExerciseId,
                                se.Exercise.Title,
                                se.Exercise.ObjectiveSummary,
                                se.Reps,
                                se.Sets,
                                se.HoldSeconds,
                                se.SortOrder,
                                se.Notes))
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (data is null) return null;

        var sessions = data.Sessions
            .Select(s => new ProgrammeVm.SessionRow(s.SessionId, s.Period, s.Objective, s.Status, s.SortOrder, s.Exercises))
            .ToList();

        return new ProgrammeVm(
            data.ProgrammeId,
            data.TreatmentCaseId,
            data.ProgrammeName,
            data.Status,
            data.StartDate,
            data.EndDate,
            data.Notes,
            data.CaseTitle,
            data.PetName,
            $"{data.OwnerFirst} {data.OwnerLast}",
            sessions);
    }
}
