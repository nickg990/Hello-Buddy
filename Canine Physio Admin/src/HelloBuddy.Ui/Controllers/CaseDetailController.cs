using HelloBuddy.Ui.Services;
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
        return vm is null ? NotFound() : View(vm);
    }
}
