namespace HelloBuddy.Admin.Pdf;

/// <summary>
/// Renders an HTML document to a PDF byte stream. Implementation lives in the
/// Pdf project so it can later be hosted as a standalone Container App (US-15+).
/// </summary>
public interface IPdfRenderer
{
    /// <param name="html">A complete, self-contained HTML document.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<byte[]> RenderAsync(string html, CancellationToken ct = default);
}
