namespace CaninePhysioApp.Models
{
    public class AppContent
    {
        public List<ContentSection> Information { get; set; } = new();
        public List<ContentSection> TermsConditions { get; set; } = new();
    }
}