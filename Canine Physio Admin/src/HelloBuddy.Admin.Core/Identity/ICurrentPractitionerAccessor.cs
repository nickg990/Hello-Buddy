namespace HelloBuddy.Admin.Core.Identity;

/// <summary>
/// Resolves the currently active practitioner identity. The UI implementation
/// reads cookie claims; the API implementation reads request headers forwarded
/// by the UI. Hardening to signed tokens is tracked as TD-005.
/// </summary>
public interface ICurrentPractitionerAccessor
{
    ulong PractitionerId { get; }

    /// <summary>Display name ("FirstName LastName") of the current practitioner.</summary>
    string PractitionerName { get; }

    /// <summary>Role string: "physiotherapist" or "administrator".</summary>
    string PractitionerRole { get; }
}
