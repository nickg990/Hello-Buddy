using HelloBuddy.Contracts;
using HelloBuddy.Ui.Models;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelloBuddy.Ui.Controllers;

[Route("Pets")]
public class PetsController : Controller
{
    private readonly IAdminApiClient _api;

    public PetsController(IAdminApiClient api)
    {
        _api = api;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var pets = await _api.ListPetsAsync(ct);
        return View(pets);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(ulong id, CancellationToken ct)
    {
        var pet = await _api.GetPetAsync(id, ct);
        return pet is null ? NotFound() : View(pet);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(ulong? ownerId, CancellationToken ct)
    {
        return View("Edit", await BuildPetEditorVmAsync(new SavePetRequest
        {
            OwnerId = ownerId ?? 0,
            Sex = "unknown",
            IsActive = true
        }, null, ct));
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PetEditorVm vm, CancellationToken ct)
    {
        vm.OwnerOptions = await GetOwnerOptionsAsync(ct);
        if (!ModelState.IsValid)
        {
            return View("Edit", vm);
        }

        try
        {
            var pet = await _api.CreatePetAsync(vm.Form, ct);
            TempData["Saved"] = $"Created pet {pet.Name}.";
            return RedirectToAction(nameof(Details), new { id = pet.PetId });
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
        var pet = await _api.GetPetAsync(id, ct);
        if (pet is null)
        {
            return NotFound();
        }

        return View(await BuildPetEditorVmAsync(new SavePetRequest
        {
            OwnerId = pet.OwnerId,
            Name = pet.Name,
            Age = pet.Age,
            DateOfBirth = pet.DateOfBirth,
            Breed = pet.Breed,
            Sex = pet.Sex,
            Weight = pet.Weight,
            IsActive = pet.IsActive
        }, id, ct));
    }

    [HttpPost("{id:long}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ulong id, PetEditorVm vm, CancellationToken ct)
    {
        vm.PetId = id;
        vm.OwnerOptions = await GetOwnerOptionsAsync(ct);
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var pet = await _api.UpdatePetAsync(id, vm.Form, ct);
            if (pet is null)
            {
                return NotFound();
            }

            TempData["Saved"] = $"Updated pet {pet.Name}.";
            return RedirectToAction(nameof(Details), new { id = pet.PetId });
        }
        catch (ApiValidationException ex)
        {
            ApplyApiValidation(ex);
            return View(vm);
        }
    }

    private async Task<PetEditorVm> BuildPetEditorVmAsync(SavePetRequest form, ulong? petId, CancellationToken ct)
    {
        return new PetEditorVm
        {
            PetId = petId,
            Form = form,
            OwnerOptions = await GetOwnerOptionsAsync(ct)
        };
    }

    private async Task<IReadOnlyList<SelectListItem>> GetOwnerOptionsAsync(CancellationToken ct)
    {
        var owners = await _api.ListOwnersAsync(includeAnonymised: false, ct);
        return owners
            .Select(owner => new SelectListItem(owner.FullName, owner.OwnerId.ToString()))
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