using HelloBuddy.Contracts;
using HelloBuddy.Ui.Models;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelloBuddy.Ui.Controllers;

[Route("Owners")]
public class OwnersController : Controller
{
    private readonly IAdminApiClient _api;

    public OwnersController(IAdminApiClient api)
    {
        _api = api;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(bool showAnonymised, CancellationToken ct)
    {
        var owners = await _api.ListOwnersAsync(showAnonymised, ct);
        ViewBag.ShowAnonymised = showAnonymised;
        return View(owners);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(ulong id, bool showAnonymised, CancellationToken ct)
    {
        var owner = await _api.GetOwnerAsync(id, showAnonymised, ct);
        ViewBag.ShowAnonymised = showAnonymised;
        return owner is null ? NotFound() : View(owner);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View("Edit", new OwnerEditorVm());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OwnerEditorVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View("Edit", vm);
        }

        try
        {
            var owner = await _api.CreateOwnerAsync(vm.Form, ct);
            TempData["Saved"] = $"Created owner {owner.FullName}.";
            return RedirectToAction(nameof(Details), new { id = owner.OwnerId });
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
        var owner = await _api.GetOwnerAsync(id, includeAnonymised: true, ct);
        if (owner is null)
        {
            return NotFound();
        }

        return View(new OwnerEditorVm
        {
            OwnerId = owner.OwnerId,
            Form = new SaveOwnerRequest
            {
                FirstName = owner.FirstName,
                LastName = owner.LastName,
                Email = owner.Email,
                PhoneNumber = owner.PhoneNumber,
                AddressLine1 = owner.AddressLine1,
                AddressLine2 = owner.AddressLine2,
                Town = owner.Town,
                Postcode = owner.Postcode
            }
        });
    }

    [HttpPost("{id:long}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ulong id, OwnerEditorVm vm, CancellationToken ct)
    {
        vm.OwnerId = id;
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var owner = await _api.UpdateOwnerAsync(id, vm.Form, ct);
            if (owner is null)
            {
                return NotFound();
            }

            TempData["Saved"] = $"Updated owner {owner.FullName}.";
            return RedirectToAction(nameof(Details), new { id = owner.OwnerId });
        }
        catch (ApiValidationException ex)
        {
            ApplyApiValidation(ex);
            return View(vm);
        }
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