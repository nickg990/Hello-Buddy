using HelloBuddy.Contracts;
using HelloBuddy.Ui.Models;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace HelloBuddy.Ui.Controllers;

[Route("Exercises")]
public class ExercisesController : Controller
{
    private readonly IAdminApiClient _api;
    private readonly MediaSearchOptions _mediaSearchOptions;

    public ExercisesController(IAdminApiClient api, IOptions<MediaSearchOptions> mediaSearchOptions)
    {
        _api = api;
        _mediaSearchOptions = mediaSearchOptions.Value;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? searchText,
        ulong? categoryId,
        bool? hasVideo,
        bool activeOnly = true,
        CancellationToken ct = default)
    {
        var filter = new ExerciseListFilter
        {
            SearchText = searchText,
            CategoryId = categoryId,
            HasVideo = hasVideo,
            ActiveOnly = activeOnly
        };

        var vm = new ExerciseIndexVm
        {
            Filter = filter,
            Exercises = await _api.ListExercisesAsync(filter, ct),
            CategoryOptions = await BuildCategoryOptionsAsync(ct)
        };

        return View(vm);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(ulong id, CancellationToken ct)
    {
        var exercise = await _api.GetExerciseAsync(id, ct);
        if (exercise is null)
        {
            return NotFound();
        }

        var auditHistory = await _api.GetExerciseAuditHistoryAsync(id, ct);
        return View(new ExerciseDetailPageVm
        {
            Exercise = exercise,
            AuditHistory = auditHistory
        });
    }

    [HttpGet("{id:long}/Image")]
    public async Task<IActionResult> Image(ulong id, CancellationToken ct)
    {
        var image = await _api.GetExerciseImageAsync(id, ct);
        if (image is null)
        {
            return NotFound();
        }

        return File(image.Bytes, image.ContentType);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        return View("Edit", await BuildEditorVmAsync(new SaveExerciseRequest
        {
            IsActive = true
        }, null, legacyInstructionsText: null, ct));
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExerciseEditorVm vm, CancellationToken ct)
    {
        vm.CategoryOptions = await BuildCategoryOptionsAsync(ct);
        vm.VideoSearchProviders = await BuildVideoSearchProviderOptionsAsync(ct);
        vm.ImageLibraryUrl = await BuildImageLibraryUrlAsync(ct);
        vm.Form.Instructions = ParseInstructions(vm.InstructionsText);

        if (!ModelState.IsValid)
        {
            return View("Edit", vm);
        }

        try
        {
            var exercise = await _api.CreateExerciseAsync(vm.Form, ct);
            TempData["Saved"] = $"Created exercise {exercise.Title}.";
            return RedirectToAction(nameof(Details), new { id = exercise.ExerciseId });
        }
        catch (ApiValidationException ex)
        {
            ApplyApiValidation(ex);
            return View("Edit", vm);
        }
    }

    [HttpGet("{id:long}/Edit")]
    public async Task<IActionResult> Edit(ulong id, CancellationToken ct)
    {
        var exercise = await _api.GetExerciseAsync(id, ct);
        if (exercise is null)
        {
            return NotFound();
        }

        return View(await BuildEditorVmAsync(new SaveExerciseRequest
        {
            ExerciseCategoryId = exercise.ExerciseCategoryId ?? 0,
            Title = exercise.Title,
            ObjectiveSummary = exercise.ObjectiveSummary,
            ImageUrl = exercise.ImageUrl,
            VideoUrl = exercise.VideoUrl,
            DefaultReps = exercise.DefaultReps,
            DefaultSets = exercise.DefaultSets,
            DefaultHoldSeconds = exercise.DefaultHoldSeconds,
            IsActive = exercise.IsActive,
            Instructions = exercise.Instructions
                .Select(step => new SaveExerciseRequest.InstructionStepInput
                {
                    StepNumber = step.StepNumber,
                    InstructionText = step.InstructionText
                })
                .ToList()
        }, id, exercise.LegacyInstructionsText, ct));
    }

    [HttpPost("{id:long}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ulong id, ExerciseEditorVm vm, CancellationToken ct)
    {
        vm.ExerciseId = id;
        vm.CategoryOptions = await BuildCategoryOptionsAsync(ct);
        vm.VideoSearchProviders = await BuildVideoSearchProviderOptionsAsync(ct);
        vm.ImageLibraryUrl = await BuildImageLibraryUrlAsync(ct);
        vm.Form.Instructions = ParseInstructions(vm.InstructionsText);

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var exercise = await _api.UpdateExerciseAsync(id, vm.Form, ct);
            if (exercise is null)
            {
                return NotFound();
            }

            TempData["Saved"] = $"Updated exercise {exercise.Title}.";
            return RedirectToAction(nameof(Details), new { id = exercise.ExerciseId });
        }
        catch (ApiValidationException ex)
        {
            ApplyApiValidation(ex);
            return View(vm);
        }
    }

    [HttpPost("{id:long}/SetActive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(ulong id, bool isActive, CancellationToken ct)
    {
        var exercise = await _api.SetExerciseActiveAsync(id, isActive, ct);
        if (exercise is null)
        {
            return NotFound();
        }

        TempData["Saved"] = isActive
            ? $"Activated exercise {exercise.Title}."
            : $"Deactivated exercise {exercise.Title}.";
        return RedirectToAction(nameof(Details), new { id = exercise.ExerciseId });
    }

    private async Task<ExerciseEditorVm> BuildEditorVmAsync(
        SaveExerciseRequest form,
        ulong? exerciseId,
        string? legacyInstructionsText,
        CancellationToken ct)
    {
        return new ExerciseEditorVm
        {
            ExerciseId = exerciseId,
            Form = form,
            CategoryOptions = await BuildCategoryOptionsAsync(ct),
            VideoSearchProviders = await BuildVideoSearchProviderOptionsAsync(ct),
            ImageLibraryUrl = await BuildImageLibraryUrlAsync(ct),
            LegacyInstructionsText = legacyInstructionsText,
            InstructionsText = string.Join(Environment.NewLine,
                form.Instructions
                    .OrderBy(x => x.StepNumber)
                    .Select(x => x.InstructionText))
        };
    }

    private async Task<IReadOnlyList<SelectListItem>> BuildCategoryOptionsAsync(CancellationToken ct)
    {
        var categories = await _api.ListExerciseCategoriesAsync(ct);
        return categories
            .Where(x => x.IsActive)
            .Select(x => new SelectListItem(x.CategoryName, x.ExerciseCategoryId.ToString()))
            .ToList();
    }

    private async Task<IReadOnlyList<VideoSearchProviderVm>> BuildVideoSearchProviderOptionsAsync(CancellationToken ct)
    {
        // Resolve the Google Drive URL from the admin-managed DB setting.
        // Falls back to appsettings when the setting is not yet configured.
        const string driveKey = "VideoLibrary.GoogleDriveUrl";
        const string driveDefault = "https://drive.google.com/drive/folders/1FQXInuGCPdFP5ywFaNnO39Be0ffeZMGm";

        string? storedDriveUrl = null;
        try
        {
            storedDriveUrl = await _api.GetAppSettingAsync(driveKey, ct);
        }
        catch
        {
            // If the settings endpoint is unavailable, fall through to defaults.
        }

        var configuredDriveEntry = _mediaSearchOptions.VideoProviders
            .FirstOrDefault(x => x.Description?.Trim() == "Google Drive");

        var driveUrl = !string.IsNullOrWhiteSpace(storedDriveUrl)
            ? storedDriveUrl.Trim()
            : (configuredDriveEntry is not null && IsHttpUrl(configuredDriveEntry.BaseUrl)
                ? configuredDriveEntry.BaseUrl.Trim()
                : driveDefault);

        var others = _mediaSearchOptions.VideoProviders
            .Where(x => x.Description?.Trim() != "Google Drive"
                        && !string.IsNullOrWhiteSpace(x.Description)
                        && IsHttpUrl(x.BaseUrl))
            .Select(x => new VideoSearchProviderVm { Description = x.Description.Trim(), BaseUrl = x.BaseUrl.Trim() })
            .ToList();

        var result = new List<VideoSearchProviderVm>
        {
            new VideoSearchProviderVm { Description = "Google Drive", BaseUrl = driveUrl }
        };
        result.AddRange(others);

        // If appsettings has no other providers at all, add the standard fallbacks.
        if (others.Count == 0)
        {
            result.Add(new VideoSearchProviderVm { Description = "YouTube", BaseUrl = "https://www.youtube.com/results?search_query={query}" });
            result.Add(new VideoSearchProviderVm { Description = "Vimeo", BaseUrl = "https://vimeo.com/search?q={query}" });
            result.Add(new VideoSearchProviderVm { Description = "General web", BaseUrl = "https://www.google.com/search?q={query}" });
        }

        return result;
    }

    private static bool IsHttpUrl(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }

    private async Task<string> BuildImageLibraryUrlAsync(CancellationToken ct)
    {
        // Resolve the image-library URL from the admin-managed DB setting
        // (Settings → File storage → Image library URL). Empty when unset.
        try
        {
            var stored = await _api.GetAppSettingAsync("FileStorage.ImageLibraryUrl", ct);
            if (!string.IsNullOrWhiteSpace(stored) && IsHttpUrl(stored.Trim()))
            {
                return stored.Trim();
            }
        }
        catch
        {
            // If the settings endpoint is unavailable, fall through to empty.
        }

        return string.Empty;
    }

    private static List<SaveExerciseRequest.InstructionStepInput> ParseInstructions(string instructionsText)
    {
        var lines = instructionsText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToList();

        var output = new List<SaveExerciseRequest.InstructionStepInput>(lines.Count);
        for (ushort i = 0; i < lines.Count; i++)
        {
            output.Add(new SaveExerciseRequest.InstructionStepInput
            {
                StepNumber = (ushort)(i + 1),
                InstructionText = lines[i]
            });
        }

        return output;
    }

    private void ApplyApiValidation(ApiValidationException ex)
    {
        foreach (var entry in ex.Errors)
        {
            var key = entry.Key.Equals(nameof(SaveExerciseRequest.Instructions), StringComparison.OrdinalIgnoreCase)
                ? nameof(ExerciseEditorVm.InstructionsText)
                : $"Form.{entry.Key}";

            foreach (var error in entry.Value)
            {
                ModelState.AddModelError(key, error);
            }
        }
    }

}
