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
    private const string LogoResourceName    = "HelloBuddy.Admin.Pdf.Templates.logo-disc.svg";

    private readonly Lazy<IRazorEngineCompiledTemplate<RazorEngineTemplateBase<ProgrammeVm>>> _compiled =
        new(CompileTemplate, isThreadSafe: true);

    private readonly Lazy<string> _logoDataUri =
        new(LoadLogoDataUri, isThreadSafe: true);

    public Task<string> RenderAsync(ProgrammeVm vm, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var html = _compiled.Value.Run(instance => instance.Model = vm);
        html = html.Replace("__LOGO_DATA_URI__", _logoDataUri.Value, StringComparison.Ordinal);
        return Task.FromResult(html);
    }

    private static string LoadLogoDataUri()
    {
        var assembly = typeof(RazorProgrammePdfTemplate).Assembly;
        using var stream = assembly.GetManifestResourceStream(LogoResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded logo '{LogoResourceName}' not found. Verify logo-disc.svg is included as <EmbeddedResource>.");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return "data:image/svg+xml;base64," + Convert.ToBase64String(ms.ToArray());
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
