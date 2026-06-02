using System.Net;
using System.Net.Http.Json;
using HelloBuddy.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HelloBuddy.Api.IntegrationTests;

/// <summary>
/// Mirrors <see cref="ApiIntegrationTests"/> but uses a per-class
/// MySQL Testcontainer (CR-001 finding #10) instead of a host MySQL instance.
/// Skipped automatically when Docker is not reachable.
/// </summary>
public sealed class ApiTestcontainerIntegrationTests
    : IClassFixture<MySqlTestcontainerFixture>, IDisposable
{
    private readonly MySqlTestcontainerFixture _fixture;
    private readonly TestcontainerFactory _factory;
    private readonly HttpClient _client;

    public ApiTestcontainerIntegrationTests(MySqlTestcontainerFixture fixture)
    {
        _fixture = fixture;
        _factory = new TestcontainerFactory(fixture.ConnectionString);
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Practitioner-Id", "1");
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task OwnerCrud_AgainstTestcontainer_RoundTrips()
    {
        var request = new SaveOwnerRequest
        {
            FirstName = "Container",
            LastName = "Owner",
            Email = $"container-owner-{Guid.NewGuid():N}@example.test"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/owners", request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(created);
        Assert.True(created.OwnerId > 0);

        var fetched = await _client.GetFromJsonAsync<OwnerDetailVm>($"/api/owners/{created.OwnerId}");
        Assert.NotNull(fetched);
        Assert.Equal("Container", fetched.FirstName);
    }

    private sealed class TestcontainerFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;

        public TestcontainerFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTesting");
            builder.UseSetting("ConnectionStrings:CaninePhysioDb", _connectionString);
            builder.UseSetting("PdfService:Uri", "http://localhost:18080");
        }
    }
}
