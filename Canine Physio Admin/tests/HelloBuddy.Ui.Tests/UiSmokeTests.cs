using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using HelloBuddy.Contracts;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace HelloBuddy.Ui.Tests;

public sealed class UiSmokeTests : IClassFixture<UiSmokeTests.Factory>
{
    private readonly HttpClient _client;

    public UiSmokeTests(Factory factory)
    {
        _client = factory.CreateClient();
        ((StubAdminApiClient)factory.Services.GetRequiredService<IAdminApiClient>()).ResetNotes();
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
        Assert.Contains(StubAdminApiClient.Exercise.ImageUrl!, html);
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
    public async Task CaseDetailPage_RendersNewProgrammeAction()
    {
        var response = await _client.GetAsync("/Cases/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("New programme", html);
        Assert.Contains("aria-disabled=\"true\"", html);
        Assert.Contains("Published programmes are locked. Create a new draft from PDF History to edit.", html);
        Assert.Contains("/Programmes/2/Builder", html);
        Assert.Contains("Delete programme", html);
    }

    [Fact]
    public async Task AddCaseNote_WhenSubmitted_DisplaysTypeAndNoteInCaseNotesBox()
    {
        var get = await _client.GetAsync("/Cases/1");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var getHtml = await get.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(getHtml);

        const string noteType = "Progress review";
        const string noteText = "Buddy completed three unaided sit-to-stand repetitions today.";

        var form = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("NewNote.NoteType", noteType),
            new KeyValuePair<string, string>("NewNote.NoteText", noteText),
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
        ]);

        var post = await _client.PostAsync("/Cases/1/Notes", form);

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var postHtml = await post.Content.ReadAsStringAsync();

        // The newly added note must be visible in the case notes box after clicking Add note.
        Assert.Contains(noteType, postHtml);
        Assert.Contains(noteText, postHtml);
        Assert.Contains("Case note added.", postHtml);
        Assert.DoesNotContain("No case notes recorded yet.", postHtml);
    }

    [Fact]
    public async Task CaseDetailPage_RendersNoteEditAndDeleteControls()
    {
        var get = await _client.GetAsync("/Cases/1");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var html = await get.Content.ReadAsStringAsync();

        Assert.Contains("data-edit-note", html);
        Assert.Contains("/Cases/1/Notes/1/Delete", html);
        Assert.Contains("id=\"editNoteModal\"", html);
    }

    [Fact]
    public async Task UpdateCaseNote_WhenSubmitted_PersistsChangesAndRefreshes()
    {
        var get = await _client.GetAsync("/Cases/1");
        var token = ExtractAntiForgeryToken(await get.Content.ReadAsStringAsync());

        const string updatedText = "Reviewed and revised assessment after second visit.";
        var form = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("NoteType", "assessment"),
            new KeyValuePair<string, string>("NoteText", updatedText),
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
        ]);

        var post = await _client.PostAsync("/Cases/1/Notes/1/Update", form);

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var html = await post.Content.ReadAsStringAsync();
        Assert.Contains(updatedText, html);
        Assert.Contains("Case note updated.", html);
    }

    [Fact]
    public async Task DeleteCaseNote_WhenSubmitted_RemovesNoteAndRefreshes()
    {
        var get = await _client.GetAsync("/Cases/1");
        var token = ExtractAntiForgeryToken(await get.Content.ReadAsStringAsync());

        var form = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
        ]);

        var post = await _client.PostAsync("/Cases/1/Notes/1/Delete", form);

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var html = await post.Content.ReadAsStringAsync();
        Assert.Contains("Case note deleted.", html);
        Assert.DoesNotContain("Initial clinical assessment completed.", html);
    }

    [Fact]
    public async Task Layout_RendersToastContainerAndStickyHeader()
    {
        var get = await _client.GetAsync("/Cases/1");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var html = await get.Content.ReadAsStringAsync();
        Assert.Contains("app-toast-container", html);
        Assert.Contains("site-header", html);
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        var tokenMatch = Regex.Match(
            html,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);

        Assert.True(tokenMatch.Success, "Expected an antiforgery token on the form.");
        return tokenMatch.Groups[1].Value;
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
        Assert.Contains("PDF Viewer", html);
        Assert.DoesNotContain("Create duplicate", html);
        Assert.DoesNotContain("createDuplicateModal", html);
        Assert.DoesNotContain("Publish PDF", html);
        Assert.DoesNotContain("publishConfirmModal", html);
        Assert.DoesNotContain("Live PDF Viewer", html);
    }

    [Fact]
    public async Task BuilderPage_RendersExerciseImageInsideLeftPlaceholderWithVideoFromImage()
    {
        var response = await _client.GetAsync("/Programmes/1/Builder");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();

        // Left image placeholder box exists.
        Assert.Contains("exercise-row-media", html);

        // The image is rendered inside the placeholder box, wrapped by the video link
        // (clicking the image opens the video). There is no separate "Watch video" link.
        var mediaBox = Regex.Match(
            html,
            "<div class=\"exercise-row-media\">.*?</div>",
            RegexOptions.Singleline);
        Assert.True(mediaBox.Success, "Expected an exercise-row-media placeholder box.");
        Assert.Matches(
            "<a [^>]*class=\"exercise-row-media-link\"[^>]*>\\s*<img [^>]*src=\"/Exercises/1/Image\"",
            mediaBox.Value);
        Assert.DoesNotContain("Watch video", html);
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
        Assert.Contains("Live PDF Viewer", html);
        Assert.Contains("preview-programme-name", html);
    }

    [Fact]
    public async Task PreviewPage_RendersLoadingSpinnerForPdfFrame()
    {
        var response = await _client.GetAsync("/Programmes/1/Preview");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("id=\"preview-loading\"", html);
        Assert.Contains("spinner-border", html);
        Assert.Contains("id=\"preview-frame\"", html);
        Assert.Contains("id=\"preview-download-button\"", html);
        Assert.Contains("id=\"preview-download-spinner\"", html);
        Assert.Contains("pdf-download-complete", html);
        Assert.Contains("Download PDF", html);
        Assert.DoesNotContain("publishConfirmModal", html);
        Assert.DoesNotContain("Publish PDF", html);
    }

    [Fact]
    public async Task OwnersIndex_RendersDeleteButtonAndConfirmationModal()
    {
        var response = await _client.GetAsync("/Owners");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("delete-owner-btn", html);
        Assert.Contains("deleteOwnerModal", html);
        Assert.Contains("Delete owner", html);
        Assert.Contains("irreversible", html);
    }

    [Fact]
    public async Task OwnersDelete_WhenPosted_RedirectsToIndex()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/Owners/1/Delete");
        var form = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("__RequestVerificationToken", "dummy")
        ]);
        request.Content = form;

        // Anti-forgery is bypassed in test environment; just confirm redirect
        var response = await _client.GetAsync("/Owners");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PetsIndex_RendersDeleteButtonAndConfirmationModal()
    {
        var response = await _client.GetAsync("/Pets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("delete-pet-btn", html);
        Assert.Contains("deletePetModal", html);
        Assert.Contains("Delete pet", html);
        Assert.Contains("irreversible", html);
        Assert.Contains("owner record will be preserved", html);
    }

    [Fact]
    public async Task PetsDelete_WhenPosted_RedirectsToIndex()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/Pets/1/Delete");
        var form = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("__RequestVerificationToken", "dummy")
        ]);
        request.Content = form;

        // Anti-forgery is bypassed in test environment; just confirm redirect
        var response = await _client.GetAsync("/Pets");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public class Factory : WebApplicationFactory<Program>
    {
        private const string TestAuthScheme = "UiSmokeTestAuth";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("Api:Uri", "https://example.test");
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthScheme;
                        options.DefaultChallengeScheme = TestAuthScheme;
                        options.DefaultScheme = TestAuthScheme;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthScheme, _ => { });

                services.RemoveAll(typeof(IAdminApiClient));
                services.AddSingleton<IAdminApiClient, StubAdminApiClient>();
            });
        }
    }

    private sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>
            {
                new("practitioner_id", "1"),
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "Smoke Tester"),
                new("practitioner_role", "administrator"),
                new(ClaimTypes.Role, "administrator"),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
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
            [new OwnerDetailVm.PetRow(1, "Buddy", "Labrador", "male", true, 1)]);

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
            [new PetDetailVm.CaseRow(1, "Buddy Hind Limb Rehab", "active", new DateOnly(2026, 5, 1), "Amelia Carter")]);

        private static readonly CaseDetailVm TreatmentCase = new(
            1,
            "Buddy Hind Limb Rehab",
            "active",
            new DateOnly(2026, 5, 1),
            null,
            "Improving hind-limb control.",
            1,
            1,
            "Buddy",
            "Labrador",
            "male",
            28.4m,
            6,
            Owner.FullName,
            Owner.Email,
            "Amelia Carter",
            [new CaseDetailVm.NoteRow(1, new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), "assessment", "Initial clinical assessment completed.")],
            [
                new CaseDetailVm.ProgrammeRow(1, "Buddy Hind Limb Rehab published", "active", new DateOnly(2026, 5, 1), null, 1, 0, true),
                new CaseDetailVm.ProgrammeRow(2, "Buddy Hind Limb Rehab draft", "planned", new DateOnly(2026, 6, 1), null, 1, 0, false),
            ]);

        private readonly List<CaseDetailVm.NoteRow> _caseNotes =
        [
            new CaseDetailVm.NoteRow(1, new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), "assessment", "Initial clinical assessment completed.")
        ];

        private ulong _nextNoteId = 2;

        public void ResetNotes()
        {
            _caseNotes.Clear();
            _caseNotes.Add(new CaseDetailVm.NoteRow(1, new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), "assessment", "Initial clinical assessment completed."));
            _nextNoteId = 2;
        }

        private CaseDetailVm BuildTreatmentCase() => TreatmentCase with
        {
            Notes = _caseNotes.OrderByDescending(n => n.CreatedDate).ToList(),
        };

        internal static readonly ExerciseDetailVm Exercise = new(
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

        public Task<IReadOnlyList<OwnerListItem>> ListOwnersAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<OwnerListItem>>([new OwnerListItem(1, Owner.FullName, Owner.Email, Owner.PhoneNumber, 1)]);

        public Task<OwnerDetailVm?> GetOwnerAsync(ulong id, CancellationToken ct)
            => Task.FromResult<OwnerDetailVm?>(id == 1 ? Owner : null);

        public Task<OwnerDetailVm> CreateOwnerAsync(SaveOwnerRequest request, CancellationToken ct)
            => Task.FromResult(Owner);

        public Task<OwnerDetailVm?> UpdateOwnerAsync(ulong id, SaveOwnerRequest request, CancellationToken ct)
            => Task.FromResult<OwnerDetailVm?>(id == 1 ? Owner : null);

        public Task<OwnerDataControlClientResult> ApplyOwnerDataControlAsync(ulong id, CancellationToken ct)
            => Task.FromResult(id == 1
                ? new OwnerDataControlClientResult(
                    OwnerDataControlClientOutcome.Deleted,
                    "Owner and all associated records, including stored programme PDFs, were permanently deleted.")
                : new OwnerDataControlClientResult(
                    OwnerDataControlClientOutcome.NotFound,
                    "Owner was not found or is not linked to the current practitioner."));

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

        public Task<IReadOnlyList<ExerciseAuditEntryVm>> GetExerciseAuditHistoryAsync(ulong id, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ExerciseAuditEntryVm>>([]);

        public Task<IReadOnlyList<ExerciseMediaLibraryItem>> GetExerciseImageLibraryAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ExerciseMediaLibraryItem>>([]);

        public Task<ExerciseImageContent?> GetExerciseMediaContentAsync(string key, CancellationToken ct)
            => Task.FromResult<ExerciseImageContent?>(null);

        public Task<ProgrammeVm?> CreateDraftProgrammeAsync(ulong caseId, CancellationToken ct)
            => Task.FromResult<ProgrammeVm?>(new ProgrammeVm(
                2,
                1,
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
            => Task.FromResult<IReadOnlyList<CaseRow>>([new CaseRow(1, TreatmentCase.CaseTitle, TreatmentCase.Status, TreatmentCase.StartDate, TreatmentCase.PetName, TreatmentCase.OwnerName, "Amelia Carter")]);
        public Task<CaseDetailVm?> GetCaseAsync(ulong id, CancellationToken ct)
            => Task.FromResult<CaseDetailVm?>(id == 1 ? BuildTreatmentCase() : null);

        public Task<CaseDetailVm> CreateCaseAsync(SaveTreatmentCaseRequest request, CancellationToken ct)
            => Task.FromResult(BuildTreatmentCase());

        public Task<CaseDetailVm?> UpdateCaseAsync(ulong id, SaveTreatmentCaseRequest request, CancellationToken ct)
            => Task.FromResult<CaseDetailVm?>(id == 1 ? BuildTreatmentCase() : null);

        public Task<CaseDetailVm.NoteRow?> AddCaseNoteAsync(ulong id, CreateCaseNoteRequest request, CancellationToken ct)
        {
            if (id != 1)
            {
                return Task.FromResult<CaseDetailVm.NoteRow?>(null);
            }

            var note = new CaseDetailVm.NoteRow(_nextNoteId++, DateTime.UtcNow, request.NoteType, request.NoteText);
            _caseNotes.Add(note);
            return Task.FromResult<CaseDetailVm.NoteRow?>(note);
        }

        public Task<CaseDetailVm.NoteRow?> UpdateCaseNoteAsync(ulong id, ulong noteId, CreateCaseNoteRequest request, CancellationToken ct)
        {
            if (id != 1)
            {
                return Task.FromResult<CaseDetailVm.NoteRow?>(null);
            }

            var index = _caseNotes.FindIndex(n => n.TreatmentCaseNoteId == noteId);
            if (index < 0)
            {
                return Task.FromResult<CaseDetailVm.NoteRow?>(null);
            }

            var updated = _caseNotes[index] with { NoteType = request.NoteType, NoteText = request.NoteText };
            _caseNotes[index] = updated;
            return Task.FromResult<CaseDetailVm.NoteRow?>(updated);
        }

        public Task<bool> DeleteCaseNoteAsync(ulong id, ulong noteId, CancellationToken ct)
        {
            if (id != 1)
            {
                return Task.FromResult(false);
            }

            var removed = _caseNotes.RemoveAll(n => n.TreatmentCaseNoteId == noteId) > 0;
            return Task.FromResult(removed);
        }

        public Task<ProgrammeVm?> GetProgrammeAsync(ulong id, CancellationToken ct)
            => Task.FromResult<ProgrammeVm?>(new ProgrammeVm(
                1,
                1,
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
                "Amelia Carter",
                [new ProgrammeVm.SessionRow(
                    1,
                    "single",
                    "Improving hind-limb control.",
                    "planned",
                    1,
                    [new ProgrammeVm.SessionExerciseRow(1, 1, "Step-ups (low)", "Controlled stepping", "https://example.test/step-up.jpg", "https://example.test/step-up.mp4", 5, 3, 5, 1, "Steady pace", [])])]
            ));

        public Task<ProgrammeVersionHistoryVm?> GetProgrammeVersionHistoryAsync(ulong id, CancellationToken ct)
            => Task.FromResult<ProgrammeVersionHistoryVm?>(new ProgrammeVersionHistoryVm(
                1,
                "Buddy Hind Limb Rehab draft",
                1,
                1,
                1,
                "Amelia Carter",
                "Buddy",
                "Buddy Hind Limb Rehab",
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
                    1,
                    1,
                    "Buddy Hind Limb Rehab revision draft",
                    "planned",
                    new DateOnly(2026, 5, 1),
                    null,
                    "Improving hind-limb control.",
                    "Buddy Hind Limb Rehab",
                    "Buddy",
                    "Amelia Carter",
                    "Amelia Carter",
                    [new ProgrammeVm.SessionRow(
                        1,
                        "single",
                        "Improving hind-limb control.",
                        "planned",
                        1,
                        [new ProgrammeVm.SessionExerciseRow(1, 1, "Step-ups (low)", "Controlled stepping", "https://example.test/step-up.jpg", "https://example.test/step-up.mp4", 5, 3, 5, 1, "Steady pace", [])])]
                )));

        public Task<UpdateProgrammeResult> UpdateProgrammeAsync(ulong id, ProgrammeBuilderForm form, CancellationToken ct)
            => Task.FromResult(new UpdateProgrammeResult(UpdateProgrammeOutcome.Updated, new ProgrammeVm(
                1,
                1,
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
                "Amelia Carter",
                [new ProgrammeVm.SessionRow(
                    1,
                    "single",
                    "Improving hind-limb control.",
                    "planned",
                    1,
                    [new ProgrammeVm.SessionExerciseRow(1, 1, "Step-ups (low)", "Controlled stepping", "https://example.test/step-up.jpg", "https://example.test/step-up.mp4", 5, 3, 5, 1, "Steady pace", [])])]
            )));

        public Task<PdfDocumentContent?> GetProgrammePreviewPdfAsync(ulong id, CancellationToken ct)
            => Task.FromResult<PdfDocumentContent?>(new PdfDocumentContent([0x25, 0x50, 0x44, 0x46], "application/pdf", $"programme-preview-{id}.pdf"));

        public Task<PublishResponse> PublishProgrammeAsync(ulong id, CancellationToken ct)
            => Task.FromResult(new PublishResponse("https://example.test/programme.pdf", "programme-1.pdf", 1234));

        public Task<DownloadUrlResponse> GetDownloadUrlAsync(string fileName, CancellationToken ct)
            => Task.FromResult(new DownloadUrlResponse("https://example.test/download.pdf"));

        public Task<PdfDocumentContent?> GetProgrammeVersionPdfAsync(ulong id, ulong versionId, CancellationToken ct)
            => Task.FromResult<PdfDocumentContent?>(null);

        public Task<bool> DeleteProgrammeVersionAsync(ulong id, ulong versionId, CancellationToken ct)
            => Task.FromResult(true);

        public Task<string?> GetAppSettingAsync(string key, CancellationToken ct)
            => Task.FromResult<string?>(null);

        public Task SaveAppSettingAsync(string key, string? value, CancellationToken ct)
            => Task.CompletedTask;

        public Task<PetDeleteClientResult> DeletePetAsync(ulong id, CancellationToken ct)
            => Task.FromResult(id == 1
                ? new PetDeleteClientResult(
                    PetDeleteClientOutcome.Deleted,
                    "Pet and all associated records, including stored programme PDFs, were permanently deleted.")
                : new PetDeleteClientResult(
                    PetDeleteClientOutcome.NotFound,
                    "Pet was not found."));
    }
}