using System.Net;
using System.Net.Http.Json;
using HelloBuddy.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using MySqlConnector;
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

    [Fact]
    public async Task CaseDraftProgramme_CreateFromCase_WorksAgainstDatabase()
    {
        var ownerCreate = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Programme",
            LastName = "Owner",
            Email = $"programme-owner-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, ownerCreate.StatusCode);
        var owner = await ownerCreate.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var petCreate = await _client.PostAsJsonAsync("/api/pets", new SavePetRequest
        {
            OwnerId = owner.OwnerId,
            Name = "Programme Buddy",
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
            CaseTitle = "Programme Draft Case",
            ClinicalSummary = "Integration test for draft programme creation.",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = "active"
        });
        Assert.Equal(HttpStatusCode.Created, caseCreate.StatusCode);
        var treatmentCase = await caseCreate.Content.ReadFromJsonAsync<CaseDetailVm>();
        Assert.NotNull(treatmentCase);

        var createProgramme = await _client.PostAsync($"/api/cases/{treatmentCase.TreatmentCaseId}/programmes", content: null);
        Assert.Equal(HttpStatusCode.OK, createProgramme.StatusCode);

        var programme = await createProgramme.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(programme);
        Assert.Equal(treatmentCase.TreatmentCaseId, programme.TreatmentCaseId);
        Assert.Equal("planned", programme.Status);
        Assert.Single(programme.Sessions);
        Assert.Equal("single", programme.Sessions[0].Period);

        var fetchedCase = await _client.GetAsync($"/api/cases/{treatmentCase.TreatmentCaseId}");
        Assert.Equal(HttpStatusCode.OK, fetchedCase.StatusCode);
        var caseVm = await fetchedCase.Content.ReadFromJsonAsync<CaseDetailVm>();
        Assert.NotNull(caseVm);
        Assert.Contains(caseVm.Programmes, p => p.ProgrammeId == programme.ProgrammeId);
    }

    [Fact]
    public async Task CaseDraftProgramme_DeleteFromCase_WorksAgainstDatabase()
    {
        var ownerCreate = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Delete",
            LastName = "Owner",
            Email = $"delete-owner-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, ownerCreate.StatusCode);
        var owner = await ownerCreate.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var petCreate = await _client.PostAsJsonAsync("/api/pets", new SavePetRequest
        {
            OwnerId = owner.OwnerId,
            Name = "Delete Buddy",
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
            CaseTitle = "Programme Delete Case",
            ClinicalSummary = "Integration test for programme deletion.",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = "active"
        });
        Assert.Equal(HttpStatusCode.Created, caseCreate.StatusCode);
        var treatmentCase = await caseCreate.Content.ReadFromJsonAsync<CaseDetailVm>();
        Assert.NotNull(treatmentCase);

        var createProgramme = await _client.PostAsync($"/api/cases/{treatmentCase.TreatmentCaseId}/programmes", content: null);
        Assert.Equal(HttpStatusCode.OK, createProgramme.StatusCode);
        var programme = await createProgramme.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(programme);

        var deleteProgramme = await _client.DeleteAsync($"/api/programmes/{programme.ProgrammeId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteProgramme.StatusCode);

        var getProgramme = await _client.GetAsync($"/api/programmes/{programme.ProgrammeId}");
        Assert.Equal(HttpStatusCode.NotFound, getProgramme.StatusCode);
    }

    [Fact]
    public async Task CaseDraftProgramme_DeleteWithVersionHistoryForceDeletes_ReturnsNoContent()
    {
        var ownerCreate = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Blocked",
            LastName = "Owner",
            Email = $"blocked-owner-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, ownerCreate.StatusCode);
        var owner = await ownerCreate.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var petCreate = await _client.PostAsJsonAsync("/api/pets", new SavePetRequest
        {
            OwnerId = owner.OwnerId,
            Name = "Blocked Buddy",
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
            CaseTitle = "Programme Blocked Delete Case",
            ClinicalSummary = "Integration test for blocked programme deletion.",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = "active"
        });
        Assert.Equal(HttpStatusCode.Created, caseCreate.StatusCode);
        var treatmentCase = await caseCreate.Content.ReadFromJsonAsync<CaseDetailVm>();
        Assert.NotNull(treatmentCase);

        var createProgramme = await _client.PostAsync($"/api/cases/{treatmentCase.TreatmentCaseId}/programmes", content: null);
        Assert.Equal(HttpStatusCode.OK, createProgramme.StatusCode);
        var programme = await createProgramme.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(programme);

        await using (var connection = new MySqlConnection(GetIntegrationDbConnectionString()))
        {
            await connection.OpenAsync();
            const string sql = @"
                INSERT INTO ProgrammeVersion
                    (ProgrammeId, VersionNumber, VersionStatus, PayloadJson, PayloadSchemaVersion, CreatedByPractitionerId, CreatedDate, PublishedDate)
                VALUES
                    (@programmeId, 1, 'published', @payloadJson, '1.0.0', 1, UTC_TIMESTAMP(), UTC_TIMESTAMP())";

            await using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@programmeId", programme.ProgrammeId);
            cmd.Parameters.AddWithValue("@payloadJson", "{}");
            await cmd.ExecuteNonQueryAsync();
        }

        // Force-delete removes version history and succeeds.
        var deleteProgramme = await _client.DeleteAsync($"/api/programmes/{programme.ProgrammeId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteProgramme.StatusCode);

        var getProgramme = await _client.GetAsync($"/api/programmes/{programme.ProgrammeId}");
        Assert.Equal(HttpStatusCode.NotFound, getProgramme.StatusCode);
    }

    [Fact]
    public async Task CaseDraftProgramme_StructureAndExerciseMutations_WorkAgainstDatabase()
    {
        var ownerCreate = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Builder",
            LastName = "Owner",
            Email = $"builder-owner-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, ownerCreate.StatusCode);
        var owner = await ownerCreate.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var petCreate = await _client.PostAsJsonAsync("/api/pets", new SavePetRequest
        {
            OwnerId = owner.OwnerId,
            Name = "Builder Buddy",
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
            CaseTitle = "Programme Builder Case",
            ClinicalSummary = "Integration test for builder mutations.",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = "active"
        });
        Assert.Equal(HttpStatusCode.Created, caseCreate.StatusCode);
        var treatmentCase = await caseCreate.Content.ReadFromJsonAsync<CaseDetailVm>();
        Assert.NotNull(treatmentCase);

        var createProgramme = await _client.PostAsync($"/api/cases/{treatmentCase.TreatmentCaseId}/programmes", content: null);
        Assert.Equal(HttpStatusCode.OK, createProgramme.StatusCode);
        var programme = await createProgramme.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(programme);

        var updateStructure = await _client.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/structure",
            new ProgrammeStructureForm
            {
                ProgrammeId = programme.ProgrammeId,
                ProgrammeName = $"{programme.ProgrammeName} AMPM",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(21)),
                SessionStructure = "am-pm",
            });
        Assert.Equal(HttpStatusCode.NoContent, updateStructure.StatusCode);

        var afterStructure = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(afterStructure);
        var targetSession = Assert.Single(afterStructure.Sessions.Where(s => s.Period == "AM"));

        var exercises = await _client.GetFromJsonAsync<List<ExerciseListItem>>("/api/exercises?activeOnly=true");
        Assert.NotNull(exercises);
        Assert.NotEmpty(exercises);

        var addExercise = await _client.PostAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/sessions/{targetSession.SessionId}/exercises",
            new AddSessionExerciseRequest { ExerciseId = exercises[0].ExerciseId });
        Assert.Equal(HttpStatusCode.NoContent, addExercise.StatusCode);

        var afterAdd = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(afterAdd);
        var sessionAfterAdd = Assert.Single(afterAdd.Sessions.Where(s => s.SessionId == targetSession.SessionId));
        var sessionExercise = Assert.Single(sessionAfterAdd.Exercises);

        var removeExercise = await _client.DeleteAsync(
            $"/api/programmes/{programme.ProgrammeId}/sessions/{targetSession.SessionId}/exercises/{sessionExercise.SessionExerciseId}");
        Assert.Equal(HttpStatusCode.NoContent, removeExercise.StatusCode);

        var publish = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/publish", content: null);
        Assert.Equal(HttpStatusCode.BadRequest, publish.StatusCode);
    }

    [Fact]
    public async Task CaseDraftProgramme_ActivateThenComplete_WorksAgainstDatabase()
    {
        var ownerCreate = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Status",
            LastName = "Owner",
            Email = $"status-owner-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, ownerCreate.StatusCode);
        var owner = await ownerCreate.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var petCreate = await _client.PostAsJsonAsync("/api/pets", new SavePetRequest
        {
            OwnerId = owner.OwnerId,
            Name = "Status Buddy",
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
            CaseTitle = "Programme Status Case",
            ClinicalSummary = "Integration test for programme status transitions.",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = "active"
        });
        Assert.Equal(HttpStatusCode.Created, caseCreate.StatusCode);
        var treatmentCase = await caseCreate.Content.ReadFromJsonAsync<CaseDetailVm>();
        Assert.NotNull(treatmentCase);

        var createProgramme = await _client.PostAsync($"/api/cases/{treatmentCase.TreatmentCaseId}/programmes", content: null);
        Assert.Equal(HttpStatusCode.OK, createProgramme.StatusCode);
        var programme = await createProgramme.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(programme);

        var activate = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/activate", content: null);
        Assert.Equal(HttpStatusCode.NoContent, activate.StatusCode);

        var afterActivate = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(afterActivate);
        Assert.Equal("active", afterActivate.Status);

        var complete = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/complete", content: null);
        Assert.Equal(HttpStatusCode.NoContent, complete.StatusCode);

        var afterComplete = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(afterComplete);
        Assert.Equal("completed", afterComplete.Status);
    }

    [Fact]
    public async Task CaseDraftProgramme_ActivateBlockedWhenAnotherActiveExists_ReturnsConflict()
    {
        var ownerCreate = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Conflict",
            LastName = "Owner",
            Email = $"conflict-owner-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, ownerCreate.StatusCode);
        var owner = await ownerCreate.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var petCreate = await _client.PostAsJsonAsync("/api/pets", new SavePetRequest
        {
            OwnerId = owner.OwnerId,
            Name = "Conflict Buddy",
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
            CaseTitle = "Programme Status Conflict Case",
            ClinicalSummary = "Integration test for single active programme rule.",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = "active"
        });
        Assert.Equal(HttpStatusCode.Created, caseCreate.StatusCode);
        var treatmentCase = await caseCreate.Content.ReadFromJsonAsync<CaseDetailVm>();
        Assert.NotNull(treatmentCase);

        var createOne = await _client.PostAsync($"/api/cases/{treatmentCase.TreatmentCaseId}/programmes", content: null);
        Assert.Equal(HttpStatusCode.OK, createOne.StatusCode);
        var programmeOne = await createOne.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(programmeOne);

        var createTwo = await _client.PostAsync($"/api/cases/{treatmentCase.TreatmentCaseId}/programmes", content: null);
        Assert.Equal(HttpStatusCode.OK, createTwo.StatusCode);
        var programmeTwo = await createTwo.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(programmeTwo);

        var activateFirst = await _client.PostAsync($"/api/programmes/{programmeOne.ProgrammeId}/activate", content: null);
        Assert.Equal(HttpStatusCode.NoContent, activateFirst.StatusCode);

        var activateSecond = await _client.PostAsync($"/api/programmes/{programmeTwo.ProgrammeId}/activate", content: null);
        Assert.Equal(HttpStatusCode.Conflict, activateSecond.StatusCode);
    }

    private static string GetIntegrationDbConnectionString()
        => Environment.GetEnvironmentVariable("HELLOBUDDY_TEST_DB_CONNECTION")
           ?? Environment.GetEnvironmentVariable("ConnectionStrings__CaninePhysioDb")
           ?? Environment.GetEnvironmentVariable("MYSQLCONNSTR_CaninePhysioDb")
           ?? "Server=localhost;Port=3306;Database=canine_physiotherapy;User=root;Password=P3nyf@n01;SslMode=None";

    public sealed class Factory : WebApplicationFactory<Program>
    {
        private string _dbConnection = string.Empty;
        private int _resetAttempted;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var dbConnection =
                Environment.GetEnvironmentVariable("HELLOBUDDY_TEST_DB_CONNECTION")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__CaninePhysioDb")
                ?? Environment.GetEnvironmentVariable("MYSQLCONNSTR_CaninePhysioDb")
                ?? "Server=localhost;Port=3306;Database=canine_physiotherapy;User=root;Password=P3nyf@n01;SslMode=None";

            _dbConnection = dbConnection;

            // Ensure schema/data is aligned before the API host starts; startup
            // seeding queries exercise attribution columns introduced in Increment 8.
            IntegrationTestDatabaseReset.ResetToSeedAsync(_dbConnection)
                .GetAwaiter()
                .GetResult();

            builder.UseEnvironment("IntegrationTesting");
            builder.UseSetting("ConnectionStrings:CaninePhysioDb", dbConnection);
            builder.UseSetting("PdfService:Uri", "http://localhost:18080");
            builder.UseSetting("Seed:ExerciseLibrary:Enabled", "true");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing
                && !string.IsNullOrWhiteSpace(_dbConnection)
                && Interlocked.Exchange(ref _resetAttempted, 1) == 0)
            {
                IntegrationTestDatabaseReset.ResetToSeedAsync(_dbConnection)
                    .GetAwaiter()
                    .GetResult();
            }

            base.Dispose(disposing);
        }
    }
}