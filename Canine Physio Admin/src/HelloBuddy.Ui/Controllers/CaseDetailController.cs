using HelloBuddy.Ui.Services;
using HelloBuddy.Ui.Models;
using HelloBuddy.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HelloBuddy.Ui.Controllers;

[Route("Cases/{id:long}")]
public class CaseDetailController : Controller
{
    private readonly IAdminApiClient _api;

    public CaseDetailController(IAdminApiClient api)
    {
        _api = api;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(ulong id, CancellationToken ct)
    {
        var vm = await _api.GetCaseAsync(id, ct);
        return vm is null ? NotFound() : View(new CaseDetailPageVm { Case = vm });
    }

    [HttpGet("ContentPanel")]
    public async Task<IActionResult> ContentPanel(ulong id, CancellationToken ct)
    {
        var vm = await _api.GetCaseAsync(id, ct);
        return vm is null ? NotFound() : PartialView("_CaseDetailBody", new CaseDetailPageVm { Case = vm });
    }

    [HttpPost("Notes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Notes(
        ulong id,
        [Bind(Prefix = nameof(CaseDetailPageVm.NewNote))] CreateCaseNoteRequest newNote,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var current = await _api.GetCaseAsync(id, ct);
            return current is null ? NotFound() : View("Index", new CaseDetailPageVm { Case = current, NewNote = newNote });
        }

        try
        {
            var note = await _api.AddCaseNoteAsync(id, newNote, ct);
            if (note is null)
            {
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                return Json(BuilderAjaxResponse.Success("Case note added."));
            }

            TempData["Saved"] = "Case note added.";
            return RedirectToAction(nameof(Index), new { id });
        }
        catch (ApiValidationException ex)
        {
            if (IsAjaxRequest())
            {
                var message = ex.Errors
                    .SelectMany(e => e.Value)
                    .FirstOrDefault() ?? "Enter valid note details before saving.";
                return Json(BuilderAjaxResponse.Failure(message));
            }

            foreach (var entry in ex.Errors)
            {
                foreach (var error in entry.Value)
                {
                    ModelState.AddModelError($"{nameof(CaseDetailPageVm.NewNote)}.{entry.Key}", error);
                }
            }

            var current = await _api.GetCaseAsync(id, ct);
            return current is null ? NotFound() : View("Index", new CaseDetailPageVm { Case = current, NewNote = newNote });
        }
    }

    [HttpPost("Notes/{noteId:long}/Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNote(ulong id, ulong noteId, CreateCaseNoteRequest editNote, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            if (IsAjaxRequest())
            {
                return Json(BuilderAjaxResponse.Failure("Enter note text before saving."));
            }

            TempData["Error"] = "Enter note text before saving.";
            return RedirectToAction(nameof(Index), new { id });
        }

        try
        {
            var note = await _api.UpdateCaseNoteAsync(id, noteId, editNote, ct);
            if (IsAjaxRequest())
            {
                return Json(note is null
                    ? BuilderAjaxResponse.Failure("Case note was not found.")
                    : BuilderAjaxResponse.Success("Case note updated."));
            }

            TempData[note is null ? "Error" : "Saved"] = note is null ? "Case note was not found." : "Case note updated.";
        }
        catch (ApiValidationException)
        {
            if (IsAjaxRequest())
            {
                return Json(BuilderAjaxResponse.Failure("Enter valid note details before saving."));
            }

            TempData["Error"] = "Enter valid note details before saving.";
        }

        return RedirectToAction(nameof(Index), new { id });
    }

    [HttpPost("Notes/{noteId:long}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNote(ulong id, ulong noteId, CancellationToken ct)
    {
        var deleted = await _api.DeleteCaseNoteAsync(id, noteId, ct);
        if (IsAjaxRequest())
        {
            return Json(deleted
                ? BuilderAjaxResponse.Success("Case note deleted.")
                : BuilderAjaxResponse.Failure("Case note was not found."));
        }

        TempData[deleted ? "Saved" : "Error"] = deleted ? "Case note deleted." : "Case note was not found.";
        return RedirectToAction(nameof(Index), new { id });
    }

    [HttpPost("Programmes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProgramme(ulong id, CancellationToken ct)
    {
        var programme = await _api.CreateDraftProgrammeAsync(id, ct);
        if (programme is null)
        {
            return NotFound();
        }

        TempData["Saved"] = $"Created draft programme {programme.ProgrammeName}.";
        return RedirectToAction("Builder", "Programmes", new { id = programme.ProgrammeId });
    }

    [HttpPost("Programmes/{programmeId:long}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProgramme(ulong id, ulong programmeId, CancellationToken ct)
    {
        var result = await _api.DeleteProgrammeAsync(programmeId, ct);
        var deleted = result.Outcome == DeleteProgrammeOutcome.Deleted;
        if (IsAjaxRequest())
        {
            return Json(deleted
                ? BuilderAjaxResponse.Success("Programme deleted.")
                : BuilderAjaxResponse.Failure("Programme not found."));
        }

        TempData[deleted ? "Saved" : "Error"] = deleted ? "Programme deleted." : "Programme not found.";
        return RedirectToAction(nameof(Index), new { id });
    }

    [HttpPost("Programmes/{programmeId:long}/Status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProgrammeStatus(ulong id, ulong programmeId, string currentStatus, string targetStatus, CancellationToken ct)
    {
        ProgrammeStatusTransitionClientResult result;
        var normalizedCurrent = currentStatus?.Trim().ToLowerInvariant() ?? string.Empty;
        var normalizedTarget = targetStatus?.Trim().ToLowerInvariant() ?? string.Empty;

        if (normalizedCurrent == normalizedTarget)
        {
            if (IsAjaxRequest())
            {
                return Json(BuilderAjaxResponse.Success("Programme status unchanged."));
            }

            TempData["Saved"] = "Programme status unchanged.";
            return RedirectToAction(nameof(Index), new { id });
        }

        switch (normalizedTarget)
        {
            case "active":
                result = await _api.ActivateProgrammeAsync(programmeId, ct);
                break;
            case "completed":
                result = await _api.CompleteProgrammeAsync(programmeId, ct);
                break;
            default:
                if (IsAjaxRequest())
                {
                    return Json(BuilderAjaxResponse.Failure("Select a valid status action."));
                }

                TempData["Error"] = "Select a valid status action.";
                return RedirectToAction(nameof(Index), new { id });
        }

        var updated = result.Outcome == ProgrammeStatusTransitionClientOutcome.Updated;
        var statusMessage = updated
            ? (normalizedTarget == "active" ? "Programme activated." : "Programme completed.")
            : (result.Message ?? "Programme status could not be updated.");

        if (IsAjaxRequest())
        {
            return Json(updated
                ? BuilderAjaxResponse.Success(statusMessage)
                : BuilderAjaxResponse.Failure(statusMessage));
        }

        TempData[updated ? "Saved" : "Error"] = statusMessage;
        return RedirectToAction(nameof(Index), new { id });
    }

    private bool IsAjaxRequest()
        => string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
