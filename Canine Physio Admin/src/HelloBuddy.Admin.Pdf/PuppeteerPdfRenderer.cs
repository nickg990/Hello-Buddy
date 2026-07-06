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

            // In container images we pre-install Chromium via apt and point
            // PuppeteerSharp at it via PUPPETEER_EXECUTABLE_PATH to avoid the
            // ~150 MB first-render download (cold start would time out).
            var executablePath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                var bf = new BrowserFetcher();
                await bf.DownloadAsync();
            }

            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                // Use the classic ("old") headless mode. PuppeteerSharp's
                // Headless=true selects the new headless mode (--headless=new),
                // which starts a full Chrome that needs a D-Bus system bus and
                // graphics stack — neither exists in the slim container, so the
                // browser crashes at launch ("Failed to connect to the bus" /
                // "Failed to launch browser"). Shell mode (--headless) is the
                // lightweight, container-proven path and needs no D-Bus/GPU.
                HeadlessMode = HeadlessMode.Shell,
                ExecutablePath = string.IsNullOrWhiteSpace(executablePath) ? null : executablePath,
                Args = new[] { "--no-sandbox", "--disable-dev-shm-usage", "--disable-gpu" }
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
