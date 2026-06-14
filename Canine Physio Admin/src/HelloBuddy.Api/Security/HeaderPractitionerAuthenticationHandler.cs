using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace HelloBuddy.Api.Security;

public sealed class HeaderPractitionerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string IdHeaderName = "X-Practitioner-Id";
    public const string NameHeaderName = "X-Practitioner-Name";
    public const string RoleHeaderName = "X-Practitioner-Role";

    public HeaderPractitionerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(IdHeaderName, out var rawId) ||
            !ulong.TryParse(rawId.ToString(), out var practitionerId) ||
            practitionerId == 0)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var practitionerName = Request.Headers.TryGetValue(NameHeaderName, out var rawName)
            ? rawName.ToString()
            : string.Empty;

        var practitionerRole = Request.Headers.TryGetValue(RoleHeaderName, out var rawRole)
            ? rawRole.ToString()
            : string.Empty;

        var claims = new List<Claim>
        {
            new("practitioner_id", practitionerId.ToString()),
            new(ClaimTypes.NameIdentifier, practitionerId.ToString()),
        };

        if (!string.IsNullOrWhiteSpace(practitionerName))
        {
            claims.Add(new Claim(ClaimTypes.Name, practitionerName));
        }

        if (!string.IsNullOrWhiteSpace(practitionerRole))
        {
            claims.Add(new Claim("practitioner_role", practitionerRole));
            claims.Add(new Claim(ClaimTypes.Role, practitionerRole));
        }

        var identity = new ClaimsIdentity(claims, ApiAuthenticationSchemes.PractitionerHeader);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiAuthenticationSchemes.PractitionerHeader);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
