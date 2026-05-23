using System;

namespace Canine_Physio_App.Helpers;

/// <summary>
/// Helper class for calculating character spacing to fill a target width.
/// Used by HeaderBlock (subtitle) and LogoDisc (tagline) for consistent text expansion.
/// </summary>
public static class CharacterSpacingHelper
{
    #region Constants

    /// <summary>Minimum character spacing to avoid letters being too close.</summary>
    private const double MinCharacterSpacing = 1.0;

    /// <summary>Maximum character spacing to prevent excessive gaps.</summary>
    private const double MaxCharacterSpacing = 30.0;

    /// <summary>Approximate width per character for bold uppercase text (conservative estimate).</summary>
    private const double DefaultAvgCharWidthRatio = 0.50;

    /// <summary>Approximate width per space character.</summary>
    private const double DefaultSpaceWidthRatio = 0.25;

    #endregion

    /// <summary>
    /// Calculates the character spacing needed to make text fill a target width.
    /// </summary>
    /// <param name="text">The text to space out.</param>
    /// <param name="targetWidth">The target width to fill.</param>
    /// <param name="fontSize">The font size of the text.</param>
    /// <param name="avgCharWidthRatio">Character width as ratio of fontSize (default 0.50).</param>
    /// <param name="spaceWidthRatio">Space width as ratio of fontSize (default 0.25).</param>
    /// <returns>The calculated character spacing, clamped to reasonable bounds.</returns>
    public static double Calculate(
        string text,
        double targetWidth,
        double fontSize,
        double avgCharWidthRatio = DefaultAvgCharWidthRatio,
        double spaceWidthRatio = DefaultSpaceWidthRatio)
    {
        if (string.IsNullOrEmpty(text) || targetWidth <= 0 || fontSize <= 0)
            return MinCharacterSpacing;

        // Count characters (excluding spaces) and spaces separately
        int charCount = text.Replace(" ", "").Length;
        int spaceCount = text.Length - charCount;

        // Calculate base text width without extra spacing
        double avgCharWidth = fontSize * avgCharWidthRatio;
        double spaceWidth = fontSize * spaceWidthRatio;
        double baseTextWidth = (charCount * avgCharWidth) + (spaceCount * spaceWidth);

        // Calculate required spacing to fill target width
        int spacingPoints = text.Length - 1; // Spacing between characters
        if (spacingPoints <= 0)
            return MinCharacterSpacing;

        double extraSpace = targetWidth - baseTextWidth;
        double calculatedSpacing = extraSpace / spacingPoints;

        // Clamp to reasonable range
        return Math.Clamp(calculatedSpacing, MinCharacterSpacing, MaxCharacterSpacing);
    }
}
