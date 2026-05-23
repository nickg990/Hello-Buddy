using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace Canine_Physio_App.Components.Controls;

/// <summary>
/// ExerciseTile - Displays an exercise with image, title, and completion state.
/// 
/// Responsive by design:
/// - Tile fills available width from parent container
/// - Image maintains aspect ratio
/// - Parent controls column count via Grid/FlexLayout
/// 
/// Token-driven styling throughout.
/// </summary>
public partial class ExerciseTile : ContentView
{
    #region Constants - Responsive Sizing

    /// <summary>Image height ratio relative to tile width (3:2 aspect).</summary>
    private const double ImageHeightRatio = 0.67;

    /// <summary>Tick size for phones.</summary>
    private const double TickSizePhone = 24;

    /// <summary>Tick size for tablets.</summary>
    private const double TickSizeTablet = 32;

    /// <summary>Tick icon size as ratio of tick container size (accounting for padding).</summary>
    private const double TickIconSizeRatio = 0.6;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Unique identifier for the exercise.
    /// </summary>
    public static readonly BindableProperty ExerciseKeyProperty =
        BindableProperty.Create(
            nameof(ExerciseKey),
            typeof(string),
            typeof(ExerciseTile),
            string.Empty);

    public string ExerciseKey
    {
        get => (string)GetValue(ExerciseKeyProperty);
        set => SetValue(ExerciseKeyProperty, value);
    }

    /// <summary>
    /// Display name of the exercise.
    /// </summary>
    public static readonly BindableProperty ExerciseNameProperty =
        BindableProperty.Create(
            nameof(ExerciseName),
            typeof(string),
            typeof(ExerciseTile),
            string.Empty);

    public string ExerciseName
    {
        get => (string)GetValue(ExerciseNameProperty);
        set => SetValue(ExerciseNameProperty, value);
    }

    /// <summary>
    /// Image source for the exercise thumbnail.
    /// </summary>
    public static readonly BindableProperty ImageSourceProperty =
        BindableProperty.Create(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(ExerciseTile),
            null);

    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    /// <summary>
    /// Whether the exercise has been completed.
    /// </summary>
    public static readonly BindableProperty IsCompleteProperty =
        BindableProperty.Create(
            nameof(IsComplete),
            typeof(bool),
            typeof(ExerciseTile),
            false);

    public bool IsComplete
    {
        get => (bool)GetValue(IsCompleteProperty);
        set => SetValue(IsCompleteProperty, value);
    }

    /// <summary>
    /// Whether the exercise has been skipped.
    /// </summary>
    public static readonly BindableProperty IsSkippedProperty =
        BindableProperty.Create(
            nameof(IsSkipped),
            typeof(bool),
            typeof(ExerciseTile),
            false);

    public bool IsSkipped
    {
        get => (bool)GetValue(IsSkippedProperty);
        set => SetValue(IsSkippedProperty, value);
    }

    /// <summary>
    /// Command to execute when tile is tapped.
    /// </summary>
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(ExerciseTile),
            null);

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    #endregion

    #region Constructor

    public ExerciseTile()
    {
        InitializeComponent();
        ApplyResponsiveSizing();
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

    #region Responsive Sizing

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        // Set image container height based on width (maintain aspect ratio)
        if (Width > 0 && !double.IsNaN(Width) && !double.IsInfinity(Width))
        {
            var imageHeight = Width * ImageHeightRatio;
            imageContainer.HeightRequest = imageHeight;
        }
    }

    private void ApplyResponsiveSizing()
    {
        // Set tick size based on device idiom
        var tickSize = DeviceInfo.Idiom == DeviceIdiom.Tablet
            ? TickSizeTablet
            : TickSizePhone;

        completionTick.WidthRequest = tickSize;
        completionTick.HeightRequest = tickSize;

        // Update corner radius for tick (half of size for circle)
        completionTick.StrokeShape = new RoundRectangle
        {
            CornerRadius = new CornerRadius(tickSize / 2)
        };

        // Update tick icon size proportionally
        var iconSize = tickSize * TickIconSizeRatio;
        tickIcon.WidthRequest = iconSize;
        tickIcon.HeightRequest = iconSize;

        // Skipped cross badge (same sizing as completion tick)
        skippedCross.WidthRequest = tickSize;
        skippedCross.HeightRequest = tickSize;
        skippedCross.StrokeShape = new RoundRectangle
        {
            CornerRadius = new CornerRadius(tickSize / 2)
        };
        crossIcon.WidthRequest = iconSize;
        crossIcon.HeightRequest = iconSize;
    }

    #endregion
}
