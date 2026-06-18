namespace HelloBuddy.Contracts;

/// <summary>Response payload for <c>DELETE /api/pets/{id}</c>.</summary>
public sealed record PetDeleteResponse(string Outcome, string Message);
