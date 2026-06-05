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
    public async Task PageLoads(string path, string expectedText)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(expectedText, html);
    }

    [Fact]
    public async Task EditExercisePage_RendersCurrentAndSelectedImagePanels()
    {
        var response = await _client.GetAsync("/Exercises/1/Edit");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Current image", html);
        Assert.Contains("Selected image (pending save)", html);
        Assert.Contains("/Exercises/1/Image", html);
        Assert.Contains("id=\"selected-image-link\"", html);
        Assert.Contains("id=\"selected-image-preview\"", html);
        Assert.DoesNotContain("selected-image-filename", html);
    }

    [Fact]
    public async Task CreateExercisePage_RendersSelectedImagePanelWithoutCurrentImagePanel()
    {
        var response = await _client.GetAsync("/Exercises/Create");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Current image", html);
        Assert.Contains("Selected image", html);
        Assert.Contains("No image selected", html);
        Assert.Contains("id=\"selected-image-link\"", html);
        Assert.Contains("id=\"selected-image-preview\"", html);
        Assert.DoesNotContain("selected-image-filename", html);
    }

    public sealed class Factory : WebApplicationFactory<Program>
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
            []);

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

        public Task<IReadOnlyList<OwnerListItem>> ListOwnersAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<OwnerListItem>>([new OwnerListItem(1, Owner.FullName, Owner.Email, Owner.PhoneNumber, 1)]);

        public Task<OwnerDetailVm?> GetOwnerAsync(ulong id, CancellationToken ct)
            => Task.FromResult<OwnerDetailVm?>(id == 1 ? Owner : null);

        public Task<OwnerDetailVm> CreateOwnerAsync(SaveOwnerRequest request, CancellationToken ct)
            => Task.FromResult(Owner);

        public Task<OwnerDetailVm?> UpdateOwnerAsync(ulong id, SaveOwnerRequest request, CancellationToken ct)
            => Task.FromResult<OwnerDetailVm?>(id == 1 ? Owner : null);

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
            => Task.FromResult<ProgrammeVm?>(null);

        public Task<ProgrammeVm?> UpdateProgrammeAsync(ulong id, ProgrammeBuilderForm form, CancellationToken ct)
            => Task.FromResult<ProgrammeVm?>(null);

        public Task<PublishResponse> PublishProgrammeAsync(ulong id, CancellationToken ct)
            => Task.FromResult(new PublishResponse("https://example.test/programme.pdf", "programme-1.pdf", 1234));

        public Task<DownloadUrlResponse> GetDownloadUrlAsync(string fileName, CancellationToken ct)
            => Task.FromResult(new DownloadUrlResponse("https://example.test/download.pdf"));
    }
}