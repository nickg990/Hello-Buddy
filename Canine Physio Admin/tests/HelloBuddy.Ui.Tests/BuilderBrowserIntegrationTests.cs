using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PuppeteerSharp;
using Xunit;

namespace HelloBuddy.Ui.Tests;

/// <summary>
/// Browser-automation E2E coverage for the in-app builder preview.
///
/// These tests are opt-in via the <c>HELLOBUDDY_RUN_BROWSER_TESTS=1</c> environment
/// variable. They are intentionally excluded from the default <c>dotnet test</c> lane so
/// the standard run never blocks on a Chromium download and never depends on a browser
/// being installed. Enable them in a dedicated E2E job where a Chromium-based browser
/// is available.
/// </summary>
public sealed class BuilderBrowserIntegrationTests
{
    private const string OptInVariable = "HELLOBUDDY_RUN_BROWSER_TESTS";

    [Fact]
    public async Task BuilderPreview_OpensDedicatedPreviewPage_WithPdfFrame()
    {
        if (!IsOptedIn())
        {
            // Opt-in E2E: skipped in the default lane to keep it fast and self-contained.
            return;
        }

        var executablePath = ResolveInstalledBrowser();
        if (executablePath is null)
        {
            // No locally installed Chromium/Edge/Chrome was found. Skip rather than block
            // the run on a large browser download.
            return;
        }

        await using var factory = new ReachableUiFactory();
        var baseAddress = factory.EnsureServerStartedAndGetBaseAddress();

        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            ExecutablePath = executablePath,
            Args = ["--no-sandbox", "--disable-dev-shm-usage"],
        });

        await using var page = await browser.NewPageAsync();

        var builderUrl = new Uri(new Uri(baseAddress), "/Programmes/1/Builder").ToString();
        await page.GoToAsync(builderUrl);

        var content = await page.GetContentAsync();
        Assert.Contains("Open preview page", content);

        await page.ClickAsync("a[href='/Programmes/1/Preview']");

        await page.WaitForSelectorAsync("iframe[title='Programme PDF preview']");

        var previewContent = await page.GetContentAsync();
        Assert.Contains("Save PDF", previewContent);
    }

    private static bool IsOptedIn()
        => string.Equals(Environment.GetEnvironmentVariable(OptInVariable), "1", StringComparison.Ordinal);

    private static string? ResolveInstalledBrowser()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
            "/usr/bin/google-chrome",
            "/usr/bin/chromium",
            "/usr/bin/chromium-browser",
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    /// <summary>
    /// WebApplicationFactory whose default <see cref="WebApplicationFactory{TEntryPoint}.CreateClient"/>
    /// host is an in-memory TestServer (not reachable by an out-of-process browser). This subclass
    /// additionally starts a real Kestrel host on a dynamic loopback port and exposes its address so a
    /// real browser can connect. This is the documented dual-host pattern for Selenium/Playwright-style
    /// tests against ASP.NET Core.
    /// </summary>
    private sealed class ReachableUiFactory : UiSmokeTests.Factory
    {
        private IHost? _kestrelHost;
        private string? _baseAddress;

        public string EnsureServerStartedAndGetBaseAddress()
        {
            // Forces the host pipeline (and therefore CreateHost) to run.
            using var _ = CreateClient();
            return _baseAddress ?? throw new InvalidOperationException("Kestrel host address was not resolved.");
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // In-memory test host required by WebApplicationFactory.
            var testHost = base.CreateHost(builder);

            // Real Kestrel host on a dynamic loopback port so an external browser can reach the app.
            builder.ConfigureWebHost(webHostBuilder => webHostBuilder
                .UseKestrel()
                .UseUrls("http://127.0.0.1:0"));

            _kestrelHost = builder.Build();
            _kestrelHost.Start();

            var server = _kestrelHost.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>()
                ?? throw new InvalidOperationException("Kestrel host did not expose server addresses.");
            _baseAddress = addresses.Addresses.First();

            return testHost;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _kestrelHost?.Dispose();
            }
        }
    }
}
