namespace CaninePhysioApp.Models
{
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

        // Derived property for tile display (Title Case)
        public string Name => ToTitleCase(Title);

        private static string ToTitleCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }
    }
}
