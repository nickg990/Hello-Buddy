using Canine_Physio_App.Models;
using System.Text.Json;

namespace Canine_Physio_App.Services
{
    /// <summary>
    /// Loads and caches physiotherapy content from PhysioContent.json.
    /// Thread-safe with lazy loading and caching.
    /// Provides access to the programme, its daily sessions, and exercises.
    /// </summary>
    public class PhysioContentService
    {
        private PhysioContent? _cachedContent;
        private readonly SemaphoreSlim _loadLock = new(1, 1);

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Loads the full PhysioContent from the embedded JSON file.
        /// Results are cached after first load.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        public async Task<PhysioContent> LoadContentAsync(CancellationToken cancellationToken = default)
        {
            if (_cachedContent is not null)
                return _cachedContent;

            await _loadLock.WaitAsync(cancellationToken);
            try
            {
                // Double-check after acquiring lock
                if (_cachedContent is not null)
                    return _cachedContent;

                using var stream = await FileSystem.OpenAppPackageFileAsync("PhysioContent.json");
                _cachedContent = await JsonSerializer.DeserializeAsync<PhysioContent>(stream, JsonOptions, cancellationToken)
                    ?? new PhysioContent();

                return _cachedContent;
            }
            finally
            {
                _loadLock.Release();
            }
        }

        /// <summary>
        /// Gets the programme from the content.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        public async Task<Programme?> GetProgrammeAsync(CancellationToken cancellationToken = default)
        {
            var content = await LoadContentAsync(cancellationToken);
            return content.Programme;
        }

        /// <summary>
        /// Gets the daily session matching the given period.
        /// </summary>
        /// <param name="period">The period to match ("AM", "PM", or empty for single-session).</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        public async Task<DailySession?> GetSessionByPeriodAsync(string period, CancellationToken cancellationToken = default)
        {
            var programme = await GetProgrammeAsync(cancellationToken);
            if (programme is null || programme.DailySessions.Count == 0)
                return null;

            // Single session — return it regardless of period
            if (programme.DailySessions.Count == 1)
                return programme.DailySessions[0];

            return programme.DailySessions.FirstOrDefault(s =>
                s.Period.Equals(period, StringComparison.OrdinalIgnoreCase))
                ?? programme.DailySessions[0];
        }

        /// <summary>
        /// Gets the exercise set for a given session period.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        public async Task<ExerciseSet?> GetExerciseSetByPeriodAsync(string period, CancellationToken cancellationToken = default)
        {
            var session = await GetSessionByPeriodAsync(period, cancellationToken);
            return session?.ExerciseSet;
        }

        /// <summary>
        /// Gets an exercise by key, searching across all sessions.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        public async Task<Exercise?> GetExerciseByKeyAsync(string exerciseKey, CancellationToken cancellationToken = default)
        {
            var programme = await GetProgrammeAsync(cancellationToken);
            if (programme is null)
                return null;

            return programme.DailySessions
                .Select(s => s.ExerciseSet)
                .SelectMany(es => es.Exercises)
                .FirstOrDefault(e => e.Key.Equals(exerciseKey, StringComparison.OrdinalIgnoreCase));
        }
    }
}
