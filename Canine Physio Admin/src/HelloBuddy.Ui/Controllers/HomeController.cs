using System.Diagnostics;
using HelloBuddy.Ui.Models;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelloBuddy.Ui.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAdminApiClient _api;

    public HomeController(ILogger<HomeController> logger, IAdminApiClient api)
    {
        _logger = logger;
        _api = api;
    }

    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Privacy(CancellationToken ct)
    {
        var owners = await _api.ListOwnersAsync(includeAnonymised: false, ct);
        return View(new HomePrivacyVm { Owners = owners });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RightToBeForgotten(ulong ownerId, CancellationToken ct)
    {
        if (ownerId == 0)
        {
            TempData["Error"] = "Select a valid owner before continuing.";
            return RedirectToAction(nameof(Privacy));
        }

        var result = await _api.ApplyOwnerDataControlAsync(ownerId, ct);
        if (result.Outcome == OwnerDataControlClientOutcome.Deleted
            || result.Outcome == OwnerDataControlClientOutcome.Anonymised)
        {
            TempData["Saved"] = result.Message;
            return RedirectToAction(nameof(Privacy));
        }

        TempData["Error"] = "Owner was not found or is not linked to the current practitioner.";
        return RedirectToAction(nameof(Privacy));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
