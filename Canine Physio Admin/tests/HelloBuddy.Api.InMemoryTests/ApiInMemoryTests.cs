using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Data.Entities;
using HelloBuddy.Admin.Pdf;
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
    public async Task OwnerDataControl_NoLinkedRecords_DeletesOwner()
    {
        var create = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Erase",
            LastName = "Owner",
            Email = $"erase-owner-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var owner = await create.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var control = await _client.PostAsync($"/api/owners/{owner.OwnerId}/data-control", content: null);
        Assert.Equal(HttpStatusCode.OK, control.StatusCode);
        var payload = await control.Content.ReadFromJsonAsync<OwnerDataControlResponse>();
        Assert.NotNull(payload);
        Assert.Equal("deleted", payload.Outcome);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CaninePhysioDbContext>();
            var audit = await db.Auditlogs
                .Where(a => a.EntityName == "owner" && a.EntityId == owner.OwnerId && a.ActionType == "gdpr-deletion")
                .OrderByDescending(a => a.ActionDateTime)
                .FirstOrDefaultAsync();
            Assert.NotNull(audit);
            Assert.Equal((ulong)1, audit.PractitionerId);
            Assert.Contains("deleted", audit.NewValuesJson ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        var get = await _client.GetAsync($"/api/owners/{owner.OwnerId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task OwnerDataControl_WithLinkedPet_DeletesOwnerAndLinkedRecords()
    {
        var ownerCreate = await _client.PostAsJsonAsync("/api/owners", new SaveOwnerRequest
        {
            FirstName = "Sensitive",
            LastName = "Owner",
            Email = $"sensitive-owner-{Guid.NewGuid():N}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, ownerCreate.StatusCode);
        var owner = await ownerCreate.Content.ReadFromJsonAsync<OwnerDetailVm>();
        Assert.NotNull(owner);

        var petCreate = await _client.PostAsJsonAsync("/api/pets", new SavePetRequest
        {
            OwnerId = owner.OwnerId,
            Name = "Buddy",
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
            CaseTitle = "Retention Case",
            ClinicalSummary = "Retained for clinical history.",
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

        var casesList = await _client.GetFromJsonAsync<List<CaseRow>>("/api/cases");
        Assert.NotNull(casesList);
        Assert.DoesNotContain(casesList, x => x.TreatmentCaseId == treatmentCase.TreatmentCaseId);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CaninePhysioDbContext>();
            var audit = await db.Auditlogs
                .Where(a => a.EntityName == "owner" && a.EntityId == owner.OwnerId && a.ActionType == "gdpr-deletion")
                .OrderByDescending(a => a.ActionDateTime)
                .FirstOrDefaultAsync();
            Assert.NotNull(audit);
            Assert.Equal((ulong)1, audit.PractitionerId);
            Assert.Contains("deleted", audit.NewValuesJson ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            var retainedPet = await db.Pets.FirstOrDefaultAsync(x => x.PetId == pet.PetId);
            Assert.Null(retainedPet);

            var retainedCase = await db.Treatmentcases.FirstOrDefaultAsync(x => x.TreatmentCaseId == treatmentCase.TreatmentCaseId);
            Assert.Null(retainedCase);
        }
    }

    [Fact]
    public async Task OwnerDataControl_ForUnlinkedPractitioner_ReturnsNotFound()
    {
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
        Assert.Equal(HttpStatusCode.NotFound, control.StatusCode);
    }

    [Fact]
    public async Task ApiRequests_WithoutPractitionerHeader_ReturnUnauthorized()
    {
        using var client = new Factory().CreateClient();
        var response = await client.GetAsync("/api/owners");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoints_WithoutPractitionerHeader_ReturnUnauthorized()
    {
        using var client = new Factory().CreateClient();

        var response = await client.GetAsync("/api/admin/practitioners");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoints_NonAdminRole_ReturnForbiddenForAllRoutes()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Practitioner-Id", "2");
        client.DefaultRequestHeaders.Add("X-Practitioner-Role", "physiotherapist");

        var requests = new List<HttpRequestMessage>
        {
            new(HttpMethod.Get, "/api/admin/practitioners"),
            new(HttpMethod.Post, "/api/admin/practitioners")
            {
                Content = JsonContent.Create(new
                {
                    FirstName = "Non",
                    LastName = "Admin",
                    Email = "non-admin@example.test",
                    PhoneNumber = "07123 456789",
                    Role = "physiotherapist",
                    InitialPassword = "not-used-in-policy-test"
                })
            },
            new(HttpMethod.Put, "/api/admin/practitioners/1")
            {
                Content = JsonContent.Create(new
                {
                    FirstName = "Renamed",
                    LastName = "User",
                    Email = "renamed@example.test",
                    PhoneNumber = "07123 456789"
                })
            },
            new(HttpMethod.Post, "/api/admin/practitioners/1/set-password")
            {
                Content = JsonContent.Create(new { NewPassword = "Password12345!" })
            },
            new(HttpMethod.Post, "/api/admin/change-password")
            {
                Content = JsonContent.Create(new
                {
                    CurrentPassword = "Current123!",
                    NewPassword = "New12345!"
                })
            },
            new(HttpMethod.Delete, "/api/admin/practitioners/1")
        };

        foreach (var request in requests)
        {
            if (request.Content is not null)
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
                {
                    CharSet = Encoding.UTF8.WebName
                };
            }

            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task CaseNotes_AddNoteAppearsAndPersistsAcrossReload()
    {
        var (treatmentCase, _) = await CreateDraftProgrammeForDeleteAsync();

        var addNote = await _client.PostAsJsonAsync(
            $"/api/cases/{treatmentCase.TreatmentCaseId}/notes",
            new CreateCaseNoteRequest
            {
                NoteType = "follow-up",
                NoteText = "Owner reports improved comfort after weekend exercises.",
            });
        Assert.Equal(HttpStatusCode.OK, addNote.StatusCode);

        var getCase = await _client.GetFromJsonAsync<CaseDetailVm>($"/api/cases/{treatmentCase.TreatmentCaseId}");
        Assert.NotNull(getCase);
        Assert.Contains(getCase.Notes, n => n.NoteText.Contains("improved comfort", StringComparison.OrdinalIgnoreCase));

        var getCaseReload = await _client.GetFromJsonAsync<CaseDetailVm>($"/api/cases/{treatmentCase.TreatmentCaseId}");
        Assert.NotNull(getCaseReload);
        Assert.Contains(getCaseReload.Notes, n => n.NoteText.Contains("improved comfort", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CaseNotes_EmptyNoteText_ReturnsBadRequest()
    {
        var (treatmentCase, _) = await CreateDraftProgrammeForDeleteAsync();

        var addNote = await _client.PostAsJsonAsync(
            $"/api/cases/{treatmentCase.TreatmentCaseId}/notes",
            new CreateCaseNoteRequest
            {
                NoteType = "follow-up",
                NoteText = "   ",
            });
        Assert.Equal(HttpStatusCode.BadRequest, addNote.StatusCode);
    }

    [Fact]
    public async Task CaseNotes_UpdateNote_PersistsNewTextAndType()
    {
        var (treatmentCase, _) = await CreateDraftProgrammeForDeleteAsync();

        var addNote = await _client.PostAsJsonAsync(
            $"/api/cases/{treatmentCase.TreatmentCaseId}/notes",
            new CreateCaseNoteRequest { NoteType = "follow-up", NoteText = "Original note text." });
        Assert.Equal(HttpStatusCode.OK, addNote.StatusCode);
        var created = await addNote.Content.ReadFromJsonAsync<CaseDetailVm.NoteRow>();
        Assert.NotNull(created);

        var update = await _client.PutAsJsonAsync(
            $"/api/cases/{treatmentCase.TreatmentCaseId}/notes/{created.TreatmentCaseNoteId}",
            new CreateCaseNoteRequest { NoteType = "assessment", NoteText = "Revised note text." });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var getCase = await _client.GetFromJsonAsync<CaseDetailVm>($"/api/cases/{treatmentCase.TreatmentCaseId}");
        Assert.NotNull(getCase);
        var stored = Assert.Single(getCase.Notes, n => n.TreatmentCaseNoteId == created.TreatmentCaseNoteId);
        Assert.Equal("assessment", stored.NoteType);
        Assert.Equal("Revised note text.", stored.NoteText);
    }

    [Fact]
    public async Task CaseNotes_UpdateMissingNote_ReturnsNotFound()
    {
        var (treatmentCase, _) = await CreateDraftProgrammeForDeleteAsync();

        var update = await _client.PutAsJsonAsync(
            $"/api/cases/{treatmentCase.TreatmentCaseId}/notes/999999",
            new CreateCaseNoteRequest { NoteType = "assessment", NoteText = "No such note." });
        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
    }

    [Fact]
    public async Task CaseNotes_DeleteNote_RemovesFromCase()
    {
        var (treatmentCase, _) = await CreateDraftProgrammeForDeleteAsync();

        var addNote = await _client.PostAsJsonAsync(
            $"/api/cases/{treatmentCase.TreatmentCaseId}/notes",
            new CreateCaseNoteRequest { NoteType = "follow-up", NoteText = "Note to be deleted." });
        Assert.Equal(HttpStatusCode.OK, addNote.StatusCode);
        var created = await addNote.Content.ReadFromJsonAsync<CaseDetailVm.NoteRow>();
        Assert.NotNull(created);

        var delete = await _client.DeleteAsync(
            $"/api/cases/{treatmentCase.TreatmentCaseId}/notes/{created.TreatmentCaseNoteId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var getCase = await _client.GetFromJsonAsync<CaseDetailVm>($"/api/cases/{treatmentCase.TreatmentCaseId}");
        Assert.NotNull(getCase);
        Assert.DoesNotContain(getCase.Notes, n => n.TreatmentCaseNoteId == created.TreatmentCaseNoteId);
    }

    [Fact]
    public async Task CaseNotes_DeleteMissingNote_ReturnsNotFound()
    {
        var (treatmentCase, _) = await CreateDraftProgrammeForDeleteAsync();

        var delete = await _client.DeleteAsync(
            $"/api/cases/{treatmentCase.TreatmentCaseId}/notes/999999");
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
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
    public async Task ProgrammeDelete_WithVersionHistory_ForceDeletes()
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
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await _client.GetAsync($"/api/programmes/{programme.ProgrammeId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
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
    public async Task ProgrammeUpdate_AllowsNullHoldSeconds()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();
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
        var updatedSession = Assert.Single(refreshed.Sessions);
        var exercise = Assert.Single(updatedSession.Exercises);

        var update = await _client.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}",
            new ProgrammeBuilderForm
            {
                ProgrammeId = programme.ProgrammeId,
                Exercises =
                [
                    new ProgrammeBuilderForm.SessionExerciseEdit
                    {
                        SessionExerciseId = exercise.SessionExerciseId,
                        Reps = 10,
                        Sets = 3,
                        HoldSeconds = null,
                        SortOrder = exercise.SortOrder,
                        Notes = "Hold is intentionally nullable.",
                    }
                ]
            });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var vm = await update.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(vm);

        var persistedExercise = Assert.Single(Assert.Single(vm.Sessions).Exercises);
        Assert.Null(persistedExercise.HoldSeconds);
    }

    [Fact]
    public async Task ProgrammeUpdate_ReordersAndRenumbersSessionExercises_WhenSortOrdersChange()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();
        var session = Assert.Single(programme.Sessions);

        var categories = await _client.GetFromJsonAsync<List<ExerciseCategoryListItem>>("/api/exercise-categories");
        Assert.NotNull(categories);
        var category = Assert.Single(categories.Where(x => x.CategoryName == "Strength"));

        var exerciseIds = new List<ulong>();
        for (var i = 1; i <= 6; i++)
        {
            var createExercise = await _client.PostAsJsonAsync("/api/exercises", new SaveExerciseRequest
            {
                ExerciseCategoryId = category.ExerciseCategoryId,
                Title = $"Sort test exercise {Guid.NewGuid():N}-{i}",
                ObjectiveSummary = "Sort re-numbering test",
                IsActive = true,
                Instructions = [new SaveExerciseRequest.InstructionStepInput { StepNumber = 1, InstructionText = "Step" }],
            });
            Assert.Equal(HttpStatusCode.Created, createExercise.StatusCode);

            var createdExercise = await createExercise.Content.ReadFromJsonAsync<ExerciseDetailVm>();
            Assert.NotNull(createdExercise);
            exerciseIds.Add(createdExercise.ExerciseId);

            var add = await _client.PostAsJsonAsync(
                $"/api/programmes/{programme.ProgrammeId}/sessions/{session.SessionId}/exercises",
                new AddSessionExerciseRequest { ExerciseId = createdExercise.ExerciseId });
            Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);
        }

        var refreshed = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(refreshed);
        var refreshedSession = Assert.Single(refreshed.Sessions);
        Assert.Equal(6, refreshedSession.Exercises.Count);

        var byExerciseId = refreshedSession.Exercises.ToDictionary(x => x.ExerciseId);
        var first = byExerciseId[exerciseIds[0]];
        var second = byExerciseId[exerciseIds[1]];
        var third = byExerciseId[exerciseIds[2]];
        var fourth = byExerciseId[exerciseIds[3]];
        var fifth = byExerciseId[exerciseIds[4]];
        var sixth = byExerciseId[exerciseIds[5]];

        var update = await _client.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}",
            new ProgrammeBuilderForm
            {
                ProgrammeId = programme.ProgrammeId,
                Exercises =
                [
                    new ProgrammeBuilderForm.SessionExerciseEdit { SessionExerciseId = first.SessionExerciseId, Reps = first.Reps, Sets = first.Sets, HoldSeconds = first.HoldSeconds, SortOrder = 1, Notes = first.Notes },
                    new ProgrammeBuilderForm.SessionExerciseEdit { SessionExerciseId = second.SessionExerciseId, Reps = second.Reps, Sets = second.Sets, HoldSeconds = second.HoldSeconds, SortOrder = 4, Notes = second.Notes },
                    new ProgrammeBuilderForm.SessionExerciseEdit { SessionExerciseId = third.SessionExerciseId, Reps = third.Reps, Sets = third.Sets, HoldSeconds = third.HoldSeconds, SortOrder = 5, Notes = third.Notes },
                    new ProgrammeBuilderForm.SessionExerciseEdit { SessionExerciseId = fourth.SessionExerciseId, Reps = fourth.Reps, Sets = fourth.Sets, HoldSeconds = fourth.HoldSeconds, SortOrder = 6, Notes = fourth.Notes },
                    new ProgrammeBuilderForm.SessionExerciseEdit { SessionExerciseId = fifth.SessionExerciseId, Reps = fifth.Reps, Sets = fifth.Sets, HoldSeconds = fifth.HoldSeconds, SortOrder = 3, Notes = fifth.Notes },
                    new ProgrammeBuilderForm.SessionExerciseEdit { SessionExerciseId = sixth.SessionExerciseId, Reps = sixth.Reps, Sets = sixth.Sets, HoldSeconds = sixth.HoldSeconds, SortOrder = 2, Notes = sixth.Notes },
                ]
            });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var vm = await update.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(vm);

        var reordered = Assert.Single(vm.Sessions).Exercises;
        var expectedSortOrders = new[] { 1, 2, 3, 4, 5, 6 };
        var actualSortOrders = reordered.Select(x => (int)x.SortOrder).ToArray();
        Assert.True(expectedSortOrders.SequenceEqual(actualSortOrders));

        var expectedSessionExerciseOrder = new[]
        {
            first.SessionExerciseId,
            sixth.SessionExerciseId,
            fifth.SessionExerciseId,
            second.SessionExerciseId,
            third.SessionExerciseId,
            fourth.SessionExerciseId,
        };
        var actualSessionExerciseOrder = reordered.Select(x => x.SessionExerciseId).ToArray();
        Assert.True(expectedSessionExerciseOrder.SequenceEqual(actualSessionExerciseOrder));
    }

    [Theory]
    [InlineData(4, new[] { 1, 2, 3, 6, 4, 5 })]
    [InlineData(1, new[] { 6, 1, 2, 3, 4, 5 })]
    public async Task ProgrammeUpdate_MovingLastExerciseUp_LandsInRequestedSlot(int requestedSortOrder, int[] expectedOrder)
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();
        var session = Assert.Single(programme.Sessions);

        var categories = await _client.GetFromJsonAsync<List<ExerciseCategoryListItem>>("/api/exercise-categories");
        Assert.NotNull(categories);
        var category = Assert.Single(categories.Where(x => x.CategoryName == "Strength"));

        var exerciseIds = new List<ulong>();
        for (var i = 1; i <= 6; i++)
        {
            var createExercise = await _client.PostAsJsonAsync("/api/exercises", new SaveExerciseRequest
            {
                ExerciseCategoryId = category.ExerciseCategoryId,
                Title = $"Move-up sort test {Guid.NewGuid():N}-{i}",
                ObjectiveSummary = "Move-up sort regression test",
                IsActive = true,
                Instructions = [new SaveExerciseRequest.InstructionStepInput { StepNumber = 1, InstructionText = "Step" }],
            });
            Assert.Equal(HttpStatusCode.Created, createExercise.StatusCode);

            var createdExercise = await createExercise.Content.ReadFromJsonAsync<ExerciseDetailVm>();
            Assert.NotNull(createdExercise);
            exerciseIds.Add(createdExercise.ExerciseId);

            var add = await _client.PostAsJsonAsync(
                $"/api/programmes/{programme.ProgrammeId}/sessions/{session.SessionId}/exercises",
                new AddSessionExerciseRequest { ExerciseId = createdExercise.ExerciseId });
            Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);
        }

        var refreshed = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(refreshed);
        var refreshedSession = Assert.Single(refreshed.Sessions);
        var orderedExercises = refreshedSession.Exercises.OrderBy(x => x.SortOrder).ToList();

        var update = await _client.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}",
            new ProgrammeBuilderForm
            {
                ProgrammeId = programme.ProgrammeId,
                Exercises = orderedExercises
                    .Select((exercise, index) => new ProgrammeBuilderForm.SessionExerciseEdit
                    {
                        SessionExerciseId = exercise.SessionExerciseId,
                        Reps = exercise.Reps,
                        Sets = exercise.Sets,
                        HoldSeconds = exercise.HoldSeconds,
                        SortOrder = index == 5 ? (ushort)requestedSortOrder : exercise.SortOrder,
                        Notes = exercise.Notes,
                    })
                    .ToList()
            });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var vm = await update.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(vm);

        var reordered = Assert.Single(vm.Sessions).Exercises;
        Assert.True(new[] { 1, 2, 3, 4, 5, 6 }.SequenceEqual(reordered.Select(x => (int)x.SortOrder)));

        var expectedSessionExerciseIds = expectedOrder
            .Select(position => orderedExercises[position - 1].SessionExerciseId)
            .ToArray();
        var actualSessionExerciseIds = reordered.Select(x => x.SessionExerciseId).ToArray();
        Assert.True(expectedSessionExerciseIds.SequenceEqual(actualSessionExerciseIds));
    }

    [Fact]
    public async Task ProgrammeUpdate_MultiSession_ReordersSessionTwoWithoutAffectingSessionOne()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();

        var structureUpdate = await _client.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/structure",
            new ProgrammeStructureForm
            {
                ProgrammeId = programme.ProgrammeId,
                ProgrammeName = $"{programme.ProgrammeName} AMPM",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(14)),
                SessionStructure = "am-pm",
            });
        Assert.Equal(HttpStatusCode.NoContent, structureUpdate.StatusCode);

        var categories = await _client.GetFromJsonAsync<List<ExerciseCategoryListItem>>("/api/exercise-categories");
        Assert.NotNull(categories);
        var category = Assert.Single(categories.Where(x => x.CategoryName == "Strength"));

        var afterStructure = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(afterStructure);
        Assert.Equal(2, afterStructure.Sessions.Count);

        var sessions = afterStructure.Sessions.OrderBy(s => s.SortOrder).ToList();
        var sessionOne = sessions[0];
        var sessionTwo = sessions[1];

        for (var i = 1; i <= 6; i++)
        {
            var createExercise = await _client.PostAsJsonAsync("/api/exercises", new SaveExerciseRequest
            {
                ExerciseCategoryId = category.ExerciseCategoryId,
                Title = $"Multi-session sort test {Guid.NewGuid():N}-{i}",
                ObjectiveSummary = "Multi-session sort test",
                IsActive = true,
                Instructions = [new SaveExerciseRequest.InstructionStepInput { StepNumber = 1, InstructionText = "Step" }],
            });
            Assert.Equal(HttpStatusCode.Created, createExercise.StatusCode);

            var createdExercise = await createExercise.Content.ReadFromJsonAsync<ExerciseDetailVm>();
            Assert.NotNull(createdExercise);

            var addSessionOne = await _client.PostAsJsonAsync(
                $"/api/programmes/{programme.ProgrammeId}/sessions/{sessionOne.SessionId}/exercises",
                new AddSessionExerciseRequest { ExerciseId = createdExercise.ExerciseId });
            Assert.Equal(HttpStatusCode.NoContent, addSessionOne.StatusCode);

            var addSessionTwo = await _client.PostAsJsonAsync(
                $"/api/programmes/{programme.ProgrammeId}/sessions/{sessionTwo.SessionId}/exercises",
                new AddSessionExerciseRequest { ExerciseId = createdExercise.ExerciseId });
            Assert.Equal(HttpStatusCode.NoContent, addSessionTwo.StatusCode);
        }

        var beforeUpdate = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(beforeUpdate);

        var beforeSessionOne = beforeUpdate.Sessions.OrderBy(s => s.SortOrder).First();
        var beforeSessionTwo = beforeUpdate.Sessions.OrderBy(s => s.SortOrder).Skip(1).First();
        var orderedSessionOne = beforeSessionOne.Exercises.OrderBy(x => x.SortOrder).ToList();
        var orderedSessionTwo = beforeSessionTwo.Exercises.OrderBy(x => x.SortOrder).ToList();

        var edits = new List<ProgrammeBuilderForm.SessionExerciseEdit>();

        foreach (var exercise in orderedSessionOne)
        {
            edits.Add(new ProgrammeBuilderForm.SessionExerciseEdit
            {
                SessionExerciseId = exercise.SessionExerciseId,
                Reps = exercise.Reps,
                Sets = exercise.Sets,
                HoldSeconds = exercise.HoldSeconds,
                SortOrder = exercise.SortOrder,
                Notes = exercise.Notes,
            });
        }

        for (var i = 0; i < orderedSessionTwo.Count; i++)
        {
            var exercise = orderedSessionTwo[i];
            edits.Add(new ProgrammeBuilderForm.SessionExerciseEdit
            {
                SessionExerciseId = exercise.SessionExerciseId,
                Reps = exercise.Reps,
                Sets = exercise.Sets,
                HoldSeconds = exercise.HoldSeconds,
                SortOrder = i == orderedSessionTwo.Count - 1 ? (ushort)1 : exercise.SortOrder,
                Notes = exercise.Notes,
            });
        }

        var update = await _client.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}",
            new ProgrammeBuilderForm
            {
                ProgrammeId = programme.ProgrammeId,
                Exercises = edits,
            });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var vm = await update.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(vm);

        var updatedSessionOne = vm.Sessions.OrderBy(s => s.SortOrder).First();
        var updatedSessionTwo = vm.Sessions.OrderBy(s => s.SortOrder).Skip(1).First();

        Assert.True(new[] { 1, 2, 3, 4, 5, 6 }.SequenceEqual(updatedSessionOne.Exercises.Select(x => (int)x.SortOrder)));
        Assert.True(new[] { 1, 2, 3, 4, 5, 6 }.SequenceEqual(updatedSessionTwo.Exercises.Select(x => (int)x.SortOrder)));

        var expectedSessionOneOrder = orderedSessionOne.Select(x => x.SessionExerciseId).ToArray();
        var actualSessionOneOrder = updatedSessionOne.Exercises.Select(x => x.SessionExerciseId).ToArray();
        Assert.True(expectedSessionOneOrder.SequenceEqual(actualSessionOneOrder));

        var expectedSessionTwoOrder = new[]
        {
            orderedSessionTwo[5].SessionExerciseId,
            orderedSessionTwo[0].SessionExerciseId,
            orderedSessionTwo[1].SessionExerciseId,
            orderedSessionTwo[2].SessionExerciseId,
            orderedSessionTwo[3].SessionExerciseId,
            orderedSessionTwo[4].SessionExerciseId,
        };
        var actualSessionTwoOrder = updatedSessionTwo.Exercises.Select(x => x.SessionExerciseId).ToArray();
        Assert.True(expectedSessionTwoOrder.SequenceEqual(actualSessionTwoOrder));
    }

    [Fact]
    public async Task ProgrammeUpdate_MultiSession_ReordersSessionOneWithoutAffectingSessionTwo()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();

        var structureUpdate = await _client.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/structure",
            new ProgrammeStructureForm
            {
                ProgrammeId = programme.ProgrammeId,
                ProgrammeName = $"{programme.ProgrammeName} AMPM",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(14)),
                SessionStructure = "am-pm",
            });
        Assert.Equal(HttpStatusCode.NoContent, structureUpdate.StatusCode);

        var categories = await _client.GetFromJsonAsync<List<ExerciseCategoryListItem>>("/api/exercise-categories");
        Assert.NotNull(categories);
        var category = Assert.Single(categories.Where(x => x.CategoryName == "Strength"));

        var afterStructure = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(afterStructure);
        Assert.Equal(2, afterStructure.Sessions.Count);

        var sessions = afterStructure.Sessions.OrderBy(s => s.SortOrder).ToList();
        var sessionOne = sessions[0];
        var sessionTwo = sessions[1];

        for (var i = 1; i <= 6; i++)
        {
            var createExercise = await _client.PostAsJsonAsync("/api/exercises", new SaveExerciseRequest
            {
                ExerciseCategoryId = category.ExerciseCategoryId,
                Title = $"Multi-session sort mirror test {Guid.NewGuid():N}-{i}",
                ObjectiveSummary = "Multi-session sort mirror test",
                IsActive = true,
                Instructions = [new SaveExerciseRequest.InstructionStepInput { StepNumber = 1, InstructionText = "Step" }],
            });
            Assert.Equal(HttpStatusCode.Created, createExercise.StatusCode);

            var createdExercise = await createExercise.Content.ReadFromJsonAsync<ExerciseDetailVm>();
            Assert.NotNull(createdExercise);

            var addSessionOne = await _client.PostAsJsonAsync(
                $"/api/programmes/{programme.ProgrammeId}/sessions/{sessionOne.SessionId}/exercises",
                new AddSessionExerciseRequest { ExerciseId = createdExercise.ExerciseId });
            Assert.Equal(HttpStatusCode.NoContent, addSessionOne.StatusCode);

            var addSessionTwo = await _client.PostAsJsonAsync(
                $"/api/programmes/{programme.ProgrammeId}/sessions/{sessionTwo.SessionId}/exercises",
                new AddSessionExerciseRequest { ExerciseId = createdExercise.ExerciseId });
            Assert.Equal(HttpStatusCode.NoContent, addSessionTwo.StatusCode);
        }

        var beforeUpdate = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(beforeUpdate);

        var beforeSessionOne = beforeUpdate.Sessions.OrderBy(s => s.SortOrder).First();
        var beforeSessionTwo = beforeUpdate.Sessions.OrderBy(s => s.SortOrder).Skip(1).First();
        var orderedSessionOne = beforeSessionOne.Exercises.OrderBy(x => x.SortOrder).ToList();
        var orderedSessionTwo = beforeSessionTwo.Exercises.OrderBy(x => x.SortOrder).ToList();

        var edits = new List<ProgrammeBuilderForm.SessionExerciseEdit>();

        for (var i = 0; i < orderedSessionOne.Count; i++)
        {
            var exercise = orderedSessionOne[i];
            edits.Add(new ProgrammeBuilderForm.SessionExerciseEdit
            {
                SessionExerciseId = exercise.SessionExerciseId,
                Reps = exercise.Reps,
                Sets = exercise.Sets,
                HoldSeconds = exercise.HoldSeconds,
                SortOrder = i == orderedSessionOne.Count - 1 ? (ushort)1 : exercise.SortOrder,
                Notes = exercise.Notes,
            });
        }

        foreach (var exercise in orderedSessionTwo)
        {
            edits.Add(new ProgrammeBuilderForm.SessionExerciseEdit
            {
                SessionExerciseId = exercise.SessionExerciseId,
                Reps = exercise.Reps,
                Sets = exercise.Sets,
                HoldSeconds = exercise.HoldSeconds,
                SortOrder = exercise.SortOrder,
                Notes = exercise.Notes,
            });
        }

        var update = await _client.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}",
            new ProgrammeBuilderForm
            {
                ProgrammeId = programme.ProgrammeId,
                Exercises = edits,
            });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var vm = await update.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(vm);

        var updatedSessionOne = vm.Sessions.OrderBy(s => s.SortOrder).First();
        var updatedSessionTwo = vm.Sessions.OrderBy(s => s.SortOrder).Skip(1).First();

        Assert.True(new[] { 1, 2, 3, 4, 5, 6 }.SequenceEqual(updatedSessionOne.Exercises.Select(x => (int)x.SortOrder)));
        Assert.True(new[] { 1, 2, 3, 4, 5, 6 }.SequenceEqual(updatedSessionTwo.Exercises.Select(x => (int)x.SortOrder)));

        var expectedSessionOneOrder = new[]
        {
            orderedSessionOne[5].SessionExerciseId,
            orderedSessionOne[0].SessionExerciseId,
            orderedSessionOne[1].SessionExerciseId,
            orderedSessionOne[2].SessionExerciseId,
            orderedSessionOne[3].SessionExerciseId,
            orderedSessionOne[4].SessionExerciseId,
        };
        var actualSessionOneOrder = updatedSessionOne.Exercises.Select(x => x.SessionExerciseId).ToArray();
        Assert.True(expectedSessionOneOrder.SequenceEqual(actualSessionOneOrder));

        var expectedSessionTwoOrder = orderedSessionTwo.Select(x => x.SessionExerciseId).ToArray();
        var actualSessionTwoOrder = updatedSessionTwo.Exercises.Select(x => x.SessionExerciseId).ToArray();
        Assert.True(expectedSessionTwoOrder.SequenceEqual(actualSessionTwoOrder));
    }

    [Fact]
    public async Task ProgrammePublish_IncompleteDraft_ReturnsBadRequest()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();

        var publish = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/publish", content: null);
        Assert.Equal(HttpStatusCode.BadRequest, publish.StatusCode);
    }

    [Fact]
    public async Task ProgrammePublish_ValidDraft_CreatesImmutablePublishedVersion()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();
        var session = Assert.Single(programme.Sessions);

        var exercises = await _client.GetFromJsonAsync<List<ExerciseListItem>>("/api/exercises?activeOnly=true");
        Assert.NotNull(exercises);
        Assert.NotEmpty(exercises);

        var add = await _client.PostAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/sessions/{session.SessionId}/exercises",
            new AddSessionExerciseRequest { ExerciseId = exercises[0].ExerciseId });
        Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);

        var publish = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/publish", content: null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);

        var response = await publish.Content.ReadFromJsonAsync<PublishResponse>();
        Assert.NotNull(response);
        Assert.Matches(@"^programme-\d+-\d{8}-\d{6}\.pdf$", response.FileName);
        Assert.True(response.Bytes > 0);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CaninePhysioDbContext>();

        var persistedProgramme = await db.Programmes
            .Include(p => p.Programmeversions)
            .FirstAsync(p => p.ProgrammeId == programme.ProgrammeId);

        Assert.True(persistedProgramme.CurrentProgrammeVersionId.HasValue);

        var version = Assert.Single(persistedProgramme.Programmeversions);
        Assert.Equal("published", version.VersionStatus);
        Assert.NotNull(version.PublishedDate);
        Assert.Equal((uint)1, version.VersionNumber);
        Assert.Equal((ulong)1, version.CreatedByPractitionerId);
        Assert.False(string.IsNullOrWhiteSpace(version.PayloadJson));
    }

    [Fact]
    public async Task ProgrammeVersionsEndpoint_AfterPublish_ReturnsHistory()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();
        var session = Assert.Single(programme.Sessions);

        var exercises = await _client.GetFromJsonAsync<List<ExerciseListItem>>("/api/exercises?activeOnly=true");
        Assert.NotNull(exercises);
        Assert.NotEmpty(exercises);

        var add = await _client.PostAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/sessions/{session.SessionId}/exercises",
            new AddSessionExerciseRequest { ExerciseId = exercises[0].ExerciseId });
        Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);

        var publish = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/publish", content: null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);

        var history = await _client.GetFromJsonAsync<ProgrammeVersionHistoryVm>($"/api/programmes/{programme.ProgrammeId}/versions");
        Assert.NotNull(history);
        Assert.Equal(programme.ProgrammeId, history.ProgrammeId);
        Assert.NotEmpty(history.Versions);
        Assert.Contains(history.Versions, v => v.VersionStatus == "published");
    }

    [Fact]
    public async Task ProgrammeCreateDraftFromPublished_AfterPublish_CreatesNewDraftProgramme()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();
        var session = Assert.Single(programme.Sessions);

        var exercises = await _client.GetFromJsonAsync<List<ExerciseListItem>>("/api/exercises?activeOnly=true");
        Assert.NotNull(exercises);
        Assert.NotEmpty(exercises);

        var add = await _client.PostAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/sessions/{session.SessionId}/exercises",
            new AddSessionExerciseRequest { ExerciseId = exercises[0].ExerciseId });
        Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);

        var publish = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/publish", content: null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);

        var createDraft = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/draft-from-published", content: null);
        Assert.Equal(HttpStatusCode.OK, createDraft.StatusCode);

        var newDraft = await createDraft.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(newDraft);
        Assert.NotEqual(programme.ProgrammeId, newDraft.ProgrammeId);
        Assert.Equal(programme.TreatmentCaseId, newDraft.TreatmentCaseId);
        Assert.Equal("planned", newDraft.Status);
        Assert.NotEmpty(newDraft.Sessions);
        Assert.NotEmpty(newDraft.Sessions.SelectMany(s => s.Exercises));
    }

    [Fact]
    public async Task ProgrammePublish_LocksProgrammeAgainstFurtherEdits()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();
        var session = Assert.Single(programme.Sessions);

        var exercises = await _client.GetFromJsonAsync<List<ExerciseListItem>>("/api/exercises?activeOnly=true");
        Assert.NotNull(exercises);
        Assert.NotEmpty(exercises);

        var add = await _client.PostAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/sessions/{session.SessionId}/exercises",
            new AddSessionExerciseRequest { ExerciseId = exercises[0].ExerciseId });
        Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);

        var beforePublish = await _client.GetFromJsonAsync<ProgrammeVm>($"/api/programmes/{programme.ProgrammeId}");
        Assert.NotNull(beforePublish);
        var persistedExercise = Assert.Single(Assert.Single(beforePublish.Sessions).Exercises);

        var publish = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/publish", content: null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);

        var update = await _client.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}",
            new ProgrammeBuilderForm
            {
                ProgrammeId = programme.ProgrammeId,
                Exercises =
                [
                    new ProgrammeBuilderForm.SessionExerciseEdit
                    {
                        SessionExerciseId = persistedExercise.SessionExerciseId,
                        Reps = persistedExercise.Reps,
                        Sets = persistedExercise.Sets,
                        HoldSeconds = persistedExercise.HoldSeconds,
                        SortOrder = persistedExercise.SortOrder,
                        Notes = "Attempted update after publish",
                    }
                ]
            });
        Assert.Equal(HttpStatusCode.Conflict, update.StatusCode);

        var structureUpdate = await _client.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/structure",
            new ProgrammeStructureForm
            {
                ProgrammeId = programme.ProgrammeId,
                ProgrammeName = "Post-publish change",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(7)),
                SessionStructure = "single",
            });
        Assert.Equal(HttpStatusCode.Conflict, structureUpdate.StatusCode);

        var addAfterPublish = await _client.PostAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/sessions/{session.SessionId}/exercises",
            new AddSessionExerciseRequest { ExerciseId = exercises[0].ExerciseId });
        Assert.Equal(HttpStatusCode.Conflict, addAfterPublish.StatusCode);

        var removeAfterPublish = await _client.DeleteAsync(
            $"/api/programmes/{programme.ProgrammeId}/sessions/{session.SessionId}/exercises/{persistedExercise.SessionExerciseId}");
        Assert.Equal(HttpStatusCode.Conflict, removeAfterPublish.StatusCode);
    }

    [Fact]
    public async Task ProgrammePublishedDownloadUrl_WithPublishedFileName_ReturnsReadUrl()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();
        var session = Assert.Single(programme.Sessions);

        var exercises = await _client.GetFromJsonAsync<List<ExerciseListItem>>("/api/exercises?activeOnly=true");
        Assert.NotNull(exercises);
        Assert.NotEmpty(exercises);

        var add = await _client.PostAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}/sessions/{session.SessionId}/exercises",
            new AddSessionExerciseRequest { ExerciseId = exercises[0].ExerciseId });
        Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);

        var publish = await _client.PostAsync($"/api/programmes/{programme.ProgrammeId}/publish", content: null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);
        var published = await publish.Content.ReadFromJsonAsync<PublishResponse>();
        Assert.NotNull(published);

        var getUrl = await _client.GetAsync($"/api/programmes/published/{Uri.EscapeDataString(published.FileName)}/download-url");
        Assert.Equal(HttpStatusCode.OK, getUrl.StatusCode);

        var download = await getUrl.Content.ReadFromJsonAsync<DownloadUrlResponse>();
        Assert.NotNull(download);
        Assert.Contains("/dev-published/", download.Url);
    }

    [Fact]
    public async Task ProgrammePublishedDownloadUrl_WithInvalidFileName_ReturnsBadRequest()
    {
        var getUrl = await _client.GetAsync("/api/programmes/published/not-a-published-file-name/download-url");
        Assert.Equal(HttpStatusCode.BadRequest, getUrl.StatusCode);
    }

    [Fact]
    public async Task ProgrammePreviewPdf_WithVisibleProgramme_ReturnsPdfBytes()
    {
        var (_, programme) = await CreateDraftProgrammeForDeleteAsync();
        var preview = await _client.GetAsync($"/api/programmes/{programme.ProgrammeId}/preview-pdf");

        Assert.Equal(HttpStatusCode.OK, preview.StatusCode);
        Assert.Equal("application/pdf", preview.Content.Headers.ContentType?.MediaType);
        var bytes = await preview.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public async Task ProgrammePdfTemplate_WithImageAndVideo_RendersImageInsideLeftMediaBoxLinkingToVideo()
    {
        var template = new RazorProgrammePdfTemplate();
        var vm = BuildProgrammeVmWithExercise(
            imageUrl: "https://media.example.test/step-up.jpg",
            videoUrl: "https://media.example.test/step-up.mp4");

        var html = await template.RenderAsync(vm);

        // The left media placeholder box exists and the image is rendered inside it,
        // wrapped by the video link (clicking the image opens the video).
        var mediaBox = Regex.Match(html, "<div class=\"ex-media\">.*?</div>", RegexOptions.Singleline);
        Assert.True(mediaBox.Success, "Expected an ex-media placeholder box in the PDF HTML.");
        Assert.Matches(
            "<a href=\"https://media.example.test/step-up.mp4\">\\s*<img src=\"https://media.example.test/step-up.jpg\"",
            mediaBox.Value);
        Assert.DoesNotContain("Watch video", html);
    }

    [Fact]
    public async Task ProgrammePdfTemplate_WithoutImage_RendersPlaceholderBox()
    {
        var template = new RazorProgrammePdfTemplate();
        var vm = BuildProgrammeVmWithExercise(imageUrl: null, videoUrl: null);

        var html = await template.RenderAsync(vm);

        var mediaBox = Regex.Match(html, "<div class=\"ex-media\">.*?</div>", RegexOptions.Singleline);
        Assert.True(mediaBox.Success, "Expected an ex-media placeholder box even when no image exists.");
        Assert.Contains("No image", mediaBox.Value);
    }

    [Fact]
    public async Task ProgrammePdfTemplate_HeaderContainsLogoImage_NotHbPlaceholder()
    {
        var template = new RazorProgrammePdfTemplate();
        var vm = BuildProgrammeVmWithExercise(imageUrl: null, videoUrl: null);

        var html = await template.RenderAsync(vm);

        // HB text placeholder must be replaced by the real logo (ERR-AT-011).
        Assert.DoesNotContain(">HB<", html);
        // Logo must be inlined as a base64 SVG data URI so it renders in headless Chromium.
        Assert.Contains("data:image/svg+xml;base64,", html);
    }

    private static ProgrammeVm BuildProgrammeVmWithExercise(string? imageUrl, string? videoUrl)
        => new(
            1,
            1,
            1,
            1,
            "Buddy Hind Limb Rehab",
            "planned",
            new DateOnly(2026, 5, 1),
            null,
            "Improving hind-limb control.",
            "Buddy Hind Limb Rehab",
            "Buddy",
            "Amelia Carter",
            "Amelia Carter",
            [
                new ProgrammeVm.SessionRow(
                    1,
                    "single",
                    "Improving hind-limb control.",
                    "planned",
                    1,
                    [
                        new ProgrammeVm.SessionExerciseRow(
                            1,
                            1,
                            "Step-ups (low)",
                            "Controlled stepping",
                            imageUrl,
                            videoUrl,
                            5,
                            3,
                            5,
                            1,
                            "Steady pace")
                    ])
            ]);

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

        var session = Assert.Single(programme.Sessions);
        var update = await _client.PutAsJsonAsync(
            $"/api/programmes/{programme.ProgrammeId}",
            new ProgrammeBuilderForm
            {
                ProgrammeId = programme.ProgrammeId,
                Sessions =
                [
                    new ProgrammeBuilderForm.SessionEdit
                    {
                        SessionId = session.SessionId,
                        Objective = "Session purpose summary for integration tests.",
                    }
                ],
            });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var updatedProgramme = await update.Content.ReadFromJsonAsync<ProgrammeVm>();
        Assert.NotNull(updatedProgramme);

        return (treatmentCase, updatedProgramme);
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
            builder.UseSetting("Seed:PractitionerLogin:Enabled", "true");
            builder.UseSetting("Seed:PractitionerLogin:InitialPassword", "InMemoryTestsOnlyPassword123!");
            builder.UseSetting("Storage:ExerciseMediaOrphanCleanupMode", "DeleteManagedOrphans");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<CaninePhysioDbContext>>();
                services.RemoveAll<CaninePhysioDbContext>();
                services.RemoveAll<IDbContextOptionsConfiguration<CaninePhysioDbContext>>();
                services.RemoveAll<IPdfRenderer>();

                services.AddDbContext<CaninePhysioDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
                services.AddSingleton<IPdfRenderer, StubPdfRenderer>();
            });
        }

        private sealed class StubPdfRenderer : IPdfRenderer
        {
            private static readonly byte[] MinimalPdf = "%PDF-1.4\n%stub\n"u8.ToArray();

            public Task<byte[]> RenderAsync(string html, CancellationToken ct = default)
                => Task.FromResult(MinimalPdf);
        }
    }
}
