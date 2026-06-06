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
        return exercise is null ? NotFound() : View(exercise);
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
        vm.VideoSearchProviders = BuildVideoSearchProviderOptions();
        vm.Form.Instructions = ParseInstructions(vm.InstructionsText);

        if (!ModelState.IsValid)
        {
            return View("Edit", vm);
        }

        try
        {
            await ApplyImageSelectionAsync(vm, ct);
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
        vm.VideoSearchProviders = BuildVideoSearchProviderOptions();
        vm.Form.Instructions = ParseInstructions(vm.InstructionsText);

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            await ApplyImageSelectionAsync(vm, ct);
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
            VideoSearchProviders = BuildVideoSearchProviderOptions(),
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

    private IReadOnlyList<VideoSearchProviderVm> BuildVideoSearchProviderOptions()
    {
        var configured = _mediaSearchOptions.VideoProviders
            .Where(x => !string.IsNullOrWhiteSpace(x.Description) && IsHttpUrl(x.BaseUrl))
            .Select(x => new VideoSearchProviderVm
            {
                Description = x.Description.Trim(),
                BaseUrl = x.BaseUrl.Trim()
            })
            .ToList();

        if (configured.Count > 0)
        {
            return configured;
        }

        return
        [
            new VideoSearchProviderVm
            {
                Description = "Google Drive",
                BaseUrl = "https://drive.google.com/drive/u/1/folders/13mCIF8x8VNVfg30xbbnrAxKEFRh2QF9C"
            },
            new VideoSearchProviderVm
            {
                Description = "YouTube",
                BaseUrl = "https://www.youtube.com/results?search_query={query}"
            },
            new VideoSearchProviderVm
            {
                Description = "Vimeo",
                BaseUrl = "https://vimeo.com/search?q={query}"
            },
            new VideoSearchProviderVm
            {
                Description = "General web",
                BaseUrl = "https://www.google.com/search?q={query}"
            }
        ];
    }

    private static bool IsHttpUrl(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
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
                : entry.Key.Equals("file", StringComparison.OrdinalIgnoreCase)
                    ? nameof(ExerciseEditorVm.UploadImage)
                    : $"Form.{entry.Key}";

            foreach (var error in entry.Value)
            {
                ModelState.AddModelError(key, error);
            }
        }
    }

    private async Task ApplyImageSelectionAsync(ExerciseEditorVm vm, CancellationToken ct)
    {
        if (vm.UploadImage is { Length: > 0 })
        {
            await using var stream = vm.UploadImage.OpenReadStream();
            var uploaded = await _api.UploadExerciseImageAsync(
                stream,
                vm.UploadImage.FileName,
                vm.UploadImage.ContentType,
                ct);
            vm.Form.ImageUrl = uploaded.Url;
            return;
        }

        if (vm.RemoveImage)
        {
            vm.Form.ImageUrl = null;
        }
    }
}
