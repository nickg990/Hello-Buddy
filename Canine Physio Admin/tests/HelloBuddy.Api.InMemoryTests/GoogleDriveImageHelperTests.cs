using HelloBuddy.Application.Media;
using HelloBuddy.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace HelloBuddy.Api.InMemoryTests;

public sealed class GoogleDriveImageHelperTests
{
    // ── TryConvertToDirectUrl ──────────────────────────────────────────────────

    [Theory]
    [InlineData("https://drive.google.com/file/d/1ABC123xyz/view", "1ABC123xyz")]
    [InlineData("https://drive.google.com/file/d/1ABC123xyz/view?usp=sharing", "1ABC123xyz")]
    [InlineData("https://drive.google.com/file/d/1ABC123xyz", "1ABC123xyz")]
    [InlineData("https://drive.google.com/file/d/1ABC123xyz/preview", "1ABC123xyz")]
    [InlineData("http://drive.google.com/file/d/FILEID99/view", "FILEID99")]
    public void TryConvertToDirectUrl_DriveFileUrl_ReturnsDirectForm(string input, string expectedId)
    {
        var converted = GoogleDriveImageHelper.TryConvertToDirectUrl(input, out var directUrl);

        Assert.True(converted);
        Assert.Equal($"https://drive.google.com/uc?export=view&id={expectedId}", directUrl);
    }

    [Theory]
    [InlineData("https://drive.google.com/drive/folders/1ABC123xyz")]
    [InlineData("https://docs.google.com/document/d/1ABC123xyz/edit")]
    [InlineData("https://photos.google.com/photo/1ABC123xyz")]
    [InlineData("https://images.example.com/photo.jpg")]
    [InlineData("https://example.com")]
    public void TryConvertToDirectUrl_NonDriveFileUrl_ReturnsFalse(string input)
    {
        var converted = GoogleDriveImageHelper.TryConvertToDirectUrl(input, out var directUrl);

        Assert.False(converted);
        Assert.Equal(string.Empty, directUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryConvertToDirectUrl_NullOrWhitespace_ReturnsFalse(string? input)
    {
        var converted = GoogleDriveImageHelper.TryConvertToDirectUrl(input, out var directUrl);

        Assert.False(converted);
        Assert.Equal(string.Empty, directUrl);
    }

    // ── ToDisplayUrl ──────────────────────────────────────────────────────────

    [Fact]
    public void ToDisplayUrl_DriveFileUrl_ReturnsDirectForm()
    {
        const string input = "https://drive.google.com/file/d/FILE123/view";

        var result = GoogleDriveImageHelper.ToDisplayUrl(input);

        Assert.Equal("https://drive.google.com/uc?export=view&id=FILE123", result);
    }

    [Fact]
    public void ToDisplayUrl_NonDriveUrl_ReturnsUnchanged()
    {
        const string input = "https://images.example.com/photo.jpg";

        var result = GoogleDriveImageHelper.ToDisplayUrl(input);

        Assert.Equal(input, result);
    }

    [Fact]
    public void ToDisplayUrl_Null_ReturnsNull()
    {
        var result = GoogleDriveImageHelper.ToDisplayUrl(null);

        Assert.Null(result);
    }
}

public sealed class ExerciseImageDriveRedirectTests : IClassFixture<ApiInMemoryTests.Factory>
{
    private readonly ApiInMemoryTests.Factory _factory;

    public ExerciseImageDriveRedirectTests(ApiInMemoryTests.Factory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ExerciseImage_WithDriveViewerUrl_Redirects302ToDirectUrl()
    {
        const string driveViewUrl = "https://drive.google.com/file/d/TESTFILEID/view";
        const string expectedRedirect = "https://drive.google.com/uc?export=view&id=TESTFILEID";

        var seedClient = _factory.CreateClient();
        seedClient.DefaultRequestHeaders.Add("X-Practitioner-Id", "1");
        seedClient.DefaultRequestHeaders.Add("X-Practitioner-Role", "administrator");

        var categories = await seedClient.GetFromJsonAsync<List<ExerciseCategoryListItem>>("/api/exercise-categories");
        Assert.NotNull(categories);
        var category = categories.First();

        var create = await seedClient.PostAsJsonAsync("/api/exercises", new SaveExerciseRequest
        {
            ExerciseCategoryId = category.ExerciseCategoryId,
            Title = $"Drive redirect test {Guid.NewGuid():N}",
            IsActive = true,
            ImageUrl = driveViewUrl,
            Instructions =
            [
                new SaveExerciseRequest.InstructionStepInput { StepNumber = 1, InstructionText = "Step one." }
            ]
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<ExerciseDetailVm>();
        Assert.NotNull(created);

        // Non-redirect client to observe the 302 directly
        var noRedirectClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        noRedirectClient.DefaultRequestHeaders.Add("X-Practitioner-Id", "1");

        var response = await noRedirectClient.GetAsync($"/api/exercises/{created.ExerciseId}/image");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(expectedRedirect, response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ExerciseImage_WithNoImage_Returns404()
    {
        var seedClient = _factory.CreateClient();
        seedClient.DefaultRequestHeaders.Add("X-Practitioner-Id", "1");
        seedClient.DefaultRequestHeaders.Add("X-Practitioner-Role", "administrator");

        var categories = await seedClient.GetFromJsonAsync<List<ExerciseCategoryListItem>>("/api/exercise-categories");
        Assert.NotNull(categories);
        var category = categories.First();

        var create = await seedClient.PostAsJsonAsync("/api/exercises", new SaveExerciseRequest
        {
            ExerciseCategoryId = category.ExerciseCategoryId,
            Title = $"No image test {Guid.NewGuid():N}",
            IsActive = true,
            ImageUrl = null,
            Instructions =
            [
                new SaveExerciseRequest.InstructionStepInput { StepNumber = 1, InstructionText = "Step one." }
            ]
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<ExerciseDetailVm>();
        Assert.NotNull(created);

        var noRedirectClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        noRedirectClient.DefaultRequestHeaders.Add("X-Practitioner-Id", "1");

        var response = await noRedirectClient.GetAsync($"/api/exercises/{created.ExerciseId}/image");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ExerciseImage_WithNonDriveNonManagedUrl_Returns404()
    {
        var seedClient = _factory.CreateClient();
        seedClient.DefaultRequestHeaders.Add("X-Practitioner-Id", "1");
        seedClient.DefaultRequestHeaders.Add("X-Practitioner-Role", "administrator");

        var categories = await seedClient.GetFromJsonAsync<List<ExerciseCategoryListItem>>("/api/exercise-categories");
        Assert.NotNull(categories);
        var category = categories.First();

        var create = await seedClient.PostAsJsonAsync("/api/exercises", new SaveExerciseRequest
        {
            ExerciseCategoryId = category.ExerciseCategoryId,
            Title = $"External url test {Guid.NewGuid():N}",
            IsActive = true,
            ImageUrl = "https://images.example.com/photo.jpg",
            Instructions =
            [
                new SaveExerciseRequest.InstructionStepInput { StepNumber = 1, InstructionText = "Step one." }
            ]
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<ExerciseDetailVm>();
        Assert.NotNull(created);

        var noRedirectClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        noRedirectClient.DefaultRequestHeaders.Add("X-Practitioner-Id", "1");

        var response = await noRedirectClient.GetAsync($"/api/exercises/{created.ExerciseId}/image");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
