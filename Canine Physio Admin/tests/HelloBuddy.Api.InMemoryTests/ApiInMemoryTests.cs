using System.Net;
using System.Net.Http.Json;
using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace HelloBuddy.Api.InMemoryTests;

public sealed class ApiInMemoryTests : IClassFixture<ApiInMemoryTests.Factory>
{
    private readonly HttpClient _client;

    public ApiInMemoryTests(Factory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Practitioner-Id", "1");
    }

    [Fact]
    public async Task OwnerCrud_CreateAndGet_WorksInMemory()
    {
        var request = new SaveOwnerRequest
        {
            FirstName = "Fast",
            LastName = "Owner",
            Email = $"fast-owner-{Guid.NewGuid():N}@example.test"
        };

        var create = await _client.PostAsJsonAsync("/api/owners", request);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var owner = await create.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var get = await _client.GetAsync($"/api/owners/{owner.OwnerId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task OwnerCreate_InvalidBody_ReturnsBadRequest()
    {
        var request = new SaveOwnerRequest
        {
            FirstName = " ",
            LastName = " ",
            Email = "not-an-email"
        };

        var response = await _client.PostAsJsonAsync("/api/owners", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ApiRequests_WithoutPractitionerHeader_ReturnUnauthorized()
    {
        using var client = new Factory().CreateClient();
        var response = await client.GetAsync("/api/owners");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public sealed class Factory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"hello-buddy-api-tests-{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("InMemoryTesting");
            builder.UseSetting("ConnectionStrings:CaninePhysioDb", "Server=localhost;Database=unused;User=unused;Password=unused");
            builder.UseSetting("PdfService:Uri", "http://localhost:18080");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<CaninePhysioDbContext>>();
                services.RemoveAll<CaninePhysioDbContext>();
                services.RemoveAll<IDbContextOptionsConfiguration<CaninePhysioDbContext>>();

                services.AddDbContext<CaninePhysioDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }
    }
}
