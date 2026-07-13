using HelloBuddy.Contracts;
using HelloBuddy.Ui.Models;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace HelloBuddy.Ui.Controllers;

[Route("Programmes/{id:long}")]
public class ProgrammesController : Controller
{
    private static readonly Regex MultiWhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

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
        var updateResult = await _api.UpdateProgrammeAsync(id, form, ct);
        if (updateResult.Outcome == UpdateProgrammeOutcome.NotFound)
        {
            return NotFound();
        }

        if (updateResult.Outcome == UpdateProgrammeOutcome.Blocked)
        {
            var blockedMessage = string.IsNullOrWhiteSpace(updateResult.Message)
                ? "Published programmes are immutable. Create a new draft to make changes."
                : updateResult.Message;

            if (IsAjaxRequest())
            {
                return Json(BuilderAjaxResponse.Failure(blockedMessage));
            }

            TempData["Error"] = blockedMessage;
            return RedirectToAction(nameof(Builder), new { id });
        }

        if (IsAjaxRequest())
        {
            var savedMessage = $"Saved {form.Sessions.Count} session summaries and {form.Exercises.Count} exercise edits.";
            return Json(BuilderAjaxResponse.Success(savedMessage));
        }

        TempData["Saved"] = $"Saved {form.Sessions.Count} session summaries and {form.Exercises.Count} exercise edits.";
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
                    return Json(BuilderAjaxResponse.Success("Programme dates/session structure saved."));
                }

                TempData["Saved"] = "Programme dates/session structure saved.";
                break;
            case UpdateProgrammeStructureOutcome.Invalid:
                if (IsAjaxRequest())
                {
                    return Json(BuilderAjaxResponse.Failure(
                        string.IsNullOrWhiteSpace(result.Message)
                            ? "Session structure is invalid."
                            : result.Message));
                }

                TempData["Error"] = string.IsNullOrWhiteSpace(result.Message)
                    ? "Session structure is invalid."
                    : result.Message;
                break;
            case UpdateProgrammeStructureOutcome.Blocked:
                if (IsAjaxRequest())
                {
                    return Json(BuilderAjaxResponse.Failure(
                        string.IsNullOrWhiteSpace(result.Message)
                            ? "Published programmes are immutable. Create a new draft to make changes."
                            : result.Message));
                }

                TempData["Error"] = string.IsNullOrWhiteSpace(result.Message)
                    ? "Published programmes are immutable. Create a new draft to make changes."
                    : result.Message;
                break;
            default:
                return NotFound();
        }

        return RedirectToAction(nameof(Builder), new { id });
    }

    [HttpPost("Sessions/{sessionId:long}/Exercises")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExercise(ulong id, ulong sessionId, [FromForm] ulong exerciseId, ProgrammeBuilderForm form, CancellationToken ct)
    {
        // Persist any unsaved session edits (purpose summaries, reps/sets/hold/notes/sort)
        // before adding the exercise, so the subsequent panel refresh does not discard them.
        if (form.ProgrammeId == id && (form.Sessions.Count > 0 || form.Exercises.Count > 0))
        {
            var saveResult = await _api.UpdateProgrammeAsync(id, form, ct);
            if (saveResult.Outcome == UpdateProgrammeOutcome.NotFound)
            {
                return NotFound();
            }

            if (saveResult.Outcome == UpdateProgrammeOutcome.Blocked)
            {
                var blockedMessage = string.IsNullOrWhiteSpace(saveResult.Message)
                    ? "Published programmes are immutable. Create a new draft to make changes."
                    : saveResult.Message;

                if (IsAjaxRequest())
                {
                    return Json(BuilderAjaxResponse.Failure(blockedMessage));
                }

                TempData["Error"] = blockedMessage;
                return RedirectToAction(nameof(Builder), new { id });
            }
        }

        var result = await _api.AddSessionExerciseAsync(id, sessionId, exerciseId, ct);
        switch (result.Outcome)
        {
            case AddSessionExerciseClientOutcome.Added:
                if (IsAjaxRequest())
                {
                    return Json(BuilderAjaxResponse.Success("Exercise added to session."));
                }

                TempData["Saved"] = "Exercise added to session.";
                break;
            case AddSessionExerciseClientOutcome.Duplicate:
                if (IsAjaxRequest())
                {
                    return Json(BuilderAjaxResponse.Failure(
                        string.IsNullOrWhiteSpace(result.Message)
                            ? "Exercise is already in this session."
                            : result.Message));
                }

                TempData["Error"] = string.IsNullOrWhiteSpace(result.Message)
                    ? "Exercise is already in this session."
                    : result.Message;
                break;
            case AddSessionExerciseClientOutcome.Blocked:
                if (IsAjaxRequest())
                {
                    return Json(BuilderAjaxResponse.Failure(
                        string.IsNullOrWhiteSpace(result.Message)
                            ? "Published programmes are immutable. Create a new draft to make changes."
                            : result.Message));
                }

                TempData["Error"] = string.IsNullOrWhiteSpace(result.Message)
                    ? "Published programmes are immutable. Create a new draft to make changes."
                    : result.Message;
                break;
            default:
                if (IsAjaxRequest())
                {
                    return Json(BuilderAjaxResponse.Failure("Unable to add exercise to this session."));
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
            return Json(result.Outcome == RemoveSessionExerciseClientOutcome.Removed
                ? BuilderAjaxResponse.Success("Exercise removed from draft programme.")
                : BuilderAjaxResponse.Failure(result.Message ?? "Exercise could not be removed."));
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
        if (vm is null)
        {
            return NotFound();
        }

        var history = await _api.GetProgrammeVersionHistoryAsync(id, ct);
        ViewData["IsLockedForEdit"] = history is { Versions.Count: > 0 };
        return View("Preview", vm);
    }

    [HttpGet("Versions/{versionId:long}/Pdf")]
    public async Task<IActionResult> ViewVersionPdf(ulong id, ulong versionId, CancellationToken ct)
    {
        var doc = await _api.GetProgrammeVersionPdfAsync(id, versionId, ct);
        if (doc is null)
        {
            return NotFound();
        }

        return File(doc.Bytes, doc.ContentType);
    }

    [HttpPost("Versions/{versionId:long}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVersion(ulong id, ulong versionId, CancellationToken ct)
    {
        var deleted = await _api.DeleteProgrammeVersionAsync(id, versionId, ct);
        TempData[deleted ? "Saved" : "Error"] = deleted
            ? "Version deleted."
            : "Version not found.";
        return RedirectToAction(nameof(History), new { id });
    }

    [HttpGet("PreviewPdf")]
    public async Task<IActionResult> PreviewPdf(ulong id, bool download, string? downloadToken, CancellationToken ct)
    {
        var pdf = await _api.GetProgrammePreviewPdfAsync(id, ct);
        if (pdf is null)
        {
            return NotFound();
        }

        if (download)
        {
            if (!string.IsNullOrWhiteSpace(downloadToken))
            {
                Response.Cookies.Append(
                    "pdf-download-complete",
                    downloadToken,
                    new CookieOptions
                    {
                        Path = "/",
                        HttpOnly = false,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax,
                        Secure = Request.IsHttps,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(1),
                    });
            }

            var programme = await _api.GetProgrammeAsync(id, ct);
            var downloadFileName = BuildPreviewDownloadFileName(programme, id, pdf.FileName);
            return File(pdf.Bytes, pdf.ContentType, downloadFileName);
        }

        return File(pdf.Bytes, pdf.ContentType);
    }

    [HttpGet("History")]
    public async Task<IActionResult> History(ulong id, CancellationToken ct)
    {
        var history = await _api.GetProgrammeVersionHistoryAsync(id, ct);
        return history is null ? NotFound() : View(history);
    }

    [HttpPost("CreateDraftFromPublished")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDraftFromPublished(ulong id, CancellationToken ct)
    {
        var result = await _api.CreateDraftFromPublishedAsync(id, ct);
        switch (result.Outcome)
        {
            case CreateDraftFromPublishedClientOutcome.Created:
                TempData["Saved"] = "Created a new editable draft from published history.";
                return RedirectToAction(nameof(Builder), new { id = result.Programme!.ProgrammeId });
            case CreateDraftFromPublishedClientOutcome.Invalid:
                TempData["Error"] = result.Message ?? "No published history is available for draft creation.";
                return RedirectToAction(nameof(Builder), new { id });
            default:
                return NotFound();
        }
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

        // Success path already set TempData["Published"]; fetch the programme to get its case id
        // for the redirect. On failure the catch set TempData["Error"] and we return to Preview.
        if (TempData.ContainsKey("Published"))
        {
            var programme = await _api.GetProgrammeAsync(id, ct);
            return RedirectToAction("Index", "CaseDetail", new { id = programme?.TreatmentCaseId ?? 0 });
        }

        return RedirectToAction(nameof(Preview), new { id });
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

        // Load the full active exercise set once; the add-exercise filter pane
        // narrows it client-side (search / category / video), so no round-trip
        // is needed per keystroke and each session panel can filter independently.
        var exercises = await _api.ListExercisesAsync(new ExerciseListFilter
        {
            ActiveOnly = true,
            HasVideo = null,
            CategoryId = null,
            SearchText = null,
        }, ct);

        var categories = await _api.ListExerciseCategoriesAsync(ct);
        var categoryOptions = categories
            .Where(x => x.IsActive)
            .Select(x => new SelectListItem(x.CategoryName, x.ExerciseCategoryId.ToString()))
            .ToList();

        return new ProgrammesBuilderPageVm
        {
            Programme = vm,
            AvailableExercises = exercises,
            CategoryOptions = categoryOptions,
        };
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    private static string BuildPreviewDownloadFileName(ProgrammeVm? programme, ulong programmeId, string fallbackFileName)
    {
        if (programme is null)
        {
            return fallbackFileName;
        }

        var baseName = $"{programme.ProgrammeName} {programme.StartDate:yyyy-MM-dd}";
        if (programme.EndDate is { } endDate)
        {
            baseName += $" to {endDate:yyyy-MM-dd}";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitizedChars = baseName
            .Select(ch => invalidChars.Contains(ch) ? ' ' : ch)
            .ToArray();
        var sanitized = MultiWhitespaceRegex.Replace(new string(sanitizedChars), " ").Trim();
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = $"programme-{programmeId} {programme.StartDate:yyyy-MM-dd}";
        }

        return $"{sanitized}.pdf";
    }

}
