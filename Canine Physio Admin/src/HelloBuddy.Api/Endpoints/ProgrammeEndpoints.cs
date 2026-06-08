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
        app.MapPost("/api/cases/{caseId:long}/programmes", async (
            long caseId,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var result = await programmes.CreateDraftAsync((ulong)caseId, practitioner.PractitionerId, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        app.MapGet("/api/programmes/{id:long}", async (
            long id,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var vm = await programmes.GetAsync((ulong)id, practitioner.PractitionerId, ct);
            return vm is null ? Results.NotFound() : Results.Ok(vm);
        });

        app.MapGet("/api/programmes/{id:long}/versions", async (
            long id,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var history = await programmes.GetVersionHistoryAsync((ulong)id, practitioner.PractitionerId, ct);
            return history is null ? Results.NotFound() : Results.Ok(history);
        });

        app.MapPost("/api/programmes/{id:long}/draft-from-published", async (
            long id,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var existing = await programmes.GetAsync((ulong)id, practitioner.PractitionerId, ct);
            if (existing is null)
            {
                return Results.NotFound();
            }

            var draft = await programmes.CreateDraftFromPublishedAsync((ulong)id, practitioner.PractitionerId, ct);
            return draft is null
                ? Results.Conflict("This programme has no published history to branch from.")
                : Results.Ok(draft);
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

            if (await programmes.IsLockedForEditAsync(key, practitioner.PractitionerId, ct))
            {
                return Results.Conflict("Published programmes are immutable. Create a new draft to make changes.");
            }

            var updated = await programmes.UpdateAsync(key, form, practitioner.PractitionerId, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        app.MapDelete("/api/programmes/{id:long}", async (
            long id,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var result = await programmes.DeleteAsync((ulong)id, practitioner.PractitionerId, ct);
            return result switch
            {
                DeleteProgrammeResult.Deleted => Results.NoContent(),
                DeleteProgrammeResult.BlockedByVersionHistory =>
                    Results.Conflict("Cannot delete this programme because it has version history."),
                _ => Results.NotFound(),
            };
        });

        app.MapPost("/api/programmes/{id:long}/activate", async (
            long id,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var result = await programmes.ActivateAsync((ulong)id, practitioner.PractitionerId, ct);
            return result switch
            {
                ProgrammeStatusTransitionResult.Updated => Results.NoContent(),
                ProgrammeStatusTransitionResult.BlockedByAnotherActiveProgramme =>
                    Results.Conflict("Another programme is already active for this case. Complete it before activating this programme."),
                ProgrammeStatusTransitionResult.InvalidTransition =>
                    Results.BadRequest("Only planned programmes can be activated."),
                _ => Results.NotFound(),
            };
        });

        app.MapPost("/api/programmes/{id:long}/complete", async (
            long id,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var result = await programmes.CompleteAsync((ulong)id, practitioner.PractitionerId, ct);
            return result switch
            {
                ProgrammeStatusTransitionResult.Updated => Results.NoContent(),
                ProgrammeStatusTransitionResult.InvalidTransition =>
                    Results.BadRequest("Only active programmes can be completed."),
                _ => Results.NotFound(),
            };
        });

        app.MapPut("/api/programmes/{id:long}/structure", async (
            long id,
            ProgrammeStructureForm form,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var key = (ulong)id;
            if (form.ProgrammeId != key) return Results.BadRequest("ProgrammeId mismatch.");

            if (await programmes.IsLockedForEditAsync(key, practitioner.PractitionerId, ct))
            {
                return Results.Conflict("Published programmes are immutable. Create a new draft to make changes.");
            }

            var result = await programmes.UpdateStructureAsync(key, form, practitioner.PractitionerId, ct);
            return result switch
            {
                ProgrammeStructureUpdateResult.Updated => Results.NoContent(),
                ProgrammeStructureUpdateResult.InvalidStructure => Results.BadRequest("SessionStructure must be 'single' or 'am-pm'."),
                ProgrammeStructureUpdateResult.InvalidName => Results.BadRequest("Programme name is required."),
                _ => Results.NotFound(),
            };
        });

        app.MapPost("/api/programmes/{id:long}/sessions/{sessionId:long}/exercises", async (
            long id,
            long sessionId,
            AddSessionExerciseRequest request,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            if (await programmes.IsLockedForEditAsync((ulong)id, practitioner.PractitionerId, ct))
            {
                return Results.Conflict("Published programmes are immutable. Create a new draft to make changes.");
            }

            var result = await programmes.AddSessionExerciseAsync(
                (ulong)id,
                (ulong)sessionId,
                request.ExerciseId,
                practitioner.PractitionerId,
                ct);

            return result switch
            {
                AddSessionExerciseResult.Added => Results.NoContent(),
                AddSessionExerciseResult.AlreadyExists => Results.Conflict("Exercise already exists in this session."),
                _ => Results.NotFound(),
            };
        });

        app.MapDelete("/api/programmes/{id:long}/sessions/{sessionId:long}/exercises/{sessionExerciseId:long}", async (
            long id,
            long sessionId,
            long sessionExerciseId,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            if (await programmes.IsLockedForEditAsync((ulong)id, practitioner.PractitionerId, ct))
            {
                return Results.Conflict("Published programmes are immutable. Create a new draft to make changes.");
            }

            var result = await programmes.RemoveSessionExerciseAsync(
                (ulong)id,
                (ulong)sessionId,
                (ulong)sessionExerciseId,
                practitioner.PractitionerId,
                ct);
            return result == RemoveSessionExerciseResult.Removed ? Results.NoContent() : Results.NotFound();
        });

        app.MapPost("/api/programmes/{id:long}/publish", async (
            long id,
            IProgrammeService programmes,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var validationErrors = await programmes.ValidateDraftForPublishAsync((ulong)id, practitioner.PractitionerId, ct);
            if (validationErrors.Count > 0)
            {
                return Results.ValidationProblem(validationErrors);
            }

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
