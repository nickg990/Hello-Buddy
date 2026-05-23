namespace Canine_Physio_App.Services
{
    /// <summary>
    /// Service to retrieve platform-specific safe area insets.
    /// Used for edge-to-edge layouts where content must avoid
    /// status bar, notches, and navigation bar areas.
    /// </summary>
    public interface ISafeAreaService
    {
        /// <summary>
        /// Top safe area inset (status bar, notch).
        /// </summary>
        double TopInset { get; }

        /// <summary>
        /// Bottom safe area inset (navigation bar, home indicator).
        /// </summary>
        double BottomInset { get; }

        /// <summary>
        /// Left safe area inset (for landscape or foldables).
        /// </summary>
        double LeftInset { get; }

        /// <summary>
        /// Right safe area inset (for landscape or foldables).
        /// </summary>
        double RightInset { get; }

        /// <summary>
        /// Refreshes inset values. Call after orientation changes.
        /// </summary>
        void Refresh();
    }
}
