namespace HelloBuddy.Admin.Core.Identity;

/// <summary>
/// Resolves the currently active practitioner. Day-2 implementation is a seeded
/// single-practitioner placeholder (see Five-Day Delivery Plan contingency #6 and
/// HANDOVER decision D4). Swap for an Entra-ID-backed implementation later
/// without touching call sites.
/// </summary>
public interface ICurrentPractitionerAccessor
{
    ulong PractitionerId { get; }
}
