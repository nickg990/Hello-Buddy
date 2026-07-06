using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using HelloBuddy.Application.Auth;
using HelloBuddy.Contracts;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace HelloBuddy.Ui.Tests;

public sealed class UiAuthAdminFlowTests : IClassFixture<UiAuthAdminFlowTests.Factory>
{
    private readonly Factory _factory;

    public UiAuthAdminFlowTests(Factory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LoginPage_RendersExpectedFields()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Account/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Admin Login", html);
        Assert.Contains("name=\"Email\"", html);
        Assert.Contains("name=\"Password\"", html);
    }

    [Fact]
    public async Task AdminRoute_WhenAnonymous_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Admin/Practitioners");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShowsError()
    {
        _factory.LoginService.SetOutcome("bad@example.test", new LoginResult(LoginOutcome.InvalidCredentials));
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true });

        var response = await PostLoginAsync(client, "bad@example.test", "wrong");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid email or password.", html);
    }

    [Fact]
    public async Task Login_AsAdmin_AllowsAccessToAdminPractitioners()
    {
        _factory.LoginService.SetOutcome("admin@example.test",
            new LoginResult(LoginOutcome.Success, 42, "Admin User", "administrator"));

        _factory.AdminService.Practitioners =
        [
            new PractitionerSummary(42, "Admin", "User", "admin@example.test", "07000 000000", true, "administrator"),
            new PractitionerSummary(77, "Physio", "One", "physio@example.test", null, true, "physiotherapist"),
        ];

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true });

        var loginResponse = await PostLoginAsync(client, "admin@example.test", "Password12345!");
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var adminResponse = await client.GetAsync("/Admin/Practitioners");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);

        var html = await adminResponse.Content.ReadAsStringAsync();
        Assert.Contains("Manage Practitioners", html);
        Assert.Contains("admin@example.test", html);
        Assert.Contains("physio@example.test", html);
    }

    [Fact]
    public async Task Login_AsNonAdmin_ForbiddenForAdminRoute()
    {
        _factory.LoginService.SetOutcome("physio@example.test",
            new LoginResult(LoginOutcome.Success, 7, "Physio User", "physiotherapist"));

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var login = await PostLoginAsync(client, "physio@example.test", "Password12345!");
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var response = await client.GetAsync("/Admin/Practitioners");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Home/AccessDenied", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Login_WhenMustChangePassword_RedirectsAndCanCompleteFlow()
    {
        _factory.LoginService.SetOutcome("firstlogin@example.test",
            new LoginResult(LoginOutcome.MustChangePassword, 88, "First Login", "administrator"));
        _factory.LoginService.ForceChangePasswordResult = true;

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var login = await PostLoginAsync(client, "firstlogin@example.test", "Password12345!");
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Contains("/Account/MustChangePassword", login.Headers.Location?.ToString());

        var mustChangeGet = await client.GetAsync("/Account/MustChangePassword");
        Assert.Equal(HttpStatusCode.OK, mustChangeGet.StatusCode);
        var mustChangeHtml = await mustChangeGet.Content.ReadAsStringAsync();
        var changeToken = Regex.Match(
            mustChangeHtml,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase).Groups[1].Value;

        var changeForm = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("NewPassword", "NewPassword123!"),
            new KeyValuePair<string, string>("ConfirmPassword", "NewPassword123!"),
            new KeyValuePair<string, string>("__RequestVerificationToken", changeToken),
        ]);

        var mustChangePost = await client.PostAsync("/Account/MustChangePassword", changeForm);
        Assert.Equal(HttpStatusCode.Redirect, mustChangePost.StatusCode);
        Assert.Equal("/", mustChangePost.Headers.Location?.ToString());
    }

    [Fact]
    public async Task AdminDelete_WhenCannotTargetSelf_ShowsMessageOnList()
    {
        _factory.LoginService.SetOutcome("admin2@example.test",
            new LoginResult(LoginOutcome.Success, 99, "Admin Two", "administrator"));

        _factory.AdminService.Practitioners =
        [
            new PractitionerSummary(99, "Admin", "Two", "admin2@example.test", "07000 000001", true, "administrator"),
            new PractitionerSummary(100, "Other", "Admin", "other.admin@example.test", null, true, "administrator"),
        ];
        _factory.AdminService.DeactivateResult = new AdminPractitionerResult(AdminPractitionerOutcome.CannotTargetSelf);

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true });

        var loginResponse = await PostLoginAsync(client, "admin2@example.test", "Password12345!");
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var deleteToken = await GetAntiForgeryTokenAsync(client, "/Admin/Practitioners");
        var deleteForm = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("__RequestVerificationToken", deleteToken),
        ]);
        var deleteResponse = await client.PostAsync("/Admin/Delete?id=99", deleteForm);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var html = await deleteResponse.Content.ReadAsStringAsync();
        Assert.Contains("Cannot deactivate your own account.", html);
    }

    [Fact]
    public async Task Admin_DataControlPage_RendersOwnerDropdown()
    {
        _factory.LoginService.SetOutcome("admin@example.test",
            new LoginResult(LoginOutcome.Success, 42, "Admin User", "administrator"));

        _factory.AdminService.Practitioners =
        [
            new PractitionerSummary(42, "Admin", "User", "admin@example.test", "07000 000000", true, "administrator"),
        ];

        _factory.ApiClient.Owners =
        [
            new OwnerListItem(1, "Amelia Carter", "amelia@example.test", "07123 456789", 1),
            new OwnerListItem(2, "James Holloway", "james@example.test", null, 1),
        ];

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true });

        var loginResponse = await PostLoginAsync(client, "admin@example.test", "Password12345!");
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var dataControlResponse = await client.GetAsync("/Admin/DataControl");
        Assert.Equal(HttpStatusCode.OK, dataControlResponse.StatusCode);

        var html = await dataControlResponse.Content.ReadAsStringAsync();
        Assert.Contains("GDPR Data Control", html);
        Assert.Contains("Right To Be Forgotten", html);
        Assert.Contains("form", html);
        Assert.Contains("id=\"OwnerId\"", html);
        Assert.Contains("Apply RTBF", html);
        Assert.Contains("Amelia Carter", html);
        Assert.Contains("James Holloway", html);
    }

    [Fact]
    public async Task NonAdmin_DataControlPage_RedirectsToAccessDenied()
    {
        _factory.LoginService.SetOutcome("physio@example.test",
            new LoginResult(LoginOutcome.Success, 7, "Physio User", "physiotherapist"));

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var login = await PostLoginAsync(client, "physio@example.test", "Password12345!");
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var response = await client.GetAsync("/Admin/DataControl");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Home/AccessDenied", response.Headers.Location?.ToString());
    }

    private static async Task<HttpResponseMessage> PostLoginAsync(HttpClient client, string email, string password)
    {
        var token = await GetAntiForgeryTokenAsync(client, "/Account/Login");
        var form = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("Email", email),
            new KeyValuePair<string, string>("Password", password),
            new KeyValuePair<string, string>("AdminEmail", string.Empty),
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
        ]);

        return await client.PostAsync("/Account/Login", form);
    }

    private static async Task<string> GetAntiForgeryTokenAsync(HttpClient client, string path)
    {
        var get = await client.GetAsync(path);
        var html = await get.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);
        Assert.True(match.Success, $"Expected an antiforgery token on the form at {path}.");
        return match.Groups[1].Value;
    }

    public sealed class Factory : WebApplicationFactory<Program>
    {
        public StubLoginService LoginService { get; } = new();
        public StubPractitionerAdminService AdminService { get; } = new();
        public StubAuthFlowAdminApiClient ApiClient { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("Api:Uri", "https://example.test");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(ILoginService));
                services.RemoveAll(typeof(IPractitionerAdminService));
                services.RemoveAll(typeof(IAdminApiClient));
                services.AddSingleton<ILoginService>(LoginService);
                services.AddSingleton<IPractitionerAdminService>(AdminService);
                services.AddSingleton<IAdminApiClient>(ApiClient);
            });
        }
    }

    public sealed class StubAuthFlowAdminApiClient : IAdminApiClient
    {
        public List<OwnerListItem> Owners { get; set; } = [];

        public Task<IReadOnlyList<OwnerListItem>> ListOwnersAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<OwnerListItem>>(Owners);

        public Task<OwnerDetailVm?> GetOwnerAsync(ulong id, CancellationToken ct)
            => Task.FromResult<OwnerDetailVm?>(null);

        public Task<OwnerDetailVm> CreateOwnerAsync(SaveOwnerRequest request, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<OwnerDetailVm?> UpdateOwnerAsync(ulong id, SaveOwnerRequest request, CancellationToken ct)
            => Task.FromResult<OwnerDetailVm?>(null);

        public Task<OwnerDataControlClientResult> ApplyOwnerDataControlAsync(ulong id, CancellationToken ct)
            => Task.FromResult(new OwnerDataControlClientResult(
                OwnerDataControlClientOutcome.Deleted,
                "Owner and all associated records, including stored programme PDFs, were permanently deleted."));

        public Task<IReadOnlyList<CaseRow>> ListCasesAsync(CancellationToken ct)
            => throw new NotImplementedException();

        public Task<CaseDetailVm?> GetCaseAsync(ulong id, CancellationToken ct)
            => Task.FromResult<CaseDetailVm?>(null);

        public Task<CaseDetailVm> CreateCaseAsync(SaveTreatmentCaseRequest request, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<CaseDetailVm?> UpdateCaseAsync(ulong id, SaveTreatmentCaseRequest request, CancellationToken ct)
            => Task.FromResult<CaseDetailVm?>(null);

        public Task<CaseDetailVm.NoteRow?> AddCaseNoteAsync(ulong id, CreateCaseNoteRequest request, CancellationToken ct)
            => Task.FromResult<CaseDetailVm.NoteRow?>(null);

        public Task<CaseDetailVm.NoteRow?> UpdateCaseNoteAsync(ulong id, ulong noteId, CreateCaseNoteRequest request, CancellationToken ct)
            => Task.FromResult<CaseDetailVm.NoteRow?>(null);

        public Task<bool> DeleteCaseNoteAsync(ulong id, ulong noteId, CancellationToken ct)
            => Task.FromResult(false);

        public Task<PetDetailVm> CreatePetAsync(SavePetRequest request, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<PetDetailVm?> GetPetAsync(ulong id, CancellationToken ct)
            => Task.FromResult<PetDetailVm?>(null);

        public Task<PetDetailVm?> UpdatePetAsync(ulong id, SavePetRequest request, CancellationToken ct)
            => Task.FromResult<PetDetailVm?>(null);

        public Task<IReadOnlyList<PetListItem>> ListPetsAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<PetListItem>>([]);

        public Task<IReadOnlyList<ExerciseListItem>> ListExercisesAsync(ExerciseListFilter filter, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ExerciseListItem>>([]);

        public Task<ExerciseDetailVm?> GetExerciseAsync(ulong id, CancellationToken ct)
            => Task.FromResult<ExerciseDetailVm?>(null);

        public Task<ExerciseImageContent?> GetExerciseImageAsync(ulong id, CancellationToken ct)
            => Task.FromResult<ExerciseImageContent?>(null);

        public Task<ExerciseMediaUploadResponse> UploadExerciseImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<ExerciseDetailVm> CreateExerciseAsync(SaveExerciseRequest request, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<ExerciseDetailVm?> UpdateExerciseAsync(ulong id, SaveExerciseRequest request, CancellationToken ct)
            => Task.FromResult<ExerciseDetailVm?>(null);

        public Task<ExerciseDetailVm?> SetExerciseActiveAsync(ulong id, bool isActive, CancellationToken ct)
            => Task.FromResult<ExerciseDetailVm?>(null);

        public Task<IReadOnlyList<ExerciseCategoryListItem>> ListExerciseCategoriesAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ExerciseCategoryListItem>>([]);

        public Task<IReadOnlyList<ExerciseAuditEntryVm>> GetExerciseAuditHistoryAsync(ulong id, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ExerciseAuditEntryVm>>([]);

        public Task<IReadOnlyList<ExerciseMediaLibraryItem>> GetExerciseImageLibraryAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ExerciseMediaLibraryItem>>([]);

        public Task<ExerciseImageContent?> GetExerciseMediaContentAsync(string key, CancellationToken ct)
            => Task.FromResult<ExerciseImageContent?>(null);

        public Task<ProgrammeVm?> CreateDraftProgrammeAsync(ulong caseId, CancellationToken ct)
            => Task.FromResult<ProgrammeVm?>(null);

        public Task<DeleteProgrammeResult> DeleteProgrammeAsync(ulong programmeId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<ProgrammeStatusTransitionClientResult> ActivateProgrammeAsync(ulong programmeId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<ProgrammeStatusTransitionClientResult> CompleteProgrammeAsync(ulong programmeId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<UpdateProgrammeStructureResult> UpdateProgrammeStructureAsync(ulong programmeId, ProgrammeStructureForm form, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<AddSessionExerciseClientResult> AddSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong exerciseId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<RemoveSessionExerciseClientResult> RemoveSessionExerciseAsync(ulong programmeId, ulong sessionId, ulong sessionExerciseId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<ProgrammeVm?> GetProgrammeAsync(ulong id, CancellationToken ct)
            => Task.FromResult<ProgrammeVm?>(null);

        public Task<ProgrammeVersionHistoryVm?> GetProgrammeVersionHistoryAsync(ulong id, CancellationToken ct)
            => Task.FromResult<ProgrammeVersionHistoryVm?>(null);

        public Task<CreateDraftFromPublishedClientResult> CreateDraftFromPublishedAsync(ulong id, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<UpdateProgrammeResult> UpdateProgrammeAsync(ulong id, ProgrammeBuilderForm form, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<PdfDocumentContent?> GetProgrammePreviewPdfAsync(ulong id, CancellationToken ct)
            => Task.FromResult<PdfDocumentContent?>(null);

        public Task<PublishResponse> PublishProgrammeAsync(ulong id, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<DownloadUrlResponse> GetDownloadUrlAsync(string fileName, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<PdfDocumentContent?> GetProgrammeVersionPdfAsync(ulong id, ulong versionId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<bool> DeleteProgrammeVersionAsync(ulong id, ulong versionId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<string?> GetAppSettingAsync(string key, CancellationToken ct)
            => Task.FromResult<string?>(null);

        public Task SaveAppSettingAsync(string key, string? value, CancellationToken ct)
            => Task.CompletedTask;

        public Task<PetDeleteClientResult> DeletePetAsync(ulong id, CancellationToken ct)
            => Task.FromResult(new PetDeleteClientResult(
                PetDeleteClientOutcome.Deleted,
                "Pet and all associated records, including stored programme PDFs, were permanently deleted."));
    }

    public sealed class StubLoginService : ILoginService
    {
        private readonly Dictionary<string, LoginResult> _outcomes = new(StringComparer.OrdinalIgnoreCase);

        public bool ForceChangePasswordResult { get; set; }

        public void SetOutcome(string email, LoginResult result)
            => _outcomes[email] = result;

        public Task<LoginResult> AuthenticateAsync(string email, string password, bool honeypotTriggered, CancellationToken ct)
        {
            if (honeypotTriggered)
            {
                return Task.FromResult(new LoginResult(LoginOutcome.HoneypotTriggered));
            }

            if (_outcomes.TryGetValue(email, out var result))
            {
                return Task.FromResult(result);
            }

            return Task.FromResult(new LoginResult(LoginOutcome.InvalidCredentials));
        }

        public Task<bool> ForceChangePasswordAsync(ulong practitionerId, string newPassword, CancellationToken ct)
            => Task.FromResult(ForceChangePasswordResult);
    }

    public sealed class StubPractitionerAdminService : IPractitionerAdminService
    {
        public List<PractitionerSummary> Practitioners { get; set; } = [];
        public AdminPractitionerResult DeactivateResult { get; set; } = new(AdminPractitionerOutcome.Success);

        public Task<IReadOnlyList<PractitionerSummary>> ListPractitionersAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<PractitionerSummary>>(Practitioners);

        public Task<AdminPractitionerResult> AddPractitionerAsync(
            string firstName,
            string lastName,
            string email,
            string? phoneNumber,
            string role,
            string initialPassword,
            CancellationToken ct)
            => Task.FromResult(new AdminPractitionerResult(AdminPractitionerOutcome.Success));

        public Task<AdminPractitionerResult> RenamePractitionerAsync(
            ulong targetPractitionerId,
            string firstName,
            string lastName,
            string newEmail,
            string? phoneNumber,
            CancellationToken ct)
            => Task.FromResult(new AdminPractitionerResult(AdminPractitionerOutcome.Success));

        public Task<AdminPractitionerResult> SetPasswordAsync(
            ulong targetPractitionerId,
            string newPassword,
            CancellationToken ct)
            => Task.FromResult(new AdminPractitionerResult(AdminPractitionerOutcome.Success));

        public Task<AdminPractitionerResult> ChangeOwnPasswordAsync(
            ulong adminPractitionerId,
            string currentPassword,
            string newPassword,
            CancellationToken ct)
            => Task.FromResult(new AdminPractitionerResult(AdminPractitionerOutcome.Success));

        public Task<AdminPractitionerResult> DeactivatePractitionerAsync(
            ulong targetPractitionerId,
            ulong requestingPractitionerId,
            CancellationToken ct)
            => Task.FromResult(DeactivateResult);

        public Task<AdminPractitionerResult> ActivatePractitionerAsync(
            ulong targetPractitionerId,
            CancellationToken ct)
            => Task.FromResult(new AdminPractitionerResult(AdminPractitionerOutcome.Success));
    }
}
