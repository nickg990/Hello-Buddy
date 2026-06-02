using HelloBuddy.Contracts;

namespace HelloBuddy.Admin.Pdf;

/// <summary>
/// Renders a <see cref="ProgrammeVm"/> to a complete, self-contained HTML
/// document suitable for <see cref="IPdfRenderer"/>. Implementations live in
/// the PDF project (see CR-001 finding #6: Razor templating).
/// </summary>
public interface IProgrammePdfTemplate
{
    Task<string> RenderAsync(ProgrammeVm vm, CancellationToken ct = default);
}
