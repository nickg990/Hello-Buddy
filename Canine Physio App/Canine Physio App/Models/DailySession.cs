namespace Canine_Physio_App.Models
{
    /// <summary>
    /// Represents one session within a daily routine.
    /// Period is "AM", "PM", or omitted/empty for single-session days.
    /// Each session contains one exercise set.
    /// </summary>
    public class DailySession
    {
        public string Period { get; set; } = string.Empty;
        public ExerciseSet ExerciseSet { get; set; } = new();
    }
}
