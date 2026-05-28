using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Identity;
using HelloBuddy.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelloBuddy.Admin.Controllers;

[Route("Cases/{id:long}")]
public class CaseDetailController : Controller
{
    private readonly CaninePhysioDbContext _db;
    private readonly ICurrentPractitionerAccessor _practitioner;

    public CaseDetailController(CaninePhysioDbContext db, ICurrentPractitionerAccessor practitioner)
    {
        _db = db;
        _practitioner = practitioner;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(ulong id, CancellationToken ct)
    {
        var tc = await _db.Treatmentcases
            .Where(x => x.TreatmentCaseId == id && x.PractitionerId == _practitioner.PractitionerId)
            .Select(x => new
            {
                x.TreatmentCaseId,
                x.CaseTitle,
                x.Status,
                x.StartDate,
                x.EndDate,
                x.ClinicalSummary,
                PetName = x.Pet.Name,
                x.Pet.Breed,
                x.Pet.Sex,
                x.Pet.Weight,
                x.Pet.Age,
                OwnerFirst = x.Pet.Owner.FirstName,
                OwnerLast = x.Pet.Owner.LastName,
                OwnerEmail = x.Pet.Owner.Email,
                Programmes = x.Programmes
                    .OrderByDescending(p => p.StartDate)
                    .Select(p => new CaseDetailVm.ProgrammeRow(
                        p.ProgrammeId,
                        p.ProgrammeName,
                        p.Status,
                        p.StartDate,
                        p.EndDate,
                        p.Sessions.Count,
                        p.Sessions.SelectMany(s => s.Sessionexercises).Count()))
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (tc is null) return NotFound();

        var vm = new CaseDetailVm(
            tc.TreatmentCaseId,
            tc.CaseTitle,
            tc.Status,
            tc.StartDate,
            tc.EndDate,
            tc.ClinicalSummary,
            tc.PetName,
            tc.Breed,
            tc.Sex,
            tc.Weight,
            tc.Age,
            $"{tc.OwnerFirst} {tc.OwnerLast}",
            tc.OwnerEmail,
            tc.Programmes);

        return View(vm);
    }
}
