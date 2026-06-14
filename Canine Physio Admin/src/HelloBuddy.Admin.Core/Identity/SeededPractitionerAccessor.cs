using Microsoft.Extensions.Configuration;

namespace HelloBuddy.Admin.Core.Identity;

/// <summary>
/// Reads a seeded practitioner id from configuration. Used in test harnesses
/// and API-layer background services where no real HTTP context is available.
/// </summary>
public sealed class SeededPractitionerAccessor : ICurrentPractitionerAccessor
{
    public SeededPractitionerAccessor(IConfiguration configuration)
    {
        PractitionerId = configuration.GetValue<ulong?>("SeededPractitionerId")
            ?? throw new InvalidOperationException(
                "SeededPractitionerId is not configured.");
        PractitionerName = configuration["SeededPractitionerName"] ?? "System";
        PractitionerRole = configuration["SeededPractitionerRole"] ?? "physiotherapist";
    }

    public ulong PractitionerId { get; }
    public string PractitionerName { get; }
    public string PractitionerRole { get; }
}
