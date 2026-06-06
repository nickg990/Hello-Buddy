using HelloBuddy.Admin.Pdf;
using HelloBuddy.Contracts;
using Microsoft.Extensions.Logging;

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

    public async Task<ProgrammeVm?> UpdateAsync(ulong programmeId, ProgrammeBuilderForm form, ulong practitionerId, CancellationToken ct)
    {
        if (!await _repository.OwnsAsync(programmeId, practitionerId, ct))
        {
            return null;
        }

        await _repository.UpdateSessionExercisesAsync(programmeId, form.Exercises, ct);
        return await _repository.GetVmAsync(programmeId, practitionerId, ct);
    }

    public Task<DeleteProgrammeResult> DeleteAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
        => _repository.DeleteAsync(programmeId, practitionerId, ct);

    public Task<ProgrammeStructureUpdateResult> UpdateStructureAsync(ulong programmeId, ProgrammeStructureForm form, ulong practitionerId, CancellationToken ct)
        => _repository.UpdateStructureAsync(programmeId, practitionerId, form, ct);

    public Task<AddSessionExerciseResult> AddSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong exerciseId, ulong practitionerId, CancellationToken ct)
        => _repository.AddSessionExerciseAsync(programmeId, practitionerId, sessionId, exerciseId, ct);

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

                if (!exercise.HoldSeconds.HasValue || exercise.HoldSeconds.Value == 0)
                {
                    errors[$"{keyPrefix}.HoldSeconds"] = ["Hold seconds must be set to a value greater than zero."];
                }
            }
        }

        return errors;
    }

    public async Task<PublishResponse?> PublishAsync(ulong programmeId, ulong practitionerId, CancellationToken ct)
    {
        var vm = await _repository.GetVmAsync(programmeId, practitionerId, ct);
        if (vm is null)
        {
            return null;
        }

        var html = await _template.RenderAsync(vm, ct);
        var pdf = await _pdfRenderer.RenderAsync(html, ct);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var fileName = $"programme-{programmeId}-{timestamp}.pdf";
        var uri = await _fileStore.WriteAsync(fileName, pdf, "application/pdf", ct);

        _logger.LogInformation(
            "Published programme {ProgrammeId} as {FileName} ({Bytes} bytes)",
            programmeId, fileName, pdf.Length);

        return new PublishResponse(uri.ToString(), fileName, pdf.Length);
    }
}
