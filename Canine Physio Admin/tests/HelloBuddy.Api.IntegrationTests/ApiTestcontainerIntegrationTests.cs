using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using HelloBuddy.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using MySqlConnector;
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

    [Fact]
    public async Task ExerciseCrud_AgainstTestcontainer_RoundTrips()
    {
        var categories = await _client.GetFromJsonAsync<List<ExerciseCategoryListItem>>("/api/exercise-categories");
        Assert.NotNull(categories);
        var category = Assert.Single(categories.Where(x => x.CategoryName == "Strength"));

        var request = new SaveExerciseRequest
        {
            ExerciseCategoryId = category.ExerciseCategoryId,
            Title = $"Container Exercise {Guid.NewGuid():N}",
            ObjectiveSummary = "Container summary",
            VideoUrl = "https://example.test/container.mp4",
            Instructions =
            [
                new SaveExerciseRequest.InstructionStepInput { StepNumber = 1, InstructionText = "Container step one" },
                new SaveExerciseRequest.InstructionStepInput { StepNumber = 2, InstructionText = "Container step two" }
            ]
        };

        var create = await _client.PostAsJsonAsync("/api/exercises", request);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<ExerciseDetailVm>();
        Assert.NotNull(created);
        Assert.Equal(2, created.Instructions.Count);

        var fetch = await _client.GetFromJsonAsync<ExerciseDetailVm>($"/api/exercises/{created.ExerciseId}");
        Assert.NotNull(fetch);
        Assert.Equal(request.Title, fetch.Title);
    }

    [Fact]
    public async Task ExerciseMediaUpload_AgainstTestcontainer_ReturnsUrl()
    {
        using var form = new MultipartFormDataContent();
        var pngBytes = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47,
            0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52
        };
        using var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "file", "container-upload.png");

        var response = await _client.PostAsync("/api/exercises/media", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ExerciseMediaUploadResponse>();
        Assert.NotNull(payload);
        Assert.Contains("exercise-media/", payload.Url, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExerciseEdit_DoesNotMutatePublishedProgrammeVersionPayload()
    {
        const string originalPayload = "{\"programme\":\"published-snapshot\",\"version\":1}";

        ulong programmeId;
        uint nextVersion;
        string payloadBeforeEdit;
        await using (var setupConnection = new MySqlConnection(_fixture.ConnectionString))
        {
            await setupConnection.OpenAsync();

            await using (var programmeCmd = new MySqlCommand("SELECT ProgrammeId FROM Programme ORDER BY ProgrammeId LIMIT 1", setupConnection))
            {
                var rawProgrammeId = await programmeCmd.ExecuteScalarAsync();
                Assert.NotNull(rawProgrammeId);
                programmeId = Convert.ToUInt64(rawProgrammeId);
            }

            await using (var versionCmd = new MySqlCommand("SELECT COALESCE(MAX(VersionNumber), 0) + 1 FROM ProgrammeVersion WHERE ProgrammeId = @programmeId", setupConnection))
            {
                versionCmd.Parameters.AddWithValue("@programmeId", programmeId);
                var rawNextVersion = await versionCmd.ExecuteScalarAsync();
                Assert.NotNull(rawNextVersion);
                nextVersion = Convert.ToUInt32(rawNextVersion);
            }

            await using (var insertCmd = new MySqlCommand(@"
                INSERT INTO ProgrammeVersion
                    (ProgrammeId, VersionNumber, VersionStatus, PayloadJson, PayloadSchemaVersion, CreatedByPractitionerId, CreatedDate, PublishedDate)
                VALUES
                    (@programmeId, @versionNumber, 'published', @payloadJson, '1.0.0', 1, UTC_TIMESTAMP(), UTC_TIMESTAMP())", setupConnection))
            {
                insertCmd.Parameters.AddWithValue("@programmeId", programmeId);
                insertCmd.Parameters.AddWithValue("@versionNumber", nextVersion);
                insertCmd.Parameters.AddWithValue("@payloadJson", originalPayload);
                await insertCmd.ExecuteNonQueryAsync();
            }

            await using (var baselineCmd = new MySqlCommand(@"
                SELECT PayloadJson
                FROM ProgrammeVersion
                WHERE ProgrammeId = @programmeId AND VersionNumber = @versionNumber", setupConnection))
            {
                baselineCmd.Parameters.AddWithValue("@programmeId", programmeId);
                baselineCmd.Parameters.AddWithValue("@versionNumber", nextVersion);
                payloadBeforeEdit = (string?)await baselineCmd.ExecuteScalarAsync() ?? string.Empty;
            }
        }

        var exercises = await _client.GetFromJsonAsync<List<ExerciseListItem>>("/api/exercises?searchText=Step-ups&activeOnly=false");
        Assert.NotNull(exercises);
        var exercise = Assert.Single(exercises.Where(x => x.ExerciseKey == "stepUp"));

        var detail = await _client.GetFromJsonAsync<ExerciseDetailVm>($"/api/exercises/{exercise.ExerciseId}");
        Assert.NotNull(detail);

        var updateRequest = new SaveExerciseRequest
        {
            ExerciseCategoryId = detail.ExerciseCategoryId ?? 0,
            Title = detail.Title + " updated",
            ObjectiveSummary = detail.ObjectiveSummary,
            ImageUrl = detail.ImageUrl,
            VideoUrl = detail.VideoUrl,
            DefaultReps = detail.DefaultReps,
            DefaultSets = detail.DefaultSets,
            DefaultHoldSeconds = detail.DefaultHoldSeconds,
            IsActive = detail.IsActive,
            Instructions = detail.Instructions
                .Select(x => new SaveExerciseRequest.InstructionStepInput
                {
                    StepNumber = x.StepNumber,
                    InstructionText = x.InstructionText
                })
                .ToList()
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/exercises/{exercise.ExerciseId}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        await using var verifyConnection = new MySqlConnection(_fixture.ConnectionString);
        await verifyConnection.OpenAsync();
        await using var verifyCmd = new MySqlCommand(@"
            SELECT PayloadJson
            FROM ProgrammeVersion
            WHERE ProgrammeId = @programmeId AND VersionNumber = @versionNumber", verifyConnection);
        verifyCmd.Parameters.AddWithValue("@programmeId", programmeId);
        verifyCmd.Parameters.AddWithValue("@versionNumber", nextVersion);
        var payloadAfterEdit = (string?)await verifyCmd.ExecuteScalarAsync();

        Assert.Equal(payloadBeforeEdit, payloadAfterEdit);
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
            builder.UseSetting("Seed:ExerciseLibrary:Enabled", "true");
        }
    }
}
