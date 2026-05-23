using CaninePhysioApp.Models;
using System.Text.Json;

namespace CaninePhysioApp.Services
{
    public class PhysioContentService
    {
        private PhysioContent? _cachedContent;
        private readonly SemaphoreSlim _loadLock = new(1, 1);

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<PhysioContent> LoadContentAsync()
        {
            if (_cachedContent is not null)
                return _cachedContent;

            await _loadLock.WaitAsync();
            try
            {
                if (_cachedContent is not null)
                    return _cachedContent;

                using var stream = await FileSystem.OpenAppPackageFileAsync("PhysioContent.json");
                _cachedContent = await JsonSerializer.DeserializeAsync<PhysioContent>(stream, JsonOptions)
                    ?? new PhysioContent();

                return _cachedContent;
            }
            finally
            {
                _loadLock.Release();
            }
        }

        public async Task<ExerciseSet?> GetExerciseSetAsync(string key)
        {
            var content = await LoadContentAsync();
            return content.ExerciseSets.FirstOrDefault(s =>
                s.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<ExerciseSet?> GetFirstExerciseSetAsync()
        {
            var content = await LoadContentAsync();
            return content.ExerciseSets.FirstOrDefault();
        }

        public async Task<Exercise?> GetExerciseAsync(string setKey, string exerciseKey)
        {
            var set = await GetExerciseSetAsync(setKey);
            return set?.Exercises.FirstOrDefault(e =>
                e.Key.Equals(exerciseKey, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets an exercise by key, searching across all exercise sets.
        /// </summary>
        public async Task<Exercise?> GetExerciseByKeyAsync(string exerciseKey)
        {
            var content = await LoadContentAsync();
            return content.ExerciseSets
                .SelectMany(s => s.Exercises)
                .FirstOrDefault(e => e.Key.Equals(exerciseKey, StringComparison.OrdinalIgnoreCase));
        }
    }
}
