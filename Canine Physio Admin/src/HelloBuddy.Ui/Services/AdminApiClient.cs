using System.Net;
using System.Net.Http.Headers;
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

    public async Task<IReadOnlyList<OwnerListItem>> ListOwnersAsync(bool includeAnonymised, CancellationToken ct)
    {
        var path = includeAnonymised ? "/api/owners?includeAnonymised=true" : "/api/owners";
        var rows = await _http.GetFromJsonAsync<List<OwnerListItem>>(path, ct);
        return rows ?? new List<OwnerListItem>();
    }

    public async Task<OwnerDetailVm?> GetOwnerAsync(ulong id, bool includeAnonymised, CancellationToken ct)
    {
        var path = includeAnonymised ? $"/api/owners/{id}?includeAnonymised=true" : $"/api/owners/{id}";
        var resp = await _http.GetAsync(path, ct);
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

    public async Task<OwnerDataControlClientResult> ApplyOwnerDataControlAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.PostAsync($"/api/owners/{id}/data-control", content: null, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return new OwnerDataControlClientResult(OwnerDataControlClientOutcome.NotFound, "Owner was not found.");
        }

        await EnsureSuccessOrThrowAsync(resp, ct);
        var payload = await ReadRequiredAsync<OwnerDataControlResponse>(resp, ct);
        if (payload.Outcome.Equals("deleted", StringComparison.OrdinalIgnoreCase))
        {
            return new OwnerDataControlClientResult(OwnerDataControlClientOutcome.Deleted, payload.Message);
        }

        if (payload.Outcome.Equals("anonymised", StringComparison.OrdinalIgnoreCase))
        {
            return new OwnerDataControlClientResult(OwnerDataControlClientOutcome.Anonymised, payload.Message, payload.Owner);
        }

        return new OwnerDataControlClientResult(OwnerDataControlClientOutcome.NotFound, payload.Message);
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

    public async Task<IReadOnlyList<ExerciseListItem>> ListExercisesAsync(ExerciseListFilter filter, CancellationToken ct)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            query.Add($"searchText={Uri.EscapeDataString(filter.SearchText.Trim())}");
        }

        if (filter.CategoryId.HasValue)
        {
            query.Add($"categoryId={filter.CategoryId.Value}");
        }

        if (filter.HasVideo.HasValue)
        {
            query.Add($"hasVideo={filter.HasVideo.Value.ToString().ToLowerInvariant()}");
        }

        query.Add($"activeOnly={filter.ActiveOnly.ToString().ToLowerInvariant()}");

        var path = "/api/exercises";
        if (query.Count > 0)
        {
            path += "?" + string.Join("&", query);
        }

        var rows = await _http.GetFromJsonAsync<List<ExerciseListItem>>(path, ct);
        return rows ?? new List<ExerciseListItem>();
    }

    public async Task<ExerciseDetailVm?> GetExerciseAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/api/exercises/{id}", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<ExerciseDetailVm>(resp, ct);
    }

    public async Task<ExerciseImageContent?> GetExerciseImageAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/api/exercises/{id}/image", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);

        var bytes = await resp.Content.ReadAsByteArrayAsync(ct);
        var contentType = resp.Content.Headers.ContentType?.MediaType;
        return new ExerciseImageContent(bytes, string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
    }

    public async Task<ExerciseMediaUploadResponse> UploadExerciseImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", fileName);

        var resp = await _http.PostAsync("/api/exercises/media", form, ct);
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<ExerciseMediaUploadResponse>(resp, ct);
    }

    public async Task<ExerciseDetailVm> CreateExerciseAsync(SaveExerciseRequest request, CancellationToken ct)
    {
        var resp = await _http.PostAsJsonAsync("/api/exercises", request, ct);
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<ExerciseDetailVm>(resp, ct);
    }

    public async Task<ExerciseDetailVm?> UpdateExerciseAsync(ulong id, SaveExerciseRequest request, CancellationToken ct)
    {
        var resp = await _http.PutAsJsonAsync($"/api/exercises/{id}", request, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<ExerciseDetailVm>(resp, ct);
    }

    public async Task<ExerciseDetailVm?> SetExerciseActiveAsync(ulong id, bool isActive, CancellationToken ct)
    {
        var endpoint = isActive ? "activate" : "deactivate";
        var resp = await _http.PostAsync($"/api/exercises/{id}/{endpoint}", content: null, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<ExerciseDetailVm>(resp, ct);
    }

    public async Task<IReadOnlyList<ExerciseCategoryListItem>> ListExerciseCategoriesAsync(CancellationToken ct)
    {
        var rows = await _http.GetFromJsonAsync<List<ExerciseCategoryListItem>>("/api/exercise-categories", ct);
        return rows ?? new List<ExerciseCategoryListItem>();
    }

    public async Task<ProgrammeVm?> CreateDraftProgrammeAsync(ulong caseId, CancellationToken ct)
    {
        var resp = await _http.PostAsync($"/api/cases/{caseId}/programmes", content: null, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<ProgrammeVm>(resp, ct);
    }

    public async Task<DeleteProgrammeResult> DeleteProgrammeAsync(ulong programmeId, CancellationToken ct)
    {
        var resp = await _http.DeleteAsync($"/api/programmes/{programmeId}", ct);
        if (resp.StatusCode == HttpStatusCode.NoContent)
        {
            return new DeleteProgrammeResult(DeleteProgrammeOutcome.Deleted);
        }

        if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return new DeleteProgrammeResult(DeleteProgrammeOutcome.NotFound, "Programme was not found.");
        }

        if (resp.StatusCode == HttpStatusCode.Conflict)
        {
            var message = await resp.Content.ReadAsStringAsync(ct);
            return new DeleteProgrammeResult(
                DeleteProgrammeOutcome.Blocked,
                string.IsNullOrWhiteSpace(message) ? "Programme cannot be deleted." : message);
        }

        await EnsureSuccessOrThrowAsync(resp, ct);
        return new DeleteProgrammeResult(DeleteProgrammeOutcome.Deleted);
    }

    public async Task<ProgrammeStatusTransitionClientResult> ActivateProgrammeAsync(ulong programmeId, CancellationToken ct)
        => await PostProgrammeStatusTransitionAsync($"/api/programmes/{programmeId}/activate", ct);

    public async Task<ProgrammeStatusTransitionClientResult> CompleteProgrammeAsync(ulong programmeId, CancellationToken ct)
        => await PostProgrammeStatusTransitionAsync($"/api/programmes/{programmeId}/complete", ct);

    public async Task<UpdateProgrammeStructureResult> UpdateProgrammeStructureAsync(ulong programmeId, ProgrammeStructureForm form, CancellationToken ct)
    {
        var resp = await _http.PutAsJsonAsync($"/api/programmes/{programmeId}/structure", form, ct);
        if (resp.StatusCode == HttpStatusCode.NoContent)
        {
            return new UpdateProgrammeStructureResult(UpdateProgrammeStructureOutcome.Updated);
        }

        if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return new UpdateProgrammeStructureResult(UpdateProgrammeStructureOutcome.NotFound, "Programme was not found.");
        }

        if (resp.StatusCode == HttpStatusCode.BadRequest)
        {
            var message = await resp.Content.ReadAsStringAsync(ct);
            return new UpdateProgrammeStructureResult(UpdateProgrammeStructureOutcome.Invalid, message);
        }

        if (resp.StatusCode == HttpStatusCode.Conflict)
        {
            var message = await resp.Content.ReadAsStringAsync(ct);
            return new UpdateProgrammeStructureResult(
                UpdateProgrammeStructureOutcome.Blocked,
                string.IsNullOrWhiteSpace(message) ? "Published programmes are immutable." : message);
        }

        await EnsureSuccessOrThrowAsync(resp, ct);
        return new UpdateProgrammeStructureResult(UpdateProgrammeStructureOutcome.Updated);
    }

    public async Task<AddSessionExerciseClientResult> AddSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong exerciseId, CancellationToken ct)
    {
        var resp = await _http.PostAsJsonAsync(
            $"/api/programmes/{programmeId}/sessions/{sessionId}/exercises",
            new AddSessionExerciseRequest { ExerciseId = exerciseId },
            ct);

        if (resp.StatusCode == HttpStatusCode.NoContent)
        {
            return new AddSessionExerciseClientResult(AddSessionExerciseClientOutcome.Added);
        }

        if (resp.StatusCode == HttpStatusCode.Conflict)
        {
            var message = await resp.Content.ReadAsStringAsync(ct);
            if (message.Contains("immutable", StringComparison.OrdinalIgnoreCase))
            {
                return new AddSessionExerciseClientResult(AddSessionExerciseClientOutcome.Blocked, message);
            }

            return new AddSessionExerciseClientResult(AddSessionExerciseClientOutcome.Duplicate, message);
        }

        if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return new AddSessionExerciseClientResult(AddSessionExerciseClientOutcome.NotFound, "Session or exercise not found.");
        }

        await EnsureSuccessOrThrowAsync(resp, ct);
        return new AddSessionExerciseClientResult(AddSessionExerciseClientOutcome.Added);
    }

    public async Task<RemoveSessionExerciseClientResult> RemoveSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong sessionExerciseId, CancellationToken ct)
    {
        var resp = await _http.DeleteAsync($"/api/programmes/{programmeId}/sessions/{sessionId}/exercises/{sessionExerciseId}", ct);
        if (resp.StatusCode == HttpStatusCode.NoContent)
        {
            return new RemoveSessionExerciseClientResult(RemoveSessionExerciseClientOutcome.Removed);
        }

        if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return new RemoveSessionExerciseClientResult(RemoveSessionExerciseClientOutcome.NotFound, "Session exercise not found.");
        }

        if (resp.StatusCode == HttpStatusCode.Conflict)
        {
            var message = await resp.Content.ReadAsStringAsync(ct);
            return new RemoveSessionExerciseClientResult(
                RemoveSessionExerciseClientOutcome.Blocked,
                string.IsNullOrWhiteSpace(message) ? "Published programmes are immutable." : message);
        }

        await EnsureSuccessOrThrowAsync(resp, ct);
        return new RemoveSessionExerciseClientResult(RemoveSessionExerciseClientOutcome.Removed);
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

    public async Task<ProgrammeVersionHistoryVm?> GetProgrammeVersionHistoryAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/api/programmes/{id}/versions", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessOrThrowAsync(resp, ct);
        return await ReadRequiredAsync<ProgrammeVersionHistoryVm>(resp, ct);
    }

    public async Task<CreateDraftFromPublishedClientResult> CreateDraftFromPublishedAsync(ulong id, CancellationToken ct)
    {
        var resp = await _http.PostAsync($"/api/programmes/{id}/draft-from-published", content: null, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return new CreateDraftFromPublishedClientResult(CreateDraftFromPublishedClientOutcome.NotFound, Message: "Programme was not found.");
        }

        if (resp.StatusCode == HttpStatusCode.Conflict)
        {
            var message = await resp.Content.ReadAsStringAsync(ct);
            return new CreateDraftFromPublishedClientResult(
                CreateDraftFromPublishedClientOutcome.Invalid,
                Message: string.IsNullOrWhiteSpace(message)
                    ? "No published version is available to branch from."
                    : message);
        }

        await EnsureSuccessOrThrowAsync(resp, ct);
        var programme = await ReadRequiredAsync<ProgrammeVm>(resp, ct);
        return new CreateDraftFromPublishedClientResult(CreateDraftFromPublishedClientOutcome.Created, programme);
    }

    public async Task<UpdateProgrammeResult> UpdateProgrammeAsync(ulong id, ProgrammeBuilderForm form, CancellationToken ct)
    {
        var resp = await _http.PutAsJsonAsync($"/api/programmes/{id}", form, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return new UpdateProgrammeResult(UpdateProgrammeOutcome.NotFound);
        }

        if (resp.StatusCode == HttpStatusCode.Conflict)
        {
            var message = await resp.Content.ReadAsStringAsync(ct);
            return new UpdateProgrammeResult(
                UpdateProgrammeOutcome.Blocked,
                Message: string.IsNullOrWhiteSpace(message) ? "Published programmes are immutable." : message);
        }

        await EnsureSuccessOrThrowAsync(resp, ct);
        var programme = await ReadRequiredAsync<ProgrammeVm>(resp, ct);
        return new UpdateProgrammeResult(UpdateProgrammeOutcome.Updated, programme);
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

    private async Task<ProgrammeStatusTransitionClientResult> PostProgrammeStatusTransitionAsync(string path, CancellationToken ct)
    {
        var resp = await _http.PostAsync(path, content: null, ct);
        if (resp.StatusCode == HttpStatusCode.NoContent)
        {
            return new ProgrammeStatusTransitionClientResult(ProgrammeStatusTransitionClientOutcome.Updated);
        }

        if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return new ProgrammeStatusTransitionClientResult(ProgrammeStatusTransitionClientOutcome.NotFound, "Programme was not found.");
        }

        if (resp.StatusCode == HttpStatusCode.BadRequest)
        {
            var message = await resp.Content.ReadAsStringAsync(ct);
            return new ProgrammeStatusTransitionClientResult(
                ProgrammeStatusTransitionClientOutcome.Invalid,
                string.IsNullOrWhiteSpace(message) ? "Status transition is not allowed." : message);
        }

        if (resp.StatusCode == HttpStatusCode.Conflict)
        {
            var message = await resp.Content.ReadAsStringAsync(ct);
            return new ProgrammeStatusTransitionClientResult(
                ProgrammeStatusTransitionClientOutcome.Blocked,
                string.IsNullOrWhiteSpace(message) ? "Status transition is blocked." : message);
        }

        await EnsureSuccessOrThrowAsync(resp, ct);
        return new ProgrammeStatusTransitionClientResult(ProgrammeStatusTransitionClientOutcome.Updated);
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
