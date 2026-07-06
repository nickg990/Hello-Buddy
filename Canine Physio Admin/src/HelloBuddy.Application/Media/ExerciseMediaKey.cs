namespace HelloBuddy.Application.Media;

/// <summary>
/// Shared key resolution for the managed exercise-media blob namespace.
/// Any URL whose path contains "/exercise-media/" is considered managed;
/// the blob key is the path segment starting at "exercise-media/".
/// </summary>
public static class ExerciseMediaKey
{
    public const string Marker = "/exercise-media/";
    public const string Prefix = "exercise-media/";

    /// <summary>
    /// Attempts to extract a blob key from an absolute URL by locating the
    /// "/exercise-media/" marker in the path.  Returns <see langword="false"/>
    /// when the URL is null, not absolute, or does not contain the marker.
    /// </summary>
    public static bool TryResolve(string? url, out string key)
    {
        key = string.Empty;

        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var full = uri.AbsolutePath.Replace('\\', '/');
        var markerIndex = full.IndexOf(Marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return false;
        }

        var relative = full[(markerIndex + 1)..].Trim('/');
        if (string.IsNullOrWhiteSpace(relative))
        {
            return false;
        }

        key = relative;
        return true;
    }
}
