namespace Canine_Physio_App.Models
{
    /// <summary>
    /// Represents a single numbered instruction step for an exercise.
    /// </summary>
    public class InstructionStep
    {
        public int Number { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
