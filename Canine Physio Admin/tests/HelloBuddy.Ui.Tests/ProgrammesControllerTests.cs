using System.Text.Json;
using HelloBuddy.Contracts;
using HelloBuddy.Ui.Controllers;
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
            .Returns(CreateProgrammeVm());

        var sut = CreateSut(api, ajaxRequest: true);
        var form = new ProgrammeBuilderForm
        {
            ProgrammeId = 1,
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
        Assert.Contains("Saved 2 exercise edits.", payload);
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

        var result = await sut.AddExercise(1, 20, 2, CancellationToken.None);

        var json = Assert.IsType<JsonResult>(result);
        var payload = JsonSerializer.Serialize(json.Value);
        Assert.Contains("\"ok\":false", payload);
        Assert.Contains("Exercise already added.", payload);
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

        _ = Assert.IsType<RedirectToActionResult>(result);
        var error = Assert.IsType<string>(sut.TempData["Error"]);
        Assert.Equal("Programme name is required. At least one exercise is required.", error);
    }

    [Fact]
    public async Task Publish_Post_WhenApiPublishes_SetsPublishedTempData()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.PublishProgrammeAsync(1, Arg.Any<CancellationToken>())
            .Returns(new PublishResponse("https://example.test/programme-1.pdf", "programme-1.pdf", 1234));

        var sut = CreateSut(api, ajaxRequest: false);

        var result = await sut.Publish(1, CancellationToken.None);

        _ = Assert.IsType<RedirectToActionResult>(result);
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
            "Buddy Hind Limb Rehab draft",
            "planned",
            new DateOnly(2026, 5, 1),
            null,
            "Improving hind-limb control.",
            "Buddy Hind Limb Rehab",
            "Buddy",
            "Amelia Carter",
            [
                new ProgrammeVm.SessionRow(
                    1,
                    "single",
                    "Improving hind-limb control.",
                    "planned",
                    1,
                    [
                        new ProgrammeVm.SessionExerciseRow(1, 1, "Step-ups (low)", "Controlled stepping", null, null, 5, 3, 5, 1, "Steady pace"),
                    ])
            ]);
}
