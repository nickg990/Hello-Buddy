using Microsoft.Extensions.Configuration;

namespace HelloBuddy.Admin.Core.Identity;

/// <summary>
/// Reads a seeded practitioner id from configuration. Intentionally not
/// authentication; this is the placeholder per Day-2 decision D4.
/// </summary>
public sealed class SeededPractitionerAccessor : ICurrentPractitionerAccessor
{
    public SeededPractitionerAccessor(IConfiguration configuration)
    {
        PractitionerId = configuration.GetValue<ulong?>("SeededPractitionerId")
            ?? throw new InvalidOperationException(
                "SeededPractitionerId is not configured.");
    }

    public ulong PractitionerId { get; }
}
