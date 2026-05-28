using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Identity;
using HelloBuddy.Contracts;
using Microsoft.EntityFrameworkCore;

namespace HelloBuddy.Api.Endpoints;

public static class CaseEndpoints
{
    public static IEndpointRouteBuilder MapCaseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cases", async (
            CaninePhysioDbContext db,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var rows = await db.Treatmentcases
                .Where(tc => tc.PractitionerId == practitioner.PractitionerId)
                .OrderByDescending(tc => tc.StartDate)
                .Select(tc => new CaseRow(
                    tc.TreatmentCaseId,
                    tc.CaseTitle,
                    tc.Status,
                    tc.StartDate,
                    tc.Pet.Name,
                    tc.Pet.Owner.FirstName + " " + tc.Pet.Owner.LastName))
                .ToListAsync(ct);
            return Results.Ok(rows);
        });

        app.MapGet("/api/cases/{id:long}", async (
            long id,
            CaninePhysioDbContext db,
            ICurrentPractitionerAccessor practitioner,
            CancellationToken ct) =>
        {
            var key = (ulong)id;
            var tc = await db.Treatmentcases
                .Where(x => x.TreatmentCaseId == key && x.PractitionerId == practitioner.PractitionerId)
                .Select(x => new
                {
                    x.TreatmentCaseId,
                    x.CaseTitle,
                    x.Status,
                    x.StartDate,
                    x.EndDate,
                    x.ClinicalSummary,
                    PetName = x.Pet.Name,
                    x.Pet.Breed,
                    x.Pet.Sex,
                    x.Pet.Weight,
                    x.Pet.Age,
                    OwnerFirst = x.Pet.Owner.FirstName,
                    OwnerLast = x.Pet.Owner.LastName,
                    OwnerEmail = x.Pet.Owner.Email,
                    Programmes = x.Programmes
                        .OrderByDescending(p => p.StartDate)
                        .Select(p => new CaseDetailVm.ProgrammeRow(
                            p.ProgrammeId,
                            p.ProgrammeName,
                            p.Status,
                            p.StartDate,
                            p.EndDate,
                            p.Sessions.Count,
                            p.Sessions.SelectMany(s => s.Sessionexercises).Count()))
                        .ToList()
                })
                .FirstOrDefaultAsync(ct);

            if (tc is null) return Results.NotFound();

            var vm = new CaseDetailVm(
                tc.TreatmentCaseId,
                tc.CaseTitle,
                tc.Status,
                tc.StartDate,
                tc.EndDate,
                tc.ClinicalSummary,
                tc.PetName,
                tc.Breed,
                tc.Sex,
                tc.Weight,
                tc.Age,
                $"{tc.OwnerFirst} {tc.OwnerLast}",
                tc.OwnerEmail,
                tc.Programmes);
            return Results.Ok(vm);
        });

        return app;
    }
}
