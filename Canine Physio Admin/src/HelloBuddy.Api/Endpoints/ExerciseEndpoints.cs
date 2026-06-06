using FluentValidation;
using HelloBuddy.Application.Records;
using HelloBuddy.Contracts;
using HelloBuddy.Admin.Pdf;
using HelloBuddy.Api.Services;
using System.Text.RegularExpressions;

namespace HelloBuddy.Api.Endpoints;

public static class ExerciseEndpoints
{
    private static readonly HashSet<string> AllowedMediaContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private static readonly HashSet<string> AllowedMediaExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    private static readonly Regex SafeFileNameChars = new("[^a-z0-9._-]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static IEndpointRouteBuilder MapExerciseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/exercises", async (
            string? searchText,
            long? categoryId,
            bool? hasVideo,
            bool? activeOnly,
            IExerciseRepository exercises,
            CancellationToken ct) =>
        {
            var filter = new ExerciseListFilter
            {
                SearchText = searchText,
                CategoryId = categoryId.HasValue && categoryId.Value > 0 ? (ulong)categoryId.Value : null,
                HasVideo = hasVideo,
                ActiveOnly = activeOnly ?? true
            };

            var rows = await exercises.ListAsync(filter, ct);
            return Results.Ok(rows);
        });

        app.MapGet("/api/exercises/{id:long}", async (long id, IExerciseRepository exercises, CancellationToken ct) =>
        {
            var exercise = await exercises.GetAsync((ulong)id, ct);
            return exercise is null ? Results.NotFound() : Results.Ok(exercise);
        });

        app.MapGet("/api/exercises/{id:long}/image", async (
            long id,
            IExerciseRepository exercises,
            IFileStore fileStore,
            CancellationToken ct) =>
        {
            var exercise = await exercises.GetAsync((ulong)id, ct);
            if (exercise is null || string.IsNullOrWhiteSpace(exercise.ImageUrl))
            {
                return Results.NotFound();
            }

            if (!TryResolveManagedKey(exercise.ImageUrl, out var key))
            {
                return Results.NotFound();
            }

            var artefact = await fileStore.OpenReadAsync(key, ct);
            if (artefact is null)
            {
                return Results.NotFound();
            }

            return Results.File(artefact.Content, artefact.ContentType);
        });

        app.MapGet("/api/exercise-categories", async (IExerciseRepository exercises, CancellationToken ct) =>
        {
            var categories = await exercises.ListCategoriesAsync(ct);
            return Results.Ok(categories);
        });

        app.MapPost("/api/exercises/media", async (
            IFormFile? file,
            IConfiguration config,
            IFileStore fileStore,
            IExerciseMediaMalwareScanner scanner,
            CancellationToken ct) =>
        {
            var validation = new Dictionary<string, string[]>();
            if (file is null || file.Length == 0)
            {
                validation["file"] = ["An image file is required."];
                return Results.ValidationProblem(validation);
            }

            var maxBytes = config.GetValue<long?>("Storage:ExerciseMediaMaxBytes") ?? (5L * 1024L * 1024L);
            if (file.Length > maxBytes)
            {
                validation["file"] = [$"File is too large. Maximum size is {maxBytes / (1024 * 1024)} MB."];
                return Results.ValidationProblem(validation);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedMediaExtensions.Contains(extension))
            {
                validation["file"] = ["Only .jpg, .jpeg, .png, and .webp files are supported."];
                return Results.ValidationProblem(validation);
            }

            var contentType = file.ContentType.Trim().ToLowerInvariant();
            if (!AllowedMediaContentTypes.Contains(contentType))
            {
                validation["file"] = ["Only JPEG, PNG, and WEBP image uploads are supported."];
                return Results.ValidationProblem(validation);
            }

            byte[] bytes;
            await using (var stream = file.OpenReadStream())
            await using (var memory = new MemoryStream())
            {
                await stream.CopyToAsync(memory, ct);
                bytes = memory.ToArray();
            }

            var scan = await scanner.ScanAsync(bytes, file.FileName, contentType, ct);
            if (!scan.IsAllowed)
            {
                validation["file"] = [scan.BlockReason ?? "File rejected by media scanning policy."];
                return Results.ValidationProblem(validation);
            }

            var safeName = BuildSafeFileName(Path.GetFileNameWithoutExtension(file.FileName));
            var key = $"exercise-media/{DateTime.UtcNow:yyyy/MM}/{safeName}-{Guid.NewGuid():N}{extension}";
            var uri = await fileStore.WriteAsync(key, bytes, contentType, ct);

            var baseUrl = config["Storage:ExerciseMediaBaseUrl"];
            var canonicalUrl = BuildCanonicalUrl(baseUrl, key, uri);

            return Results.Ok(new ExerciseMediaUploadResponse(
                canonicalUrl,
                file.FileName,
                contentType,
                file.Length));
        })
        // Service-to-service endpoint: UI server posts multipart content to API inside
        // infrastructure trust boundaries; browser-origin CSRF tokens are not applicable.
        .DisableAntiforgery();

        app.MapPost("/api/exercises", async (
            SaveExerciseRequest request,
            IExerciseRepository exercises,
            IValidator<SaveExerciseRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            if (!await exercises.CategoryExistsAsync(request.ExerciseCategoryId, ct))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.ExerciseCategoryId)] = ["Selected category was not found."]
                });
            }

            var exerciseId = await exercises.CreateAsync(request, ct);
            var exercise = await exercises.GetAsync(exerciseId, ct);
            return Results.Created($"/api/exercises/{exerciseId}", exercise);
        });

        app.MapPut("/api/exercises/{id:long}", async (
            long id,
            SaveExerciseRequest request,
            IExerciseRepository exercises,
            IValidator<SaveExerciseRequest> validator,
            IExerciseMediaGovernanceService governance,
            CancellationToken ct) =>
        {
            var existing = await exercises.GetAsync((ulong)id, ct);
            if (existing is null)
            {
                return Results.NotFound();
            }

            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            if (!await exercises.CategoryExistsAsync(request.ExerciseCategoryId, ct))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.ExerciseCategoryId)] = ["Selected category was not found."]
                });
            }

            var updated = await exercises.UpdateAsync((ulong)id, request, ct);
            if (!updated)
            {
                return Results.NotFound();
            }

            var exercise = await exercises.GetAsync((ulong)id, ct);
            await governance.HandleImageUrlChangeAsync(existing.ImageUrl, exercise?.ImageUrl, ct);
            return Results.Ok(exercise);
        });

        app.MapPost("/api/exercises/{id:long}/activate", async (long id, IExerciseRepository exercises, CancellationToken ct) =>
        {
            var updated = await exercises.SetActiveAsync((ulong)id, true, ct);
            if (!updated)
            {
                return Results.NotFound();
            }

            var exercise = await exercises.GetAsync((ulong)id, ct);
            return Results.Ok(exercise);
        });

        app.MapPost("/api/exercises/{id:long}/deactivate", async (long id, IExerciseRepository exercises, CancellationToken ct) =>
        {
            var updated = await exercises.SetActiveAsync((ulong)id, false, ct);
            if (!updated)
            {
                return Results.NotFound();
            }

            var exercise = await exercises.GetAsync((ulong)id, ct);
            return Results.Ok(exercise);
        });

        return app;
    }

    private static string BuildSafeFileName(string fileName)
    {
        var collapsed = SafeFileNameChars.Replace(fileName.Trim().ToLowerInvariant(), "-");
        collapsed = collapsed.Trim('-');
        while (collapsed.Contains("--", StringComparison.Ordinal))
        {
            collapsed = collapsed.Replace("--", "-", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(collapsed) ? "exercise-image" : collapsed;
    }

    private static string BuildCanonicalUrl(string? baseUrl, string key, Uri fallbackUri)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return fallbackUri.ToString();
        }

        return $"{baseUrl.TrimEnd('/')}/{key}";
    }

    private static bool TryResolveManagedKey(string url, out string key)
    {
        key = string.Empty;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        const string marker = "/exercise-media/";
        var full = uri.AbsolutePath.Replace('\\', '/');
        var markerIndex = full.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return false;
        }

        var relative = full[(markerIndex + 1)..].Trim('/');
        if (string.IsNullOrWhiteSpace(relative))
        {
            return false;
        }

        key = relative;
        return true;
    }
}
