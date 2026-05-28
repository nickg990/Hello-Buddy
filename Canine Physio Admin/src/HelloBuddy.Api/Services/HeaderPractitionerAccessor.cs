using HelloBuddy.Admin.Core.Identity;
using Microsoft.AspNetCore.Http;

namespace HelloBuddy.Api.Services;

/// <summary>
/// Resolves the current practitioner from the <c>X-Practitioner-Id</c>
/// request header injected by the UI (Release 1 service-to-service
/// identity; Entra-issued JWT replaces this — TD-005).
/// </summary>
public sealed class HeaderPractitionerAccessor : ICurrentPractitionerAccessor
{
    public const string HeaderName = "X-Practitioner-Id";

    public HeaderPractitionerAccessor(IHttpContextAccessor accessor)
    {
        var ctx = accessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext unavailable.");
        if (!ctx.Request.Headers.TryGetValue(HeaderName, out var raw) ||
            !ulong.TryParse(raw.ToString(), out var id))
        {
            throw new InvalidOperationException($"{HeaderName} header is missing or invalid.");
        }
        PractitionerId = id;
    }

    public ulong PractitionerId { get; }
}
