namespace Canine_Physio_App.Models
{
    /// <summary>
    /// Root model for PhysioContent.json.
    /// Contains a physiotherapy programme with daily sessions.
    /// </summary>
    public class PhysioContent
    {
        public Programme Programme { get; set; } = new();
    }
}
