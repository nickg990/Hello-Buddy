namespace CaninePhysioApp.Models
{
    public class AppContent
    {
        public List<ContentSection> Information { get; set; } = new();
        public List<ContentSection> TermsConditions { get; set; } = new();
        public WarningContent Warnings { get; set; } = new();
    }

    public class WarningContent
    {
        public string ExerciseDisclaimer { get; set; } = string.Empty;
    }
}