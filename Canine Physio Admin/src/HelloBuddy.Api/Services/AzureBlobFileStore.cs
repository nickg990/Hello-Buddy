using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

using HelloBuddy.Admin.Pdf;

namespace HelloBuddy.Api.Services;

/// <summary>
/// Writes published artefacts to an Azure Blob container. The container
/// client is authenticated with the Container App's managed identity
/// (Storage Blob Data Contributor on the container scope).
/// </summary>
public sealed class AzureBlobFileStore : IFileStore
{
    // User-delegation keys can live up to 7 days. We refresh well before that
    // to avoid edge-case clock skew on the issuing side.
    private static readonly TimeSpan UserDelegationKeyLifetime = TimeSpan.FromDays(6);
    private static readonly TimeSpan UserDelegationKeyRefreshBuffer = TimeSpan.FromHours(1);

    private readonly BlobContainerClient _container;
    private readonly BlobServiceClient _service;
    private readonly ILogger<AzureBlobFileStore> _logger;
    private readonly SemaphoreSlim _containerInitLock = new(1, 1);
    private readonly SemaphoreSlim _udkLock = new(1, 1);

    private bool _containerInitialized;
    private UserDelegationKey? _cachedKey;
    private DateTimeOffset _cachedKeyExpiresOn;

    public AzureBlobFileStore(BlobServiceClient service, BlobContainerClient container, ILogger<AzureBlobFileStore> logger)
    {
        _container = container;
        _service = service;
        _logger = logger;
    }

    public async Task<Uri> WriteAsync(string key, byte[] bytes, string contentType, CancellationToken ct = default)
    {
        await EnsureContainerExistsAsync(ct);

        var blob = _container.GetBlobClient(key);
        using var stream = new MemoryStream(bytes, writable: false);
        await blob.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            },
            ct);
        _logger.LogInformation(
            "Uploaded {Bytes} bytes to blob {Container}/{Key}",
            bytes.Length,
            _container.Name,
            key);
        return blob.Uri;
    }

    public async Task<Uri> GetReadUrlAsync(string key, TimeSpan ttl, CancellationToken ct = default)
    {
        await EnsureContainerExistsAsync(ct);

        var blob = _container.GetBlobClient(key);
        var startsOn = DateTimeOffset.UtcNow.AddMinutes(-5);
        var expiresOn = DateTimeOffset.UtcNow.Add(ttl);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = key,
            Resource = "b",
            StartsOn = startsOn,
            ExpiresOn = expiresOn,
            Protocol = blob.Uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                ? SasProtocol.Https
                : SasProtocol.HttpsAndHttp
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        Uri uri;
        if (blob.CanGenerateSasUri)
        {
            uri = blob.GenerateSasUri(sasBuilder);
        }
        else
        {
            var udk = await GetUserDelegationKeyAsync(expiresOn, ct);
            var sasQuery = sasBuilder.ToSasQueryParameters(udk, _service.AccountName).ToString();
            uri = new UriBuilder(blob.Uri) { Query = sasQuery }.Uri;
        }

        _logger.LogInformation(
            "Issued read SAS for {Container}/{Key} expiring at {ExpiresOn:o}",
            _container.Name,
            key,
            expiresOn);
        return uri;
    }

    public async Task<bool> DeleteIfExistsAsync(string key, CancellationToken ct = default)
    {
        await EnsureContainerExistsAsync(ct);
        var blob = _container.GetBlobClient(key);
        var response = await blob.DeleteIfExistsAsync(cancellationToken: ct);
        if (response.Value)
        {
            _logger.LogInformation("Deleted blob {Container}/{Key}", _container.Name, key);
        }

        return response.Value;
    }

    public async Task<int> DeleteByPrefixAsync(string keyPrefix, CancellationToken ct = default)
    {
        await EnsureContainerExistsAsync(ct);
        var deleted = 0;

        await foreach (var blobItem in _container.GetBlobsAsync(prefix: keyPrefix, cancellationToken: ct))
        {
            var blob = _container.GetBlobClient(blobItem.Name);
            var response = await blob.DeleteIfExistsAsync(cancellationToken: ct);
            if (!response.Value)
            {
                continue;
            }

            deleted++;
            _logger.LogInformation("Deleted blob {Container}/{Key}", _container.Name, blobItem.Name);
        }

        return deleted;
    }

    public async Task<ArtefactReadResult?> OpenReadAsync(string key, CancellationToken ct = default)
    {
        await EnsureContainerExistsAsync(ct);
        var blob = _container.GetBlobClient(key);
        if (!await blob.ExistsAsync(ct))
        {
            return null;
        }

        var download = await blob.DownloadStreamingAsync(cancellationToken: ct);
        var contentType = download.Value.Details.ContentType;
        return new ArtefactReadResult(
            download.Value.Content,
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
    }

    private async Task EnsureContainerExistsAsync(CancellationToken ct)
    {
        if (_containerInitialized)
        {
            return;
        }

        await _containerInitLock.WaitAsync(ct);
        try
        {
            if (_containerInitialized)
            {
                return;
            }

            await _container.CreateIfNotExistsAsync(cancellationToken: ct);
            _containerInitialized = true;
            _logger.LogInformation("Verified blob container {Container} exists", _container.Name);
        }
        finally
        {
            _containerInitLock.Release();
        }
    }

    private async Task<UserDelegationKey> GetUserDelegationKeyAsync(DateTimeOffset minimumExpiry, CancellationToken ct)
    {
        if (_cachedKey is not null && _cachedKeyExpiresOn - UserDelegationKeyRefreshBuffer > minimumExpiry)
        {
            return _cachedKey;
        }

        await _udkLock.WaitAsync(ct);
        try
        {
            if (_cachedKey is not null && _cachedKeyExpiresOn - UserDelegationKeyRefreshBuffer > minimumExpiry)
            {
                return _cachedKey;
            }

            var startsOn = DateTimeOffset.UtcNow.AddMinutes(-5);
            var expiresOn = DateTimeOffset.UtcNow.Add(UserDelegationKeyLifetime);
            var response = await _service.GetUserDelegationKeyAsync(startsOn, expiresOn, ct);
            _cachedKey = response.Value;
            _cachedKeyExpiresOn = expiresOn;
            _logger.LogInformation("Refreshed user-delegation key, valid until {ExpiresOn:o}", expiresOn);
            return _cachedKey;
        }
        finally
        {
            _udkLock.Release();
        }
    }
}
