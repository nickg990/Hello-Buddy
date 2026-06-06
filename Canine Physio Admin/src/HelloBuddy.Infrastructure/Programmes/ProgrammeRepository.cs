using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Data.Entities;
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

    public async Task<ProgrammeVm?> CreateDraftAsync(ulong treatmentCaseId, ulong practitionerId, CancellationToken ct)
    {
        var treatmentCase = await _db.Treatmentcases
            .Where(tc => tc.TreatmentCaseId == treatmentCaseId && tc.PractitionerId == practitionerId)
            .Select(tc => new
            {
                tc.TreatmentCaseId,
                tc.CaseTitle,
                tc.StartDate,
                tc.EndDate,
                tc.ClinicalSummary,
            })
            .FirstOrDefaultAsync(ct);

        if (treatmentCase is null)
        {
            return null;
        }

        var programme = new Programme
        {
            TreatmentCaseId = treatmentCase.TreatmentCaseId,
            ProgrammeName = $"{treatmentCase.CaseTitle} draft",
            StartDate = treatmentCase.StartDate,
            EndDate = treatmentCase.EndDate,
            Status = "planned",
            IsCurrent = true,
            Notes = treatmentCase.ClinicalSummary,
            Sessions =
            [
                new Session
                {
                    Period = "single",
                    Objective = treatmentCase.ClinicalSummary,
                    Status = "planned",
                    SortOrder = 1,
                },
            ],
        };

        _db.Programmes.Add(programme);
        await _db.SaveChangesAsync(ct);

        return await GetVmAsync(programme.ProgrammeId, practitionerId, ct);
    }

    public async Task<bool> OwnsAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        return await _db.Programmes
            .AnyAsync(p => p.ProgrammeId == programmeId && p.TreatmentCase.PractitionerId == practitionerId, ct);
    }

    public async Task<DeleteProgrammeResult> DeleteAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        var visible = await OwnsAsync(programmeId, practitionerId, ct);
        if (!visible)
        {
            return DeleteProgrammeResult.NotFound;
        }

        // Guardrail: a programme with version history should not be hard-deleted.
        var hasVersionHistory = await _db.Programmeversions
            .AnyAsync(v => v.ProgrammeId == programmeId, ct);
        if (hasVersionHistory)
        {
            return DeleteProgrammeResult.BlockedByVersionHistory;
        }

        var sessionIds = await _db.Sessions
            .Where(s => s.ProgrammeId == programmeId)
            .Select(s => s.SessionId)
            .ToListAsync(ct);

        if (sessionIds.Count > 0)
        {
            var sessionExercises = await _db.Sessionexercises
                .Where(se => sessionIds.Contains(se.SessionId))
                .ToListAsync(ct);
            _db.Sessionexercises.RemoveRange(sessionExercises);

            var sessions = await _db.Sessions
                .Where(s => s.ProgrammeId == programmeId)
                .ToListAsync(ct);
            _db.Sessions.RemoveRange(sessions);
        }

        var programme = await _db.Programmes
            .FirstAsync(p => p.ProgrammeId == programmeId, ct);
        _db.Programmes.Remove(programme);

        await _db.SaveChangesAsync(ct);
        return DeleteProgrammeResult.Deleted;
    }

    public async Task<ProgrammeStructureUpdateResult> UpdateStructureAsync(ulong programmeId, ulong practitionerId, ProgrammeStructureForm form, CancellationToken ct)
    {
        var programme = await _db.Programmes
            .Include(p => p.Sessions)
            .ThenInclude(s => s.Sessionexercises)
            .FirstOrDefaultAsync(p => p.ProgrammeId == programmeId && p.TreatmentCase.PractitionerId == practitionerId, ct);

        if (programme is null)
        {
            return ProgrammeStructureUpdateResult.NotFound;
        }

        var structure = form.SessionStructure.Trim().ToLowerInvariant();
        if (structure is not ("single" or "am-pm"))
        {
            return ProgrammeStructureUpdateResult.InvalidStructure;
        }

        var programmeName = form.ProgrammeName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(programmeName))
        {
            return ProgrammeStructureUpdateResult.InvalidName;
        }

        programme.ProgrammeName = programmeName;
        programme.StartDate = form.StartDate;
        programme.EndDate = form.EndDate;

        var targetPeriods = structure == "am-pm"
            ? new[] { "AM", "PM" }
            : new[] { "single" };

        var existingPeriods = programme.Sessions
            .OrderBy(s => s.SortOrder)
            .Select(s => s.Period)
            .ToArray();

        var requiresRebuild = existingPeriods.Length != targetPeriods.Length
            || existingPeriods.Except(targetPeriods, StringComparer.OrdinalIgnoreCase).Any();

        if (requiresRebuild)
        {
            var allSessionExercises = programme.Sessions
                .SelectMany(s => s.Sessionexercises)
                .ToList();
            _db.Sessionexercises.RemoveRange(allSessionExercises);
            _db.Sessions.RemoveRange(programme.Sessions.ToList());

            for (var i = 0; i < targetPeriods.Length; i++)
            {
                programme.Sessions.Add(new Session
                {
                    Period = targetPeriods[i],
                    Objective = programme.Notes,
                    Status = "planned",
                    SortOrder = (byte)(i + 1),
                });
            }
        }

        await _db.SaveChangesAsync(ct);
        return ProgrammeStructureUpdateResult.Updated;
    }

    public async Task<AddSessionExerciseResult> AddSessionExerciseAsync(
        ulong programmeId,
        ulong practitionerId,
        ulong sessionId,
        ulong exerciseId,
        CancellationToken ct)
    {
        var session = await _db.Sessions
            .Include(s => s.Sessionexercises)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId
                                      && s.ProgrammeId == programmeId
                                      && s.Programme.TreatmentCase.PractitionerId == practitionerId, ct);

        if (session is null)
        {
            return AddSessionExerciseResult.NotFound;
        }

        var exerciseExists = await _db.Exercises
            .AnyAsync(e => e.ExerciseId == exerciseId && e.IsActive == true, ct);
        if (!exerciseExists)
        {
            return AddSessionExerciseResult.NotFound;
        }

        var duplicate = session.Sessionexercises.Any(se => se.ExerciseId == exerciseId);
        if (duplicate)
        {
            return AddSessionExerciseResult.AlreadyExists;
        }

        var nextSort = session.Sessionexercises.Count == 0
            ? 1
            : session.Sessionexercises.Max(se => se.SortOrder) + 1;

        session.Sessionexercises.Add(new Sessionexercise
        {
            ExerciseId = exerciseId,
            SortOrder = (ushort)nextSort,
            Reps = 1,
            Sets = 1,
            HoldSeconds = 1,
            Notes = null,
        });

        await _db.SaveChangesAsync(ct);
        return AddSessionExerciseResult.Added;
    }

    public async Task<RemoveSessionExerciseResult> RemoveSessionExerciseAsync(
        ulong programmeId,
        ulong practitionerId,
        ulong sessionId,
        ulong sessionExerciseId,
        CancellationToken ct)
    {
        var entity = await _db.Sessionexercises
            .FirstOrDefaultAsync(se => se.SessionExerciseId == sessionExerciseId
                                       && se.SessionId == sessionId
                                       && se.Session.ProgrammeId == programmeId
                                       && se.Session.Programme.TreatmentCase.PractitionerId == practitionerId, ct);

        if (entity is null)
        {
            return RemoveSessionExerciseResult.NotFound;
        }

        _db.Sessionexercises.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return RemoveSessionExerciseResult.Removed;
    }

    public async Task<ProgrammeStatusTransitionResult> ActivateAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var programme = await _db.Programmes
            .FirstOrDefaultAsync(p => p.ProgrammeId == programmeId && p.TreatmentCase.PractitionerId == practitionerId, ct);
        if (programme is null)
        {
            return ProgrammeStatusTransitionResult.NotFound;
        }

        if (!string.Equals(programme.Status, "planned", StringComparison.OrdinalIgnoreCase))
        {
            return ProgrammeStatusTransitionResult.InvalidTransition;
        }

        var activeExists = await _db.Programmes
            .AnyAsync(p => p.TreatmentCaseId == programme.TreatmentCaseId
                           && p.ProgrammeId != programmeId
                           && p.Status == "active", ct);
        if (activeExists)
        {
            return ProgrammeStatusTransitionResult.BlockedByAnotherActiveProgramme;
        }

        var otherProgrammes = await _db.Programmes
            .Where(p => p.TreatmentCaseId == programme.TreatmentCaseId && p.ProgrammeId != programmeId)
            .ToListAsync(ct);

        programme.Status = "active";
        programme.IsCurrent = true;

        foreach (var other in otherProgrammes)
        {
            other.IsCurrent = false;
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return ProgrammeStatusTransitionResult.Updated;
    }

    public async Task<ProgrammeStatusTransitionResult> CompleteAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        var programme = await _db.Programmes
            .FirstOrDefaultAsync(p => p.ProgrammeId == programmeId && p.TreatmentCase.PractitionerId == practitionerId, ct);
        if (programme is null)
        {
            return ProgrammeStatusTransitionResult.NotFound;
        }

        if (!string.Equals(programme.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            return ProgrammeStatusTransitionResult.InvalidTransition;
        }

        programme.Status = "completed";
        programme.IsCurrent = false;

        await _db.SaveChangesAsync(ct);
        return ProgrammeStatusTransitionResult.Updated;
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
