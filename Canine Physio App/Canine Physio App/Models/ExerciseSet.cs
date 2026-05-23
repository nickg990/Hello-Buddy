namespace Canine_Physio_App.Models
{
    /// <summary>
    /// Represents a set of related exercises.
    /// </summary>
    public class ExerciseSet
    {
        public string Key { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Exercise> Exercises { get; set; } = new();
    }
}
