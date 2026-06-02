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

    public async Task<ProgrammeVm?> UpdateAsync(ulong programmeId, ProgrammeBuilderForm form, ulong practitionerId, CancellationToken ct)
    {
        if (!await _repository.OwnsAsync(programmeId, practitionerId, ct))
        {
            return null;
        }

        await _repository.UpdateSessionExercisesAsync(programmeId, form.Exercises, ct);
        return await _repository.GetVmAsync(programmeId, practitionerId, ct);
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
        var uri = await _fileStore.WriteAsync(fileName, pdf, ct);

        _logger.LogInformation(
            "Published programme {ProgrammeId} as {FileName} ({Bytes} bytes)",
            programmeId, fileName, pdf.Length);

        return new PublishResponse(uri.ToString(), fileName, pdf.Length);
    }
}
