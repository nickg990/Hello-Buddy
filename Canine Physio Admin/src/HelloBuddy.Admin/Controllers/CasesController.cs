using HelloBuddy.Admin.Core.Data;
using HelloBuddy.Admin.Core.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelloBuddy.Admin.Controllers;

public class CasesController : Controller
{
    private readonly CaninePhysioDbContext _db;
    private readonly ICurrentPractitionerAccessor _practitioner;

    public CasesController(CaninePhysioDbContext db, ICurrentPractitionerAccessor practitioner)
    {
        _db = db;
        _practitioner = practitioner;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var cases = await _db.Treatmentcases
            .Where(tc => tc.PractitionerId == _practitioner.PractitionerId)
            .OrderByDescending(tc => tc.StartDate)
            .Select(tc => new CaseRow(
                tc.TreatmentCaseId,
                tc.CaseTitle,
                tc.Status,
                tc.StartDate,
                tc.Pet.Name,
                tc.Pet.Owner.FirstName + " " + tc.Pet.Owner.LastName))
            .ToListAsync(ct);

        return View(cases);
    }

    public sealed record CaseRow(
        ulong TreatmentCaseId,
        string CaseTitle,
        string Status,
        DateOnly StartDate,
        string PetName,
        string OwnerName);
}
