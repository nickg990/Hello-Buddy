using HelloBuddy.Contracts;
using HelloBuddy.Ui.Models;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HelloBuddy.Ui.Controllers;

[Route("Programmes/{id:long}")]
public class ProgrammesController : Controller
{
    private readonly IAdminApiClient _api;
    private readonly ILogger<ProgrammesController> _logger;

    public ProgrammesController(IAdminApiClient api, ILogger<ProgrammesController> logger)
    {
        _api = api;
        _logger = logger;
    }

    [HttpGet("Builder")]
    public async Task<IActionResult> Builder(ulong id, CancellationToken ct)
    {
        var vm = await BuildBuilderVmAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpGet("Builder/EditorPanel")]
    public async Task<IActionResult> BuilderEditorPanel(ulong id, CancellationToken ct)
    {
        var vm = await BuildBuilderVmAsync(id, ct);
        return vm is null ? NotFound() : PartialView("_BuilderEditor", vm);
    }

    [HttpGet("Builder/PreviewPanel")]
    public async Task<IActionResult> BuilderPreviewPanel(ulong id, CancellationToken ct)
    {
        var vm = await _api.GetProgrammeAsync(id, ct);
        return vm is null ? NotFound() : PartialView("_BuilderPreviewPane", vm);
    }

    [HttpPost("Builder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Builder(ulong id, ProgrammeBuilderForm form, CancellationToken ct)
    {
        if (form.ProgrammeId != id) return BadRequest();
        var updated = await _api.UpdateProgrammeAsync(id, form, ct);
        if (updated is null) return NotFound();
        if (IsAjaxRequest())
        {
            return Json(new { ok = true, message = $"Saved {form.Exercises.Count} exercise edits." });
        }

        TempData["Saved"] = $"Saved {form.Exercises.Count} exercise edits.";
        return RedirectToAction(nameof(Builder), new { id });
    }

    [HttpPost("Structure")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Structure(ulong id, ProgrammeStructureForm form, CancellationToken ct)
    {
        if (form.ProgrammeId != id) return BadRequest();

        var result = await _api.UpdateProgrammeStructureAsync(id, form, ct);
        switch (result.Outcome)
        {
            case UpdateProgrammeStructureOutcome.Updated:
                if (IsAjaxRequest())
                {
                    return Json(new { ok = true, message = "Programme dates/session structure saved." });
                }

                TempData["Saved"] = "Programme dates/session structure saved.";
                break;
            case UpdateProgrammeStructureOutcome.Invalid:
                if (IsAjaxRequest())
                {
                    return Json(new
                    {
                        ok = false,
                        error = string.IsNullOrWhiteSpace(result.Message)
                            ? "Session structure is invalid."
                            : result.Message,
                    });
                }

                TempData["Error"] = string.IsNullOrWhiteSpace(result.Message)
                    ? "Session structure is invalid."
                    : result.Message;
                break;
            default:
                return NotFound();
        }

        return RedirectToAction(nameof(Builder), new { id });
    }

    [HttpPost("Sessions/{sessionId:long}/Exercises")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExercise(ulong id, ulong sessionId, [FromForm] ulong exerciseId, CancellationToken ct)
    {
        var result = await _api.AddSessionExerciseAsync(id, sessionId, exerciseId, ct);
        switch (result.Outcome)
        {
            case AddSessionExerciseClientOutcome.Added:
                if (IsAjaxRequest())
                {
                    return Json(new { ok = true, message = "Exercise added to session." });
                }

                TempData["Saved"] = "Exercise added to session.";
                break;
            case AddSessionExerciseClientOutcome.Duplicate:
                if (IsAjaxRequest())
                {
                    return Json(new
                    {
                        ok = false,
                        error = string.IsNullOrWhiteSpace(result.Message)
                            ? "Exercise is already in this session."
                            : result.Message,
                    });
                }

                TempData["Error"] = string.IsNullOrWhiteSpace(result.Message)
                    ? "Exercise is already in this session."
                    : result.Message;
                break;
            default:
                if (IsAjaxRequest())
                {
                    return Json(new { ok = false, error = "Unable to add exercise to this session." });
                }

                TempData["Error"] = "Unable to add exercise to this session.";
                break;
        }
        return RedirectToAction(nameof(Builder), new { id });
    }

    [HttpPost("Sessions/{sessionId:long}/Exercises/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveExercise(ulong id, ulong sessionId, [FromForm] ulong sessionExerciseId, CancellationToken ct)
    {
        var result = await _api.RemoveSessionExerciseAsync(id, sessionId, sessionExerciseId, ct);
        if (IsAjaxRequest())
        {
            return Json(new
            {
                ok = result.Outcome == RemoveSessionExerciseClientOutcome.Removed,
                message = result.Outcome == RemoveSessionExerciseClientOutcome.Removed
                    ? "Exercise removed from draft programme."
                    : null,
                error = result.Outcome == RemoveSessionExerciseClientOutcome.Removed
                    ? null
                    : (result.Message ?? "Exercise could not be removed."),
            });
        }

        TempData[result.Outcome == RemoveSessionExerciseClientOutcome.Removed ? "Saved" : "Error"] =
            result.Outcome == RemoveSessionExerciseClientOutcome.Removed
                ? "Exercise removed from draft programme."
                : (result.Message ?? "Exercise could not be removed.");
        return RedirectToAction(nameof(Builder), new { id });
    }

    [HttpGet("Preview")]
    public async Task<IActionResult> Preview(ulong id, CancellationToken ct)
    {
        var vm = await _api.GetProgrammeAsync(id, ct);
        return vm is null ? NotFound() : View("Preview", vm);
    }

    [HttpPost("Publish")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(ulong id, CancellationToken ct)
    {
        try
        {
            var resp = await _api.PublishProgrammeAsync(id, ct);
            TempData["PublishedFile"] = resp.FileName;
            TempData["Published"] = $"Published {resp.FileName} ({resp.Bytes:N0} bytes).";
        }
        catch (ApiValidationException ex)
        {
            var allErrors = ex.Errors
                .SelectMany(kvp => kvp.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .ToList();
            TempData["Error"] = allErrors.Count == 0
                ? "Draft is incomplete and cannot be published yet."
                : string.Join(" ", allErrors);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected publish failure for programme {ProgrammeId}", id);
            throw;
        }

        return RedirectToAction(nameof(Builder), new { id });
    }

    // Absolute route so this action lives outside the {id:long} class prefix
    // and the layout can link to it knowing only the filename.
    [HttpGet("/Programmes/Download/{fileName}")]
    public async Task<IActionResult> Download(string fileName, CancellationToken ct)
    {
        var resp = await _api.GetDownloadUrlAsync(fileName, ct);
        return Redirect(resp.Url);
    }

    private async Task<ProgrammesBuilderPageVm?> BuildBuilderVmAsync(ulong id, CancellationToken ct)
    {
        var vm = await _api.GetProgrammeAsync(id, ct);
        if (vm is null)
        {
            return null;
        }

        var exercises = await _api.ListExercisesAsync(new ExerciseListFilter
        {
            ActiveOnly = true,
            HasVideo = null,
            CategoryId = null,
            SearchText = null,
        }, ct);

        return new ProgrammesBuilderPageVm
        {
            Programme = vm,
            AvailableExercises = exercises,
        };
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
