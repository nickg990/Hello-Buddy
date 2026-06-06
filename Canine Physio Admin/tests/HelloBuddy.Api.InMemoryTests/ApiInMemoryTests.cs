using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
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
