using System.Net.Http;
using Azure.Storage.Blobs;
using HelloBuddy.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HelloBuddy.Api.IntegrationTests;

public sealed class AzuriteBlobStoreIntegrationTests
{
    [Fact]
    public async Task AzureBlobFileStore_AzuriteRoundTrip_WritesAndReadsBytes()
    {
        var runAzuriteTests = string.Equals(
            Environment.GetEnvironmentVariable("HELLOBUDDY_RUN_AZURITE_TESTS"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (!runAzuriteTests)
        {
            return;
        }

        var connectionString = Environment.GetEnvironmentVariable("HELLOBUDDY_AZURITE_CONNECTION")
            ?? "UseDevelopmentStorage=true";

        var service = new BlobServiceClient(connectionString);
        var containerName = $"hb-azurite-{Guid.NewGuid():N}";
        var container = service.GetBlobContainerClient(containerName);

        var store = new AzureBlobFileStore(
            service,
            container,
            NullLogger<AzureBlobFileStore>.Instance);

        var fileName = $"programme-1-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
        var bytes = new byte[] { 1, 2, 3, 4, 5, 9 };

        try
        {
            var writeUri = await store.WriteAsync(fileName, bytes, "application/pdf");
            Assert.Contains(containerName, writeUri.ToString(), StringComparison.OrdinalIgnoreCase);

            var downloadUri = await store.GetReadUrlAsync(fileName, TimeSpan.FromMinutes(5));
            using var http = new HttpClient();
            var downloaded = await http.GetByteArrayAsync(downloadUri);

            Assert.Equal(bytes, downloaded);
        }
        finally
        {
            await container.DeleteIfExistsAsync();
        }
    }
}
