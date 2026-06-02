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
}
