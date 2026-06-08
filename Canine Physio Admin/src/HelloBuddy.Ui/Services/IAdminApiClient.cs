using HelloBuddy.Contracts;
using System.IO;

namespace HelloBuddy.Ui.Services;

/// <summary>
/// Thin typed client over the HelloBuddy API service. Read/write methods
/// map 1:1 onto the API endpoints. The <see cref="PractitionerHeaderHandler"/>
/// delegating handler injects the X-Practitioner-Id header on every call.
/// </summary>
public interface IAdminApiClient
{
    Task<IReadOnlyList<CaseRow>> ListCasesAsync(CancellationToken ct);
    Task<CaseDetailVm?> GetCaseAsync(ulong id, CancellationToken ct);
    Task<CaseDetailVm> CreateCaseAsync(SaveTreatmentCaseRequest request, CancellationToken ct);
    Task<CaseDetailVm?> UpdateCaseAsync(ulong id, SaveTreatmentCaseRequest request, CancellationToken ct);
    Task<CaseDetailVm.NoteRow?> AddCaseNoteAsync(ulong id, CreateCaseNoteRequest request, CancellationToken ct);
    Task<IReadOnlyList<OwnerListItem>> ListOwnersAsync(CancellationToken ct);
    Task<OwnerDetailVm?> GetOwnerAsync(ulong id, CancellationToken ct);
    Task<OwnerDetailVm> CreateOwnerAsync(SaveOwnerRequest request, CancellationToken ct);
    Task<OwnerDetailVm?> UpdateOwnerAsync(ulong id, SaveOwnerRequest request, CancellationToken ct);
    Task<OwnerDataControlClientResult> ApplyOwnerDataControlAsync(ulong id, CancellationToken ct);
    Task<IReadOnlyList<PetListItem>> ListPetsAsync(CancellationToken ct);
    Task<PetDetailVm?> GetPetAsync(ulong id, CancellationToken ct);
    Task<PetDetailVm> CreatePetAsync(SavePetRequest request, CancellationToken ct);
    Task<PetDetailVm?> UpdatePetAsync(ulong id, SavePetRequest request, CancellationToken ct);
    Task<IReadOnlyList<ExerciseListItem>> ListExercisesAsync(ExerciseListFilter filter, CancellationToken ct);
    Task<ExerciseDetailVm?> GetExerciseAsync(ulong id, CancellationToken ct);
    Task<ExerciseImageContent?> GetExerciseImageAsync(ulong id, CancellationToken ct);
    Task<ExerciseMediaUploadResponse> UploadExerciseImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct);
    Task<ExerciseDetailVm> CreateExerciseAsync(SaveExerciseRequest request, CancellationToken ct);
    Task<ExerciseDetailVm?> UpdateExerciseAsync(ulong id, SaveExerciseRequest request, CancellationToken ct);
    Task<ExerciseDetailVm?> SetExerciseActiveAsync(ulong id, bool isActive, CancellationToken ct);
    Task<IReadOnlyList<ExerciseCategoryListItem>> ListExerciseCategoriesAsync(CancellationToken ct);
    Task<ProgrammeVm?> CreateDraftProgrammeAsync(ulong caseId, CancellationToken ct);
    Task<DeleteProgrammeResult> DeleteProgrammeAsync(ulong programmeId, CancellationToken ct);
    Task<ProgrammeStatusTransitionClientResult> ActivateProgrammeAsync(ulong programmeId, CancellationToken ct);
    Task<ProgrammeStatusTransitionClientResult> CompleteProgrammeAsync(ulong programmeId, CancellationToken ct);
    Task<UpdateProgrammeStructureResult> UpdateProgrammeStructureAsync(ulong programmeId, ProgrammeStructureForm form, CancellationToken ct);
    Task<AddSessionExerciseClientResult> AddSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong exerciseId, CancellationToken ct);
    Task<RemoveSessionExerciseClientResult> RemoveSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong sessionExerciseId, CancellationToken ct);
    Task<ProgrammeVm?> GetProgrammeAsync(ulong id, CancellationToken ct);
    Task<ProgrammeVersionHistoryVm?> GetProgrammeVersionHistoryAsync(ulong id, CancellationToken ct);
    Task<CreateDraftFromPublishedClientResult> CreateDraftFromPublishedAsync(ulong id, CancellationToken ct);
    Task<UpdateProgrammeResult> UpdateProgrammeAsync(ulong id, ProgrammeBuilderForm form, CancellationToken ct);
    Task<PublishResponse> PublishProgrammeAsync(ulong id, CancellationToken ct);
    Task<DownloadUrlResponse> GetDownloadUrlAsync(string fileName, CancellationToken ct);
}

public sealed record ExerciseImageContent(byte[] Bytes, string ContentType);

public enum DeleteProgrammeOutcome
{
    Deleted,
    NotFound,
    Blocked,
}

public sealed record DeleteProgrammeResult(DeleteProgrammeOutcome Outcome, string? Message = null);

public enum OwnerDataControlClientOutcome
{
    Deleted,
    Anonymised,
    NotFound,
}

public sealed record OwnerDataControlClientResult(OwnerDataControlClientOutcome Outcome, string Message, OwnerDetailVm? Owner = null);

public enum ProgrammeStatusTransitionClientOutcome
{
    Updated,
    NotFound,
    Invalid,
    Blocked,
}

public sealed record ProgrammeStatusTransitionClientResult(ProgrammeStatusTransitionClientOutcome Outcome, string? Message = null);

public enum UpdateProgrammeStructureOutcome
{
    Updated,
    NotFound,
    Invalid,
    Blocked,
}

public sealed record UpdateProgrammeStructureResult(UpdateProgrammeStructureOutcome Outcome, string? Message = null);

public enum UpdateProgrammeOutcome
{
    Updated,
    NotFound,
    Blocked,
}

public sealed record UpdateProgrammeResult(UpdateProgrammeOutcome Outcome, ProgrammeVm? Programme = null, string? Message = null);

public enum AddSessionExerciseClientOutcome
{
    Added,
    NotFound,
    Duplicate,
    Blocked,
}

public sealed record AddSessionExerciseClientResult(AddSessionExerciseClientOutcome Outcome, string? Message = null);

public enum RemoveSessionExerciseClientOutcome
{
    Removed,
    NotFound,
    Blocked,
}

public sealed record RemoveSessionExerciseClientResult(RemoveSessionExerciseClientOutcome Outcome, string? Message = null);

public enum CreateDraftFromPublishedClientOutcome
{
    Created,
    NotFound,
    Invalid,
}

public sealed record CreateDraftFromPublishedClientResult(CreateDraftFromPublishedClientOutcome Outcome, ProgrammeVm? Programme = null, string? Message = null);
