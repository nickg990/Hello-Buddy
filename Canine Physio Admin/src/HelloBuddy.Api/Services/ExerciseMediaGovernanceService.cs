using HelloBuddy.Admin.Pdf;
using HelloBuddy.Application.Media;

namespace HelloBuddy.Api.Services;

public interface IExerciseMediaMalwareScanner
{
    Task<ExerciseMediaScanResult> ScanAsync(byte[] bytes, string fileName, string contentType, CancellationToken ct);
}

public sealed record ExerciseMediaScanResult(bool IsAllowed, string? BlockReason = null);

public sealed class StubAllowAllExerciseMediaMalwareScanner : IExerciseMediaMalwareScanner
{
    private readonly ILogger<StubAllowAllExerciseMediaMalwareScanner> _logger;

    public StubAllowAllExerciseMediaMalwareScanner(ILogger<StubAllowAllExerciseMediaMalwareScanner> logger)
    {
        _logger = logger;
    }

    public Task<ExerciseMediaScanResult> ScanAsync(byte[] bytes, string fileName, string contentType, CancellationToken ct)
    {
        _logger.LogInformation(
            "Exercise media malware scan hook executed in stub mode for {FileName} ({ContentType}, {Bytes} bytes)",
            fileName,
            contentType,
            bytes.Length);
        return Task.FromResult(new ExerciseMediaScanResult(true));
    }
}

public interface IExerciseMediaGovernanceService
{
    Task HandleImageUrlChangeAsync(string? previousImageUrl, string? currentImageUrl, CancellationToken ct);
}

public sealed class ExerciseMediaGovernanceService : IExerciseMediaGovernanceService
{
    private readonly IConfiguration _config;
    private readonly IFileStore _fileStore;
    private readonly ILogger<ExerciseMediaGovernanceService> _logger;

    public ExerciseMediaGovernanceService(
        IConfiguration config,
        IFileStore fileStore,
        ILogger<ExerciseMediaGovernanceService> logger)
    {
        _config = config;
        _fileStore = fileStore;
        _logger = logger;
    }

    public async Task HandleImageUrlChangeAsync(string? previousImageUrl, string? currentImageUrl, CancellationToken ct)
    {
        var previous = (previousImageUrl ?? string.Empty).Trim();
        var current = (currentImageUrl ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(previous))
        {
            return;
        }

        if (string.Equals(previous, current, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var mode = (_config["Storage:ExerciseMediaOrphanCleanupMode"] ?? "Retain").Trim();
        if (mode.Equals("Retain", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Retained previous exercise image URL due to orphan cleanup mode Retain: {Url}", previous);
            return;
        }

        if (!mode.Equals("DeleteManagedOrphans", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Unknown orphan cleanup mode {Mode}; retaining previous image URL {Url}", mode, previous);
            return;
        }

        if (!ExerciseMediaKey.TryResolve(previous, out var key))
        {
            _logger.LogInformation("Previous image URL is outside managed exercise-media namespace; retained: {Url}", previous);
            return;
        }

        var deleted = await _fileStore.DeleteIfExistsAsync(key, ct);
        if (!deleted)
        {
            _logger.LogInformation("No orphan artefact found for key {Key}; nothing to delete", key);
        }
    }
}