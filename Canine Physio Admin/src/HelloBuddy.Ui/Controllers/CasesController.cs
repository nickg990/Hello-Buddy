using HelloBuddy.Contracts;
using HelloBuddy.Ui.Services;
using HelloBuddy.Ui.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelloBuddy.Ui.Controllers;

[Route("Cases")]
public class CasesController : Controller
{
    private readonly IAdminApiClient _api;

    public CasesController(IAdminApiClient api)
    {
        _api = api;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var rows = await _api.ListCasesAsync(ct);
        return View(rows);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(ulong? petId, CancellationToken ct)
    {
        return View("Edit", await BuildCaseEditorVmAsync(new SaveTreatmentCaseRequest
        {
            PetId = petId ?? 0,
            Status = "planned",
            StartDate = DateOnly.FromDateTime(DateTime.Today)
        }, null, ct));
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CaseEditorVm vm, CancellationToken ct)
    {
        vm.PetOptions = await GetPetOptionsAsync(ct);
        if (!ModelState.IsValid)
        {
            return View("Edit", vm);
        }

        try
        {
            var treatmentCase = await _api.CreateCaseAsync(vm.Form, ct);
            TempData["Saved"] = $"Created treatment case {treatmentCase.CaseTitle}.";
            return RedirectToAction("Index", "CaseDetail", new { id = treatmentCase.TreatmentCaseId });
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
        var treatmentCase = await _api.GetCaseAsync(id, ct);
        if (treatmentCase is null)
        {
            return NotFound();
        }

        return View(await BuildCaseEditorVmAsync(new SaveTreatmentCaseRequest
        {
            PetId = treatmentCase.PetId,
            CaseTitle = treatmentCase.CaseTitle,
            ClinicalSummary = treatmentCase.ClinicalSummary,
            StartDate = treatmentCase.StartDate,
            EndDate = treatmentCase.EndDate,
            Status = treatmentCase.Status
        }, id, ct));
    }

    [HttpPost("{id:long}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ulong id, CaseEditorVm vm, CancellationToken ct)
    {
        vm.TreatmentCaseId = id;
        vm.PetOptions = await GetPetOptionsAsync(ct);
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var treatmentCase = await _api.UpdateCaseAsync(id, vm.Form, ct);
            if (treatmentCase is null)
            {
                return NotFound();
            }

            TempData["Saved"] = $"Updated treatment case {treatmentCase.CaseTitle}.";
            return RedirectToAction("Index", "CaseDetail", new { id = treatmentCase.TreatmentCaseId });
        }
        catch (ApiValidationException ex)
        {
            ApplyApiValidation(ex);
            return View(vm);
        }
    }

    private async Task<CaseEditorVm> BuildCaseEditorVmAsync(SaveTreatmentCaseRequest form, ulong? treatmentCaseId, CancellationToken ct)
    {
        return new CaseEditorVm
        {
            TreatmentCaseId = treatmentCaseId,
            Form = form,
            PetOptions = await GetPetOptionsAsync(ct)
        };
    }

    private async Task<IReadOnlyList<SelectListItem>> GetPetOptionsAsync(CancellationToken ct)
    {
        var pets = await _api.ListPetsAsync(ct);
        return pets
            .Select(pet => new SelectListItem($"{pet.Name} ({pet.OwnerName})", pet.PetId.ToString()))
            .ToList();
    }

    private void ApplyApiValidation(ApiValidationException ex)
    {
        foreach (var entry in ex.Errors)
        {
            foreach (var error in entry.Value)
            {
                ModelState.AddModelError($"Form.{entry.Key}", error);
            }
        }
    }
}
