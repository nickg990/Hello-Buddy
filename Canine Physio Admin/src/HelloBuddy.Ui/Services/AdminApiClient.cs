using System.Net;
using System.Net.Http.Json;
using HelloBuddy.Contracts;

namespace HelloBuddy.Ui.Services;

public sealed class AdminApiClient : IAdminApiClient
{
    private readonly HttpClient _http;

    public AdminApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<CaseRow>> ListCasesAsync(CancellationToken ct)
    {
        var rows = await _http.GetFromJsonAsync<List<CaseRow>>("/api/cases", ct);
        return rows ?? new List<CaseRow>();
    }

    public async Task<CaseDetailVm?> GetCaseAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/api/cases/{id}", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<CaseDetailVm>(cancellationToken: ct);
    }

    public async Task<ProgrammeVm?> GetProgrammeAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/api/programmes/{id}", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ProgrammeVm>(cancellationToken: ct);
    }

    public async Task<ProgrammeVm?> UpdateProgrammeAsync(ulong id, ProgrammeBuilderForm form, CancellationToken ct)
    {
        var resp = await _http.PutAsJsonAsync($"/api/programmes/{id}", form, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ProgrammeVm>(cancellationToken: ct);
    }

    public async Task<PublishResponse> PublishProgrammeAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.PostAsync($"/api/programmes/{id}/publish", content: null, ct);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<PublishResponse>(cancellationToken: ct))
            ?? throw new InvalidOperationException("Empty publish response.");
    }

    public async Task<DownloadUrlResponse> GetDownloadUrlAsync(string fileName, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/api/programmes/published/{Uri.EscapeDataString(fileName)}/download-url", ct);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<DownloadUrlResponse>(cancellationToken: ct))
            ?? throw new InvalidOperationException("Empty download-url response.");
    }
}
