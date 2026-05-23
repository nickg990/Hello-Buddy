namespace Canine_Physio_App.Models
{
    /// <summary>
    /// Root model for AppContent.json.
    /// Contains all text content sections used throughout the app.
    /// </summary>
    public class AppContent
    {
        public List<ContentSection> Information { get; set; } = new();
        public List<ContentSection> TermsConditions { get; set; } = new();
        public WarningContent Warnings { get; set; } = new();
    }

    /// <summary>
    /// Warning messages displayed in specific contexts.
    /// </summary>
    public class WarningContent
    {
        public string ExerciseDisclaimer { get; set; } = string.Empty;
    }
}
