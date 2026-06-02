using FluentValidation;
using HelloBuddy.Admin.Core.Identity;
using HelloBuddy.Application.Records;
using HelloBuddy.Contracts;

namespace HelloBuddy.Api.Endpoints;

public static class PetEndpoints
{
    public static IEndpointRouteBuilder MapPetEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/pets", async (IPetRepository pets, CancellationToken ct) =>
        {
            var rows = await pets.ListAsync(ct);
            return Results.Ok(rows);
        });

        app.MapGet("/api/pets/{id:long}", async (
            long id,
            IPetRepository pets,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var pet = await pets.GetAsync((ulong)id, practitioner.PractitionerId, ct);
            return pet is null ? Results.NotFound() : Results.Ok(pet);
        });

        app.MapPost("/api/pets", async (
            SavePetRequest request,
            IPetRepository pets,
            ICurrentPractitionerAccessor practitioner,
            IValidator<SavePetRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            if (!await pets.OwnerExistsAsync(request.OwnerId, ct))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.OwnerId)] = ["Selected owner was not found."]
                });
            }

            var petId = await pets.CreateAsync(request, practitioner.PractitionerId, ct);
            var pet = await pets.GetAsync(petId, practitioner.PractitionerId, ct);
            return Results.Created($"/api/pets/{petId}", pet);
        });

        app.MapPut("/api/pets/{id:long}", async (
            long id,
            SavePetRequest request,
            IPetRepository pets,
            ICurrentPractitionerAccessor practitioner,
            IValidator<SavePetRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            if (!await pets.OwnerExistsAsync(request.OwnerId, ct))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.OwnerId)] = ["Selected owner was not found."]
                });
            }

            var updated = await pets.UpdateAsync((ulong)id, request, practitioner.PractitionerId, ct);
            if (!updated)
            {
                return Results.NotFound();
            }

            var pet = await pets.GetAsync((ulong)id, practitioner.PractitionerId, ct);
            return Results.Ok(pet);
        });

        return app;
    }
}