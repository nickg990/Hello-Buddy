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

    [Fact]
    public async Task OwnerDataControl_NoLinkedData_DeletesOwnerAndWritesAudit()
    {
        var create = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Audit",
            LastName = "Delete",
            Email = $"audit-delete-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var owner = await create.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var control = await _client.PostAsync($"/api/owners/{owner.OwnerId}/data-control", content: null);
        Assert.Equal(HttpStatusCode.OK, control.StatusCode);

        var payload = await control.Content.ReadFromJsonAsync<OwnerDataControlResponse>();
        Assert.NotNull(payload);
        Assert.Equal("deleted", payload.Outcome);

        var get = await _client.GetAsync($"/api/owners/{owner.OwnerId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        await using var connection = new MySqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();
        await using var auditCmd = new MySqlCommand(@"
            SELECT NewValuesJson
            FROM AuditLog
            WHERE PractitionerId = 1
              AND EntityName = 'owner'
              AND EntityId = @ownerId
              AND ActionType = 'gdpr-deletion'
            ORDER BY ActionDateTime DESC
            LIMIT 1", connection);
        auditCmd.Parameters.AddWithValue("@ownerId", owner.OwnerId);
        var rawAudit = (string?)await auditCmd.ExecuteScalarAsync();
        Assert.False(string.IsNullOrWhiteSpace(rawAudit));
        Assert.Contains("deleted", rawAudit, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OwnerDataControl_WithLinkedPet_DeletesLinkedRecordsAndWritesAudit()
    {
        var ownerCreate = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Audit",
            LastName = "Delete",
            Email = $"audit-linked-delete-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, ownerCreate.StatusCode);
        var owner = await ownerCreate.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var petCreate = await _client.PostAsJsonAsync("/api/pets", new SavePetRequest
        {
            OwnerId = owner.OwnerId,
            Name = "Audit Buddy",
            Breed = "Labrador",
            Sex = "male",
            IsActive = true,
        });
        Assert.Equal(HttpStatusCode.Created, petCreate.StatusCode);
        var pet = await petCreate.Content.ReadFromJsonAsync<PetDetailVm>();
        Assert.NotNull(pet);

        var caseCreate = await _client.PostAsJsonAsync("/api/cases", new SaveTreatmentCaseRequest
        {
            PetId = pet.PetId,
            CaseTitle = "Audit deletion case",
            ClinicalSummary = "Clinical history deleted after RTBF request.",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = "active"
        });
        Assert.Equal(HttpStatusCode.Created, caseCreate.StatusCode);
        var treatmentCase = await caseCreate.Content.ReadFromJsonAsync<CaseDetailVm>();
        Assert.NotNull(treatmentCase);

        var control = await _client.PostAsync($"/api/owners/{owner.OwnerId}/data-control", content: null);
        Assert.Equal(HttpStatusCode.OK, control.StatusCode);

        var payload = await control.Content.ReadFromJsonAsync<OwnerDataControlResponse>();
        Assert.NotNull(payload);
        Assert.Equal("deleted", payload.Outcome);
        Assert.Null(payload.Owner);

        var ownerGet = await _client.GetAsync($"/api/owners/{owner.OwnerId}");
        Assert.Equal(HttpStatusCode.NotFound, ownerGet.StatusCode);

        var ownerGetAfterDeletion = await _client.GetAsync($"/api/owners/{owner.OwnerId}");
        Assert.Equal(HttpStatusCode.NotFound, ownerGetAfterDeletion.StatusCode);

        var petGet = await _client.GetAsync($"/api/pets/{pet.PetId}");
        Assert.Equal(HttpStatusCode.NotFound, petGet.StatusCode);

        var caseGet = await _client.GetAsync($"/api/cases/{treatmentCase.TreatmentCaseId}");
        Assert.Equal(HttpStatusCode.NotFound, caseGet.StatusCode);

        await using var connection = new MySqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();
        await using var auditCmd = new MySqlCommand(@"
            SELECT NewValuesJson
            FROM AuditLog
            WHERE PractitionerId = 1
              AND EntityName = 'owner'
              AND EntityId = @ownerId
              AND ActionType = 'gdpr-deletion'
            ORDER BY ActionDateTime DESC
            LIMIT 1", connection);
        auditCmd.Parameters.AddWithValue("@ownerId", owner.OwnerId);
        var rawAudit = (string?)await auditCmd.ExecuteScalarAsync();
        Assert.False(string.IsNullOrWhiteSpace(rawAudit));
        Assert.Contains("deleted", rawAudit, StringComparison.OrdinalIgnoreCase);

        await using var retainedPetCmd = new MySqlCommand("SELECT COUNT(*) FROM Pet WHERE PetId = @petId", connection);
        retainedPetCmd.Parameters.AddWithValue("@petId", pet.PetId);
        var retainedPetCount = Convert.ToInt32(await retainedPetCmd.ExecuteScalarAsync());
        Assert.Equal(0, retainedPetCount);

        await using var retainedCaseCmd = new MySqlCommand("SELECT COUNT(*) FROM TreatmentCase WHERE TreatmentCaseId = @caseId", connection);
        retainedCaseCmd.Parameters.AddWithValue("@caseId", treatmentCase.TreatmentCaseId);
        var retainedCaseCount = Convert.ToInt32(await retainedCaseCmd.ExecuteScalarAsync());
        Assert.Equal(0, retainedCaseCount);
    }

    [Fact]
    public async Task OwnerDataControl_ForAnyAuthenticatedPractitioner_DeletesOwner()
    {
        // GDPR right-to-be-forgotten is an administrative erase: any authenticated
        // practitioner may delete an existing owner, even one whose pets belong to a
        // different practitioner. The deletion must NOT be gated on practitioner linkage.
        var ownerCreate = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Scoped",
            LastName = "Owner",
            Email = $"scoped-owner-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, ownerCreate.StatusCode);
        var owner = await ownerCreate.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var petCreate = await _client.PostAsJsonAsync("/api/pets", new SavePetRequest
        {
            OwnerId = owner.OwnerId,
            Name = "Scoped Buddy",
            Breed = "Spaniel",
            Sex = "male",
            IsActive = true,
        });
        Assert.Equal(HttpStatusCode.Created, petCreate.StatusCode);

        using var otherClient = _factory.CreateClient();
        otherClient.DefaultRequestHeaders.Add("X-Practitioner-Id", "2");

        var control = await otherClient.PostAsync($"/api/owners/{owner.OwnerId}/data-control", content: null);
        Assert.Equal(HttpStatusCode.OK, control.StatusCode);

        var payload = await control.Content.ReadFromJsonAsync<OwnerDataControlResponse>();
        Assert.NotNull(payload);
        Assert.Equal("deleted", payload.Outcome);
    }

    [Fact]
    public async Task ProgrammeBuilderUpdate_WhenPractitionerDoesNotOwnProgramme_Succeeds()
    {
        var ownerCreate = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Scoped",
            LastName = "Programme",
            Email = $"scoped-programme-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, ownerCreate.StatusCode);
        var owner = await ownerCreate.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var petCreate = await _client.PostAsJsonAsync("/api/pets", new SavePetRequest
        {
            OwnerId = owner.OwnerId,
            Name = "Scoped Programme Buddy",
            Breed = "Collie",
            Sex = "male",
            IsActive = true
        });
        Assert.Equal(HttpStatusCode.Created, petCreate.StatusCode);
        var pet = await petCreate.Content.ReadFromJsonAsync<PetDetailVm>();
        Assert.NotNull(pet);

        var caseCreate = await _client.PostAsJsonAsync("/api/cases", new SaveTreatmentCaseRequest
        {
            PetId = pet.PetId,
            CaseTitle = "Scoped Programme Case",
            ClinicalSummary = "ownership test",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = "active"
        });
        Assert.Equal(HttpStatusCode.Created, caseCreate.StatusCode);
        var treatmentCase = await caseCreate.Content.ReadFromJsonAsync<CaseDetailVm>();
        Assert.NotNull(treatmentCase);

        var draftCreate = await _client.PostAsync($"/api/cases/{treatmentCase.TreatmentCaseId}/programmes", content: null);
        Assert.Equal(HttpStatusCode.OK, draftCreate.StatusCode);
        var programme = await draftCreate.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(programme);

        var session = Assert.Single(programme.Sessions);
        var exercises = await _client.GetFromJsonAsync<List<ExerciseListItem>>("/api/exercises?activeOnly=true");
        Assert.NotNull(exercises);
        Assert.NotEmpty(exercises);

        var add = await _client.PostAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/sessions/{session.SessionId}/exercises",
            new AddSessionExerciseRequest { ExerciseId = exercises[0].ExerciseId });
        Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);

        var refreshed = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(refreshed);
        var editRow = Assert.Single(Assert.Single(refreshed.Sessions).Exercises);

        using var otherClient = _factory.CreateClient();
        otherClient.DefaultRequestHeaders.Add("X-Practitioner-Id", "2");

        var update = await otherClient.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}",
            new ProgrammeBuilderForm
            {
                ProgrammeId = programme.ProgrammeId,
                Exercises =
                [
                    new ProgrammeBuilderForm.SessionExerciseEdit
                    {
                        SessionExerciseId = editRow.SessionExerciseId,
                        Reps = 99,
                        Sets = editRow.Sets,
                        HoldSeconds = editRow.HoldSeconds,
                        SortOrder = editRow.SortOrder,
                        Notes = editRow.Notes,
                    }
                ]
            });

        // Cases are not practitioner-specific: another practitioner can take over
        // and edit the programme (e.g. covering for a colleague who is off sick).
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var afterUpdate = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(afterUpdate);
        var updatedRow = Assert.Single(Assert.Single(afterUpdate.Sessions).Exercises);
        Assert.Equal((ushort?)99, updatedRow.Reps);
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
