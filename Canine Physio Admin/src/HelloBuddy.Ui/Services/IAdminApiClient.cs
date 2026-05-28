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
    Task<ProgrammeVm?> GetProgrammeAsync(ulong id, CancellationToken ct);
    Task<ProgrammeVm?> UpdateProgrammeAsync(ulong id, ProgrammeBuilderForm form, CancellationToken ct);
    Task<PublishResponse> PublishProgrammeAsync(ulong id, CancellationToken ct);
    Task<DownloadUrlResponse> GetDownloadUrlAsync(string fileName, CancellationToken ct);
}
