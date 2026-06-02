using System.Net;
using System.Net.Http.Json;
using HelloBuddy.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HelloBuddy.Api.IntegrationTests;

public sealed class ApiIntegrationTests : IClassFixture<ApiIntegrationTests.Factory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(Factory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Practitioner-Id", "1");
    }

    [Fact]
    public async Task OwnerCrud_CreateAndGet_WorksAgainstDatabase()
    {
        var ownerRequest = new SaveOwnerRequest
        {
            FirstName = "Integration",
            LastName = "Owner",
            Email = $"integration-owner-{Guid.NewGuid():N}@example.test",
            PhoneNumber = "07000 000001",
            Town = "Leeds"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/owners", ownerRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(created);
        Assert.True(created.OwnerId > 0);
        Assert.Equal(ownerRequest.Email.ToLowerInvariant(), created.Email);

        var getResponse = await _client.GetAsync($"/api/owners/{created.OwnerId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(fetched);
        Assert.Equal(created.OwnerId, fetched.OwnerId);
        Assert.Equal("Integration", fetched.FirstName);
    }

    [Fact]
    public async Task PetAndCaseFlow_CreatePetCreateCaseAddNote_WorksAgainstDatabase()
    {
        var ownerCreate = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Integration",
            LastName = "PetOwner",
            Email = $"integration-petowner-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, ownerCreate.StatusCode);
        var owner = await ownerCreate.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var petCreate = await _client.PostAsJsonAsync("/api/pets", new SavePetRequest
        {
            OwnerId = owner.OwnerId,
            Name = "Integration Buddy",
            Breed = "Labrador",
            Sex = "male",
            IsActive = true
        });
        Assert.Equal(HttpStatusCode.Created, petCreate.StatusCode);

        var pet = await petCreate.Content.ReadFromJsonAsync<PetDetailVm>();
        Assert.NotNull(pet);
        Assert.True(pet.PetId > 0);

        var caseCreate = await _client.PostAsJsonAsync("/api/cases", new SaveTreatmentCaseRequest
        {
            PetId = pet.PetId,
            CaseTitle = "Integration Case",
            ClinicalSummary = "Integration summary",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = "active"
        });
        Assert.Equal(HttpStatusCode.Created, caseCreate.StatusCode);

        var treatmentCase = await caseCreate.Content.ReadFromJsonAsync<CaseDetailVm>();
        Assert.NotNull(treatmentCase);
        Assert.True(treatmentCase.TreatmentCaseId > 0);

        var noteText = $"Integration note {Guid.NewGuid():N}";
        var noteCreate = await _client.PostAsJsonAsync($"/api/cases/{treatmentCase.TreatmentCaseId}/notes", new CreateCaseNoteRequest
        {
            NoteType = "assessment",
            NoteText = noteText
        });
        Assert.Equal(HttpStatusCode.OK, noteCreate.StatusCode);

        var caseGet = await _client.GetAsync($"/api/cases/{treatmentCase.TreatmentCaseId}");
        Assert.Equal(HttpStatusCode.OK, caseGet.StatusCode);
        var fetched = await caseGet.Content.ReadFromJsonAsync<CaseDetailVm>();
        Assert.NotNull(fetched);
        Assert.Contains(fetched.Notes, n => n.NoteText == noteText);
    }

    public sealed class Factory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var dbConnection =
                Environment.GetEnvironmentVariable("HELLOBUDDY_TEST_DB_CONNECTION")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__CaninePhysioDb")
                ?? Environment.GetEnvironmentVariable("MYSQLCONNSTR_CaninePhysioDb")
                ?? "Server=localhost;Port=3306;Database=canine_physiotherapy;User=root;Password=devroot;SslMode=None";

            builder.UseEnvironment("IntegrationTesting");
            builder.UseSetting("ConnectionStrings:CaninePhysioDb", dbConnection);
            builder.UseSetting("PdfService:Uri", "http://localhost:18080");
        }
    }
}