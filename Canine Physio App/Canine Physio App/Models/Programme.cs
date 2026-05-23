namespace Canine_Physio_App.Models
{
    /// <summary>
    /// Represents a physiotherapy programme prescribed by a practitioner.
    /// A programme repeats daily for a defined period (startDate → endDate),
    /// with 1 or 2 sessions per day (e.g. AM only, or AM + PM).
    /// </summary>
    public class Programme
    {
        public string Id { get; set; } = string.Empty;
        public string DogName { get; set; } = string.Empty;
        public string PractitionerName { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string WarningText { get; set; } = string.Empty;
        public List<DailySession> DailySessions { get; set; } = new();
    }
}
