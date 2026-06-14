using HelloBuddy.Admin.Pdf;

namespace HelloBuddy.Api.Services;

/// <summary>
/// Writes published artefacts to a directory on the local file system.
/// Used in Development; in Azure the <see cref="AzureBlobFileStore"/> is
/// registered instead.
/// </summary>
public sealed class LocalFileStore : IFileStore
{
    private readonly string _rootPath;
    private readonly ILogger<LocalFileStore> _logger;

    public LocalFileStore(string rootPath, ILogger<LocalFileStore> logger)
    {
        _rootPath = rootPath;
        _logger = logger;
        Directory.CreateDirectory(_rootPath);
    }

    public Task<Uri> WriteAsync(string key, byte[] bytes, string contentType, CancellationToken ct = default)
    {
        var path = Path.Combine(_rootPath, key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return WriteCoreAsync(path, key, bytes, ct);
    }

    private async Task<Uri> WriteCoreAsync(string path, string key, byte[] bytes, CancellationToken ct)
    {
        await File.WriteAllBytesAsync(path, bytes, ct);
        _logger.LogInformation("Wrote {Bytes} bytes to local file store at {Path}", bytes.Length, path);
        return new Uri(Path.GetFullPath(path));
    }

    public Task<Uri> GetReadUrlAsync(string key, TimeSpan ttl, CancellationToken ct = default)
    {
        // Dev convenience: a relative URI to a streaming controller route
        // hosted on the API (see Program.cs). The TTL is ignored locally.
        return Task.FromResult(new Uri($"/dev-published/{Uri.EscapeDataString(key)}", UriKind.Relative));
    }

    public Task<bool> DeleteIfExistsAsync(string key, CancellationToken ct = default)
    {
        var path = Path.Combine(_rootPath, key.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            return Task.FromResult(false);
        }

        File.Delete(path);
        _logger.LogInformation("Deleted local file store artefact at {Path}", path);
        return Task.FromResult(true);
    }

    public Task<int> DeleteByPrefixAsync(string keyPrefix, CancellationToken ct = default)
    {
        var deleted = 0;
        foreach (var path in Directory.EnumerateFiles(_rootPath, "*", SearchOption.AllDirectories))
        {
            var relativeKey = Path.GetRelativePath(_rootPath, path).Replace(Path.DirectorySeparatorChar, '/');
            if (!relativeKey.StartsWith(keyPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            File.Delete(path);
            deleted++;
            _logger.LogInformation("Deleted local file store artefact at {Path}", path);
        }

        return Task.FromResult(deleted);
    }

    public Task<ArtefactReadResult?> OpenReadAsync(string key, CancellationToken ct = default)
    {
        var path = Path.Combine(_rootPath, key.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            return Task.FromResult<ArtefactReadResult?>(null);
        }

        var stream = File.OpenRead(path);
        return Task.FromResult<ArtefactReadResult?>(new ArtefactReadResult(stream, GuessContentType(path)));
    }

    internal Stream? OpenReadOrNull(string key)
    {
        var path = Path.Combine(_rootPath, key);
        if (!File.Exists(path)) return null;
        return File.OpenRead(path);
    }

    private static string GuessContentType(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
