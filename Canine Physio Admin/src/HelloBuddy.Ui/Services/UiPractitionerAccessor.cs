using HelloBuddy.Admin.Core.Identity;
using System.Security.Claims;

namespace HelloBuddy.Ui.Services;

/// <summary>
/// Resolves the currently signed-in practitioner from cookie claims.
/// Used in the UI to populate forms, check authorization, and inject
/// headers for API calls. Returns zero/empty for unsigned-out users.
/// </summary>
public sealed class UiPractitionerAccessor : ICurrentPractitionerAccessor
{
    public UiPractitionerAccessor(IHttpContextAccessor accessor)
    {
        var ctx = accessor.HttpContext;
        var user = ctx?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            PractitionerId = 0;
            PractitionerName = string.Empty;
            PractitionerRole = string.Empty;
            return;
        }

        var idClaim = user.FindFirst("practitioner_id")?.Value;
        PractitionerId = ulong.TryParse(idClaim, out var id) ? id : 0;
        PractitionerName = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        PractitionerRole = user.FindFirst("practitioner_role")?.Value ?? string.Empty;
    }

    public ulong PractitionerId { get; }
    public string PractitionerName { get; }
    public string PractitionerRole { get; }
}
