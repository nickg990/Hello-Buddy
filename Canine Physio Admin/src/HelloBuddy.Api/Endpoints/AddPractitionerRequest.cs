namespace HelloBuddy.Api.Endpoints;

public sealed record AddPractitionerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Role,
    string InitialPassword);
