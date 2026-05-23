namespace Canine_Physio_App.Models
{
    /// <summary>
    /// Represents a single content section with a header and body.
    /// Used for Information page and Terms and Conditions.
    /// </summary>
    public class ContentSection
    {
        public string Key { get; set; } = string.Empty;
        public string Header { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
