using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace HelloBuddy.Admin.Pdf;

/// <summary>
/// PuppeteerSharp-backed PDF renderer. Lazily downloads Chromium on first
/// render (one-time, ~150 MB) and reuses a single browser instance for the
/// process lifetime.
/// </summary>
public sealed class PuppeteerPdfRenderer : IPdfRenderer, IAsyncDisposable
{
    private static readonly SemaphoreSlim _initLock = new(1, 1);
    private static IBrowser? _browser;

    public async Task<byte[]> RenderAsync(string html, CancellationToken ct = default)
    {
        var browser = await GetOrCreateBrowserAsync(ct);
        await using var page = await browser.NewPageAsync();
        await page.SetContentAsync(html, new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
        });
        return await page.PdfDataAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "12mm",
                Bottom = "12mm",
                Left = "12mm",
                Right = "12mm"
            }
        });
    }

    private static async Task<IBrowser> GetOrCreateBrowserAsync(CancellationToken ct)
    {
        if (_browser is not null) return _browser;
        await _initLock.WaitAsync(ct);
        try
        {
            if (_browser is not null) return _browser;
            var bf = new BrowserFetcher();
            await bf.DownloadAsync();
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
            });
            return _browser;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
            _browser = null;
        }
    }
}
