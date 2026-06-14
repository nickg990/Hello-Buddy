namespace HelloBuddy.Api.Endpoints;

public sealed record ChangeOwnPasswordRequest(string CurrentPassword, string NewPassword);
