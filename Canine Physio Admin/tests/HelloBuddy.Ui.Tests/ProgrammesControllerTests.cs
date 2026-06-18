using System.Text.Json;
using HelloBuddy.Contracts;
using HelloBuddy.Ui.Controllers;
using HelloBuddy.Ui.Models;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HelloBuddy.Ui.Tests;

public sealed class ProgrammesControllerTests
{
    [Fact]
    public async Task Builder_Post_AjaxRequest_ReturnsJsonSuccessPayload()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.UpdateProgrammeAsync(1, Arg.Any<ProgrammeBuilderForm>(), Arg.Any<CancellationToken>())
            .Returns(new UpdateProgrammeResult(UpdateProgrammeOutcome.Updated, CreateProgrammeVm()));

        var sut = CreateSut(api, ajaxRequest: true);
        var form = new ProgrammeBuilderForm
        {
            ProgrammeId = 1,
            Sessions =
            {
                new ProgrammeBuilderForm.SessionEdit { SessionId = 1, Objective = "Improve stability" },
            },
            Exercises =
            {
                new ProgrammeBuilderForm.SessionExerciseEdit { SessionExerciseId = 10, Reps = 10, Sets = 3, SortOrder = 1 },
                new ProgrammeBuilderForm.SessionExerciseEdit { SessionExerciseId = 11, Reps = 8, Sets = 2, SortOrder = 2 },
            },
        };

        var result = await sut.Builder(1, form, CancellationToken.None);

        var json = Assert.IsType<JsonResult>(result);
        var payload = JsonSerializer.Serialize(json.Value);
        Assert.Contains("\"ok\":true", payload);
        Assert.Contains("Saved 1 session summaries and 2 exercise edits.", payload);
    }

    [Fact]
    public async Task Structure_Post_AjaxInvalid_ReturnsJsonErrorPayload()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.UpdateProgrammeStructureAsync(1, Arg.Any<ProgrammeStructureForm>(), Arg.Any<CancellationToken>())
            .Returns(new UpdateProgrammeStructureResult(UpdateProgrammeStructureOutcome.Invalid, "End date must be after start date."));

        var sut = CreateSut(api, ajaxRequest: true);
        var form = new ProgrammeStructureForm
        {
            ProgrammeId = 1,
            ProgrammeName = "Draft",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 5, 1),
            SessionStructure = "single",
        };

        var result = await sut.Structure(1, form, CancellationToken.None);

        var json = Assert.IsType<JsonResult>(result);
        var payload = JsonSerializer.Serialize(json.Value);
        Assert.Contains("\"ok\":false", payload);
        Assert.Contains("End date must be after start date.", payload);
    }

    [Fact]
    public async Task AddExercise_Post_AjaxDuplicate_ReturnsJsonErrorPayload()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.AddSessionExerciseAsync(1, 20, 2, Arg.Any<CancellationToken>())
            .Returns(new AddSessionExerciseClientResult(AddSessionExerciseClientOutcome.Duplicate, "Exercise already added."));

        var sut = CreateSut(api, ajaxRequest: true);

        var result = await sut.AddExercise(1, 20, 2, new ProgrammeBuilderForm(), CancellationToken.None);

        var json = Assert.IsType<JsonResult>(result);
        var payload = JsonSerializer.Serialize(json.Value);
        Assert.Contains("\"ok\":false", payload);
        Assert.Contains("Exercise already added.", payload);
    }

    [Fact]
    public async Task AddExercise_Post_PersistsSessionEditsBeforeAddingExercise()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.UpdateProgrammeAsync(1, Arg.Any<ProgrammeBuilderForm>(), Arg.Any<CancellationToken>())
            .Returns(new UpdateProgrammeResult(UpdateProgrammeOutcome.Updated, CreateProgrammeVm()));
        api.AddSessionExerciseAsync(1, 20, 2, Arg.Any<CancellationToken>())
            .Returns(new AddSessionExerciseClientResult(AddSessionExerciseClientOutcome.Added, null));

        var sut = CreateSut(api, ajaxRequest: true);
        var form = new ProgrammeBuilderForm
        {
            ProgrammeId = 1,
            Sessions =
            {
                new ProgrammeBuilderForm.SessionEdit { SessionId = 1, Objective = "AM purpose" },
                new ProgrammeBuilderForm.SessionEdit { SessionId = 2, Objective = "PM purpose" },
            },
            Exercises =
            {
                new ProgrammeBuilderForm.SessionExerciseEdit { SessionExerciseId = 10, Reps = 12, Sets = 4, HoldSeconds = 6, SortOrder = 1, Notes = "Keep steady" },
            },
        };

        var result = await sut.AddExercise(1, 20, 2, form, CancellationToken.None);

        // Edits must be saved BEFORE the exercise is added so the panel refresh keeps them.
        Received.InOrder(() =>
        {
            api.UpdateProgrammeAsync(1, Arg.Is<ProgrammeBuilderForm>(f =>
                f.ProgrammeId == 1
                && f.Sessions.Count == 2
                && f.Sessions[0].Objective == "AM purpose"
                && f.Sessions[1].Objective == "PM purpose"
                && f.Exercises.Count == 1
                && f.Exercises[0].Reps == 12
                && f.Exercises[0].Sets == 4
                && f.Exercises[0].HoldSeconds == 6
                && f.Exercises[0].Notes == "Keep steady"), Arg.Any<CancellationToken>());
            api.AddSessionExerciseAsync(1, 20, 2, Arg.Any<CancellationToken>());
        });

        var json = Assert.IsType<JsonResult>(result);
        var payload = JsonSerializer.Serialize(json.Value);
        Assert.Contains("\"ok\":true", payload);
    }

    [Fact]
    public async Task AddExercise_Post_WithEmptyForm_DoesNotCallUpdateProgramme()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.AddSessionExerciseAsync(1, 20, 2, Arg.Any<CancellationToken>())
            .Returns(new AddSessionExerciseClientResult(AddSessionExerciseClientOutcome.Added, null));

        var sut = CreateSut(api, ajaxRequest: true);

        var result = await sut.AddExercise(1, 20, 2, new ProgrammeBuilderForm { ProgrammeId = 1 }, CancellationToken.None);

        await api.DidNotReceive().UpdateProgrammeAsync(Arg.Any<ulong>(), Arg.Any<ProgrammeBuilderForm>(), Arg.Any<CancellationToken>());
        var json = Assert.IsType<JsonResult>(result);
        Assert.Contains("\"ok\":true", JsonSerializer.Serialize(json.Value));
    }

    [Fact]
    public async Task AddExercise_Post_WhenPreSaveBlocked_ReturnsErrorAndDoesNotAddExercise()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.UpdateProgrammeAsync(1, Arg.Any<ProgrammeBuilderForm>(), Arg.Any<CancellationToken>())
            .Returns(new UpdateProgrammeResult(UpdateProgrammeOutcome.Blocked, null, "Published programmes are immutable. Create a new draft to make changes."));

        var sut = CreateSut(api, ajaxRequest: true);
        var form = new ProgrammeBuilderForm
        {
            ProgrammeId = 1,
            Sessions = { new ProgrammeBuilderForm.SessionEdit { SessionId = 1, Objective = "AM purpose" } },
        };

        var result = await sut.AddExercise(1, 20, 2, form, CancellationToken.None);

        await api.DidNotReceive().AddSessionExerciseAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<CancellationToken>());
        var json = Assert.IsType<JsonResult>(result);
        var payload = JsonSerializer.Serialize(json.Value);
        Assert.Contains("\"ok\":false", payload);
        Assert.Contains("Published programmes are immutable.", payload);
    }

    [Fact]
    public async Task RemoveExercise_Post_AjaxNotFound_ReturnsJsonErrorPayload()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.RemoveSessionExerciseAsync(1, 20, 99, Arg.Any<CancellationToken>())
            .Returns(new RemoveSessionExerciseClientResult(RemoveSessionExerciseClientOutcome.NotFound, "Exercise could not be removed."));

        var sut = CreateSut(api, ajaxRequest: true);

        var result = await sut.RemoveExercise(1, 20, 99, CancellationToken.None);

        var json = Assert.IsType<JsonResult>(result);
        var payload = JsonSerializer.Serialize(json.Value);
        Assert.Contains("\"ok\":false", payload);
        Assert.Contains("Exercise could not be removed.", payload);
    }

    [Fact]
    public async Task Publish_Post_WhenApiValidationException_AggregatesDistinctErrorsIntoTempData()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.PublishProgrammeAsync(1, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<PublishResponse>(new ApiValidationException(new Dictionary<string, string[]>
            {
                ["Programme"] = ["Programme name is required.", ""],
                ["Sessions"] = ["At least one exercise is required.", "Programme name is required."],
            })));

        var sut = CreateSut(api, ajaxRequest: false);

        var result = await sut.Publish(1, CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProgrammesController.Preview), redirect.ActionName);
        var error = Assert.IsType<string>(sut.TempData["Error"]);
        Assert.Equal("Programme name is required. At least one exercise is required.", error);
    }

    [Fact]
    public async Task Publish_Post_WhenApiPublishes_SetsPublishedTempData()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.PublishProgrammeAsync(1, Arg.Any<CancellationToken>())
            .Returns(new PublishResponse("https://example.test/programme-1.pdf", "programme-1.pdf", 1234));
        api.GetProgrammeAsync(1, Arg.Any<CancellationToken>())
            .Returns(CreateProgrammeVm());

        var sut = CreateSut(api, ajaxRequest: false);

        var result = await sut.Publish(1, CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("CaseDetail", redirect.ControllerName);
        Assert.Equal((ulong)1, redirect.RouteValues?["id"]);
        Assert.Equal("programme-1.pdf", sut.TempData["PublishedFile"]);
        var publishedMessage = Assert.IsType<string>(sut.TempData["Published"]);
        Assert.Contains("Published programme-1.pdf", publishedMessage);
    }

    [Fact]
    public async Task Publish_Post_WhenApiThrowsRuntimeError_ThrowsAndDoesNotSetTempData()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.PublishProgrammeAsync(1, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<PublishResponse>(new HttpRequestException("Service unavailable")));

        var sut = CreateSut(api, ajaxRequest: false);

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.Publish(1, CancellationToken.None));
        Assert.False(sut.TempData.ContainsKey("Error"));
    }

    [Fact]
    public async Task PreviewPdf_DownloadWithToken_SetsCompletionCookieAndReturnsAttachment()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetProgrammePreviewPdfAsync(1, Arg.Any<CancellationToken>())
            .Returns(new PdfDocumentContent([1, 2, 3], "application/pdf", "programme-preview-1.pdf"));
        api.GetProgrammeAsync(1, Arg.Any<CancellationToken>())
            .Returns(CreateProgrammeVm());

        var sut = CreateSut(api, ajaxRequest: false);

        var result = await sut.PreviewPdf(1, download: true, downloadToken: "tok-123", CancellationToken.None);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("Buddy Hind Limb Rehab draft 2026-05-01.pdf", file.FileDownloadName);

        var setCookieHeaders = sut.Response.Headers.SetCookie.ToArray();
        var setCookieHeader = Assert.Single(setCookieHeaders);
        Assert.Contains("pdf-download-complete=tok-123", setCookieHeader ?? string.Empty);
    }

    [Fact]
    public async Task PreviewPdf_Download_UsesSanitizedProgrammeNameAndDateRangeAsFileName()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetProgrammePreviewPdfAsync(1, Arg.Any<CancellationToken>())
            .Returns(new PdfDocumentContent([1, 2, 3], "application/pdf", "programme-preview-1.pdf"));
        api.GetProgrammeAsync(1, Arg.Any<CancellationToken>())
            .Returns(new ProgrammeVm(
                1,
                2,
                1,
                1,
                "Buddy: Rehab / Week 1?",
                "planned",
                new DateOnly(2026, 6, 12),
                new DateOnly(2026, 7, 12),
                null,
                "Case",
                "Buddy",
                "Owner Name",
                "Test Practitioner",
                []));

        var sut = CreateSut(api, ajaxRequest: false);

        var result = await sut.PreviewPdf(1, download: true, downloadToken: null, CancellationToken.None);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("Buddy Rehab Week 1 2026-06-12 to 2026-07-12.pdf", file.FileDownloadName);
    }

    [Fact]
    public async Task Preview_WhenVersionHistoryExists_SetsLockedForEditFlagTrue()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetProgrammeAsync(1, Arg.Any<CancellationToken>()).Returns(CreateProgrammeVm());
        api.GetProgrammeVersionHistoryAsync(1, Arg.Any<CancellationToken>())
            .Returns(new ProgrammeVersionHistoryVm(
                1,
                "Draft",
                1,
                1,
                1,
                "Tester",
                "Pet",
                "Case",
                [
                    new ProgrammeVersionHistoryVm.VersionRow(1, 1, "published", null, 1, "Tester", DateTime.UtcNow, DateTime.UtcNow, null, null),
                ]));

        var sut = CreateSut(api, ajaxRequest: false);

        var result = await sut.Preview(1, CancellationToken.None);

        _ = Assert.IsType<ViewResult>(result);
        Assert.True(Assert.IsType<bool>(sut.ViewData["IsLockedForEdit"]));
    }

    [Fact]
    public async Task Publish_Post_WhenApiPublishes_RedirectsToCaseDetailWithTreatmentCaseId()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.PublishProgrammeAsync(1, Arg.Any<CancellationToken>())
            .Returns(new PublishResponse("https://example.test/p.pdf", "p.pdf", 999));
        api.GetProgrammeAsync(1, Arg.Any<CancellationToken>())
            .Returns(CreateProgrammeVm()); // TreatmentCaseId = 1 in CreateProgrammeVm

        var sut = CreateSut(api, ajaxRequest: false);

        var result = await sut.Publish(1, CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("CaseDetail", redirect.ControllerName);
        Assert.Equal((ulong)1, redirect.RouteValues?["id"]);
    }

    [Fact]
    public async Task Publish_Post_WhenValidationFails_RedirectsToPreview()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.PublishProgrammeAsync(1, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<PublishResponse>(new ApiValidationException(new Dictionary<string, string[]>
            {
                ["Sessions"] = ["Add at least one exercise before publishing."],
            })));

        var sut = CreateSut(api, ajaxRequest: false);

        var result = await sut.Publish(1, CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProgrammesController.Preview), redirect.ActionName);
        Assert.NotNull(sut.TempData["Error"]);
    }

    [Fact]
    public async Task Preview_WhenNoVersionHistory_SetsLockedForEditFlagFalse()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetProgrammeAsync(1, Arg.Any<CancellationToken>()).Returns(CreateProgrammeVm());
        api.GetProgrammeVersionHistoryAsync(1, Arg.Any<CancellationToken>())
            .Returns(new ProgrammeVersionHistoryVm(1, "Draft", 1, 1, 1, "Tester", "Pet", "Case", []));

        var sut = CreateSut(api, ajaxRequest: false);

        var result = await sut.Preview(1, CancellationToken.None);

        _ = Assert.IsType<ViewResult>(result);
        Assert.False(Assert.IsType<bool>(sut.ViewData["IsLockedForEdit"]));
    }

    private static ProgrammesController CreateSut(IAdminApiClient api, bool ajaxRequest)
    {
        var logger = Substitute.For<ILogger<ProgrammesController>>();
        var sut = new ProgrammesController(api, logger);
        var httpContext = new DefaultHttpContext();
        if (ajaxRequest)
        {
            httpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        }

        sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
        sut.TempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());
        return sut;
    }

    private static ProgrammeVm CreateProgrammeVm()
        => new(
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
            [
                new ProgrammeVm.SessionRow(
                    1,
                    "single",
                    "Improving hind-limb control.",
                    "planned",
                    1,
                    [
                        new ProgrammeVm.SessionExerciseRow(1, 1, "Step-ups (low)", "Controlled stepping", null, null, 5, 3, 5, 1, "Steady pace", []),
                    ])
            ]);
}
