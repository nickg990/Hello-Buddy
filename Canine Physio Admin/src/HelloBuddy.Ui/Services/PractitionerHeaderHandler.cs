using HelloBuddy.Admin.Core.Identity;

namespace HelloBuddy.Ui.Services;

/// <summary>
/// Delegate handler that injects practitioner identity headers into outbound
/// API requests. Reads claims from the signed-in cookie and forwards them
/// to the API (X-Practitioner-Id, X-Practitioner-Name, X-Practitioner-Role).
/// The API HeaderPractitionerAccessor reads these headers.
/// </summary>
public sealed class PractitionerHeaderHandler : DelegatingHandler
{
    private readonly ICurrentPractitionerAccessor _accessor;

    public PractitionerHeaderHandler(ICurrentPractitionerAccessor accessor)
    {
        _accessor = accessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_accessor.PractitionerId > 0)
        {
            request.Headers.Add("X-Practitioner-Id", _accessor.PractitionerId.ToString());
            request.Headers.Add("X-Practitioner-Name", _accessor.PractitionerName);
            request.Headers.Add("X-Practitioner-Role", _accessor.PractitionerRole);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
