using HelloBuddy.Contracts;

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
    Task<IReadOnlyList<PetListItem>> ListPetsAsync(CancellationToken ct);
    Task<PetDetailVm?> GetPetAsync(ulong id, CancellationToken ct);
    Task<PetDetailVm> CreatePetAsync(SavePetRequest request, CancellationToken ct);
    Task<PetDetailVm?> UpdatePetAsync(ulong id, SavePetRequest request, CancellationToken ct);
    Task<ProgrammeVm?> GetProgrammeAsync(ulong id, CancellationToken ct);
    Task<ProgrammeVm?> UpdateProgrammeAsync(ulong id, ProgrammeBuilderForm form, CancellationToken ct);
    Task<PublishResponse> PublishProgrammeAsync(ulong id, CancellationToken ct);
    Task<DownloadUrlResponse> GetDownloadUrlAsync(string fileName, CancellationToken ct);
}
