namespace Canine_Physio_App.Components.Decor
{
    /// <summary>
    /// Specifies which edge the sine wave attaches to.
    /// The wave curve automatically orients to face "inward" toward the content.
    /// </summary>
    public enum WaveEdge
    {
        /// <summary>
        /// Wave sits at bottom edge, curve dips downward into body content.
        /// </summary>
        Bottom,

        /// <summary>
        /// Wave sits at top edge, curve dips upward (flipped) into header/content.
        /// </summary>
        Top
    }
}
