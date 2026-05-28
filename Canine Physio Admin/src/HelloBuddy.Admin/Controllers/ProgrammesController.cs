using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Identity;
using HelloBuddy.Admin.Models;
using HelloBuddy.Admin.Pdf;
using HelloBuddy.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelloBuddy.Admin.Controllers;

[Route("Programmes/{id:long}")]
public class ProgrammesController : Controller
{
    private readonly CaninePhysioDbContext _db;
    private readonly ICurrentPractitionerAccessor _practitioner;
    private readonly IRazorViewToStringRenderer _viewRenderer;
    private readonly IPdfRenderer _pdfRenderer;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ProgrammesController> _logger;

    public ProgrammesController(
        CaninePhysioDbContext db,
        ICurrentPractitionerAccessor practitioner,
        IRazorViewToStringRenderer viewRenderer,
        IPdfRenderer pdfRenderer,
        IWebHostEnvironment env,
        ILogger<ProgrammesController> logger)
    {
        _db = db;
        _practitioner = practitioner;
        _viewRenderer = viewRenderer;
        _pdfRenderer = pdfRenderer;
        _env = env;
        _logger = logger;
    }

    [HttpGet("Builder")]
    public async Task<IActionResult> Builder(ulong id, CancellationToken ct)
    {
        var vm = await LoadProgrammeVmAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost("Builder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Builder(ulong id, ProgrammeBuilderForm form, CancellationToken ct)
    {
        if (form.ProgrammeId != id) return BadRequest();

        // Authorisation: ensure the programme belongs to a case owned by this practitioner.
        var ownsProgramme = await _db.Programmes
            .AnyAsync(p => p.ProgrammeId == id && p.TreatmentCase.PractitionerId == _practitioner.PractitionerId, ct);
        if (!ownsProgramme) return NotFound();

        var ids = form.Exercises.Select(e => e.SessionExerciseId).ToList();
        var entities = await _db.Sessionexercises
            .Where(se => ids.Contains(se.SessionExerciseId) && se.Session.ProgrammeId == id)
            .ToListAsync(ct);

        foreach (var edit in form.Exercises)
        {
            var entity = entities.FirstOrDefault(e => e.SessionExerciseId == edit.SessionExerciseId);
            if (entity is null) continue;
            entity.Reps = edit.Reps;
            entity.Sets = edit.Sets;
            entity.HoldSeconds = edit.HoldSeconds;
            entity.SortOrder = edit.SortOrder;
            entity.Notes = edit.Notes;
        }

        await _db.SaveChangesAsync(ct);
        TempData["Saved"] = $"Saved {entities.Count} exercise edits.";
        return RedirectToAction(nameof(Builder), new { id });
    }

    [HttpGet("Preview")]
    public async Task<IActionResult> Preview(ulong id, CancellationToken ct)
    {
        var vm = await LoadProgrammeVmAsync(id, ct);
        return vm is null ? NotFound() : View("Preview", vm);
    }

    [HttpPost("Publish")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(ulong id, CancellationToken ct)
    {
        var vm = await LoadProgrammeVmAsync(id, ct);
        if (vm is null) return NotFound();

        var html = await _viewRenderer.RenderAsync("Preview", vm, ct);
        var pdf = await _pdfRenderer.RenderAsync(html, ct);

        var outputDir = Path.Combine(_env.ContentRootPath, "published-programmes");
        Directory.CreateDirectory(outputDir);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var fileName = $"programme-{id}-{timestamp}.pdf";
        var path = Path.Combine(outputDir, fileName);
        await System.IO.File.WriteAllBytesAsync(path, pdf, ct);
        _logger.LogInformation("Published programme {ProgrammeId} to {Path} ({Bytes} bytes)", id, path, pdf.Length);

        TempData["Published"] = $"Wrote {fileName} ({pdf.Length:N0} bytes) to published-programmes/.";
        return RedirectToAction(nameof(Builder), new { id });
    }

    private async Task<ProgrammeVm?> LoadProgrammeVmAsync(ulong id, CancellationToken ct)
    {
        var data = await _db.Programmes
            .Where(p => p.ProgrammeId == id && p.TreatmentCase.PractitionerId == _practitioner.PractitionerId)
            .Select(p => new
            {
                p.ProgrammeId,
                p.TreatmentCaseId,
                p.ProgrammeName,
                p.Status,
                p.StartDate,
                p.EndDate,
                p.Notes,
                CaseTitle = p.TreatmentCase.CaseTitle,
                PetName = p.TreatmentCase.Pet.Name,
                OwnerFirst = p.TreatmentCase.Pet.Owner.FirstName,
                OwnerLast = p.TreatmentCase.Pet.Owner.LastName,
                Sessions = p.Sessions
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new
                    {
                        s.SessionId,
                        s.Period,
                        s.Objective,
                        s.Status,
                        s.SortOrder,
                        Exercises = s.Sessionexercises
                            .OrderBy(se => se.SortOrder)
                            .Select(se => new ProgrammeVm.SessionExerciseRow(
                                se.SessionExerciseId,
                                se.ExerciseId,
                                se.Exercise.Title,
                                se.Exercise.ObjectiveSummary,
                                se.Reps,
                                se.Sets,
                                se.HoldSeconds,
                                se.SortOrder,
                                se.Notes))
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (data is null) return null;

        var sessions = data.Sessions
            .Select(s => new ProgrammeVm.SessionRow(s.SessionId, s.Period, s.Objective, s.Status, s.SortOrder, s.Exercises))
            .ToList();

        return new ProgrammeVm(
            data.ProgrammeId,
            data.TreatmentCaseId,
            data.ProgrammeName,
            data.Status,
            data.StartDate,
            data.EndDate,
            data.Notes,
            data.CaseTitle,
            data.PetName,
            $"{data.OwnerFirst} {data.OwnerLast}",
            sessions);
    }
}
