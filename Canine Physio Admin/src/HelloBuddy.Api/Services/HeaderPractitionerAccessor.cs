using HelloBuddy.Admin.Core.Identity;
using Microsoft.AspNetCore.Http;

namespace HelloBuddy.Api.Services;

/// <summary>
/// Resolves the current practitioner from request headers injected by the UI
/// (Release 1 service-to-service identity; TD-005 tracks JWT hardening).
/// Returns zero/empty defaults when no HTTP context is available (e.g. hosted services).
/// </summary>
public sealed class HeaderPractitionerAccessor : ICurrentPractitionerAccessor
{
    public const string IdHeaderName = "X-Practitioner-Id";
    public const string NameHeaderName = "X-Practitioner-Name";
    public const string RoleHeaderName = "X-Practitioner-Role";

    public HeaderPractitionerAccessor(IHttpContextAccessor accessor)
    {
        var ctx = accessor.HttpContext;
        if (ctx is null)
        {
            PractitionerId = 0;
            PractitionerName = string.Empty;
            PractitionerRole = string.Empty;
            return;
        }

        if (!ctx.Request.Headers.TryGetValue(IdHeaderName, out var raw)
            || !ulong.TryParse(raw.ToString(), out var id))
        {
            PractitionerId = 0;
            PractitionerName = string.Empty;
            PractitionerRole = string.Empty;
            return;
        }

        PractitionerId = id;
        PractitionerName = ctx.Request.Headers.TryGetValue(NameHeaderName, out var name)
            ? name.ToString()
            : string.Empty;
        PractitionerRole = ctx.Request.Headers.TryGetValue(RoleHeaderName, out var role)
            ? role.ToString()
            : string.Empty;
    }

    public ulong PractitionerId { get; }
    public string PractitionerName { get; }
    public string PractitionerRole { get; }
}
