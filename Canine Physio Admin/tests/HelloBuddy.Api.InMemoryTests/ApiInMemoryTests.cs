using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Data.Entities;
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
    private readonly Factory _factory;

    public ApiInMemoryTests(Factory factory)
    {
        _factory = factory;
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

    [Fact]
    public async Task ExerciseCrud_CreateListAndFilter_WorksInMemory()
    {
        var categories = await _client.GetFromJsonAsync<List<ExerciseCategoryListItem>>("/api/exercise-categories");
        Assert.NotNull(categories);
        var category = Assert.Single(categories.Where(x => x.CategoryName == "Strength"));

        var request = new SaveExerciseRequest
        {
            ExerciseCategoryId = category.ExerciseCategoryId,
            Title = $"In-memory exercise {Guid.NewGuid():N}",
            ObjectiveSummary = "In-memory summary",
            VideoUrl = "https://example.test/exercise.mp4",
            IsActive = true,
            Instructions =
            [
                new SaveExerciseRequest.InstructionStepInput { StepNumber = 1, InstructionText = "Step one cue." },
                new SaveExerciseRequest.InstructionStepInput { StepNumber = 2, InstructionText = "Step two cue." }
            ]
        };

        var create = await _client.PostAsJsonAsync("/api/exercises", request);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<ExerciseDetailVm>();
        Assert.NotNull(created);
        Assert.Equal(2, created.Instructions.Count);

        var listByTitle = await _client.GetFromJsonAsync<List<ExerciseListItem>>(
            $"/api/exercises?searchText={Uri.EscapeDataString(request.Title)}&activeOnly=true");
        Assert.NotNull(listByTitle);
        Assert.Contains(listByTitle, x => x.ExerciseId == created.ExerciseId);

        var listByInstruction = await _client.GetFromJsonAsync<List<ExerciseListItem>>(
            "/api/exercises?searchText=Step%20two%20cue.&activeOnly=true");
        Assert.NotNull(listByInstruction);
        Assert.Contains(listByInstruction, x => x.ExerciseId == created.ExerciseId);
    }

    [Fact]
    public async Task ExerciseMediaUpload_WithValidPng_ReturnsUrl()
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
        form.Add(fileContent, "file", "exercise-test.png");

        var response = await _client.PostAsync("/api/exercises/media", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ExerciseMediaUploadResponse>();
        Assert.NotNull(payload);
        Assert.Contains("exercise-media/", payload.Url, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("image/png", payload.ContentType);
    }

    [Fact]
    public async Task ExerciseMediaUpload_WithInvalidFileType_ReturnsBadRequest()
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent([0x00, 0x01, 0x02, 0x03]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "file", "exercise-test.exe");

        var response = await _client.PostAsync("/api/exercises/media", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDraftProgramme_FromCaseCreatesBuilderTarget()
    {
        var ownerCreate = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Draft",
            LastName = "Owner",
            Email = $"draft-owner-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, ownerCreate.StatusCode);
        var owner = await ownerCreate.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var petCreate = await _client.PostAsJsonAsync("/api/pets", new SavePetRequest
        {
            OwnerId = owner.OwnerId,
            Name = "Draft Buddy",
            Breed = "Labrador",
            Sex = "male",
            IsActive = true
        });
        Assert.Equal(HttpStatusCode.Created, petCreate.StatusCode);
        var pet = await petCreate.Content.ReadFromJsonAsync<PetDetailVm>();
        Assert.NotNull(pet);

        var caseCreate = await _client.PostAsJsonAsync("/api/cases", new SaveTreatmentCaseRequest
        {
            PetId = pet.PetId,
            CaseTitle = "Draft Case",
            ClinicalSummary = "Case used for draft programme creation.",
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
    }

    [Fact]
    public async Task ProgrammeDelete_DraftWithoutVersionHistory_ReturnsNoContent()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();

        var delete = await _client.DeleteAsync($"/api/programmes/{programme.ProgrammeId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await _client.GetAsync($"/api/programmes/{programme.ProgrammeId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task ProgrammeDelete_WithVersionHistory_ReturnsConflict()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CaninePhysioDbContext>();
            db.Programmeversions.Add(new Programmeversion
            {
                ProgrammeId = programme.ProgrammeId,
                VersionNumber = 1,
                VersionStatus = "published",
                PayloadJson = "{}",
                PayloadSchemaVersion = "1.0.0",
                CreatedByPractitionerId = 1,
                CreatedDate = DateTime.UtcNow,
                PublishedDate = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        var delete = await _client.DeleteAsync($"/api/programmes/{programme.ProgrammeId}");
        Assert.Equal(HttpStatusCode.Conflict, delete.StatusCode);
    }

    [Fact]
    public async Task ProgrammeStructure_UpdateToAmPm_RebuildsSessions()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();

        var update = await _client.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/structure",
            new ProgrammeStructureForm
            {
                ProgrammeId = programme.ProgrammeId,
                ProgrammeName = $"{programme.ProgrammeName} AMPM",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(14)),
                SessionStructure = "am-pm",
            });

        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var get = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(get);
        Assert.Equal(2, get.Sessions.Count);
        Assert.Contains(get.Sessions, s => s.Period == "AM");
        Assert.Contains(get.Sessions, s => s.Period == "PM");
    }

    [Fact]
    public async Task Programme_AddAndRemoveExerciseInSession_Works()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();
        var session = Assert.Single(programme.Sessions);

        var exercises = await _client.GetFromJsonAsync<List<ExerciseListItem>>("/api/exercises?activeOnly=true");
        Assert.NotNull(exercises);
        Assert.NotEmpty(exercises);
        var exerciseId = exercises[0].ExerciseId;

        var add = await _client.PostAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/sessions/{session.SessionId}/exercises",
            new AddSessionExerciseRequest { ExerciseId = exerciseId });
        Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);

        var refreshed = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(refreshed);
        var refreshedSession = Assert.Single(refreshed.Sessions);
        var sessionExercise = Assert.Single(refreshedSession.Exercises);

        var remove = await _client.DeleteAsync(
            $"/api/programmes/{programme.ProgrammeId}/sessions/{session.SessionId}/exercises/{sessionExercise.SessionExerciseId}");
        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);

        var afterRemove = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(afterRemove);
        Assert.Empty(Assert.Single(afterRemove.Sessions).Exercises);
    }

    [Fact]
    public async Task ProgrammePublish_IncompleteDraft_ReturnsBadRequest()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();

        var publish = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/publish", content: null);
        Assert.Equal(HttpStatusCode.BadRequest, publish.StatusCode);
    }

    [Fact]
    public async Task ProgrammeStatus_ActivateThenComplete_Works()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();

        var activate = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/activate", content: null);
        Assert.Equal(HttpStatusCode.NoContent, activate.StatusCode);

        var afterActivate = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(afterActivate);
        Assert.Equal(ProgrammeDomainConstants.StatusActive, afterActivate.Status);

        var complete = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/complete", content: null);
        Assert.Equal(HttpStatusCode.NoContent, complete.StatusCode);

        var afterComplete = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(afterComplete);
        Assert.Equal(ProgrammeDomainConstants.StatusCompleted, afterComplete.Status);
    }

    [Fact]
    public async Task ProgrammeStatus_ActivateBlocked_WhenAnotherProgrammeIsActive()
    {
        var (treatmentCase, programmeOne) = await CreateDraftProgrammeForDeleteAsync();

        var secondCreate = await _client.PostAsync($"/api/cases/{treatmentCase.TreatmentCaseId}/programmes", content: null);
        Assert.Equal(HttpStatusCode.OK, secondCreate.StatusCode);
        var programmeTwo = await secondCreate.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(programmeTwo);

        var activateFirst = await _client.PostAsync($"/api/programmes/{programmeOne.ProgrammeId}/activate", content: null);
        Assert.Equal(HttpStatusCode.NoContent, activateFirst.StatusCode);

        var activateSecond = await _client.PostAsync($"/api/programmes/{programmeTwo.ProgrammeId}/activate", content: null);
        Assert.Equal(HttpStatusCode.Conflict, activateSecond.StatusCode);
    }

    private async Task<(CaseDetailVm treatmentCase, ProgrammeVm programme)> CreateDraftProgrammeForDeleteAsync()
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
            Breed = "Labrador",
            Sex = "male",
            IsActive = true
        });
        Assert.Equal(HttpStatusCode.Created, petCreate.StatusCode);
        var pet = await petCreate.Content.ReadFromJsonAsync<PetDetailVm>();
        Assert.NotNull(pet);

        var caseCreate = await _client.PostAsJsonAsync("/api/cases", new SaveTreatmentCaseRequest
        {
            PetId = pet.PetId,
            CaseTitle = "Delete Case",
            ClinicalSummary = "Case used for programme delete tests.",
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

        return (treatmentCase, programme);
    }

    [Fact]
    public async Task ExerciseUpdate_ImageReplacement_DeletesManagedOrphan_WhenPolicyEnabled()
    {
        static MultipartFormDataContent BuildUpload(string fileName)
        {
            var form = new MultipartFormDataContent();
            var pngBytes = new byte[]
            {
                0x89, 0x50, 0x4E, 0x47,
                0x0D, 0x0A, 0x1A, 0x0A,
                0x00, 0x00, 0x00, 0x0D,
                0x49, 0x48, 0x44, 0x52
            };

            var fileContent = new ByteArrayContent(pngBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(fileContent, "file", fileName);
            return form;
        }

        using var upload1 = BuildUpload("orphan-before.png");
        var upload1Response = await _client.PostAsync("/api/exercises/media", upload1);
        Assert.Equal(HttpStatusCode.OK, upload1Response.StatusCode);
        var media1 = await upload1Response.Content.ReadFromJsonAsync<ExerciseMediaUploadResponse>();
        Assert.NotNull(media1);

        using var upload2 = BuildUpload("orphan-after.png");
        var upload2Response = await _client.PostAsync("/api/exercises/media", upload2);
        Assert.Equal(HttpStatusCode.OK, upload2Response.StatusCode);
        var media2 = await upload2Response.Content.ReadFromJsonAsync<ExerciseMediaUploadResponse>();
        Assert.NotNull(media2);

        var categories = await _client.GetFromJsonAsync<List<ExerciseCategoryListItem>>("/api/exercise-categories");
        Assert.NotNull(categories);
        var category = Assert.Single(categories.Where(x => x.CategoryName == "Strength"));

        var createRequest = new SaveExerciseRequest
        {
            ExerciseCategoryId = category.ExerciseCategoryId,
            Title = $"Orphan policy exercise {Guid.NewGuid():N}",
            ObjectiveSummary = "Orphan policy test",
            ImageUrl = media1.Url,
            IsActive = true,
            Instructions =
            [
                new SaveExerciseRequest.InstructionStepInput { StepNumber = 1, InstructionText = "Step one." }
            ]
        };

        var create = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<ExerciseDetailVm>();
        Assert.NotNull(created);

        var updateRequest = new SaveExerciseRequest
        {
            ExerciseCategoryId = createRequest.ExerciseCategoryId,
            Title = createRequest.Title,
            ObjectiveSummary = createRequest.ObjectiveSummary,
            ImageUrl = media2.Url,
            IsActive = true,
            Instructions = createRequest.Instructions
        };

        var update = await _client.PutAsJsonAsync($"/api/exercises/{created.ExerciseId}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var firstPath = new Uri(media1.Url).LocalPath;
        Assert.False(File.Exists(firstPath));
    }

    public sealed class Factory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"hello-buddy-api-tests-{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("InMemoryTesting");
            builder.UseSetting("ConnectionStrings:CaninePhysioDb", "Server=localhost;Database=unused;User=unused;Password=unused");
            builder.UseSetting("PdfService:Uri", "http://localhost:18080");
            builder.UseSetting("Seed:ExerciseLibrary:Enabled", "true");
            builder.UseSetting("Storage:ExerciseMediaOrphanCleanupMode", "DeleteManagedOrphans");

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
