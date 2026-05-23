using Canine_Physio_App.Helpers;

namespace Canine_Physio_App.Models
{
    /// <summary>
    /// Represents a single exercise with instructions and metadata.
    /// </summary>
    public class Exercise
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> Instructions { get; set; } = new();
        public int Reps { get; set; }
        public int Sets { get; set; }
        public string VideoName { get; set; } = string.Empty;

        /// <summary>
        /// Display name in Title Case format.
        /// </summary>
        public string Name => StringHelper.ToTitleCase(Title);
    }
}
