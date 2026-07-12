using HelloBuddy.Contracts;
using HelloBuddy.Ui.Controllers;
using HelloBuddy.Ui.Models;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using Xunit;

namespace HelloBuddy.Ui.Tests;

public sealed class ExercisesControllerTests
{
    [Fact]
    public async Task Create_Get_UsesConfiguredGoogleDriveUrlAsDefault_WhenNoStoredSetting()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetAppSettingAsync("VideoLibrary.GoogleDriveUrl", Arg.Any<CancellationToken>())
            .Returns((string?)null);
        api.GetAppSettingAsync("FileStorage.ImageLibraryFolder", Arg.Any<CancellationToken>())
            .Returns((string?)null);
        api.ListExerciseCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseCategoryListItem>());

        var options = new MediaSearchOptions
        {
            VideoProviders =
            [
                new MediaSearchProviderOption { Description = "Google Drive", BaseUrl = "https://drive.google.com/drive/folders/configured" },
            ],
        };

        var sut = CreateSut(api, options);

        var result = await sut.Create(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Edit", view.ViewName);
        var vm = Assert.IsType<ExerciseEditorVm>(view.Model);
        var drive = Assert.Single(vm.VideoSearchProviders, x => x.Description == "Google Drive");
        Assert.Equal("https://drive.google.com/drive/folders/configured", drive.BaseUrl);
    }

    [Fact]
    public async Task Create_Get_PrefersStoredGoogleDriveUrl_OverConfiguredDefault()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetAppSettingAsync("VideoLibrary.GoogleDriveUrl", Arg.Any<CancellationToken>())
            .Returns("https://drive.google.com/drive/folders/stored");
        api.GetAppSettingAsync("FileStorage.ImageLibraryFolder", Arg.Any<CancellationToken>())
            .Returns((string?)null);
        api.ListExerciseCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseCategoryListItem>());

        var options = new MediaSearchOptions
        {
            VideoProviders =
            [
                new MediaSearchProviderOption { Description = "Google Drive", BaseUrl = "https://drive.google.com/drive/folders/configured" },
            ],
        };

        var sut = CreateSut(api, options);

        var result = await sut.Create(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<ExerciseEditorVm>(view.Model);
        var drive = Assert.Single(vm.VideoSearchProviders, x => x.Description == "Google Drive");
        Assert.Equal("https://drive.google.com/drive/folders/stored", drive.BaseUrl);
    }

    [Fact]
    public async Task Create_Get_WhenSettingsEndpointThrows_FallsBackToDefaultDriveUrl()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetAppSettingAsync("VideoLibrary.GoogleDriveUrl", Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string?>(new HttpRequestException("unavailable")));
        api.GetAppSettingAsync("FileStorage.ImageLibraryFolder", Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string?>(new HttpRequestException("unavailable")));
        api.ListExerciseCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseCategoryListItem>());

        var sut = CreateSut(api, new MediaSearchOptions());

        var result = await sut.Create(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<ExerciseEditorVm>(view.Model);
        var drive = Assert.Single(vm.VideoSearchProviders, x => x.Description == "Google Drive");
        Assert.Equal("https://drive.google.com/drive/folders/1FQXInuGCPdFP5ywFaNnO39Be0ffeZMGm", drive.BaseUrl);
        Assert.Equal("exercise-media/images/", vm.ImageLibraryFolder);
    }

    [Fact]
    public async Task Create_Get_NoOtherProvidersConfigured_AddsStandardFallbackProviders()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetAppSettingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((string?)null);
        api.ListExerciseCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseCategoryListItem>());

        var sut = CreateSut(api, new MediaSearchOptions());

        var result = await sut.Create(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<ExerciseEditorVm>(view.Model);
        Assert.Contains(vm.VideoSearchProviders, x => x.Description == "YouTube");
        Assert.Contains(vm.VideoSearchProviders, x => x.Description == "Vimeo");
        Assert.Contains(vm.VideoSearchProviders, x => x.Description == "General web");
    }

    [Fact]
    public async Task Create_Get_ImageLibraryFolder_UsesStoredFolderValue()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetAppSettingAsync("VideoLibrary.GoogleDriveUrl", Arg.Any<CancellationToken>())
            .Returns((string?)null);
        api.GetAppSettingAsync("FileStorage.ImageLibraryFolder", Arg.Any<CancellationToken>())
            .Returns("exercise-media/custom/");
        api.ListExerciseCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseCategoryListItem>());

        var sut = CreateSut(api, new MediaSearchOptions());

        var result = await sut.Create(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<ExerciseEditorVm>(view.Model);
        Assert.Equal("exercise-media/custom/", vm.ImageLibraryFolder);
    }

    [Fact]
    public async Task Create_Get_ImageLibraryFolder_DefaultsWhenNotConfigured()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetAppSettingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((string?)null);
        api.ListExerciseCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseCategoryListItem>());

        var sut = CreateSut(api, new MediaSearchOptions());

        var result = await sut.Create(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<ExerciseEditorVm>(view.Model);
        Assert.Equal("exercise-media/images/", vm.ImageLibraryFolder);
    }

    [Fact]
    public async Task Details_ExerciseNotFound_ReturnsNotFound()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetExerciseAsync(999, Arg.Any<CancellationToken>()).Returns((ExerciseDetailVm?)null);

        var sut = CreateSut(api, new MediaSearchOptions());

        var result = await sut.Details(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Image_ExerciseImageNotFound_ReturnsNotFound()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetExerciseImageAsync(1, Arg.Any<CancellationToken>()).Returns((ExerciseImageContent?)null);

        var sut = CreateSut(api, new MediaSearchOptions());

        var result = await sut.Image(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ExerciseNotFound_ReturnsNotFound()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetExerciseAsync(999, Arg.Any<CancellationToken>()).Returns((ExerciseDetailVm?)null);

        var sut = CreateSut(api, new MediaSearchOptions());

        var result = await sut.Edit(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_WhenApiReturnsNull_ReturnsNotFound()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetAppSettingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((string?)null);
        api.ListExerciseCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseCategoryListItem>());
        api.UpdateExerciseAsync(1, Arg.Any<SaveExerciseRequest>(), Arg.Any<CancellationToken>())
            .Returns((ExerciseDetailVm?)null);

        var sut = CreateSut(api, new MediaSearchOptions());
        var vm = new ExerciseEditorVm { Form = new SaveExerciseRequest { Title = "Test", IsActive = true } };

        var result = await sut.Edit(1, vm, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_WhenApiValidationFails_AddsModelErrorsAndReturnsView()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.GetAppSettingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((string?)null);
        api.ListExerciseCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseCategoryListItem>());
        api.UpdateExerciseAsync(1, Arg.Any<SaveExerciseRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ExerciseDetailVm?>(new ApiValidationException(new Dictionary<string, string[]>
            {
                ["Title"] = ["Title is required."],
            })));

        var sut = CreateSut(api, new MediaSearchOptions());
        var vm = new ExerciseEditorVm { Form = new SaveExerciseRequest { Title = "", IsActive = true } };

        var result = await sut.Edit(1, vm, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(vm, view.Model);
        Assert.False(sut.ModelState.IsValid);
        Assert.True(sut.ModelState.ContainsKey("Form.Title"));
    }

    [Fact]
    public async Task SetActive_WhenApiReturnsNull_ReturnsNotFound()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.SetExerciseActiveAsync(1, false, Arg.Any<CancellationToken>()).Returns((ExerciseDetailVm?)null);

        var sut = CreateSut(api, new MediaSearchOptions());

        var result = await sut.SetActive(1, false, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Index_ExplicitFilter_PersistsFilterToCookie()
    {
        var api = CreateIndexApi();
        var sut = CreateSut(api, new MediaSearchOptions());
        SetHttpContext(sut, queryKeys: new Dictionary<string, StringValues>
        {
            ["searchText"] = "stretch",
            ["categoryId"] = "7",
            ["hasVideo"] = "true",
            ["activeOnly"] = "false",
        });

        var result = await sut.Index("stretch", 7, true, activeOnly: false, CancellationToken.None);

        var vm = Assert.IsType<ExerciseIndexVm>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal("stretch", vm.Filter.SearchText);
        Assert.Equal(7ul, vm.Filter.CategoryId);
        Assert.True(vm.Filter.HasVideo);
        Assert.False(vm.Filter.ActiveOnly);

        var setCookie = sut.HttpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("hb_exercise_filter=", setCookie);
    }

    [Fact]
    public async Task Index_NoQuery_RestoresFilterFromCookie()
    {
        var api = CreateIndexApi();

        // First request applies an explicit filter and produces the Set-Cookie value.
        var saver = CreateSut(api, new MediaSearchOptions());
        SetHttpContext(saver, queryKeys: new Dictionary<string, StringValues>
        {
            ["searchText"] = "balance",
            ["activeOnly"] = "false",
        });
        await saver.Index("balance", null, null, activeOnly: false, CancellationToken.None);
        var cookieValue = ExtractCookieValue(saver.HttpContext.Response.Headers["Set-Cookie"].ToString());

        // Second request carries no query string but presents the saved cookie.
        var restorer = CreateSut(api, new MediaSearchOptions());
        SetHttpContext(restorer, cookieHeader: $"hb_exercise_filter={cookieValue}");

        var result = await restorer.Index(null, null, null, activeOnly: true, CancellationToken.None);

        var vm = Assert.IsType<ExerciseIndexVm>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal("balance", vm.Filter.SearchText);
        Assert.False(vm.Filter.ActiveOnly);
    }

    [Fact]
    public async Task Index_NoQueryNoCookie_UsesDefaults()
    {
        var api = CreateIndexApi();
        var sut = CreateSut(api, new MediaSearchOptions());
        SetHttpContext(sut);

        var result = await sut.Index(null, null, null, activeOnly: true, CancellationToken.None);

        var vm = Assert.IsType<ExerciseIndexVm>(Assert.IsType<ViewResult>(result).Model);
        Assert.Null(vm.Filter.SearchText);
        Assert.Null(vm.Filter.CategoryId);
        Assert.Null(vm.Filter.HasVideo);
        Assert.True(vm.Filter.ActiveOnly);
    }

    private static IAdminApiClient CreateIndexApi()
    {
        var api = Substitute.For<IAdminApiClient>();
        api.ListExercisesAsync(Arg.Any<ExerciseListFilter>(), Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseListItem>());
        api.ListExerciseCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ExerciseCategoryListItem>());
        return api;
    }

    private static void SetHttpContext(
        ExercisesController controller,
        IDictionary<string, StringValues>? queryKeys = null,
        string? cookieHeader = null)
    {
        var httpContext = new DefaultHttpContext();
        if (queryKeys is not null)
        {
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>(queryKeys));
        }
        if (cookieHeader is not null)
        {
            httpContext.Request.Headers["Cookie"] = cookieHeader;
        }
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
        };
    }

    private static string ExtractCookieValue(string setCookieHeader)
    {
        // "hb_exercise_filter=<value>; path=/; secure; samesite=lax; httponly"
        var firstSegment = setCookieHeader.Split(';', 2)[0];
        return firstSegment.Split('=', 2)[1];
    }

    private static ExercisesController CreateSut(IAdminApiClient api, MediaSearchOptions options)
        => new(api, Options.Create(options));
}