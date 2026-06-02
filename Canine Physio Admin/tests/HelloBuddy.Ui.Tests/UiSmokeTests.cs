using System.Net;
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
    [InlineData("/Owners/1", "Amelia Carter")]
    [InlineData("/Pets/1", "Buddy")]
    [InlineData("/Cases/1", "Buddy Hind Limb Rehab")]
    public async Task PageLoads(string path, string expectedText)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(expectedText, html);
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