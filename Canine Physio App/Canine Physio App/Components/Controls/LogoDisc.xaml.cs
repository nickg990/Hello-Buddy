using System;
using Canine_Physio_App.Helpers;
using Microsoft.Maui.Controls;

namespace Canine_Physio_App.Components.Controls;

/// <summary>
/// LogoDisc - Brand logo component with responsive sizing.
/// 
/// Displays the Hello Buddy dog logo with "GETTING BETTER" tagline
/// inside a circular disc with shadow and border ring.
/// 
/// Sizing behavior:
/// - If DiscSize > 0: uses the explicit size
/// - If DiscSize == 0 (default): auto-sizes based on allocated space
/// 
/// All internal element sizes are proportionally derived from the computed disc size.
/// </summary>
public partial class LogoDisc : ContentView
{
    #region Constants - Proportional Sizing Ratios

    // These ratios define the visual proportions of all elements relative to DiscSize.
    // Adjust these values to fine-tune the appearance to match Figma.

    /// <summary>Ratio of disc size to allocated space when auto-sizing.</summary>
    private const double AutoSizeRatio = 0.765;

    /// <summary>Minimum disc size to prevent tiny rendering.</summary>
    private const double MinDiscSize = 120;

    /// <summary>Maximum disc size to prevent oversized rendering.</summary>
    private const double MaxDiscSize = 400;

    /// <summary>Border ring is slightly larger than the disc.</summary>
    private const double BorderRingRatio = 1.05;

    /// <summary>Shadow offset as a fraction of disc size.</summary>
    private const double ShadowOffsetRatio = 0.04;

    /// <summary>Logo size relative to disc size (bigger to fill more of the disc).</summary>
    private const double LogoSizeRatio = 0.66;

    /// <summary>Logo horizontal offset (negative = left) to centre the dog's body mass.</summary>
    private const double LogoHorizontalOffsetRatio = -0.06;

    /// <summary>Logo stack vertical offset from center (moves stack up slightly).</summary>
    private const double LogoStackVerticalOffsetRatio = -0.04;

    /// <summary>Negative top margin on tagline to pull it closer to the image (fraction of disc size).</summary>
    private const double TaglineTopMarginRatio = -0.03;

    /// <summary>Tagline width as fraction of disc size (accounts for circular chord at label position).</summary>
    private const double TaglineWidthRatio = 0.70;

    /// <summary>Character width ratio for tagline (conservative for cross-device consistency).</summary>
    private const double TaglineCharWidthRatio = 0.60;

    /// <summary>Space width ratio for tagline.</summary>
    private const double TaglineSpaceWidthRatio = 0.30;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Explicit disc size. If 0 (default), the control auto-sizes based on allocated space.
    /// </summary>
    public static readonly BindableProperty DiscSizeProperty =
        BindableProperty.Create(
            nameof(DiscSize),
            typeof(double),
            typeof(LogoDisc),
            0.0,
            propertyChanged: OnDiscSizeChanged);

    public double DiscSize
    {
        get => (double)GetValue(DiscSizeProperty);
        set => SetValue(DiscSizeProperty, value);
    }

    /// <summary>
    /// Optional logo image source. Defaults to hellobuddy_logo.png.
    /// </summary>
    public static readonly BindableProperty LogoSourceProperty =
        BindableProperty.Create(
            nameof(LogoSource),
            typeof(ImageSource),
            typeof(LogoDisc),
            null,
            propertyChanged: OnLogoSourceChanged);

    public ImageSource? LogoSource
    {
        get => (ImageSource?)GetValue(LogoSourceProperty);
        set => SetValue(LogoSourceProperty, value);
    }

    #endregion

    #region Private Fields

    private double _computedDiscSize;
    private bool _hasSizeAllocated;

    #endregion

    #region Constructor

    public LogoDisc()
    {
        InitializeComponent();
    }

    #endregion

    #region Lifecycle - Event Subscription Management

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        
        if (Handler != null)
        {
            // Subscribe when handler is attached
            SizeChanged += OnSizeChanged;
        }
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        
        if (args.OldHandler != null)
        {
            // Unsubscribe when handler is detaching to prevent memory leaks
            SizeChanged -= OnSizeChanged;
        }
    }

    #endregion

    #region Property Changed Handlers

    private static void OnDiscSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LogoDisc logoDisc && newValue is double size)
        {
            if (size > 0)
            {
                // Explicit size provided - apply immediately
                logoDisc.ApplySizing(size);
            }
            else if (logoDisc._hasSizeAllocated)
            {
                // Switched back to auto-size, recalculate from current dimensions
                logoDisc.RecalculateAutoSize();
            }
        }
    }

    private static void OnLogoSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LogoDisc logoDisc && newValue is ImageSource source)
        {
            logoDisc.logoImage.Source = source;
        }
    }

    #endregion

    #region Size Changed Handler

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        _hasSizeAllocated = true;

        // Only auto-size if DiscSize is not explicitly set
        if (DiscSize <= 0)
        {
            RecalculateAutoSize();
        }
    }

    private void RecalculateAutoSize()
    {
        var availableWidth = Width;
        var availableHeight = Height;

        // Guard against invalid dimensions during layout
        if (availableWidth <= 0 || availableHeight <= 0 || 
            double.IsNaN(availableWidth) || double.IsNaN(availableHeight) ||
            double.IsInfinity(availableWidth) || double.IsInfinity(availableHeight))
        {
            return;
        }

        // Auto-size: use the smaller dimension, scaled by AutoSizeRatio
        var autoSize = Math.Min(availableWidth, availableHeight) * AutoSizeRatio;

        // Clamp to min/max bounds
        autoSize = Math.Clamp(autoSize, MinDiscSize, MaxDiscSize);

        ApplySizing(autoSize);
    }

    #endregion

    #region Core Sizing Method

    /// <summary>
    /// Applies proportional sizing to all visual elements based on the given disc size.
    /// This is the single source of truth for all element dimensions.
    /// </summary>
    /// <param name="discSize">The computed or explicit disc diameter.</param>
    private void ApplySizing(double discSize)
    {
        if (discSize <= 0)
            return;

        _computedDiscSize = discSize;

        // Calculate derived sizes
        var shadowSize = discSize;
        var shadowOffset = discSize * ShadowOffsetRatio;
        var borderSize = discSize * BorderRingRatio;
        var logoSize = discSize * LogoSizeRatio;
        var logoHorizontalOffset = discSize * LogoHorizontalOffsetRatio;
        var logoStackVerticalOffset = discSize * LogoStackVerticalOffsetRatio;
        var taglineWidth = discSize * TaglineWidthRatio; // 80% of disc (accounts for circular chord)

        // Apply to shadow ellipse
        shadowEllipse.WidthRequest = shadowSize;
        shadowEllipse.HeightRequest = shadowSize;
        shadowEllipse.TranslationX = shadowOffset;
        shadowEllipse.TranslationY = shadowOffset;

        // Apply to border ellipse (white ring)
        borderEllipse.WidthRequest = borderSize;
        borderEllipse.HeightRequest = borderSize;

        // Apply to main disc
        discEllipse.WidthRequest = discSize;
        discEllipse.HeightRequest = discSize;

        // Apply to logo image (shifted left to centre body mass)
        logoImage.WidthRequest = logoSize;
        logoImage.HeightRequest = logoSize;
        logoImage.TranslationX = logoHorizontalOffset;

        // Apply to logo stack (vertical offset to position in disc)
        logoStack.TranslationY = logoStackVerticalOffset;

        // Pull tagline up closer to the image (compensates for AspectFit padding)
        var taglineTopMargin = discSize * TaglineTopMarginRatio;
        taglineLabel.Margin = new Thickness(0, taglineTopMargin, 0, 0);

        // Note: Do NOT set taglineLabel.WidthRequest — constraining the label
        // to the target width causes clipping when the character-width estimate
        // is slightly off. Instead, let the label size naturally to its content
        // (text + calculated spacing) and rely on HorizontalOptions="Center"
        // to keep it centred in the disc.

        // Set the root grid size to accommodate the border ring
        rootGrid.WidthRequest = borderSize + shadowOffset;
        rootGrid.HeightRequest = borderSize + shadowOffset;

        // Update tagline character spacing to fill width (like HeaderBlock subtitle)
        UpdateTaglineSpacing(discSize);
    }

    #endregion

    #region Character Spacing (uses shared helper)

    /// <summary>
    /// Calculates and applies character spacing to make the tagline fill the available width.
    /// Uses CharacterSpacingHelper for consistent calculation across components.
    /// </summary>
    /// <param name="discSize">The current disc size.</param>
    private void UpdateTaglineSpacing(double discSize)
    {
        // Available width for tagline (70% of disc to fit within circular chord)
        double availableWidth = discSize * TaglineWidthRatio;
        if (availableWidth <= 0)
            return;

        // Get font size from the label
        double fontSize = taglineLabel.FontSize;
        if (fontSize <= 0)
            fontSize = 11; // Fallback to TextXs default

        // Fixed tagline text
        const string text = "GETTING BETTER";

        // Calculate spacing using shared helper (with LogoDisc-specific ratios)
        taglineLabel.CharacterSpacing = CharacterSpacingHelper.Calculate(
            text,
            availableWidth,
            fontSize,
            TaglineCharWidthRatio,
            TaglineSpaceWidthRatio);
    }

    #endregion
}
