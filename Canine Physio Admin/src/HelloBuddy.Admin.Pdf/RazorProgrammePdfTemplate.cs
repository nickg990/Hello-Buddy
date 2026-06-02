using System.Reflection;
using HelloBuddy.Contracts;
using RazorEngineCore;

namespace HelloBuddy.Admin.Pdf;

/// <summary>
/// Razor-based implementation of <see cref="IProgrammePdfTemplate"/> that
/// compiles the embedded <c>Templates/Programme.cshtml</c> once and re-uses
/// the compiled template for subsequent renders. Replaces the previous
/// string-concatenated HTML approach (CR-001 finding #6).
/// </summary>
public sealed class RazorProgrammePdfTemplate : IProgrammePdfTemplate
{
    private const string TemplateResourceName = "HelloBuddy.Admin.Pdf.Templates.Programme.cshtml";

    private readonly Lazy<IRazorEngineCompiledTemplate<RazorEngineTemplateBase<ProgrammeVm>>> _compiled =
        new(CompileTemplate, isThreadSafe: true);

    public Task<string> RenderAsync(ProgrammeVm vm, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var html = _compiled.Value.Run(instance => instance.Model = vm);
        return Task.FromResult(html);
    }

    private static IRazorEngineCompiledTemplate<RazorEngineTemplateBase<ProgrammeVm>> CompileTemplate()
    {
        var assembly = typeof(RazorProgrammePdfTemplate).Assembly;
        using var stream = assembly.GetManifestResourceStream(TemplateResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded template '{TemplateResourceName}' not found. Verify Templates/Programme.cshtml is included as <EmbeddedResource>.");
        using var reader = new StreamReader(stream);
        var source = reader.ReadToEnd();

        var engine = new RazorEngine();
        return engine.Compile<RazorEngineTemplateBase<ProgrammeVm>>(source, builder =>
        {
            builder.AddAssemblyReference(typeof(ProgrammeVm).Assembly);
            builder.AddAssemblyReferenceByName("System.Linq");
            builder.AddAssemblyReferenceByName("System.Collections");
            builder.AddAssemblyReferenceByName("netstandard");
            builder.AddUsing("System");
            builder.AddUsing("System.Linq");
            builder.AddUsing("HelloBuddy.Contracts");
        });
    }
}
