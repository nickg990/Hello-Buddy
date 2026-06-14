namespace HelloBuddy.Application.Programmes;

public sealed record ProgrammeVersionPayloadVm(
    ulong ProgrammeVersionId,
    uint VersionNumber,
    string PayloadJson);
