namespace HelloBuddy.Api.Endpoints;

public sealed record RenamePractitionerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);
