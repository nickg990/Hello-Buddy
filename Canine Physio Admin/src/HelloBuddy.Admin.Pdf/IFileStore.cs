namespace HelloBuddy.Admin.Pdf;

/// <summary>
/// Abstraction over the destination for published artefacts (currently
/// programme PDFs). Local file system in Development, Azure Blob Storage
/// in Azure deployments. Selected via DI in the API host.
/// </summary>
public interface IFileStore
{
    /// <summary>Writes the bytes and returns a stable URI identifying the artefact.</summary>
    Task<Uri> WriteAsync(string key, byte[] bytes, string contentType, CancellationToken ct = default);

    /// <summary>
    /// Best-effort deletion for an artefact key. Returns true when the object existed
    /// and was removed, false when the key did not exist.
    /// </summary>
    Task<bool> DeleteIfExistsAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Opens a readable stream for an artefact key. Returns null when not found.
    /// Caller owns the returned stream lifetime.
    /// </summary>
    Task<ArtefactReadResult?> OpenReadAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Issues a short-lived read URL for the given key. The local file store
    /// returns a controller route that streams from disk; the blob store
    /// returns a user-delegation SAS URL.
    /// </summary>
    Task<Uri> GetReadUrlAsync(string key, TimeSpan ttl, CancellationToken ct = default);
}

public sealed record ArtefactReadResult(Stream Content, string ContentType);
