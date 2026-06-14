using HelloBuddy.Admin.Pdf;
using HelloBuddy.Contracts;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HelloBuddy.Application.Programmes;

public sealed class ProgrammeService : IProgrammeService
{
    private readonly IProgrammeRepository _repository;
    private readonly IProgrammePdfTemplate _template;
    private readonly IPdfRenderer _pdfRenderer;
    private readonly IFileStore _fileStore;
    private readonly ILogger<ProgrammeService> _logger;

    public ProgrammeService(
        IProgrammeRepository repository,
        IProgrammePdfTemplate template,
        IPdfRenderer pdfRenderer,
        IFileStore fileStore,
        ILogger<ProgrammeService> logger)
    {
        _repository = repository;
        _template = template;
        _pdfRenderer = pdfRenderer;
        _fileStore = fileStore;
        _logger = logger;
    }

    public Task<ProgrammeVm?> GetAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
        => _repository.GetVmAsync(programmeId, practitionerId, ct);

    public Task<ProgrammeVm?> CreateDraftAsync(ulong treatmentCaseId, ulong practitionerId, CancellationToken ct)
        => _repository.CreateDraftAsync(treatmentCaseId, practitionerId, ct);

    public Task<bool> IsLockedForEditAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
        => _repository.IsLockedForEditAsync(programmeId, practitionerId, ct);

    public Task<ProgrammeVersionHistoryVm?> GetVersionHistoryAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
        => _repository.GetVersionHistoryAsync(programmeId, practitionerId, ct);

    public Task<ProgrammeVm?> CreateDraftFromPublishedAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
        => _repository.CreateDraftFromPublishedAsync(programmeId, practitionerId, ct);

    public async Task<ProgrammeVm?> UpdateAsync(ulong programmeId, ProgrammeBuilderForm form, ulong practitionerId, CancellationToken ct)
    {
        if (!await _repository.OwnsAsync(programmeId, practitionerId, ct))
        {
            return null;
        }

        await _repository.UpdateSessionObjectivesAsync(programmeId, practitionerId, form.Sessions, ct);
        await _repository.UpdateSessionExercisesAsync(programmeId, practitionerId, form.Exercises, ct);
        return await _repository.GetVmAsync(programmeId, practitionerId, ct);
    }

    public Task<DeleteProgrammeResult> DeleteAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
        => _repository.DeleteAsync(programmeId, practitionerId, ct);

    public Task<ProgrammeStructureUpdateResult> UpdateStructureAsync(ulong programmeId, ProgrammeStructureForm form, ulong practitionerId, CancellationToken ct)
        => _repository.UpdateStructureAsync(programmeId, practitionerId, form, ct);

    public Task<AddSessionExerciseResult> AddSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong exerciseId, ulong practitionerId, CancellationToken ct)
        => _repository.AddSessionExerciseAsync(
            programmeId: programmeId,
            practitionerId: practitionerId,
            sessionId: sessionId,
            exerciseId: exerciseId,
            ct: ct);

    public Task<RemoveSessionExerciseResult> RemoveSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong sessionExerciseId, ulong practitionerId, CancellationToken ct)
        => _repository.RemoveSessionExerciseAsync(programmeId, practitionerId, sessionId, sessionExerciseId, ct);

    public Task<ProgrammeStatusTransitionResult> ActivateAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
        => _repository.ActivateAsync(programmeId, practitionerId, ct);

    public Task<ProgrammeStatusTransitionResult> CompleteAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
        => _repository.CompleteAsync(programmeId, practitionerId, ct);

    public async Task<Dictionary<string, string[]>> ValidateDraftForPublishAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();
        var vm = await _repository.GetVmAsync(programmeId, practitionerId, ct);
        if (vm is null)
        {
            errors[nameof(programmeId)] = ["Programme was not found."];
            return errors;
        }

        if (vm.Sessions.Count == 0)
        {
            errors[nameof(vm.Sessions)] = ["Add at least one session before publishing."];
            return errors;
        }

        var totalExercises = vm.Sessions.Sum(s => s.Exercises.Count);
        if (totalExercises == 0)
        {
            errors[nameof(vm.Sessions)] = ["Add at least one exercise before publishing."];
            return errors;
        }

        foreach (var session in vm.Sessions)
        {
            if (string.IsNullOrWhiteSpace(session.Objective))
            {
                errors[$"Session[{session.SessionId}].Objective"] = ["Add a purpose summary for every session before publishing."];
            }

            foreach (var exercise in session.Exercises)
            {
                var keyPrefix = $"Session[{session.SessionId}]Exercise[{exercise.SessionExerciseId}]";
                if (!exercise.Reps.HasValue || exercise.Reps.Value == 0)
                {
                    errors[$"{keyPrefix}.Reps"] = ["Reps must be set to a value greater than zero."];
                }

                if (!exercise.Sets.HasValue || exercise.Sets.Value == 0)
                {
                    errors[$"{keyPrefix}.Sets"] = ["Sets must be set to a value greater than zero."];
                }

                if (exercise.HoldSeconds.HasValue && exercise.HoldSeconds.Value == 0)
                {
                    errors[$"{keyPrefix}.HoldSeconds"] = ["Hold seconds must be greater than zero when provided."];
                }
            }
        }

        return errors;
    }

    public async Task<PreviewPdfDocument?> RenderPreviewPdfAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        var vm = await _repository.GetVmAsync(programmeId, practitionerId, ct);
        if (vm is null)
        {
            return null;
        }

        var renderVm = await BuildRenderVmAsync(vm, ct);
        var html = await _template.RenderAsync(renderVm, ct);
        var pdf = await _pdfRenderer.RenderAsync(html, ct);
        var fileName = $"programme-preview-{programmeId}.pdf";
        return new PreviewPdfDocument(pdf, "application/pdf", fileName);
    }

    public async Task<PublishResponse?> PublishAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        var vm = await _repository.GetVmAsync(programmeId, practitionerId, ct);
        if (vm is null)
        {
            return null;
        }

        var renderVm = await BuildRenderVmAsync(vm, ct);
        var html = await _template.RenderAsync(renderVm, ct);
        var pdf = await _pdfRenderer.RenderAsync(html, ct);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var fileName = $"programme-{programmeId}-{timestamp}.pdf";
        var uri = await _fileStore.WriteAsync(fileName, pdf, "application/pdf", ct);

        var payloadJson = JsonSerializer.Serialize(renderVm);
        await _repository.PersistPublishedVersionAsync(programmeId, practitionerId, payloadJson, ct);

        _logger.LogInformation(
            "Published programme {ProgrammeId} as {FileName} ({Bytes} bytes)",
            programmeId, fileName, pdf.Length);

        return new PublishResponse(uri.ToString(), fileName, pdf.Length);
    }

    public async Task<PreviewPdfDocument?> RenderVersionPdfAsync(ulong programmeId, ulong programmeVersionId, ulong practitionerId, CancellationToken ct)
    {
        var versionPayload = await _repository.GetPublishedVersionPayloadAsync(
            programmeId,
            practitionerId,
            programmeVersionId,
            ct);

        if (versionPayload is null)
        {
            return null;
        }

        var renderVm = DeserializeProgrammeVm(versionPayload.PayloadJson);
        if (renderVm is null)
        {
            return null;
        }

        var html = await _template.RenderAsync(renderVm, ct);
        var pdf = await _pdfRenderer.RenderAsync(html, ct);
        var fileName = $"programme-version-{programmeVersionId}.pdf";
        return new PreviewPdfDocument(pdf, "application/pdf", fileName);
    }

    public Task<bool> DeleteVersionAsync(ulong programmeId, ulong programmeVersionId, ulong practitionerId, CancellationToken ct)
        => _repository.DeleteVersionAsync(programmeId, programmeVersionId, practitionerId, ct);

    private static ProgrammeVm? DeserializeProgrammeVm(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ProgrammeVm>(payloadJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<ProgrammeVm> BuildRenderVmAsync(ProgrammeVm vm, CancellationToken ct)
    {
        var imageCache = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var sessions = new List<ProgrammeVm.SessionRow>(vm.Sessions.Count);

        foreach (var session in vm.Sessions)
        {
            var exercises = new List<ProgrammeVm.SessionExerciseRow>(session.Exercises.Count);
            foreach (var exercise in session.Exercises)
            {
                var imageUrl = await ResolveRenderableImageUrlAsync(exercise.ImageUrl, imageCache, ct);
                exercises.Add(exercise with { ImageUrl = imageUrl });
            }

            sessions.Add(session with { Exercises = exercises });
        }

        return vm with { Sessions = sessions };
    }

    private async Task<string?> ResolveRenderableImageUrlAsync(
        string? originalUrl,
        Dictionary<string, string?> imageCache,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(originalUrl))
        {
            return originalUrl;
        }

        if (!TryResolveManagedKey(originalUrl, out var key))
        {
            return originalUrl;
        }

        if (imageCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var readResult = await _fileStore.OpenReadAsync(key, ct);
        if (readResult is null)
        {
            imageCache[key] = originalUrl;
            return originalUrl;
        }

        await using (readResult.Content)
        await using (var buffer = new MemoryStream())
        {
            await readResult.Content.CopyToAsync(buffer, ct);
            var bytes = buffer.ToArray();
            var mime = string.IsNullOrWhiteSpace(readResult.ContentType)
                ? "application/octet-stream"
                : readResult.ContentType;
            var encoded = Convert.ToBase64String(bytes);
            var dataUrl = $"data:{mime};base64,{encoded}";
            imageCache[key] = dataUrl;
            return dataUrl;
        }
    }

    private static bool TryResolveManagedKey(string url, out string key)
    {
        key = string.Empty;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        const string marker = "/exercise-media/";
        var full = uri.AbsolutePath.Replace('\\', '/');
        var markerIndex = full.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return false;
        }

        var relative = full[(markerIndex + 1)..].Trim('/');
        if (string.IsNullOrWhiteSpace(relative))
        {
            return false;
        }

        key = relative;
        return true;
    }
}
