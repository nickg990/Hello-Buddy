using FluentValidation;
using HelloBuddy.Application.Records;
using HelloBuddy.Contracts;

namespace HelloBuddy.Api.Endpoints;

public static class OwnerEndpoints
{
    public static IEndpointRouteBuilder MapOwnerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/owners", async (IOwnerRepository owners, CancellationToken ct) =>
        {
            var rows = await owners.ListAsync(ct);
            return Results.Ok(rows);
        });

        app.MapGet("/api/owners/{id:long}", async (long id, IOwnerRepository owners, CancellationToken ct) =>
        {
            var owner = await owners.GetAsync((ulong)id, ct);
            return owner is null ? Results.NotFound() : Results.Ok(owner);
        });

        app.MapPost("/api/owners", async (
            SaveOwnerRequest request,
            IOwnerRepository owners,
            IValidator<SaveOwnerRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            if (await owners.EmailInUseAsync(request.Email, excludedOwnerId: null, ct))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.Email)] = ["Email address is already in use."]
                });
            }

            var ownerId = await owners.CreateAsync(request, ct);
            var owner = await owners.GetAsync(ownerId, ct);
            return Results.Created($"/api/owners/{ownerId}", owner);
        });

        app.MapPut("/api/owners/{id:long}", async (
            long id,
            SaveOwnerRequest request,
            IOwnerRepository owners,
            IValidator<SaveOwnerRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            if (await owners.EmailInUseAsync(request.Email, (ulong)id, ct))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.Email)] = ["Email address is already in use."]
                });
            }

            var updated = await owners.UpdateAsync((ulong)id, request, ct);
            if (!updated)
            {
                return Results.NotFound();
            }

            var owner = await owners.GetAsync((ulong)id, ct);
            return Results.Ok(owner);
        });

        return app;
    }
}