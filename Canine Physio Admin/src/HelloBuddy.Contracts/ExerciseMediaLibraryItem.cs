namespace HelloBuddy.Contracts;

/// <summary>
/// A single image available in the exercise image library (blob storage).
/// </summary>
public sealed record ExerciseMediaLibraryItem(
    /// <summary>The blob filename (last segment of the key), e.g. "step_up.jpg".</summary>
    string Name,
    /// <summary>The managed canonical URL containing the /exercise-media/ marker so
    /// all existing consumers (builder, preview, PDF proxy) resolve it correctly.</summary>
    string Url);
