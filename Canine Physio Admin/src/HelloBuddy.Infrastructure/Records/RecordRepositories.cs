using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Data.Entities;
using HelloBuddy.Application.Programmes;
using HelloBuddy.Application.Records;
using HelloBuddy.Contracts;
using HelloBuddy.Infrastructure.Programmes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelloBuddy.Infrastructure.Records;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddHelloBuddyInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IOwnerRepository, OwnerRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<ITreatmentCaseRepository, TreatmentCaseRepository>();
        services.AddScoped<IProgrammeRepository, ProgrammeRepository>();
        return services;
    }
}

public sealed class OwnerRepository : IOwnerRepository
{
    private readonly CaninePhysioDbContext _db;

    public OwnerRepository(CaninePhysioDbContext db)
    {
        _db = db;
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
            Postcode = RecordNormalization.NullIfWhiteSpace(request.Postcode)
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

        await _db.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class PetRepository : IPetRepository
{
    private readonly CaninePhysioDbContext _db;

    public PetRepository(CaninePhysioDbContext db)
    {
        _db = db;
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
        return _db.Owners.AnyAsync(o => o.OwnerId == ownerId, ct);
    }

    public Task<bool> ExistsAsync(ulong petId, CancellationToken ct)
    {
        return _db.Pets.AnyAsync(p => p.PetId == petId, ct);
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
            IsActive = request.IsActive
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

    public TreatmentCaseRepository(CaninePhysioDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CaseRow>> ListAsync(ulong practitionerId, CancellationToken ct)
    {
        return await _db.Treatmentcases
            .Where(tc => tc.PractitionerId == practitionerId)
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
            .Where(x => x.TreatmentCaseId == treatmentCaseId && x.PractitionerId == practitionerId)
            .Select(x => new
            {
                x.TreatmentCaseId,
                x.CaseTitle,
                x.Status,
                x.StartDate,
                x.EndDate,
                x.ClinicalSummary,
                x.PetId,
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
                        p.Sessions.SelectMany(s => s.Sessionexercises).Count()))
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
            Status = RecordNormalization.NormalizeStatus(request.Status)
        };

        _db.Treatmentcases.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.TreatmentCaseId;
    }

    public async Task<bool> UpdateAsync(ulong treatmentCaseId, SaveTreatmentCaseRequest request, ulong practitionerId, CancellationToken ct)
    {
        var entity = await _db.Treatmentcases
            .FirstOrDefaultAsync(tc => tc.TreatmentCaseId == treatmentCaseId && tc.PractitionerId == practitionerId, ct);
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

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<CaseDetailVm.NoteRow?> AddNoteAsync(ulong treatmentCaseId, CreateCaseNoteRequest request, ulong practitionerId, CancellationToken ct)
    {
        var treatmentCaseExists = await _db.Treatmentcases.AnyAsync(
            tc => tc.TreatmentCaseId == treatmentCaseId && tc.PractitionerId == practitionerId,
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
            IsActive = true
        };

        _db.Treatmentcasenotes.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new CaseDetailVm.NoteRow(
            entity.TreatmentCaseNoteId,
            entity.CreatedDate,
            entity.NoteType,
            entity.NoteText);
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