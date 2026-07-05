using HelloBuddy.Application.Auth;
using HelloBuddy.Ui.Models;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelloBuddy.Ui.Controllers;

/// <summary>
/// Handles practitioner (admin) management operations.
/// All routes require authentication and administrator role.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public sealed class AdminController : Controller
{
    private readonly IPractitionerAdminService _adminService;
    private readonly IAdminApiClient _api;

    public AdminController(IPractitionerAdminService adminService, IAdminApiClient api)
    {
        _adminService = adminService;
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Practitioners(CancellationToken ct)
    {
        var practitioners = await _adminService.ListPractitionersAsync(ct);
        return View(practitioners);
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add([FromForm] AddPractitionerViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _adminService.AddPractitionerAsync(
            firstName: model.FirstName,
            lastName: model.LastName,
            email: model.Email,
            phoneNumber: model.Phone,
            role: model.Role,
            initialPassword: model.Password,
            ct);

        switch (result.Outcome)
        {
            case AdminPractitionerOutcome.Success:
                TempData["Saved"] = $"Practitioner '{model.FirstName} {model.LastName}' created.";
                return RedirectToAction("Practitioners");

            case AdminPractitionerOutcome.EmailAlreadyInUse:
                ModelState.AddModelError(nameof(model.Email), "Email is already in use.");
                return View(model);

            default:
                ModelState.AddModelError(string.Empty, result.Message ?? "Error creating practitioner.");
                return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(ulong id, CancellationToken ct)
    {
        var result = await _adminService.ListPractitionersAsync(ct);
        var practitioner = result.FirstOrDefault(p => p.PractitionerId == id);

        if (practitioner == null)
            return NotFound();

        var model = new RenamePractitionerViewModel
        {
            PractitionerId = practitioner.PractitionerId,
            FirstName = practitioner.FirstName,
            LastName = practitioner.LastName,
            Email = practitioner.Email,
            Phone = practitioner.PhoneNumber ?? string.Empty,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] RenamePractitionerViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _adminService.RenamePractitionerAsync(
            targetPractitionerId: model.PractitionerId,
            firstName: model.FirstName,
            lastName: model.LastName,
            newEmail: model.Email,
            phoneNumber: model.Phone,
            ct);

        switch (result.Outcome)
        {
            case AdminPractitionerOutcome.Success:
                TempData["Saved"] = "Practitioner updated.";
                return RedirectToAction("Practitioners");

            case AdminPractitionerOutcome.NotFound:
                return NotFound();

            case AdminPractitionerOutcome.EmailAlreadyInUse:
                ModelState.AddModelError(nameof(model.Email), "Email is already in use.");
                return View(model);

            default:
                ModelState.AddModelError(string.Empty, result.Message ?? "Error updating practitioner.");
                return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> SetPassword(ulong id, CancellationToken ct)
    {
        var result = await _adminService.ListPractitionersAsync(ct);
        var practitioner = result.FirstOrDefault(p => p.PractitionerId == id);

        if (practitioner == null)
            return NotFound();

        var model = new SetPasswordViewModel
        {
            PractitionerId = id,
            PractitionerName = $"{practitioner.FirstName} {practitioner.LastName}",
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword([FromForm] SetPasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _adminService.SetPasswordAsync(
            targetPractitionerId: model.PractitionerId,
            newPassword: model.NewPassword,
            ct);

        switch (result.Outcome)
        {
            case AdminPractitionerOutcome.Success:
                TempData["Saved"] = "Password updated.";
                return RedirectToAction("Practitioners");

            case AdminPractitionerOutcome.NotFound:
                return NotFound();

            default:
                ModelState.AddModelError(string.Empty, result.Message ?? "Error updating password.");
                return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
    {
        var result = await _adminService.DeactivatePractitionerAsync(
            targetPractitionerId: id,
            requestingPractitionerId: GetCurrentPractitionerId(),
            ct);

        switch (result.Outcome)
        {
            case AdminPractitionerOutcome.Success:
                TempData["Saved"] = "Practitioner deactivated.";
                return RedirectToAction("Practitioners");

            case AdminPractitionerOutcome.NotFound:
                TempData["Error"] = "Practitioner not found.";
                return RedirectToAction("Practitioners");

            case AdminPractitionerOutcome.CannotTargetSelf:
                TempData["Error"] = "Cannot deactivate your own account.";
                return RedirectToAction("Practitioners");

            default:
                TempData["Error"] = result.Message ?? "Error deactivating practitioner.";
                return RedirectToAction("Practitioners");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(ulong id, CancellationToken ct)
    {
        var result = await _adminService.ActivatePractitionerAsync(
            targetPractitionerId: id,
            ct);

        switch (result.Outcome)
        {
            case AdminPractitionerOutcome.Success:
                TempData["Saved"] = "Practitioner re-activated.";
                return RedirectToAction("Practitioners");

            case AdminPractitionerOutcome.NotFound:
                TempData["Error"] = "Practitioner not found.";
                return RedirectToAction("Practitioners");

            default:
                TempData["Error"] = result.Message ?? "Error activating practitioner.";
                return RedirectToAction("Practitioners");
        }
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([FromForm] ChangeOwnPasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var idClaim = User.FindFirst("practitioner_id")?.Value;
        if (!ulong.TryParse(idClaim, out var practitionerId) || practitionerId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _adminService.ChangeOwnPasswordAsync(
            adminPractitionerId: practitionerId,
            currentPassword: model.CurrentPassword,
            newPassword: model.NewPassword,
            ct);

        switch (result.Outcome)
        {
            case AdminPractitionerOutcome.Success:
                TempData["Saved"] = "Password changed.";
                return RedirectToAction("Practitioners");

            case AdminPractitionerOutcome.NotFound:
                TempData["Error"] = "Account not found.";
                return View(model);

            default:
                ModelState.AddModelError(string.Empty, result.Message ?? "Invalid current password or error updating.");
                return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> DataControl(CancellationToken ct)
    {
        var model = await BuildOwnerDataControlViewModelAsync(ownerId: null, ct);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DataControl([FromForm] OwnerDataControlRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var invalidModel = await BuildOwnerDataControlViewModelAsync(request.OwnerId, ct);
            return View(invalidModel);
        }

        var result = await _api.ApplyOwnerDataControlAsync(request.OwnerId, ct);
        switch (result.Outcome)
        {
            case OwnerDataControlClientOutcome.Deleted:
                TempData["Saved"] = result.Message;
                return RedirectToAction(nameof(DataControl));

            default:
                ModelState.AddModelError(nameof(request.OwnerId),
                    "Owner was not found or is not linked to the current practitioner.");
                var notFoundModel = await BuildOwnerDataControlViewModelAsync(request.OwnerId, ct);
                return View(notFoundModel);
        }
    }

    private async Task<AdminOwnerDataControlViewModel> BuildOwnerDataControlViewModelAsync(ulong? ownerId, CancellationToken ct)
    {
        var owners = await _api.ListOwnersAsync(ct);
        return new AdminOwnerDataControlViewModel
        {
            OwnerId = ownerId,
            Owners = owners
                .OrderBy(o => o.FullName, StringComparer.OrdinalIgnoreCase)
                .ToList(),
        };
    }

    [HttpGet]
    public async Task<IActionResult> Settings(CancellationToken ct)
    {
        var driveUrl = await _api.GetAppSettingAsync("VideoLibrary.GoogleDriveUrl", ct);
        var imageUrl = await _api.GetAppSettingAsync("FileStorage.ImageLibraryUrl", ct);
        return View(new SettingsPageVm
        {
            GoogleDriveUrl = driveUrl ?? string.Empty,
            ImageLibraryUrl = imageUrl ?? string.Empty,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings([FromForm] SettingsPageVm model, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(model.GoogleDriveUrl))
        {
            if (!Uri.TryCreate(model.GoogleDriveUrl.Trim(), UriKind.Absolute, out var uri)
                || uri.Scheme != Uri.UriSchemeHttps
                || !uri.Host.Equals("drive.google.com", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.GoogleDriveUrl),
                    "Must be a valid https://drive.google.com/... URL.");
                return View(model);
            }
        }

        if (!string.IsNullOrWhiteSpace(model.ImageLibraryUrl))
        {
            if (!Uri.TryCreate(model.ImageLibraryUrl.Trim(), UriKind.Absolute, out var imageUri)
                || (imageUri.Scheme != Uri.UriSchemeHttps && imageUri.Scheme != Uri.UriSchemeHttp))
            {
                ModelState.AddModelError(nameof(model.ImageLibraryUrl),
                    "Must be a valid http(s):// URL.");
                return View(model);
            }
        }

        await _api.SaveAppSettingAsync("VideoLibrary.GoogleDriveUrl",
            string.IsNullOrWhiteSpace(model.GoogleDriveUrl) ? null : model.GoogleDriveUrl.Trim(),
            ct);
        await _api.SaveAppSettingAsync("FileStorage.ImageLibraryUrl",
            string.IsNullOrWhiteSpace(model.ImageLibraryUrl) ? null : model.ImageLibraryUrl.Trim(),
            ct);
        TempData["Saved"] = "Settings saved.";
        return RedirectToAction(nameof(Settings));
    }

    private ulong GetCurrentPractitionerId()
    {
        var idClaim = User.FindFirst("practitioner_id")?.Value;
        return ulong.TryParse(idClaim, out var id) ? id : 0;
    }
}
