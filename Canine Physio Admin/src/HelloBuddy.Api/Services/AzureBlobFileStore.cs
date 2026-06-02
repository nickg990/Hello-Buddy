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
    private readonly SemaphoreSlim _udkLock = new(1, 1);

    private UserDelegationKey? _cachedKey;
    private DateTimeOffset _cachedKeyExpiresOn;

    public AzureBlobFileStore(BlobServiceClient service, BlobContainerClient container, ILogger<AzureBlobFileStore> logger)
    {
        _container = container;
        _service = service;
        _logger = logger;
    }

    public async Task<Uri> WriteAsync(string key, byte[] bytes, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(key);
        using var stream = new MemoryStream(bytes, writable: false);
        await blob.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = "application/pdf" }
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
        var blob = _container.GetBlobClient(key);
        var startsOn = DateTimeOffset.UtcNow.AddMinutes(-5);
        var expiresOn = DateTimeOffset.UtcNow.Add(ttl);

        var udk = await GetUserDelegationKeyAsync(expiresOn, ct);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = key,
            Resource = "b",
            StartsOn = startsOn,
            ExpiresOn = expiresOn,
            Protocol = SasProtocol.Https
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasQuery = sasBuilder.ToSasQueryParameters(udk, _service.AccountName).ToString();
        var uri = new UriBuilder(blob.Uri) { Query = sasQuery }.Uri;
        _logger.LogInformation(
            "Issued read SAS for {Container}/{Key} expiring at {ExpiresOn:o}",
            _container.Name,
            key,
            expiresOn);
        return uri;
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
