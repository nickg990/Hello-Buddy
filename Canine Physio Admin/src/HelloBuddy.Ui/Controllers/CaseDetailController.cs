using HelloBuddy.Ui.Services;
using HelloBuddy.Ui.Models;
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

    [HttpPost("Notes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Notes(ulong id, CaseDetailPageVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var current = await _api.GetCaseAsync(id, ct);
            return current is null ? NotFound() : View("Index", new CaseDetailPageVm { Case = current, NewNote = vm.NewNote });
        }

        try
        {
            var note = await _api.AddCaseNoteAsync(id, vm.NewNote, ct);
            if (note is null)
            {
                return NotFound();
            }

            TempData["Saved"] = "Case note added.";
            return RedirectToAction(nameof(Index), new { id });
        }
        catch (ApiValidationException ex)
        {
            foreach (var entry in ex.Errors)
            {
                foreach (var error in entry.Value)
                {
                    ModelState.AddModelError($"NewNote.{entry.Key}", error);
                }
            }

            var current = await _api.GetCaseAsync(id, ct);
            return current is null ? NotFound() : View("Index", new CaseDetailPageVm { Case = current, NewNote = vm.NewNote });
        }
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
        switch (result.Outcome)
        {
            case DeleteProgrammeOutcome.Deleted:
                TempData["CaseProgrammeMessage"] = "Programme deleted.";
                TempData["CaseProgrammeMessageType"] = "success";
                break;
            case DeleteProgrammeOutcome.Blocked:
                TempData["CaseProgrammeMessage"] = result.Message ?? "Programme cannot be deleted because it has version history.";
                TempData["CaseProgrammeMessageType"] = "danger";
                break;
            default:
                TempData["CaseProgrammeMessage"] = "Programme not found.";
                TempData["CaseProgrammeMessageType"] = "danger";
                break;
        }

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
            TempData["CaseProgrammeMessage"] = "Programme status unchanged.";
            TempData["CaseProgrammeMessageType"] = "success";
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
                TempData["CaseProgrammeMessage"] = "Select a valid status action.";
                TempData["CaseProgrammeMessageType"] = "danger";
                return RedirectToAction(nameof(Index), new { id });
        }

        if (result.Outcome == ProgrammeStatusTransitionClientOutcome.Updated)
        {
            TempData["CaseProgrammeMessage"] = normalizedTarget == "active"
                ? "Programme activated."
                : "Programme completed.";
            TempData["CaseProgrammeMessageType"] = "success";
        }
        else
        {
            TempData["CaseProgrammeMessage"] = result.Message ?? "Programme status could not be updated.";
            TempData["CaseProgrammeMessageType"] = "danger";
        }

        return RedirectToAction(nameof(Index), new { id });
    }
}
