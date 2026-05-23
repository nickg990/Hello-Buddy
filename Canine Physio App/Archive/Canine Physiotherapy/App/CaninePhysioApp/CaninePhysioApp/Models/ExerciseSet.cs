namespace CaninePhysioApp.Models
{
    public class ExerciseSet
    {
        public string Key { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Exercise> Exercises { get; set; } = new();
    }
}
