using System.Net;
using System.Text;
using HelloBuddy.Contracts;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace HelloBuddy.Ui.Tests;

public sealed class UiSmokeTests : IClassFixture<UiSmokeTests.Factory>
{
    private readonly HttpClient _client;

    public UiSmokeTests(Factory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/Owners", "Owners")]
    [InlineData("/Pets", "Pets")]
    [InlineData("/Cases", "Treatment cases")]
    [InlineData("/Exercises", "Exercise library")]
    [InlineData("/Owners/1", "Amelia Carter")]
    [InlineData("/Pets/1", "Buddy")]
    [InlineData("/Cases/1", "Buddy Hind Limb Rehab")]
    [InlineData("/Exercises/1", "Step-ups (low)")]
    public async Task GetPage_GivenSmokeRoute_ReturnsExpectedText(string path, string expectedText)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(expectedText, html);
    }

    [Fact]
    public async Task EditExercisePage_RendersCurrentAndSelectedMediaPanels()
    {
        var response = await _client.GetAsync("/Exercises/1/Edit");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Media", html);
        Assert.Contains("Current image", html);
        Assert.Contains("Selected image (pending save)", html);
        Assert.Contains("Current video", html);
        Assert.Contains("Selected video (pending save)", html);
        Assert.Contains("/Exercises/1/Image", html);
        Assert.Contains("id=\"selected-image-link\"", html);
        Assert.Contains("id=\"selected-image-preview\"", html);
        Assert.Contains("id=\"selected-video-link\"", html);
        Assert.Contains("id=\"open-video-search\"", html);
        Assert.Contains("id=\"video-search-provider\"", html);
        Assert.Contains(">Google Drive<", html);
        Assert.DoesNotContain("Image URL (optional manual override)", html);
        Assert.DoesNotContain("Remove current image on save", html);
    }

    [Fact]
    public async Task CreateExercisePage_RendersSelectedImagePanelWithoutCurrentImagePanel()
    {
        var response = await _client.GetAsync("/Exercises/Create");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Current image", html);
        Assert.DoesNotContain("Current video", html);
        Assert.Contains("Selected image", html);
        Assert.Contains("Selected video", html);
        Assert.Contains("No image selected", html);
        Assert.Contains("No video selected", html);
        Assert.Contains("id=\"selected-image-link\"", html);
        Assert.Contains("id=\"selected-image-preview\"", html);
        Assert.Contains("id=\"selected-video-link\"", html);
        Assert.Contains("id=\"open-video-search\"", html);
        Assert.Contains("id=\"video-search-provider\"", html);
        Assert.Contains(">Google Drive<", html);
        Assert.DoesNotContain("Image URL (optional manual override)", html);
        Assert.DoesNotContain("Remove current image on save", html);
    }

    [Fact]
    public async Task ExerciseDetailsPage_RendersHorizontalMediaTilesWithoutLinkLabels()
    {
        var response = await _client.GetAsync("/Exercises/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Media", html);
        Assert.Contains("Image", html);
        Assert.Contains("Video", html);
        Assert.Contains("d-flex flex-wrap gap-3", html);
        Assert.Contains("id=\"details-video-link\"", html);
        Assert.DoesNotContain(">View image<", html);
        Assert.DoesNotContain(">Open video<", html);
    }

    [Fact]
    public async Task CaseDetailPage_RendersCreateDraftProgrammeAction()
    {
        var response = await _client.GetAsync("/Cases/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Create draft programme", html);
        Assert.Contains("Delete programme", html);
    }

    [Fact]
    public async Task BuilderPage_RendersSessionStructureAndExerciseMutationControls()
    {
        var response = await _client.GetAsync("/Programmes/1/Builder");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Dates and session structure", html);
        Assert.Contains("Session structure", html);
        Assert.Contains("Add exercise", html);
        Assert.Contains("Remove exercise", html);
        Assert.Contains("Live preview", html);
        Assert.DoesNotContain("Open preview", html);
    }

    [Fact]
    public async Task BuilderEditorPanel_RendersExpectedContent()
    {
        var response = await _client.GetAsync("/Programmes/1/Builder/EditorPanel");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Dates and session structure", html);
        Assert.Contains("data-async-builder=\"true\"", html);
    }

    [Fact]
    public async Task BuilderPreviewPanel_RendersExpectedContent()
    {
        var response = await _client.GetAsync("/Programmes/1/Builder/PreviewPanel");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Live preview", html);
        Assert.Contains("preview-programme-name", html);
    }

    public class Factory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("Api:Uri", "https://example.test");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IAdminApiClient));
                services.AddSingleton<IAdminApiClient, StubAdminApiClient>();
            });
        }
    }

    private sealed class StubAdminApiClient : IAdminApiClient
    {
        private static readonly OwnerDetailVm Owner = new(
            1,
            "Amelia",
            "Carter",
            "amelia@example.test",
            "07123 456789",
            "1 High Street",
            null,
            "Leeds",
            "LS1 1AA",
            [new OwnerDetailVm.PetRow(1, "Buddy", "Labrador", "male", true, 1)],
            false);

        private static readonly PetDetailVm Pet = new(
            1,
            1,
            "Buddy",
            6,
            new DateOnly(2020, 1, 15),
            "Labrador",
            "male",
            28.4m,
            true,
            Owner.FullName,
            Owner.Email,
            [new PetDetailVm.CaseRow(1, "Buddy Hind Limb Rehab", "active", new DateOnly(2026, 5, 1))]);

        private static readonly CaseDetailVm TreatmentCase = new(
            1,
            "Buddy Hind Limb Rehab",
            "active",
            new DateOnly(2026, 5, 1),
            null,
            "Improving hind-limb control.",
            1,
            "Buddy",
            "Labrador",
            "male",
            28.4m,
            6,
            Owner.FullName,
            Owner.Email,
            [new CaseDetailVm.NoteRow(1, new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), "assessment", "Initial clinical assessment completed.")],
            [new CaseDetailVm.ProgrammeRow(1, "Buddy Hind Limb Rehab draft", "planned", new DateOnly(2026, 5, 1), null, 1, 0)]);

        private static readonly ExerciseDetailVm Exercise = new(
            1,
            1,
            "Strength",
            "step-up",
            "Step-ups (low)",
            "Controlled stepping exercise for hind-limb strength.",
            "https://example.test/step-up.jpg",
            "https://example.test/step-up.mp4",
            5,
            3,
            null,
            true,
            null,
            new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            [
                new ExerciseDetailVm.InstructionStepVm(1, "Lead dog onto the low step."),
                new ExerciseDetailVm.InstructionStepVm(2, "Pause and encourage controlled step down.")
            ]);

        public Task<IReadOnlyList<OwnerListItem>> ListOwnersAsync(bool includeAnonymised, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<OwnerListItem>>([new OwnerListItem(1, Owner.FullName, Owner.Email, Owner.PhoneNumber, 1, false)]);

        public Task<OwnerDetailVm?> GetOwnerAsync(ulong id, bool includeAnonymised, CancellationToken ct)
            => Task.FromResult<OwnerDetailVm?>(id == 1 ? Owner : null);

        public Task<OwnerDetailVm> CreateOwnerAsync(SaveOwnerRequest request, CancellationToken ct)
            => Task.FromResult(Owner);

        public Task<OwnerDetailVm?> UpdateOwnerAsync(ulong id, SaveOwnerRequest request, CancellationToken ct)
            => Task.FromResult<OwnerDetailVm?>(id == 1 ? Owner : null);

        public Task<OwnerDataControlClientResult> ApplyOwnerDataControlAsync(ulong id, CancellationToken ct)
            => Task.FromResult(new OwnerDataControlClientResult(
                OwnerDataControlClientOutcome.Anonymised,
                "Owner personal data was anonymised while linked clinical records were retained.",
                Owner));

        public Task<IReadOnlyList<PetListItem>> ListPetsAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<PetListItem>>([new PetListItem(1, 1, "Buddy", Owner.FullName, "Labrador", "male", true, 1)]);

        public Task<PetDetailVm?> GetPetAsync(ulong id, CancellationToken ct)
            => Task.FromResult<PetDetailVm?>(id == 1 ? Pet : null);

        public Task<PetDetailVm> CreatePetAsync(SavePetRequest request, CancellationToken ct)
            => Task.FromResult(Pet);

        public Task<PetDetailVm?> UpdatePetAsync(ulong id, SavePetRequest request, CancellationToken ct)
            => Task.FromResult<PetDetailVm?>(id == 1 ? Pet : null);

        public Task<IReadOnlyList<ExerciseListItem>> ListExercisesAsync(ExerciseListFilter filter, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ExerciseListItem>>([
                new ExerciseListItem(1, "step-up", Exercise.Title, 1, "Strength", Exercise.ObjectiveSummary, true, true, true, Exercise.UpdatedDate)
            ]);

        public Task<ExerciseDetailVm?> GetExerciseAsync(ulong id, CancellationToken ct)
            => Task.FromResult<ExerciseDetailVm?>(id == 1 ? Exercise : null);

        public Task<ExerciseImageContent?> GetExerciseImageAsync(ulong id, CancellationToken ct)
            => Task.FromResult<ExerciseImageContent?>(
                id == 1
                    ? new ExerciseImageContent([0x89, 0x50, 0x4E, 0x47], "image/png")
                    : null);

        public Task<ExerciseMediaUploadResponse> UploadExerciseImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct)
            => Task.FromResult(new ExerciseMediaUploadResponse(
                "https://example.test/exercise-upload.jpg",
                fileName,
                string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType,
                fileStream.CanSeek ? fileStream.Length : Encoding.UTF8.GetByteCount("stub")));

        public Task<ExerciseDetailVm> CreateExerciseAsync(SaveExerciseRequest request, CancellationToken ct)
            => Task.FromResult(Exercise);

        public Task<ExerciseDetailVm?> UpdateExerciseAsync(ulong id, SaveExerciseRequest request, CancellationToken ct)
            => Task.FromResult<ExerciseDetailVm?>(id == 1 ? Exercise : null);

        public Task<ExerciseDetailVm?> SetExerciseActiveAsync(ulong id, bool isActive, CancellationToken ct)
            => Task.FromResult<ExerciseDetailVm?>(id == 1 ? Exercise with { IsActive = isActive } : null);

        public Task<IReadOnlyList<ExerciseCategoryListItem>> ListExerciseCategoriesAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ExerciseCategoryListItem>>([
                new ExerciseCategoryListItem(1, "Strength", true),
                new ExerciseCategoryListItem(2, "Mobility", true)
            ]);

        public Task<ProgrammeVm?> CreateDraftProgrammeAsync(ulong caseId, CancellationToken ct)
            => Task.FromResult<ProgrammeVm?>(new ProgrammeVm(
                2,
                1,
                "Buddy Hind Limb Rehab draft",
                "planned",
                new DateOnly(2026, 5, 1),
                null,
                "Improving hind-limb control.",
                "Buddy Hind Limb Rehab",
                "Buddy",
                "Amelia Carter",
                [new ProgrammeVm.SessionRow(1, "single", "Improving hind-limb control.", "planned", 1, [])]));

        public Task<DeleteProgrammeResult> DeleteProgrammeAsync(ulong programmeId, CancellationToken ct)
            => Task.FromResult(new DeleteProgrammeResult(DeleteProgrammeOutcome.Deleted));

        public Task<ProgrammeStatusTransitionClientResult> ActivateProgrammeAsync(ulong programmeId, CancellationToken ct)
            => Task.FromResult(new ProgrammeStatusTransitionClientResult(ProgrammeStatusTransitionClientOutcome.Updated));

        public Task<ProgrammeStatusTransitionClientResult> CompleteProgrammeAsync(ulong programmeId, CancellationToken ct)
            => Task.FromResult(new ProgrammeStatusTransitionClientResult(ProgrammeStatusTransitionClientOutcome.Updated));

        public Task<UpdateProgrammeStructureResult> UpdateProgrammeStructureAsync(ulong programmeId, ProgrammeStructureForm form, CancellationToken ct)
            => Task.FromResult(new UpdateProgrammeStructureResult(UpdateProgrammeStructureOutcome.Updated));

        public Task<AddSessionExerciseClientResult> AddSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong exerciseId, CancellationToken ct)
            => Task.FromResult(new AddSessionExerciseClientResult(AddSessionExerciseClientOutcome.Added));

        public Task<RemoveSessionExerciseClientResult> RemoveSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong sessionExerciseId, CancellationToken ct)
            => Task.FromResult(new RemoveSessionExerciseClientResult(RemoveSessionExerciseClientOutcome.Removed));

        public Task<IReadOnlyList<CaseRow>> ListCasesAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<CaseRow>>([new CaseRow(1, TreatmentCase.CaseTitle, TreatmentCase.Status, TreatmentCase.StartDate, TreatmentCase.PetName, TreatmentCase.OwnerName)]);

        public Task<CaseDetailVm?> GetCaseAsync(ulong id, CancellationToken ct)
            => Task.FromResult<CaseDetailVm?>(id == 1 ? TreatmentCase : null);

        public Task<CaseDetailVm> CreateCaseAsync(SaveTreatmentCaseRequest request, CancellationToken ct)
            => Task.FromResult(TreatmentCase);

        public Task<CaseDetailVm?> UpdateCaseAsync(ulong id, SaveTreatmentCaseRequest request, CancellationToken ct)
            => Task.FromResult<CaseDetailVm?>(id == 1 ? TreatmentCase : null);

        public Task<CaseDetailVm.NoteRow?> AddCaseNoteAsync(ulong id, CreateCaseNoteRequest request, CancellationToken ct)
            => Task.FromResult<CaseDetailVm.NoteRow?>(new CaseDetailVm.NoteRow(2, DateTime.UtcNow, request.NoteType, request.NoteText));

        public Task<ProgrammeVm?> GetProgrammeAsync(ulong id, CancellationToken ct)
            => Task.FromResult<ProgrammeVm?>(new ProgrammeVm(
                1,
                1,
                "Buddy Hind Limb Rehab draft",
                "planned",
                new DateOnly(2026, 5, 1),
                null,
                "Improving hind-limb control.",
                "Buddy Hind Limb Rehab",
                "Buddy",
                "Amelia Carter",
                [new ProgrammeVm.SessionRow(
                    1,
                    "single",
                    "Improving hind-limb control.",
                    "planned",
                    1,
                    [new ProgrammeVm.SessionExerciseRow(1, 1, "Step-ups (low)", "Controlled stepping", "https://example.test/step-up.jpg", "https://example.test/step-up.mp4", 5, 3, 5, 1, "Steady pace")])]
            ));

        public Task<ProgrammeVersionHistoryVm?> GetProgrammeVersionHistoryAsync(ulong id, CancellationToken ct)
            => Task.FromResult<ProgrammeVersionHistoryVm?>(new ProgrammeVersionHistoryVm(
                1,
                "Buddy Hind Limb Rehab draft",
                1,
                [
                    new ProgrammeVersionHistoryVm.VersionRow(
                        1,
                        1,
                        "published",
                        "Initial publication.",
                        1,
                        "Amelia Carter",
                        DateTime.UtcNow.AddDays(-2),
                        DateTime.UtcNow.AddDays(-2),
                        null,
                        null)
                ]));

        public Task<CreateDraftFromPublishedClientResult> CreateDraftFromPublishedAsync(ulong id, CancellationToken ct)
            => Task.FromResult(new CreateDraftFromPublishedClientResult(
                CreateDraftFromPublishedClientOutcome.Created,
                new ProgrammeVm(
                    2,
                    1,
                    "Buddy Hind Limb Rehab revision draft",
                    "planned",
                    new DateOnly(2026, 5, 1),
                    null,
                    "Improving hind-limb control.",
                    "Buddy Hind Limb Rehab",
                    "Buddy",
                    "Amelia Carter",
                    [new ProgrammeVm.SessionRow(
                        1,
                        "single",
                        "Improving hind-limb control.",
                        "planned",
                        1,
                        [new ProgrammeVm.SessionExerciseRow(1, 1, "Step-ups (low)", "Controlled stepping", "https://example.test/step-up.jpg", "https://example.test/step-up.mp4", 5, 3, 5, 1, "Steady pace")])]
                )));

        public Task<UpdateProgrammeResult> UpdateProgrammeAsync(ulong id, ProgrammeBuilderForm form, CancellationToken ct)
            => Task.FromResult(new UpdateProgrammeResult(UpdateProgrammeOutcome.Updated, new ProgrammeVm(
                1,
                1,
                "Buddy Hind Limb Rehab draft",
                "planned",
                new DateOnly(2026, 5, 1),
                null,
                "Improving hind-limb control.",
                "Buddy Hind Limb Rehab",
                "Buddy",
                "Amelia Carter",
                [new ProgrammeVm.SessionRow(
                    1,
                    "single",
                    "Improving hind-limb control.",
                    "planned",
                    1,
                    [new ProgrammeVm.SessionExerciseRow(1, 1, "Step-ups (low)", "Controlled stepping", "https://example.test/step-up.jpg", "https://example.test/step-up.mp4", 5, 3, 5, 1, "Steady pace")])]
            )));

        public Task<PublishResponse> PublishProgrammeAsync(ulong id, CancellationToken ct)
            => Task.FromResult(new PublishResponse("https://example.test/programme.pdf", "programme-1.pdf", 1234));

        public Task<DownloadUrlResponse> GetDownloadUrlAsync(string fileName, CancellationToken ct)
            => Task.FromResult(new DownloadUrlResponse("https://example.test/download.pdf"));
    }
}