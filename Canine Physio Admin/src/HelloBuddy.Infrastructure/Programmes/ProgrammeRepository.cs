using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Application.Programmes;
using HelloBuddy.Contracts;
using Microsoft.EntityFrameworkCore;

namespace HelloBuddy.Infrastructure.Programmes;

public sealed class ProgrammeRepository : IProgrammeRepository
{
    private readonly CaninePhysioDbContext _db;

    public ProgrammeRepository(CaninePhysioDbContext db)
    {
        _db = db;
    }

    public async Task<bool> OwnsAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        return await _db.Programmes
            .AnyAsync(p => p.ProgrammeId == programmeId && p.TreatmentCase.PractitionerId == practitionerId, ct);
    }

    public async Task UpdateSessionExercisesAsync(
        ulong programmeId,
        IReadOnlyList<ProgrammeBuilderForm.SessionExerciseEdit> edits,
        CancellationToken ct)
    {
        var ids = edits.Select(e => e.SessionExerciseId).ToList();
        var entities = await _db.Sessionexercises
            .Where(se => ids.Contains(se.SessionExerciseId) && se.Session.ProgrammeId == programmeId)
            .ToListAsync(ct);

        foreach (var edit in edits)
        {
            var entity = entities.FirstOrDefault(e => e.SessionExerciseId == edit.SessionExerciseId);
            if (entity is null) continue;
            entity.Reps = edit.Reps;
            entity.Sets = edit.Sets;
            entity.HoldSeconds = edit.HoldSeconds;
            entity.SortOrder = edit.SortOrder;
            entity.Notes = edit.Notes;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<ProgrammeVm?> GetVmAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        var data = await _db.Programmes
            .Where(p => p.ProgrammeId == programmeId && p.TreatmentCase.PractitionerId == practitionerId)
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
                                se.Exercise.ImageUrl,
                                se.Exercise.VideoUrl,
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
