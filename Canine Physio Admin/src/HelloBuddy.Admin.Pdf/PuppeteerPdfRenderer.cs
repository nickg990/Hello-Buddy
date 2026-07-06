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
        // The shared browser process can die between renders (crash, replica
        // reschedule, child-process OOM). If a page operation fails because the
        // browser is gone, discard it and retry once with a fresh launch so a
        // transient failure does not surface as a user-facing 500.
        for (var attempt = 1; ; attempt++)
        {
            var browser = await GetOrCreateBrowserAsync(ct);
            try
            {
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
            catch when (attempt < 2)
            {
                await ResetBrowserAsync();
            }
        }
    }

    private static async Task<IBrowser> GetOrCreateBrowserAsync(CancellationToken ct)
    {
        // Reuse the cached browser only while it is still alive; a disconnected
        // instance must be relaunched or every subsequent render fails.
        if (_browser is { IsConnected: true }) return _browser;
        await _initLock.WaitAsync(ct);
        try
        {
            if (_browser is { IsConnected: true }) return _browser;

            // Drop any dead/disconnected instance before relaunching.
            if (_browser is not null)
            {
                try { await _browser.DisposeAsync(); } catch { /* already gone */ }
                _browser = null;
            }

            // In container images we pre-install Chromium via apt and point
            // PuppeteerSharp at it via PUPPETEER_EXECUTABLE_PATH to avoid the
            // ~150 MB first-render download (cold start would time out).
            var executablePath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                var bf = new BrowserFetcher();
                await bf.DownloadAsync();
            }

            var launchOptions = new LaunchOptions
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
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    // Chromium's zygote spawns a helper that reaches for the D-Bus
                    // system bus (/run/dbus/system_bus_socket), absent in the slim
                    // container. On a cold or CPU-pressured replica this intermittently
                    // crashes the launch ("Failed to connect to the bus"). Disabling
                    // the zygote removes that dependency and makes headless-shell
                    // launch deterministic.
                    "--no-zygote"
                }
            };

            // Cold-started replicas occasionally fail the very first launch under
            // CPU pressure; a short retry loop turns an intermittent 500 into a
            // reliable render.
            ProcessException? lastError = null;
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    _browser = await Puppeteer.LaunchAsync(launchOptions);
                    return _browser;
                }
                catch (ProcessException ex)
                {
                    lastError = ex;
                    await Task.Delay(TimeSpan.FromMilliseconds(500 * attempt), ct);
                }
            }

            throw lastError!;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static async Task ResetBrowserAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            if (_browser is not null)
            {
                try { await _browser.DisposeAsync(); } catch { /* already gone */ }
                _browser = null;
            }
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
