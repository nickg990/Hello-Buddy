using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Data.Entities;
using HelloBuddy.Admin.Pdf;
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
        return services;
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

        var existingSteps = await _db.Exerciseinstructions
            .Where(x => x.ExerciseId == exerciseId)
            .ToListAsync(ct);
        _db.Exerciseinstructions.RemoveRange(existingSteps);
        await _db.SaveChangesAsync(ct);

        ApplyInstructionRows(exerciseId, request.Instructions);
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

        entity.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return true;
    }

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
        var owner = await _db.Owners
            .Include(o => o.Pets)
                .ThenInclude(p => p.PractitionerPets)
            .Include(o => o.Pets)
                .ThenInclude(p => p.Treatmentcases)
            .Include(o => o.Useraccounts)
            .FirstOrDefaultAsync(o => o.OwnerId == ownerId, ct);

        if (owner is null)
        {
            return OwnerDataControlResult.NotFound;
        }

        var hasNoLinkedData = owner.Pets.Count == 0 && owner.Useraccounts.Count == 0;
        var linkedToPractitioner = hasNoLinkedData || owner.Pets.Any(p =>
            p.PractitionerPets.Any(pp => pp.PractitionerId == practitionerId && pp.AssignedTo == null)
            || p.Treatmentcases.Any(tc => tc.PractitionerId == practitionerId));

        if (!linkedToPractitioner)
        {
            return OwnerDataControlResult.NotFound;
        }

        // Delete all related data in cascading order
        await DeleteOwnerAndRelatedDataAsync(ownerId, practitionerId, ct);

        AddOwnerGdprDeletionAudit(ownerId, practitionerId);
        await _db.SaveChangesAsync(ct);
        return OwnerDataControlResult.Deleted;
    }

    private async Task DeleteOwnerAndRelatedDataAsync(ulong ownerId, ulong practitionerId, CancellationToken ct)
    {
        // Step 1: Get all programmes for this owner to delete associated PDF files
        var programmes = await _db.Programmes
            .Where(p => p.TreatmentCase.Pet.OwnerId == ownerId)
            .Include(p => p.Programmeversions)
            .ToListAsync(ct);

        // Step 2: Delete PDF files from blob storage for deleted programmes.
        foreach (var programme in programmes)
        {
            await _fileStore.DeleteByPrefixAsync($"programme-{programme.ProgrammeId}-", ct);
        }

        var programmeIds = programmes.Select(p => p.ProgrammeId).ToList();
        var versionIds = programmes.SelectMany(p => p.Programmeversions).Select(v => v.ProgrammeVersionId).ToList();

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

        foreach (var programme in programmes)
        {
            programme.CurrentProgrammeVersionId = null;
        }

        var programmeVersions = programmes.SelectMany(p => p.Programmeversions).ToList();
        _db.Programmeversions.RemoveRange(programmeVersions);
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

    public PetRepository(
        CaninePhysioDbContext db,
        HelloBuddy.Admin.Core.Identity.ICurrentPractitionerAccessor accessor)
    {
        _db = db;
        _accessor = accessor;
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
                    .Where(tc => tc.PractitionerId == practitionerId)
                    .OrderByDescending(tc => tc.StartDate)
                    .Select(tc => new PetDetailVm.CaseRow(
                        tc.TreatmentCaseId,
                        tc.CaseTitle,
                        tc.Status,
                        tc.StartDate))
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
            .Where(tc => tc.PractitionerId == practitionerId
            )
            .OrderByDescending(tc => tc.StartDate)
            .Select(tc => new CaseRow(
                tc.TreatmentCaseId,
                tc.CaseTitle,
                tc.Status,
                tc.StartDate,
                tc.Pet.Name,
                tc.Pet.Owner.FirstName + " " + tc.Pet.Owner.LastName))
            .ToListAsync(ct);
    }

    public async Task<CaseDetailVm?> GetAsync(ulong treatmentCaseId, ulong practitionerId, CancellationToken ct)
    {
        var tc = await _db.Treatmentcases
            .Where(x => x.TreatmentCaseId == treatmentCaseId
                && x.PractitionerId == practitionerId)
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
            .FirstOrDefaultAsync(tc => tc.TreatmentCaseId == treatmentCaseId
                && tc.PractitionerId == practitionerId,
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
            tc => tc.TreatmentCaseId == treatmentCaseId
                && tc.PractitionerId == practitionerId
,
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
                    && n.TreatmentCaseId == treatmentCaseId
                    && n.TreatmentCase.PractitionerId == practitionerId,
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
                    && n.TreatmentCaseId == treatmentCaseId
                    && n.TreatmentCase.PractitionerId == practitionerId,
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