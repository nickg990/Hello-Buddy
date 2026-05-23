using Canine_Physio_App.Models;
using System.Text.Json;

namespace Canine_Physio_App.Helpers
{
    /// <summary>
    /// Loads and caches text content from AppContent.json.
    /// Thread-safe with lazy loading and caching.
    /// </summary>
    public class TextContentLoader
    {
        private AppContent? _cachedContent;
        private readonly SemaphoreSlim _loadLock = new(1, 1);

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Loads the full AppContent from the embedded JSON file.
        /// Results are cached after first load.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        public async Task<AppContent> LoadContentAsync(CancellationToken cancellationToken = default)
        {
            if (_cachedContent is not null)
                return _cachedContent;

            await _loadLock.WaitAsync(cancellationToken);
            try
            {
                // Double-check after acquiring lock
                if (_cachedContent is not null)
                    return _cachedContent;

                using var stream = await FileSystem.OpenAppPackageFileAsync("AppContent.json");
                _cachedContent = await JsonSerializer.DeserializeAsync<AppContent>(stream, JsonOptions, cancellationToken)
                    ?? new AppContent();

                return _cachedContent;
            }
            finally
            {
                _loadLock.Release();
            }
        }

        /// <summary>
        /// Gets content sections for a specific page key.
        /// </summary>
        /// <param name="pageKey">The page identifier (e.g., "information", "termsconditions")</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>List of content sections for the page</returns>
        public async Task<List<ContentSection>> GetSectionsAsync(string pageKey, CancellationToken cancellationToken = default)
        {
            var content = await LoadContentAsync(cancellationToken);

            return pageKey.ToLowerInvariant() switch
            {
                "information" => content.Information,
                "termsconditions" => content.TermsConditions,
                _ => new List<ContentSection>()
            };
        }

        /// <summary>
        /// Gets a specific section by page key and section key.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        public async Task<ContentSection?> GetSectionAsync(string pageKey, string sectionKey, CancellationToken cancellationToken = default)
        {
            var sections = await GetSectionsAsync(pageKey, cancellationToken);
            return sections.FirstOrDefault(s =>
                s.Key.Equals(sectionKey, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets a warning message by key.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        public async Task<string> GetWarningAsync(string warningKey, CancellationToken cancellationToken = default)
        {
            var content = await LoadContentAsync(cancellationToken);

            return warningKey.ToLowerInvariant() switch
            {
                "exercisedisclaimer" => content.Warnings.ExerciseDisclaimer,
                _ => string.Empty
            };
        }
    }
}
