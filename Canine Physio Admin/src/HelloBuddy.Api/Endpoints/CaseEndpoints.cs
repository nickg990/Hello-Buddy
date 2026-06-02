using HelloBuddy.Admin.Core.Identity;
using HelloBuddy.Application.Records;
using HelloBuddy.Contracts;
using FluentValidation;

namespace HelloBuddy.Api.Endpoints;

public static class CaseEndpoints
{
    public static IEndpointRouteBuilder MapCaseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cases", async (
            ITreatmentCaseRepository cases,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var rows = await cases.ListAsync(practitioner.PractitionerId, ct);
            return Results.Ok(rows);
        });

        app.MapGet("/api/cases/{id:long}", async (
            long id,
            ITreatmentCaseRepository cases,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var vm = await cases.GetAsync((ulong)id, practitioner.PractitionerId, ct);
            return vm is null ? Results.NotFound() : Results.Ok(vm);
        });

        app.MapPost("/api/cases", async (
            SaveTreatmentCaseRequest request,
            ITreatmentCaseRepository cases,
            IPetRepository pets,
            ICurrentPractitionerAccessor practitioner,
            IValidator<SaveTreatmentCaseRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            if (!await pets.ExistsAsync(request.PetId, ct))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.PetId)] = ["Selected pet was not found."]
                });
            }

            var treatmentCaseId = await cases.CreateAsync(request, practitioner.PractitionerId, ct);
            var vm = await cases.GetAsync(treatmentCaseId, practitioner.PractitionerId, ct);
            return Results.Created($"/api/cases/{treatmentCaseId}", vm);
        });

        app.MapPut("/api/cases/{id:long}", async (
            long id,
            SaveTreatmentCaseRequest request,
            ITreatmentCaseRepository cases,
            IPetRepository pets,
            ICurrentPractitionerAccessor practitioner,
            IValidator<SaveTreatmentCaseRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            if (!await pets.ExistsAsync(request.PetId, ct))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.PetId)] = ["Selected pet was not found."]
                });
            }

            var updated = await cases.UpdateAsync((ulong)id, request, practitioner.PractitionerId, ct);
            if (!updated)
            {
                return Results.NotFound();
            }

            var vm = await cases.GetAsync((ulong)id, practitioner.PractitionerId, ct);
            return Results.Ok(vm);
        });

        app.MapPost("/api/cases/{id:long}/notes", async (
            long id,
            CreateCaseNoteRequest request,
            ITreatmentCaseRepository cases,
            ICurrentPractitionerAccessor practitioner,
            IValidator<CreateCaseNoteRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var note = await cases.AddNoteAsync((ulong)id, request, practitioner.PractitionerId, ct);
            return note is null ? Results.NotFound() : Results.Ok(note);
        });

        return app;
    }
}
