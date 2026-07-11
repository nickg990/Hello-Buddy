using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Data.Entities;
using HelloBuddy.Admin.Pdf;
using HelloBuddy.Application.Admin;
using HelloBuddy.Application.Auth;
using HelloBuddy.Application.Programmes;
using HelloBuddy.Application.Records;
using HelloBuddy.Contracts;
using HelloBuddy.Infrastructure.Auth;
using HelloBuddy.Infrastructure.Programmes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System.Text.Json;

namespace HelloBuddy.Infrastructure.Records;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddHelloBuddyInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IOwnerRepository, OwnerRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<ITreatmentCaseRepository, TreatmentCaseRepository>();
        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        services.AddScoped<IProgrammeRepository, ProgrammeRepository>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IPractitionerAdminService, PractitionerAdminService>();
        services.AddScoped<IAppSettingRepository, AppSettingRepository>();
        return services;
    }
}

public sealed class AppSettingRepository : IAppSettingRepository
{
    private readonly CaninePhysioDbContext _db;

    public AppSettingRepository(CaninePhysioDbContext db)
    {
        _db = db;
    }

    public async Task<string?> GetAsync(string key, CancellationToken ct)
    {
        var entity = await _db.Appsettings.FindAsync([key], ct);
        return entity?.SettingValue;
    }

    public async Task UpsertAsync(string key, string? value, ulong? practitionerId, CancellationToken ct)
    {
        var entity = await _db.Appsettings.FindAsync([key], ct);
        if (entity is null)
        {
            entity = new Appsetting { SettingKey = key };
            _db.Appsettings.Add(entity);
        }

        entity.SettingValue = value;
        entity.UpdatedByPractitionerId = practitionerId;

        await _db.SaveChangesAsync(ct);

        _db.Auditlogs.Add(new Auditlog
        {
            EntityName = "AppSetting",
            EntityId = 0,
            ActionType = "Update",
            PractitionerId = practitionerId,
            ActionDateTime = DateTime.UtcNow,
            NewValuesJson = System.Text.Json.JsonSerializer.Serialize(new { Key = key, Value = value }),
        });
        await _db.SaveChangesAsync(ct);
    }
}

public sealed class ExerciseRepository : IExerciseRepository
{
    private readonly CaninePhysioDbContext _db;
    private readonly HelloBuddy.Admin.Core.Identity.ICurrentPractitionerAccessor _accessor;

    public ExerciseRepository(
        CaninePhysioDbContext db,
        HelloBuddy.Admin.Core.Identity.ICurrentPractitionerAccessor accessor)
    {
        _db = db;
        _accessor = accessor;
    }

    public async Task<IReadOnlyList<ExerciseListItem>> ListAsync(ExerciseListFilter filter, CancellationToken ct)
    {
        var query = _db.Exercises.AsQueryable();

        if (filter.ActiveOnly)
        {
            query = query.Where(x => x.IsActive ?? true);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.ExerciseCategoryId == filter.CategoryId.Value);
        }

        if (filter.HasVideo.HasValue)
        {
            if (filter.HasVideo.Value)
            {
                query = query.Where(x => !string.IsNullOrWhiteSpace(x.VideoUrl));
            }
            else
            {
                query = query.Where(x => string.IsNullOrWhiteSpace(x.VideoUrl));
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = filter.SearchText.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Title.ToLower().Contains(search) ||
                (x.ObjectiveSummary != null && x.ObjectiveSummary.ToLower().Contains(search)) ||
                x.Exerciseinstructions.Any(step => step.InstructionText.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(x => x.Title)
            .Select(x => new ExerciseListItem(
                x.ExerciseId,
                x.ExerciseKey,
                x.Title,
                x.ExerciseCategoryId,
                x.ExerciseCategory != null ? x.ExerciseCategory.CategoryName : null,
                x.ObjectiveSummary,
                x.IsActive ?? true,
                !string.IsNullOrWhiteSpace(x.ImageUrl),
                !string.IsNullOrWhiteSpace(x.VideoUrl),
                x.UpdatedDate))
            .ToListAsync(ct);
    }

    public async Task<ExerciseDetailVm?> GetAsync(ulong exerciseId, CancellationToken ct)
    {
        return await _db.Exercises
            .Where(x => x.ExerciseId == exerciseId)
            .Select(x => new ExerciseDetailVm(
                x.ExerciseId,
                x.ExerciseCategoryId,
                x.ExerciseCategory != null ? x.ExerciseCategory.CategoryName : null,
                x.ExerciseKey,
                x.Title,
                x.ObjectiveSummary,
                x.ImageUrl,
                x.VideoUrl,
                x.DefaultReps,
                x.DefaultSets,
                x.DefaultHoldSeconds,
                x.IsActive ?? true,
                x.InstructionsText,
                x.UpdatedDate,
                x.Exerciseinstructions
                    .OrderBy(step => step.StepNumber)
                    .Select(step => new ExerciseDetailVm.InstructionStepVm(step.StepNumber, step.InstructionText))
                    .ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<ExerciseCategoryListItem>> ListCategoriesAsync(CancellationToken ct)
    {
        return await _db.Exercisecategories
            .OrderBy(x => x.CategoryName)
            .Select(x => new ExerciseCategoryListItem(
                x.ExerciseCategoryId,
                x.CategoryName,
                x.IsActive ?? true))
            .ToListAsync(ct);
    }

    public Task<bool> CategoryExistsAsync(ulong exerciseCategoryId, CancellationToken ct)
    {
        return _db.Exercisecategories.AnyAsync(x => x.ExerciseCategoryId == exerciseCategoryId, ct);
    }

    public async Task<ulong> CreateAsync(SaveExerciseRequest request, CancellationToken ct)
    {
        var useTransaction = !string.Equals(_db.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal);
        if (useTransaction)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            var exerciseId = await CreateInternalAsync(request, ct);
            await tx.CommitAsync(ct);
            return exerciseId;
        }

        return await CreateInternalAsync(request, ct);
    }

    public async Task<bool> UpdateAsync(ulong exerciseId, SaveExerciseRequest request, CancellationToken ct)
    {
        var useTransaction = !string.Equals(_db.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal);
        if (useTransaction)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            var updated = await UpdateInternalAsync(exerciseId, request, ct);
            if (updated)
            {
                await tx.CommitAsync(ct);
            }

            return updated;
        }

        return await UpdateInternalAsync(exerciseId, request, ct);
    }

    private async Task<ulong> CreateInternalAsync(SaveExerciseRequest request, CancellationToken ct)
    {

        var entity = new Exercise
        {
            ExerciseCategoryId = request.ExerciseCategoryId,
            ExerciseKey = await BuildExerciseKeyAsync(request.Title, null, ct),
            Title = request.Title.Trim(),
            ObjectiveSummary = RecordNormalization.NullIfWhiteSpace(request.ObjectiveSummary),
            ImageUrl = RecordNormalization.NullIfWhiteSpace(request.ImageUrl),
            VideoUrl = RecordNormalization.NullIfWhiteSpace(request.VideoUrl),
            DefaultReps = request.DefaultReps,
            DefaultSets = request.DefaultSets,
            DefaultHoldSeconds = request.DefaultHoldSeconds,
            IsActive = request.IsActive,
            CreatedByPractitionerId = _accessor.PractitionerId > 0 ? _accessor.PractitionerId : null,
            CreatedByPractitionerName = _accessor.PractitionerId > 0 ? _accessor.PractitionerName : null,
        };

        _db.Exercises.Add(entity);
        await _db.SaveChangesAsync(ct);

        ApplyInstructionRows(entity.ExerciseId, request.Instructions);

        var categoryName = await GetCategoryNameAsync(entity.ExerciseCategoryId, ct);
        var newSnapshot = BuildSnapshot(entity, categoryName, request.Instructions);
        AddExerciseAudit(entity.ExerciseId, "Create", oldSnapshot: null, newSnapshot);

        await _db.SaveChangesAsync(ct);
        return entity.ExerciseId;
    }

    private async Task<bool> UpdateInternalAsync(ulong exerciseId, SaveExerciseRequest request, CancellationToken ct)
    {
        var entity = await _db.Exercises.FirstOrDefaultAsync(x => x.ExerciseId == exerciseId, ct);
        if (entity is null)
        {
            return false;
        }

        var oldCategoryName = await GetCategoryNameAsync(entity.ExerciseCategoryId, ct);
        var existingSteps = await _db.Exerciseinstructions
            .Where(x => x.ExerciseId == exerciseId)
            .OrderBy(x => x.StepNumber)
            .ToListAsync(ct);
        var oldSnapshot = BuildSnapshot(entity, oldCategoryName, existingSteps.Select(x => x.InstructionText).ToList());

        entity.ExerciseCategoryId = request.ExerciseCategoryId;
        entity.Title = request.Title.Trim();
        entity.ObjectiveSummary = RecordNormalization.NullIfWhiteSpace(request.ObjectiveSummary);
        entity.ImageUrl = RecordNormalization.NullIfWhiteSpace(request.ImageUrl);
        entity.VideoUrl = RecordNormalization.NullIfWhiteSpace(request.VideoUrl);
        entity.DefaultReps = request.DefaultReps;
        entity.DefaultSets = request.DefaultSets;
        entity.DefaultHoldSeconds = request.DefaultHoldSeconds;
        entity.IsActive = request.IsActive;
        if (_accessor.PractitionerId > 0)
        {
            entity.UpdatedByPractitionerId = _accessor.PractitionerId;
            entity.UpdatedByPractitionerName = _accessor.PractitionerName;
        }

        _db.Exerciseinstructions.RemoveRange(existingSteps);
        await _db.SaveChangesAsync(ct);

        ApplyInstructionRows(exerciseId, request.Instructions);

        var newCategoryName = await GetCategoryNameAsync(entity.ExerciseCategoryId, ct);
        var newSnapshot = BuildSnapshot(entity, newCategoryName, request.Instructions);
        AddExerciseAudit(exerciseId, "Update", oldSnapshot, newSnapshot);

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SetActiveAsync(ulong exerciseId, bool isActive, CancellationToken ct)
    {
        var entity = await _db.Exercises.FirstOrDefaultAsync(x => x.ExerciseId == exerciseId, ct);
        if (entity is null)
        {
            return false;
        }

        var categoryName = await GetCategoryNameAsync(entity.ExerciseCategoryId, ct);
        var instructions = await _db.Exerciseinstructions
            .Where(x => x.ExerciseId == exerciseId)
            .OrderBy(x => x.StepNumber)
            .Select(x => x.InstructionText)
            .ToListAsync(ct);
        var oldSnapshot = BuildSnapshot(entity, categoryName, instructions);

        entity.IsActive = isActive;
        var newSnapshot = oldSnapshot with { IsActive = isActive };
        AddExerciseAudit(exerciseId, isActive ? "Activate" : "Deactivate", oldSnapshot, newSnapshot);

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<ExerciseAuditEntryVm>> GetAuditHistoryAsync(ulong exerciseId, CancellationToken ct)
    {
        var rows = await _db.Auditlogs
            .Where(a => a.EntityName == ExerciseAuditEntityName && a.EntityId == exerciseId)
            .OrderByDescending(a => a.ActionDateTime)
            .ToListAsync(ct);

        var result = new List<ExerciseAuditEntryVm>(rows.Count);
        foreach (var row in rows)
        {
            var oldPayload = row.OldValuesJson is null
                ? null
                : JsonSerializer.Deserialize<ExerciseAuditPayload>(row.OldValuesJson);
            var newPayload = row.NewValuesJson is null
                ? null
                : JsonSerializer.Deserialize<ExerciseAuditPayload>(row.NewValuesJson);

            result.Add(new ExerciseAuditEntryVm(
                row.AuditLogId,
                newPayload?.AuthorName,
                row.ActionDateTime,
                row.ActionType,
                BuildChanges(oldPayload?.Snapshot, newPayload?.Snapshot)));
        }

        return result;
    }

    private const string ExerciseAuditEntityName = "Exercise";

    private static ExerciseAuditSnapshot BuildSnapshot(Exercise entity, string? categoryName, IReadOnlyList<string> instructions)
    {
        return new ExerciseAuditSnapshot(
            entity.Title,
            entity.ExerciseCategoryId,
            categoryName,
            entity.ObjectiveSummary,
            entity.VideoUrl,
            entity.ImageUrl,
            entity.DefaultReps,
            entity.DefaultSets,
            entity.DefaultHoldSeconds,
            entity.IsActive,
            instructions);
    }

    private static ExerciseAuditSnapshot BuildSnapshot(Exercise entity, string? categoryName, IReadOnlyList<SaveExerciseRequest.InstructionStepInput> instructions)
    {
        return BuildSnapshot(
            entity,
            categoryName,
            instructions.OrderBy(x => x.StepNumber).Select(x => x.InstructionText).ToList());
    }

    private Task<string?> GetCategoryNameAsync(ulong? exerciseCategoryId, CancellationToken ct)
    {
        if (!exerciseCategoryId.HasValue)
        {
            return Task.FromResult<string?>(null);
        }

        return _db.Exercisecategories
            .Where(x => x.ExerciseCategoryId == exerciseCategoryId.Value)
            .Select(x => (string?)x.CategoryName)
            .FirstOrDefaultAsync(ct);
    }

    private void AddExerciseAudit(ulong exerciseId, string actionType, ExerciseAuditSnapshot? oldSnapshot, ExerciseAuditSnapshot newSnapshot)
    {
        var practitionerId = _accessor.PractitionerId > 0 ? _accessor.PractitionerId : (ulong?)null;
        var authorName = practitionerId.HasValue ? _accessor.PractitionerName : null;

        _db.Auditlogs.Add(new Auditlog
        {
            PractitionerId = practitionerId,
            EntityName = ExerciseAuditEntityName,
            EntityId = exerciseId,
            ActionType = actionType,
            OldValuesJson = oldSnapshot is null
                ? null
                : JsonSerializer.Serialize(new ExerciseAuditPayload(authorName, oldSnapshot)),
            NewValuesJson = JsonSerializer.Serialize(new ExerciseAuditPayload(authorName, newSnapshot)),
            ActionDateTime = DateTime.UtcNow,
        });
    }

    private static IReadOnlyList<ExerciseAuditEntryVm.FieldChangeVm> BuildChanges(
        ExerciseAuditSnapshot? oldSnapshot,
        ExerciseAuditSnapshot? newSnapshot)
    {
        if (newSnapshot is null)
        {
            return [];
        }

        static string? Fmt<T>(T? value) where T : struct => value.HasValue ? value.Value.ToString() : null;

        var changes = new List<ExerciseAuditEntryVm.FieldChangeVm>();

        void AddIfChanged(string fieldName, string? oldValue, string? newValue)
        {
            if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
            {
                changes.Add(new ExerciseAuditEntryVm.FieldChangeVm(fieldName, oldValue, newValue));
            }
        }

        AddIfChanged("Title", oldSnapshot?.Title, newSnapshot.Title);
        AddIfChanged("Category", oldSnapshot?.CategoryName, newSnapshot.CategoryName);
        AddIfChanged("Objective summary", oldSnapshot?.ObjectiveSummary, newSnapshot.ObjectiveSummary);
        AddIfChanged("Video URL", oldSnapshot?.VideoUrl, newSnapshot.VideoUrl);
        AddIfChanged("Image URL", oldSnapshot?.ImageUrl, newSnapshot.ImageUrl);
        AddIfChanged("Default reps", Fmt(oldSnapshot?.DefaultReps), Fmt(newSnapshot.DefaultReps));
        AddIfChanged("Default sets", Fmt(oldSnapshot?.DefaultSets), Fmt(newSnapshot.DefaultSets));
        AddIfChanged("Default hold seconds", Fmt(oldSnapshot?.DefaultHoldSeconds), Fmt(newSnapshot.DefaultHoldSeconds));
        AddIfChanged("Active", Fmt(oldSnapshot?.IsActive), Fmt(newSnapshot.IsActive));

        var oldInstructionsText = oldSnapshot is null ? null : string.Join(" | ", oldSnapshot.Instructions);
        var newInstructionsText = string.Join(" | ", newSnapshot.Instructions);
        AddIfChanged("Instructions", oldInstructionsText, newInstructionsText);

        return changes;
    }

    private sealed record ExerciseAuditSnapshot(
        string? Title,
        ulong? ExerciseCategoryId,
        string? CategoryName,
        string? ObjectiveSummary,
        string? VideoUrl,
        string? ImageUrl,
        ushort? DefaultReps,
        ushort? DefaultSets,
        ushort? DefaultHoldSeconds,
        bool? IsActive,
        IReadOnlyList<string> Instructions);

    private sealed record ExerciseAuditPayload(string? AuthorName, ExerciseAuditSnapshot Snapshot);

    private void ApplyInstructionRows(ulong exerciseId, IReadOnlyList<SaveExerciseRequest.InstructionStepInput> instructions)
    {
        for (var i = 0; i < instructions.Count; i++)
        {
            _db.Exerciseinstructions.Add(new Exerciseinstruction
            {
                ExerciseId = exerciseId,
                StepNumber = (ushort)(i + 1),
                InstructionText = instructions[i].InstructionText.Trim()
            });
        }
    }

    private async Task<string> BuildExerciseKeyAsync(string title, ulong? excludedExerciseId, CancellationToken ct)
    {
        var baseKey = BuildSlug(title);
        var candidate = baseKey;
        var suffix = 2;

        while (await _db.Exercises.AnyAsync(
                   x => x.ExerciseKey == candidate &&
                        (!excludedExerciseId.HasValue || x.ExerciseId != excludedExerciseId.Value),
                   ct))
        {
            candidate = $"{baseKey}-{suffix}";
            suffix++;
        }

        return candidate;
    }

    private static string BuildSlug(string value)
    {
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();

        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        slug = slug.Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "exercise" : slug;
    }
}

public sealed class OwnerRepository : IOwnerRepository
{
    private readonly CaninePhysioDbContext _db;
    private readonly HelloBuddy.Admin.Core.Identity.ICurrentPractitionerAccessor _accessor;
    private readonly IFileStore _fileStore;

    public OwnerRepository(
        CaninePhysioDbContext db,
        HelloBuddy.Admin.Core.Identity.ICurrentPractitionerAccessor accessor,
        IFileStore fileStore)
    {
        _db = db;
        _accessor = accessor;
        _fileStore = fileStore;
    }

    public async Task<IReadOnlyList<OwnerListItem>> ListAsync(CancellationToken ct)
    {
        return await _db.Owners
            .OrderBy(o => o.LastName)
            .ThenBy(o => o.FirstName)
            .Select(o => new OwnerListItem(
                o.OwnerId,
                o.FirstName + " " + o.LastName,
                o.Email,
                o.PhoneNumber,
                o.Pets.Count))
            .ToListAsync(ct);
    }

    public async Task<OwnerDetailVm?> GetAsync(ulong ownerId, CancellationToken ct)
    {
        return await _db.Owners
            .Where(o => o.OwnerId == ownerId)
            .Select(o => new OwnerDetailVm(
                o.OwnerId,
                o.FirstName,
                o.LastName,
                o.Email,
                o.PhoneNumber,
                o.AddressLine1,
                o.AddressLine2,
                o.Town,
                o.Postcode,
                o.Pets
                    .OrderBy(p => p.Name)
                    .Select(p => new OwnerDetailVm.PetRow(
                        p.PetId,
                        p.Name,
                        p.Breed,
                        p.Sex,
                        p.IsActive ?? true,
                        p.Treatmentcases.Count))
                        .ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public Task<bool> EmailInUseAsync(string email, ulong? excludedOwnerId, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _db.Owners.AnyAsync(o => o.Email.ToLower() == normalized && (!excludedOwnerId.HasValue || o.OwnerId != excludedOwnerId.Value), ct);
    }

    public async Task<ulong> CreateAsync(SaveOwnerRequest request, CancellationToken ct)
    {
        var entity = new Owner
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = RecordNormalization.NullIfWhiteSpace(request.PhoneNumber),
            AddressLine1 = RecordNormalization.NullIfWhiteSpace(request.AddressLine1),
            AddressLine2 = RecordNormalization.NullIfWhiteSpace(request.AddressLine2),
            Town = RecordNormalization.NullIfWhiteSpace(request.Town),
            Postcode = RecordNormalization.NullIfWhiteSpace(request.Postcode),
            CreatedByPractitionerId = _accessor.PractitionerId > 0 ? _accessor.PractitionerId : null,
            CreatedByPractitionerName = _accessor.PractitionerId > 0 ? _accessor.PractitionerName : null,
        };

        _db.Owners.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.OwnerId;
    }

    public async Task<bool> UpdateAsync(ulong ownerId, SaveOwnerRequest request, CancellationToken ct)
    {
        var entity = await _db.Owners.FirstOrDefaultAsync(o => o.OwnerId == ownerId, ct);
        if (entity is null)
        {
            return false;
        }

        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Email = request.Email.Trim().ToLowerInvariant();
        entity.PhoneNumber = RecordNormalization.NullIfWhiteSpace(request.PhoneNumber);
        entity.AddressLine1 = RecordNormalization.NullIfWhiteSpace(request.AddressLine1);
        entity.AddressLine2 = RecordNormalization.NullIfWhiteSpace(request.AddressLine2);
        entity.Town = RecordNormalization.NullIfWhiteSpace(request.Town);
        entity.Postcode = RecordNormalization.NullIfWhiteSpace(request.Postcode);
        if (_accessor.PractitionerId > 0)
        {
            entity.UpdatedByPractitionerId = _accessor.PractitionerId;
            entity.UpdatedByPractitionerName = _accessor.PractitionerName;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<OwnerDataControlResult> ApplyDataControlAsync(ulong ownerId, ulong practitionerId, CancellationToken ct)
    {
        var ownerExists = await _db.Owners.AnyAsync(o => o.OwnerId == ownerId, ct);
        if (!ownerExists)
        {
            return OwnerDataControlResult.NotFound;
        }

        // GDPR right-to-be-forgotten is an administrative erase: any authenticated
        // practitioner may delete an owner and all associated records. We deliberately
        // do NOT gate on practitioner-pet linkage here - that check produced false
        // "not linked" rejections (e.g. for the admin account, or owners whose pets had
        // already been partially removed) which left owners permanently undeletable.
        var programmePdfPrefixes = await DeleteOwnerAndRelatedDataAsync(ownerId, ct);

        AddOwnerGdprDeletionAudit(ownerId, practitionerId);
        await _db.SaveChangesAsync(ct);

        // Stored PDFs are removed only after the database delete has committed, so a
        // database failure can never leave records intact while their blobs are gone.
        foreach (var prefix in programmePdfPrefixes)
        {
            await _fileStore.DeleteByPrefixAsync(prefix, ct);
        }

        return OwnerDataControlResult.Deleted;
    }

    private async Task<List<string>> DeleteOwnerAndRelatedDataAsync(ulong ownerId, CancellationToken ct)
    {
        // Step 1: Get all programmes for this owner to delete associated PDF files.
        // NOTE: do NOT Include(p => p.Programmeversions) here. That eager-load joins the
        // ProgrammeVersion table (which carries the large PayloadJson column) and forces a
        // MySQL filesort, which exhausts sort_buffer_size on small server tiers ("Out of sort
        // memory"). We only need IDs, so load programmes alone and fetch version ids separately.
        var programmes = await _db.Programmes
            .Where(p => p.TreatmentCase.Pet.OwnerId == ownerId)
            .ToListAsync(ct);

        // Step 2: Collect stored PDF prefixes. The blobs are deleted by the caller only
        // after the database delete has committed, so a DB failure cannot orphan-delete PDFs.
        var programmePdfPrefixes = programmes
            .Select(p => $"programme-{p.ProgrammeId}-")
            .ToList();

        var programmeIds = programmes.Select(p => p.ProgrammeId).ToList();
        var versionIds = programmeIds.Count > 0
            ? await _db.Programmeversions
                .Where(v => programmeIds.Contains(v.ProgrammeId))
                .Select(v => v.ProgrammeVersionId)
                .ToListAsync(ct)
            : new List<ulong>();

        if (versionIds.Count > 0)
        {
            var sessionSkips = await _db.Sessionskips
                .Where(x => versionIds.Contains(x.SessionOccurrence.ProgrammeVersionId))
                .ToListAsync(ct);
            _db.Sessionskips.RemoveRange(sessionSkips);

            var exerciseCompletions = await _db.Exercisecompletions
                .Where(x => versionIds.Contains(x.SessionOccurrence.ProgrammeVersionId))
                .ToListAsync(ct);
            _db.Exercisecompletions.RemoveRange(exerciseCompletions);

            var sessionOccurrences = await _db.Sessionoccurrences
                .Where(x => versionIds.Contains(x.ProgrammeVersionId))
                .ToListAsync(ct);
            _db.Sessionoccurrences.RemoveRange(sessionOccurrences);
        }

        if (programmeIds.Count > 0)
        {
            var sessionExercises = await _db.Sessionexercises
                .Where(x => programmeIds.Contains(x.Session.ProgrammeId))
                .ToListAsync(ct);
            _db.Sessionexercises.RemoveRange(sessionExercises);

            var sessions = await _db.Sessions
                .Where(x => programmeIds.Contains(x.ProgrammeId))
                .ToListAsync(ct);
            _db.Sessions.RemoveRange(sessions);
        }

        // Session occurrences (deleted above) are the only rows that RESTRICT deleting a
        // ProgrammeVersion, so the versions can now be removed. Do NOT delete them via EF:
        // Programme.CurrentProgrammeVersionId -> ProgrammeVersion and
        // ProgrammeVersion.ProgrammeId -> Programme form a cycle EF cannot order when both
        // ends are Deleted. Instead we delete the Programme and let MySQL's
        // FK_ProgrammeVersion_Programme (ON DELETE CASCADE) remove the version rows.
        _db.Programmes.RemoveRange(programmes);

        var treatmentCases = await _db.Treatmentcases
            .Where(tc => tc.Pet.OwnerId == ownerId)
            .Include(tc => tc.Treatmentcasenotes)
            .ToListAsync(ct);

        foreach (var treatmentCase in treatmentCases)
        {
            _db.Treatmentcasenotes.RemoveRange(treatmentCase.Treatmentcasenotes);
        }

        _db.Treatmentcases.RemoveRange(treatmentCases);

        // Step 4: Delete UserAccount records (they have FK_UserAccount_Owner with RESTRICT)
        var userAccountIds = await _db.Useraccounts
            .Where(ua => ua.OwnerId == ownerId)
            .Select(ua => ua.UserAccountId)
            .ToListAsync(ct);

        if (userAccountIds.Count > 0)
        {
            var notificationPreferences = await _db.Notificationpreferences
                .Where(np => userAccountIds.Contains(np.UserAccountId))
                .ToListAsync(ct);
            _db.Notificationpreferences.RemoveRange(notificationPreferences);

            var termsAcceptances = await _db.Termsacceptances
                .Where(ta => userAccountIds.Contains(ta.UserAccountId))
                .ToListAsync(ct);
            _db.Termsacceptances.RemoveRange(termsAcceptances);

            var passwordResetRequests = await _db.Passwordresetrequests
                .Where(pr => userAccountIds.Contains(pr.UserAccountId))
                .ToListAsync(ct);
            _db.Passwordresetrequests.RemoveRange(passwordResetRequests);
        }

        var userAccounts = await _db.Useraccounts
            .Where(ua => ua.OwnerId == ownerId)
            .ToListAsync(ct);
        _db.Useraccounts.RemoveRange(userAccounts);

        // Step 5: Delete RegistrationCode records linked to owner's pets
        var pets = await _db.Pets
            .Where(p => p.OwnerId == ownerId)
            .Include(p => p.Registrationcodes)
            .ToListAsync(ct);

        foreach (var pet in pets)
        {
            _db.Registrationcodes.RemoveRange(pet.Registrationcodes);
        }

        // Step 6: Delete PractitionerPet records (they have RESTRICT FK on Pet)
        var practitionerPets = await _db.PractitionerPets
            .Where(pp => pp.Pet.OwnerId == ownerId)
            .ToListAsync(ct);
        _db.PractitionerPets.RemoveRange(practitionerPets);

        // Step 7: Delete Pet records.
        _db.Pets.RemoveRange(pets);

        // Step 8: Delete Owner
        var owner = await _db.Owners.FirstOrDefaultAsync(o => o.OwnerId == ownerId, ct);
        if (owner is not null)
        {
            _db.Owners.Remove(owner);
        }

        return programmePdfPrefixes;
    }

    private void AddOwnerGdprDeletionAudit(ulong ownerId, ulong practitionerId)
    {
        _db.Auditlogs.Add(new Auditlog
        {
            PractitionerId = practitionerId,
            EntityName = "owner",
            EntityId = ownerId,
            ActionType = "gdpr-deletion",
            NewValuesJson = JsonSerializer.Serialize(new
            {
                outcome = "deleted",
                ownerId,
            }),
            ActionDateTime = DateTime.UtcNow,
        });
    }
}

public sealed class PetRepository : IPetRepository
{
    private readonly CaninePhysioDbContext _db;
    private readonly HelloBuddy.Admin.Core.Identity.ICurrentPractitionerAccessor _accessor;
    private readonly IFileStore _fileStore;

    public PetRepository(
        CaninePhysioDbContext db,
        HelloBuddy.Admin.Core.Identity.ICurrentPractitionerAccessor accessor,
        IFileStore fileStore)
    {
        _db = db;
        _accessor = accessor;
        _fileStore = fileStore;
    }

    public async Task<IReadOnlyList<PetListItem>> ListAsync(CancellationToken ct)
    {
        return await _db.Pets
            .OrderBy(p => p.Name)
            .Select(p => new PetListItem(
                p.PetId,
                p.OwnerId,
                p.Name,
                p.Owner.FirstName + " " + p.Owner.LastName,
                p.Breed,
                p.Sex,
                p.IsActive ?? true,
                p.Treatmentcases.Count))
            .ToListAsync(ct);
    }

    public async Task<PetDetailVm?> GetAsync(ulong petId, ulong practitionerId, CancellationToken ct)
    {
        return await _db.Pets
            .Where(p => p.PetId == petId)
            .Select(p => new PetDetailVm(
                p.PetId,
                p.OwnerId,
                p.Name,
                p.Age,
                p.DateOfBirth,
                p.Breed,
                p.Sex,
                p.Weight,
                p.IsActive ?? true,
                p.Owner.FirstName + " " + p.Owner.LastName,
                p.Owner.Email,
                p.Treatmentcases
                    .OrderByDescending(tc => tc.StartDate)
                    .Select(tc => new PetDetailVm.CaseRow(
                        tc.TreatmentCaseId,
                        tc.CaseTitle,
                        tc.Status,
                        tc.StartDate,
                        tc.Practitioner.FirstName + " " + tc.Practitioner.LastName))
                    .ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public Task<bool> OwnerExistsAsync(ulong ownerId, CancellationToken ct)
    {
        return _db.Owners.AnyAsync(
            o => o.OwnerId == ownerId,
            ct);
    }

    public Task<bool> ExistsAsync(ulong petId, CancellationToken ct)
    {
        return _db.Pets.AnyAsync(
            p => p.PetId == petId,
            ct);
    }

    public async Task<ulong> CreateAsync(SavePetRequest request, ulong practitionerId, CancellationToken ct)
    {
        var entity = new Pet
        {
            OwnerId = request.OwnerId,
            Name = request.Name.Trim(),
            Age = request.Age,
            DateOfBirth = request.DateOfBirth,
            Breed = RecordNormalization.NullIfWhiteSpace(request.Breed),
            Sex = RecordNormalization.NormalizeSex(request.Sex),
            Weight = request.Weight,
            IsActive = request.IsActive,
            CreatedByPractitionerId = _accessor.PractitionerId > 0 ? _accessor.PractitionerId : null,
            CreatedByPractitionerName = _accessor.PractitionerId > 0 ? _accessor.PractitionerName : null,
        };

        _db.Pets.Add(entity);
        await _db.SaveChangesAsync(ct);

        var assignmentExists = await _db.PractitionerPets.AnyAsync(
            p => p.PetId == entity.PetId && p.PractitionerId == practitionerId && p.AssignedTo == null,
            ct);
        if (!assignmentExists)
        {
            _db.PractitionerPets.Add(new PractitionerPet
            {
                PractitionerId = practitionerId,
                PetId = entity.PetId,
                AssignedFrom = DateTime.UtcNow,
                IsPrimary = true,
                Status = "active"
            });
            await _db.SaveChangesAsync(ct);
        }

        return entity.PetId;
    }

    public async Task<bool> UpdateAsync(ulong petId, SavePetRequest request, ulong practitionerId, CancellationToken ct)
    {
        var entity = await _db.Pets.FirstOrDefaultAsync(p => p.PetId == petId, ct);
        if (entity is null)
        {
            return false;
        }

        entity.OwnerId = request.OwnerId;
        entity.Name = request.Name.Trim();
        entity.Age = request.Age;
        entity.DateOfBirth = request.DateOfBirth;
        entity.Breed = RecordNormalization.NullIfWhiteSpace(request.Breed);
        entity.Sex = RecordNormalization.NormalizeSex(request.Sex);
        entity.Weight = request.Weight;
        entity.IsActive = request.IsActive;
        if (_accessor.PractitionerId > 0)
        {
            entity.UpdatedByPractitionerId = _accessor.PractitionerId;
            entity.UpdatedByPractitionerName = _accessor.PractitionerName;
        }

        await _db.SaveChangesAsync(ct);

        var assignmentExists = await _db.PractitionerPets.AnyAsync(
            p => p.PetId == petId && p.PractitionerId == practitionerId && p.AssignedTo == null,
            ct);
        if (!assignmentExists)
        {
            _db.PractitionerPets.Add(new PractitionerPet
            {
                PractitionerId = practitionerId,
                PetId = petId,
                AssignedFrom = DateTime.UtcNow,
                IsPrimary = false,
                Status = "active"
            });
            await _db.SaveChangesAsync(ct);
        }

        return true;
    }

    public async Task<PetDeleteResult> DeleteAsync(ulong petId, ulong practitionerId, CancellationToken ct)
    {
        var pet = await _db.Pets
            .Include(p => p.Registrationcodes)
            .Include(p => p.PractitionerPets)
            .FirstOrDefaultAsync(p => p.PetId == petId, ct);

        if (pet is null)
        {
            return PetDeleteResult.NotFound;
        }

        // Step 1: Get all programmes for this pet to delete associated PDF files.
        // NOTE: do NOT Include(p => p.Programmeversions); that joins the large PayloadJson
        // column and forces a MySQL filesort which exhausts sort_buffer_size on small server
        // tiers ("Out of sort memory"). We only need IDs, so fetch version ids separately.
        var programmes = await _db.Programmes
            .Where(p => p.TreatmentCase.PetId == petId)
            .ToListAsync(ct);

        // Step 2: Collect stored PDF prefixes. Blobs are deleted only after the database
        // delete has committed (below), so a DB failure cannot orphan-delete PDFs.
        var programmePdfPrefixes = programmes
            .Select(p => $"programme-{p.ProgrammeId}-")
            .ToList();

        var programmeIds = programmes.Select(p => p.ProgrammeId).ToList();
        var versionIds = programmeIds.Count > 0
            ? await _db.Programmeversions
                .Where(v => programmeIds.Contains(v.ProgrammeId))
                .Select(v => v.ProgrammeVersionId)
                .ToListAsync(ct)
            : new List<ulong>();

        if (versionIds.Count > 0)
        {
            var sessionSkips = await _db.Sessionskips
                .Where(x => versionIds.Contains(x.SessionOccurrence.ProgrammeVersionId))
                .ToListAsync(ct);
            _db.Sessionskips.RemoveRange(sessionSkips);

            var exerciseCompletions = await _db.Exercisecompletions
                .Where(x => versionIds.Contains(x.SessionOccurrence.ProgrammeVersionId))
                .ToListAsync(ct);
            _db.Exercisecompletions.RemoveRange(exerciseCompletions);

            var sessionOccurrences = await _db.Sessionoccurrences
                .Where(x => versionIds.Contains(x.ProgrammeVersionId))
                .ToListAsync(ct);
            _db.Sessionoccurrences.RemoveRange(sessionOccurrences);
        }

        if (programmeIds.Count > 0)
        {
            var sessionExercises = await _db.Sessionexercises
                .Where(x => programmeIds.Contains(x.Session.ProgrammeId))
                .ToListAsync(ct);
            _db.Sessionexercises.RemoveRange(sessionExercises);

            var sessions = await _db.Sessions
                .Where(x => programmeIds.Contains(x.ProgrammeId))
                .ToListAsync(ct);
            _db.Sessions.RemoveRange(sessions);
        }

        // Session occurrences (deleted above) are the only rows that RESTRICT deleting a
        // ProgrammeVersion, so the versions can now be removed. Do NOT delete them via EF:
        // Programme.CurrentProgrammeVersionId -> ProgrammeVersion and
        // ProgrammeVersion.ProgrammeId -> Programme form a cycle EF cannot order when both
        // ends are Deleted. Instead we delete the Programme and let MySQL's
        // FK_ProgrammeVersion_Programme (ON DELETE CASCADE) remove the version rows.
        _db.Programmes.RemoveRange(programmes);

        // Step 3: Delete treatment case notes and treatment cases
        var treatmentCases = await _db.Treatmentcases
            .Where(tc => tc.PetId == petId)
            .Include(tc => tc.Treatmentcasenotes)
            .ToListAsync(ct);

        foreach (var tc in treatmentCases)
        {
            _db.Treatmentcasenotes.RemoveRange(tc.Treatmentcasenotes);
        }

        _db.Treatmentcases.RemoveRange(treatmentCases);

        // Step 4: Delete RegistrationCode records
        _db.Registrationcodes.RemoveRange(pet.Registrationcodes);

        // Step 5: Delete PractitionerPet records
        _db.PractitionerPets.RemoveRange(pet.PractitionerPets);

        // Step 6: Delete Pet
        _db.Pets.Remove(pet);

        await _db.SaveChangesAsync(ct);

        // Stored PDFs are removed only after the database delete has committed.
        foreach (var prefix in programmePdfPrefixes)
        {
            await _fileStore.DeleteByPrefixAsync(prefix, ct);
        }

        return PetDeleteResult.Deleted;
    }
}

public sealed class TreatmentCaseRepository : ITreatmentCaseRepository
{
    private readonly CaninePhysioDbContext _db;
    private readonly HelloBuddy.Admin.Core.Identity.ICurrentPractitionerAccessor _accessor;

    public TreatmentCaseRepository(
        CaninePhysioDbContext db,
        HelloBuddy.Admin.Core.Identity.ICurrentPractitionerAccessor accessor)
    {
        _db = db;
        _accessor = accessor;
    }

    public async Task<IReadOnlyList<CaseRow>> ListAsync(ulong practitionerId, CancellationToken ct)
    {
        return await _db.Treatmentcases
            .OrderByDescending(tc => tc.StartDate)
            .Select(tc => new CaseRow(
                tc.TreatmentCaseId,
                tc.CaseTitle,
                tc.Status,
                tc.StartDate,
                tc.Pet.Name,
                tc.Pet.Owner.FirstName + " " + tc.Pet.Owner.LastName,
                tc.Practitioner.FirstName + " " + tc.Practitioner.LastName))
            .ToListAsync(ct);
    }

    public async Task<CaseDetailVm?> GetAsync(ulong treatmentCaseId, ulong practitionerId, CancellationToken ct)
    {
        var tc = await _db.Treatmentcases
            .Where(x => x.TreatmentCaseId == treatmentCaseId)
            .Select(x => new
            {
                x.TreatmentCaseId,
                x.CaseTitle,
                x.Status,
                x.StartDate,
                x.EndDate,
                x.ClinicalSummary,
                x.PetId,
                OwnerId = x.Pet.OwnerId,
                PetName = x.Pet.Name,
                x.Pet.Breed,
                x.Pet.Sex,
                x.Pet.Weight,
                x.Pet.Age,
                OwnerFirst = x.Pet.Owner.FirstName,
                OwnerLast = x.Pet.Owner.LastName,
                OwnerEmail = x.Pet.Owner.Email,
                PractitionerName = x.Practitioner.FirstName + " " + x.Practitioner.LastName,
                Notes = x.Treatmentcasenotes
                    .OrderByDescending(n => n.CreatedDate)
                    .Select(n => new CaseDetailVm.NoteRow(
                        n.TreatmentCaseNoteId,
                        n.CreatedDate,
                        n.NoteType,
                        n.NoteText))
                    .ToList(),
                Programmes = x.Programmes
                    .OrderByDescending(p => p.StartDate)
                    .Select(p => new CaseDetailVm.ProgrammeRow(
                        p.ProgrammeId,
                        p.ProgrammeName,
                        p.Status,
                        p.StartDate,
                        p.EndDate,
                        p.Sessions.Count,
                        p.Sessions.SelectMany(s => s.Sessionexercises).Count(),
                        p.Programmeversions.Any()))
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (tc is null)
        {
            return null;
        }

        return new CaseDetailVm(
            tc.TreatmentCaseId,
            tc.CaseTitle,
            tc.Status,
            tc.StartDate,
            tc.EndDate,
            tc.ClinicalSummary,
            tc.PetId,
            tc.OwnerId,
            tc.PetName,
            tc.Breed,
            tc.Sex,
            tc.Weight,
            tc.Age,
            $"{tc.OwnerFirst} {tc.OwnerLast}",
            tc.OwnerEmail,
            tc.PractitionerName,
            tc.Notes,
            tc.Programmes);
    }

    public async Task<ulong> CreateAsync(SaveTreatmentCaseRequest request, ulong practitionerId, CancellationToken ct)
    {
        var entity = new Treatmentcase
        {
            PetId = request.PetId,
            PractitionerId = practitionerId,
            CaseTitle = request.CaseTitle.Trim(),
            ClinicalSummary = RecordNormalization.NullIfWhiteSpace(request.ClinicalSummary),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = RecordNormalization.NormalizeStatus(request.Status),
            CreatedByPractitionerId = _accessor.PractitionerId > 0 ? _accessor.PractitionerId : null,
            CreatedByPractitionerName = _accessor.PractitionerId > 0 ? _accessor.PractitionerName : null,
        };

        _db.Treatmentcases.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.TreatmentCaseId;
    }

    public async Task<bool> UpdateAsync(ulong treatmentCaseId, SaveTreatmentCaseRequest request, ulong practitionerId, CancellationToken ct)
    {
        var entity = await _db.Treatmentcases
            .FirstOrDefaultAsync(tc => tc.TreatmentCaseId == treatmentCaseId,
                ct);
        if (entity is null)
        {
            return false;
        }

        entity.PetId = request.PetId;
        entity.CaseTitle = request.CaseTitle.Trim();
        entity.ClinicalSummary = RecordNormalization.NullIfWhiteSpace(request.ClinicalSummary);
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Status = RecordNormalization.NormalizeStatus(request.Status);
        if (_accessor.PractitionerId > 0)
        {
            entity.UpdatedByPractitionerId = _accessor.PractitionerId;
            entity.UpdatedByPractitionerName = _accessor.PractitionerName;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<CaseDetailVm.NoteRow?> AddNoteAsync(ulong treatmentCaseId, CreateCaseNoteRequest request, ulong practitionerId, CancellationToken ct)
    {
        var treatmentCaseExists = await _db.Treatmentcases.AnyAsync(
            tc => tc.TreatmentCaseId == treatmentCaseId,
            ct);
        if (!treatmentCaseExists)
        {
            return null;
        }

        var entity = new Treatmentcasenote
        {
            TreatmentCaseId = treatmentCaseId,
            PractitionerId = practitionerId,
            NoteType = RecordNormalization.NullIfWhiteSpace(request.NoteType),
            NoteText = request.NoteText.Trim(),
            IsActive = true,
            CreatedByPractitionerName = _accessor.PractitionerId > 0 ? _accessor.PractitionerName : null,
        };

        _db.Treatmentcasenotes.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new CaseDetailVm.NoteRow(
            entity.TreatmentCaseNoteId,
            entity.CreatedDate,
            entity.NoteType,
            entity.NoteText);
    }

    public async Task<CaseDetailVm.NoteRow?> UpdateNoteAsync(ulong treatmentCaseId, ulong noteId, CreateCaseNoteRequest request, ulong practitionerId, CancellationToken ct)
    {
        var entity = await _db.Treatmentcasenotes
            .FirstOrDefaultAsync(
                n => n.TreatmentCaseNoteId == noteId
                    && n.TreatmentCaseId == treatmentCaseId,
                ct);
        if (entity is null)
        {
            return null;
        }

        entity.NoteType = RecordNormalization.NullIfWhiteSpace(request.NoteType);
        entity.NoteText = request.NoteText.Trim();

        await _db.SaveChangesAsync(ct);

        return new CaseDetailVm.NoteRow(
            entity.TreatmentCaseNoteId,
            entity.CreatedDate,
            entity.NoteType,
            entity.NoteText);
    }

    public async Task<bool> DeleteNoteAsync(ulong treatmentCaseId, ulong noteId, ulong practitionerId, CancellationToken ct)
    {
        var entity = await _db.Treatmentcasenotes
            .FirstOrDefaultAsync(
                n => n.TreatmentCaseNoteId == noteId
                    && n.TreatmentCaseId == treatmentCaseId,
                ct);
        if (entity is null)
        {
            return false;
        }

        _db.Treatmentcasenotes.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

internal static class RecordNormalization
{
    private static readonly HashSet<string> ValidStatuses = ["planned", "active", "completed", "cancelled"];
    private static readonly HashSet<string> ValidSexes = ["male", "female", "unknown"];

    public static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static string NormalizeStatus(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? "planned";
        return ValidStatuses.Contains(normalized) ? normalized : "planned";
    }

    public static string NormalizeSex(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? "unknown";
        return ValidSexes.Contains(normalized) ? normalized : "unknown";
    }
}