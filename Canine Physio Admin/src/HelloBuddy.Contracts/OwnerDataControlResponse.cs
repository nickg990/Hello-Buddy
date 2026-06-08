namespace HelloBuddy.Contracts;

public sealed record OwnerDataControlResponse(
    string Outcome,
    string Message,
    OwnerDetailVm? Owner);