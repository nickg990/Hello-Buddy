using System.Net.Http.Json;
using HelloBuddy.Contracts;

namespace HelloBuddy.Admin.Pdf;

/// <summary>
/// <see cref="IPdfRenderer"/> implementation that delegates rendering to a
/// remote PDF service (ca-hello-buddy-pdf). The HttpClient is configured by
/// the DI container with the service base address.
/// </summary>
public sealed class HttpPdfRenderer : IPdfRenderer
{
    private readonly HttpClient _http;

    public HttpPdfRenderer(HttpClient http)
    {
        _http = http;
    }

    public async Task<byte[]> RenderAsync(string html, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("/render", new RenderRequest(html), ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsByteArrayAsync(ct);
    }
}
