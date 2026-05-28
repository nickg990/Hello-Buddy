using HelloBuddy.Contracts;
using HelloBuddy.Ui.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelloBuddy.Ui.Controllers;

[Route("Programmes/{id:long}")]
public class ProgrammesController : Controller
{
    private readonly IAdminApiClient _api;

    public ProgrammesController(IAdminApiClient api)
    {
        _api = api;
    }

    [HttpGet("Builder")]
    public async Task<IActionResult> Builder(ulong id, CancellationToken ct)
    {
        var vm = await _api.GetProgrammeAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost("Builder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Builder(ulong id, ProgrammeBuilderForm form, CancellationToken ct)
    {
        if (form.ProgrammeId != id) return BadRequest();
        var updated = await _api.UpdateProgrammeAsync(id, form, ct);
        if (updated is null) return NotFound();
        TempData["Saved"] = $"Saved {form.Exercises.Count} exercise edits.";
        return RedirectToAction(nameof(Builder), new { id });
    }

    [HttpGet("Preview")]
    public async Task<IActionResult> Preview(ulong id, CancellationToken ct)
    {
        var vm = await _api.GetProgrammeAsync(id, ct);
        return vm is null ? NotFound() : View("Preview", vm);
    }

    [HttpPost("Publish")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(ulong id, CancellationToken ct)
    {
        var resp = await _api.PublishProgrammeAsync(id, ct);
        TempData["PublishedFile"] = resp.FileName;
        TempData["Published"] = $"Published {resp.FileName} ({resp.Bytes:N0} bytes).";
        return RedirectToAction(nameof(Builder), new { id });
    }

    // Absolute route so this action lives outside the {id:long} class prefix
    // and the layout can link to it knowing only the filename.
    [HttpGet("/Programmes/Download/{fileName}")]
    public async Task<IActionResult> Download(string fileName, CancellationToken ct)
    {
        var resp = await _api.GetDownloadUrlAsync(fileName, ct);
        return Redirect(resp.Url);
    }
}
