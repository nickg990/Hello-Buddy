using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelloBuddy.Ui.Controllers;

public class CasesController : Controller
{
    private readonly IAdminApiClient _api;

    public CasesController(IAdminApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var rows = await _api.ListCasesAsync(ct);
        return View(rows);
    }
}
