using System.Text.RegularExpressions;

namespace HelloBuddy.Application.Media;

/// <summary>
/// Converts Google Drive share/viewer URLs to direct-download form so they can
/// be rendered as <c>&lt;img src&gt;</c> or fetched by Puppeteer/Chromium.
/// </summary>
public static class GoogleDriveImageHelper
{
    // Matches drive.google.com/file/d/{fileId} regardless of trailing path or query string.
    private static readonly Regex DriveFilePattern =
        new(@"drive\.google\.com/file/d/([^/?#]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Attempts to convert a Google Drive share/viewer URL to the direct-download form
    /// <c>https://drive.google.com/uc?export=view&amp;id={fileId}</c>.
    /// Returns <see langword="false"/> if <paramref name="url"/> is null, empty, or not a
    /// recognised Drive file URL.
    /// </summary>
    public static bool TryConvertToDirectUrl(string? url, out string directUrl)
    {
        directUrl = string.Empty;
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var match = DriveFilePattern.Match(url);
        if (!match.Success)
        {
            return false;
        }

        var fileId = match.Groups[1].Value;
        directUrl = $"https://drive.google.com/uc?export=view&id={fileId}";
        return true;
    }

    /// <summary>
    /// Returns the direct-download URL when <paramref name="url"/> is a Drive share/viewer URL;
    /// otherwise returns <paramref name="url"/> unchanged (including <see langword="null"/>).
    /// </summary>
    public static string? ToDisplayUrl(string? url) =>
        TryConvertToDirectUrl(url, out var direct) ? direct : url;
}
