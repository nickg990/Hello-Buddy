using System.Net;
using System.Net.Http.Json;
using HelloBuddy.Contracts;
using Microsoft.AspNetCore.Mvc;

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

    public async Task<IReadOnlyList<OwnerListItem>> ListOwnersAsync(CancellationToken ct)
    {
        var rows = await _http.GetFromJsonAsync<List<OwnerListItem>>("/api/owners", ct);
        return rows ?? new List<OwnerListItem>();
    }

    public async Task<OwnerDetailVm?> GetOwnerAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/api/owners/{id}", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<OwnerDetailVm>(resp, ct);
    }

    public async Task<OwnerDetailVm> CreateOwnerAsync(SaveOwnerRequest request, CancellationToken ct)
    {
        var resp = await _http.PostAsJsonAsync("/api/owners", request, ct);
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<OwnerDetailVm>(resp, ct);
    }

    public async Task<OwnerDetailVm?> UpdateOwnerAsync(ulong id, SaveOwnerRequest request, CancellationToken ct)
    {
        var resp = await _http.PutAsJsonAsync($"/api/owners/{id}", request, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<OwnerDetailVm>(resp, ct);
    }

    public async Task<IReadOnlyList<PetListItem>> ListPetsAsync(CancellationToken ct)
    {
        var rows = await _http.GetFromJsonAsync<List<PetListItem>>("/api/pets", ct);
        return rows ?? new List<PetListItem>();
    }

    public async Task<PetDetailVm?> GetPetAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/api/pets/{id}", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<PetDetailVm>(resp, ct);
    }

    public async Task<PetDetailVm> CreatePetAsync(SavePetRequest request, CancellationToken ct)
    {
        var resp = await _http.PostAsJsonAsync("/api/pets", request, ct);
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<PetDetailVm>(resp, ct);
    }

    public async Task<PetDetailVm?> UpdatePetAsync(ulong id, SavePetRequest request, CancellationToken ct)
    {
        var resp = await _http.PutAsJsonAsync($"/api/pets/{id}", request, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<PetDetailVm>(resp, ct);
    }

    public async Task<CaseDetailVm?> GetCaseAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/api/cases/{id}", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<CaseDetailVm>(resp, ct);
    }

    public async Task<CaseDetailVm> CreateCaseAsync(SaveTreatmentCaseRequest request, CancellationToken ct)
    {
        var resp = await _http.PostAsJsonAsync("/api/cases", request, ct);
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<CaseDetailVm>(resp, ct);
    }

    public async Task<CaseDetailVm?> UpdateCaseAsync(ulong id, SaveTreatmentCaseRequest request, CancellationToken ct)
    {
        var resp = await _http.PutAsJsonAsync($"/api/cases/{id}", request, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<CaseDetailVm>(resp, ct);
    }

    public async Task<CaseDetailVm.NoteRow?> AddCaseNoteAsync(ulong id, CreateCaseNoteRequest request, CancellationToken ct)
    {
        var resp = await _http.PostAsJsonAsync($"/api/cases/{id}/notes", request, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<CaseDetailVm.NoteRow>(resp, ct);
    }

    public async Task<ProgrammeVm?> GetProgrammeAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/api/programmes/{id}", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<ProgrammeVm>(resp, ct);
    }

    public async Task<ProgrammeVm?> UpdateProgrammeAsync(ulong id, ProgrammeBuilderForm form, CancellationToken ct)
    {
        var resp = await _http.PutAsJsonAsync($"/api/programmes/{id}", form, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<ProgrammeVm>(resp, ct);
    }

    public async Task<PublishResponse> PublishProgrammeAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.PostAsync($"/api/programmes/{id}/publish", content: null, ct);
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<PublishResponse>(resp, ct);
    }

    public async Task<DownloadUrlResponse> GetDownloadUrlAsync(string fileName, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/api/programmes/published/{Uri.EscapeDataString(fileName)}/download-url", ct);
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<DownloadUrlResponse>(resp, ct);
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: ct);
            if (problem?.Errors?.Count > 0)
            {
                throw new ApiValidationException(problem.Errors);
            }
        }

        response.EnsureSuccessStatusCode();
    }

    private static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct)
            ?? throw new InvalidOperationException($"Empty response for {typeof(T).Name}.");
    }
}
