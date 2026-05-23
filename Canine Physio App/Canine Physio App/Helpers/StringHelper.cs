using System.Globalization;

namespace Canine_Physio_App.Helpers;

/// <summary>
/// Common string manipulation utilities shared across the app.
/// Centralises methods previously duplicated in multiple files.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Converts a string to Title Case using the current culture.
    /// </summary>
    public static string ToTitleCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input.ToLower());
    }
}
