using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Data.Entities;
using HelloBuddy.Application.Programmes;
using HelloBuddy.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
            Status = ProgrammeDomainConstants.StatusPlanned,
            IsCurrent = true,
            Notes = treatmentCase.ClinicalSummary,
            Sessions =
            [
                new Session
                {
                    Period = ProgrammeDomainConstants.SessionPeriodSingle,
                    Objective = null,
                    Status = ProgrammeDomainConstants.StatusPlanned,
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

    public async Task<bool> IsLockedForEditAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        var visible = await OwnsAsync(programmeId, practitionerId, ct);
        if (!visible)
        {
            return false;
        }

        return await _db.Programmeversions
            .AnyAsync(v => v.ProgrammeId == programmeId, ct);
    }

    public async Task<ProgrammeVersionHistoryVm?> GetVersionHistoryAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        var programmeMeta = await _db.Programmes
            .Where(p => p.ProgrammeId == programmeId && p.TreatmentCase.PractitionerId == practitionerId)
            .Select(p => new
            {
                p.ProgrammeId,
                p.ProgrammeName,
                p.TreatmentCaseId,
            })
            .FirstOrDefaultAsync(ct);

        if (programmeMeta is null)
        {
            return null;
        }

        var versions = await _db.Programmeversions
            .Where(v => v.ProgrammeId == programmeId)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new ProgrammeVersionHistoryVm.VersionRow(
                v.ProgrammeVersionId,
                v.VersionNumber,
                v.VersionStatus,
                v.ChangeSummary,
                v.CreatedByPractitionerId,
                _db.Practitioners
                    .Where(p => p.PractitionerId == v.CreatedByPractitionerId)
                    .Select(p => p.FirstName + " " + p.LastName)
                    .FirstOrDefault() ?? $"Practitioner #{v.CreatedByPractitionerId}",
                v.CreatedDate,
                v.PublishedDate,
                v.SupersededDate,
                v.RetiredDate))
            .ToListAsync(ct);

        return new ProgrammeVersionHistoryVm(
            programmeMeta.ProgrammeId,
            programmeMeta.ProgrammeName,
            programmeMeta.TreatmentCaseId,
            versions);
    }

    public async Task<ProgrammeVm?> CreateDraftFromPublishedAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        var source = await _db.Programmes
            .Include(p => p.Sessions)
                .ThenInclude(s => s.Sessionexercises)
            .Include(p => p.Programmeversions)
            .FirstOrDefaultAsync(p => p.ProgrammeId == programmeId && p.TreatmentCase.PractitionerId == practitionerId, ct);

        if (source is null)
        {
            return null;
        }

        var hasPublishHistory = source.Programmeversions.Any(v =>
            string.Equals(v.VersionStatus, ProgrammeDomainConstants.VersionStatusPublished, StringComparison.OrdinalIgnoreCase)
            || string.Equals(v.VersionStatus, ProgrammeDomainConstants.VersionStatusSuperseded, StringComparison.OrdinalIgnoreCase)
            || v.PublishedDate.HasValue);

        if (!hasPublishHistory)
        {
            return null;
        }

        var clone = new Programme
        {
            TreatmentCaseId = source.TreatmentCaseId,
            ProgrammeName = $"{source.ProgrammeName} revision draft",
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            Status = ProgrammeDomainConstants.StatusPlanned,
            IsCurrent = true,
            Notes = source.Notes,
        };

        foreach (var sourceSession in source.Sessions.OrderBy(s => s.SortOrder))
        {
            var clonedSession = new Session
            {
                Period = sourceSession.Period,
                Objective = sourceSession.Objective,
                Status = ProgrammeDomainConstants.StatusPlanned,
                SortOrder = sourceSession.SortOrder,
            };

            foreach (var sourceExercise in sourceSession.Sessionexercises.OrderBy(se => se.SortOrder))
            {
                clonedSession.Sessionexercises.Add(new Sessionexercise
                {
                    ExerciseId = sourceExercise.ExerciseId,
                    SortOrder = sourceExercise.SortOrder,
                    Reps = sourceExercise.Reps,
                    Sets = sourceExercise.Sets,
                    HoldSeconds = sourceExercise.HoldSeconds,
                    Notes = sourceExercise.Notes,
                });
            }

            clone.Sessions.Add(clonedSession);
        }

        _db.Programmes.Add(clone);
        await _db.SaveChangesAsync(ct);

        var draftVersion = new Programmeversion
        {
            ProgrammeId = clone.ProgrammeId,
            VersionNumber = 1,
            VersionStatus = ProgrammeDomainConstants.VersionStatusDraft,
            PayloadJson = "{}",
            PayloadSchemaVersion = "1.0.0",
            ChangeSummary = $"Created from published programme {source.ProgrammeId}.",
            CreatedByPractitionerId = practitionerId,
            CreatedDate = DateTime.UtcNow,
        };

        _db.Programmeversions.Add(draftVersion);
        await _db.SaveChangesAsync(ct);

        clone.CurrentProgrammeVersionId = draftVersion.ProgrammeVersionId;
        await _db.SaveChangesAsync(ct);

        return await GetVmAsync(clone.ProgrammeId, practitionerId, ct);
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

        var targetPeriods = structure == ProgrammeDomainConstants.SessionStructureAmPm
            ? new[] { ProgrammeDomainConstants.SessionPeriodAm, ProgrammeDomainConstants.SessionPeriodPm }
            : new[] { ProgrammeDomainConstants.SessionPeriodSingle };

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
                    Objective = null,
                    Status = ProgrammeDomainConstants.StatusPlanned,
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
        IDbContextTransaction? tx = null;
        if (_db.Database.IsRelational())
        {
            tx = await _db.Database.BeginTransactionAsync(ct);
        }

        try
        {
            var programme = await _db.Programmes
                .FirstOrDefaultAsync(p => p.ProgrammeId == programmeId && p.TreatmentCase.PractitionerId == practitionerId, ct);
            if (programme is null)
            {
                return ProgrammeStatusTransitionResult.NotFound;
            }

            if (!string.Equals(programme.Status, ProgrammeDomainConstants.StatusPlanned, StringComparison.OrdinalIgnoreCase))
            {
                return ProgrammeStatusTransitionResult.InvalidTransition;
            }

            var activeExists = await _db.Programmes
                .AnyAsync(p => p.TreatmentCaseId == programme.TreatmentCaseId
                               && p.ProgrammeId != programmeId
                               && p.Status == ProgrammeDomainConstants.StatusActive, ct);
            if (activeExists)
            {
                return ProgrammeStatusTransitionResult.BlockedByAnotherActiveProgramme;
            }

            var otherProgrammes = await _db.Programmes
                .Where(p => p.TreatmentCaseId == programme.TreatmentCaseId && p.ProgrammeId != programmeId)
                .ToListAsync(ct);

            programme.Status = ProgrammeDomainConstants.StatusActive;
            programme.IsCurrent = true;

            foreach (var other in otherProgrammes)
            {
                other.IsCurrent = false;
            }

            await _db.SaveChangesAsync(ct);

            if (tx is not null)
            {
                await tx.CommitAsync(ct);
            }

            return ProgrammeStatusTransitionResult.Updated;
        }
        finally
        {
            if (tx is not null)
            {
                await tx.DisposeAsync();
            }
        }
    }

    public async Task<ProgrammeStatusTransitionResult> CompleteAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        var programme = await _db.Programmes
            .FirstOrDefaultAsync(p => p.ProgrammeId == programmeId && p.TreatmentCase.PractitionerId == practitionerId, ct);
        if (programme is null)
        {
            return ProgrammeStatusTransitionResult.NotFound;
        }

        if (!string.Equals(programme.Status, ProgrammeDomainConstants.StatusActive, StringComparison.OrdinalIgnoreCase))
        {
            return ProgrammeStatusTransitionResult.InvalidTransition;
        }

        programme.Status = ProgrammeDomainConstants.StatusCompleted;
        programme.IsCurrent = false;

        await _db.SaveChangesAsync(ct);
        return ProgrammeStatusTransitionResult.Updated;
    }

    public async Task UpdateSessionExercisesAsync(
        ulong programmeId,
        ulong practitionerId,
        IReadOnlyList<ProgrammeBuilderForm.SessionExerciseEdit> edits,
        CancellationToken ct)
    {
        if (edits.Count == 0)
        {
            return;
        }

        var ids = edits.Select(e => e.SessionExerciseId).ToList();
        var editedEntities = await _db.Sessionexercises
            .Where(se => ids.Contains(se.SessionExerciseId)
                         && se.Session.ProgrammeId == programmeId
                         && se.Session.Programme.TreatmentCase.PractitionerId == practitionerId)
            .ToListAsync(ct);

        var touchedSessionIds = editedEntities
            .Select(se => se.SessionId)
            .Distinct()
            .ToList();

        var sessionEntities = await _db.Sessionexercises
            .Where(se => touchedSessionIds.Contains(se.SessionId)
                         && se.Session.ProgrammeId == programmeId
                         && se.Session.Programme.TreatmentCase.PractitionerId == practitionerId)
            .ToListAsync(ct);

        var editsById = edits.ToDictionary(e => e.SessionExerciseId);

        foreach (var edit in edits)
        {
            var entity = editedEntities.FirstOrDefault(e => e.SessionExerciseId == edit.SessionExerciseId);
            if (entity is null) continue;
            entity.Reps = edit.Reps;
            entity.Sets = edit.Sets;
            entity.HoldSeconds = edit.HoldSeconds;
            entity.Notes = edit.Notes;
        }

        foreach (var sessionGroup in sessionEntities.GroupBy(se => se.SessionId))
        {
            var sorted = sessionGroup
                .OrderBy(se => se.SortOrder)
                .ThenBy(se => se.SessionExerciseId)
                .ToList();

            var moves = sorted
                .Select(se => new
                {
                    Entity = se,
                    CurrentSortOrder = se.SortOrder,
                    RequestedSortOrder = editsById.TryGetValue(se.SessionExerciseId, out var edit)
                        ? edit.SortOrder
                        : se.SortOrder,
                })
                .Where(x => x.RequestedSortOrder != x.CurrentSortOrder)
                .OrderBy(x => x.RequestedSortOrder)
                .ThenBy(x => x.CurrentSortOrder)
                .ThenBy(x => x.Entity.SessionExerciseId)
                .ToList();

            foreach (var move in moves)
            {
                var currentIndex = sorted.FindIndex(se => se.SessionExerciseId == move.Entity.SessionExerciseId);
                if (currentIndex < 0)
                {
                    continue;
                }

                var targetIndex = Math.Clamp(move.RequestedSortOrder - 1, 0, sorted.Count - 1);
                if (targetIndex == currentIndex)
                {
                    continue;
                }

                var entity = sorted[currentIndex];
                sorted.RemoveAt(currentIndex);
                sorted.Insert(targetIndex, entity);
            }

            for (ushort index = 0; index < sorted.Count; index++)
            {
                sorted[index].SortOrder = (ushort)(index + 1);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateSessionObjectivesAsync(
        ulong programmeId,
        ulong practitionerId,
        IReadOnlyList<ProgrammeBuilderForm.SessionEdit> edits,
        CancellationToken ct)
    {
        if (edits.Count == 0)
        {
            return;
        }

        var ids = edits.Select(e => e.SessionId).ToList();
        var sessions = await _db.Sessions
            .Where(s => ids.Contains(s.SessionId)
                        && s.ProgrammeId == programmeId
                        && s.Programme.TreatmentCase.PractitionerId == practitionerId)
            .ToListAsync(ct);

        var editsById = edits.ToDictionary(e => e.SessionId);
        foreach (var session in sessions)
        {
            if (!editsById.TryGetValue(session.SessionId, out var edit))
            {
                continue;
            }

            session.Objective = string.IsNullOrWhiteSpace(edit.Objective)
                ? null
                : edit.Objective.Trim();
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task PersistPublishedVersionAsync(ulong programmeId, ulong practitionerId, string payloadJson, CancellationToken ct)
    {
        IDbContextTransaction? tx = null;
        if (_db.Database.IsRelational())
        {
            tx = await _db.Database.BeginTransactionAsync(ct);
        }

        try
        {
        var programme = await _db.Programmes
            .Include(p => p.CurrentProgrammeVersion)
            .FirstOrDefaultAsync(p => p.ProgrammeId == programmeId && p.TreatmentCase.PractitionerId == practitionerId, ct)
            ?? throw new InvalidOperationException("Programme not found for publish version persistence.");

        if (programme.CurrentProgrammeVersion is not null
            && string.Equals(programme.CurrentProgrammeVersion.VersionStatus, ProgrammeDomainConstants.VersionStatusPublished, StringComparison.OrdinalIgnoreCase))
        {
            programme.CurrentProgrammeVersion.VersionStatus = ProgrammeDomainConstants.VersionStatusSuperseded;
            programme.CurrentProgrammeVersion.SupersededDate = DateTime.UtcNow;
        }

        var nextVersion = await _db.Programmeversions
            .Where(v => v.ProgrammeId == programmeId)
            .Select(v => (uint?)v.VersionNumber)
            .MaxAsync(ct) ?? 0;
        nextVersion++;

        var publishedVersion = new Programmeversion
        {
            ProgrammeId = programmeId,
            VersionNumber = nextVersion,
            VersionStatus = ProgrammeDomainConstants.VersionStatusPublished,
            PayloadJson = payloadJson,
            PayloadSchemaVersion = "1.0.0",
            CreatedByPractitionerId = practitionerId,
            CreatedDate = DateTime.UtcNow,
            PublishedDate = DateTime.UtcNow,
        };

        _db.Programmeversions.Add(publishedVersion);
        await _db.SaveChangesAsync(ct);

        programme.CurrentProgrammeVersionId = publishedVersion.ProgrammeVersionId;
        await _db.SaveChangesAsync(ct);

        if (tx is not null)
        {
            await tx.CommitAsync(ct);
        }
        }
        finally
        {
            if (tx is not null)
            {
                await tx.DisposeAsync();
            }
        }
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
