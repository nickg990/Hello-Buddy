namespace HelloBuddy.Application.Admin;

public interface IAppSettingRepository
{
    Task<string?> GetAsync(string key, CancellationToken ct);
    Task UpsertAsync(string key, string? value, ulong? practitionerId, CancellationToken ct);
}
