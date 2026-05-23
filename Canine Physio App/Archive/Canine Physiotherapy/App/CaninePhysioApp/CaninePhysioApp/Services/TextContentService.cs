using CaninePhysioApp.Models;
using System.Text.Json;

namespace CaninePhysioApp.Services
{
    public class TextContentService
    {
        private AppContent? _cachedContent;
        private readonly SemaphoreSlim _loadLock = new(1, 1);

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<AppContent> LoadContentAsync()
        {
            if (_cachedContent is not null)
                return _cachedContent;

            await _loadLock.WaitAsync();
            try
            {
                if (_cachedContent is not null)
                    return _cachedContent;

                using var stream = await FileSystem.OpenAppPackageFileAsync("AppContent.json");
                _cachedContent = await JsonSerializer.DeserializeAsync<AppContent>(stream, JsonOptions)
                    ?? new AppContent();

                return _cachedContent;
            }
            finally
            {
                _loadLock.Release();
            }
        }

        public async Task<List<ContentSection>> GetSectionsAsync(string pageKey)
        {
            var content = await LoadContentAsync();

            return pageKey.ToLowerInvariant() switch
            {
                "information" => content.Information,
                "termsconditions" => content.TermsConditions,
                _ => new List<ContentSection>()
            };
        }

        public async Task<ContentSection?> GetSectionAsync(string pageKey, string sectionKey)
        {
            var sections = await GetSectionsAsync(pageKey);
            return sections.FirstOrDefault(s => 
                s.Key.Equals(sectionKey, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<string> GetWarningAsync(string warningKey)
        {
            var content = await LoadContentAsync();
            
            return warningKey.ToLowerInvariant() switch
            {
                "exercisedisclaimer" => content.Warnings.ExerciseDisclaimer,
                _ => string.Empty
            };
        }
    }
}