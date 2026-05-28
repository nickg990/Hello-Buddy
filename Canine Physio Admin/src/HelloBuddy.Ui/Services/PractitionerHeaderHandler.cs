using Microsoft.Extensions.Configuration;

namespace HelloBuddy.Ui.Services;

/// <summary>
/// Injects the X-Practitioner-Id header on every outbound API request.
/// Header value is the seeded practitioner id from configuration; this is
/// the Release 1 service-to-service identity (TD-005 replaces with Entra JWT).
/// </summary>
public sealed class PractitionerHeaderHandler : DelegatingHandler
{
    public const string HeaderName = "X-Practitioner-Id";

    private readonly string _practitionerId;

    public PractitionerHeaderHandler(IConfiguration configuration)
    {
        var id = configuration.GetValue<ulong?>("SeededPractitionerId")
            ?? throw new InvalidOperationException("SeededPractitionerId is not configured.");
        _practitionerId = id.ToString();
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains(HeaderName))
            request.Headers.Add(HeaderName, _practitionerId);
        return base.SendAsync(request, cancellationToken);
    }
}
