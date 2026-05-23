namespace Canine_Physio_App.Components.Controls;

/// <summary>
/// DiscomfortSlider - A thick slider component with tick labels (0-10) for rating discomfort.
/// Uses custom-drawn track similar to progress bar for visual consistency.
/// 
/// Bindable Properties:
/// - Value: Current slider value (two-way binding)
/// - MaxValue: Maximum value (default 10)
/// - FilledWidth: Computed width of the filled track portion
/// </summary>
public partial class DiscomfortSlider : ContentView
{
    private double _trackWidth;

    #region Bindable Properties

    /// <summary>
    /// Current slider value (0 to MaxValue).
    /// </summary>
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(
            nameof(Value),
            typeof(double),
            typeof(DiscomfortSlider),
            0.0,
            BindingMode.TwoWay,
            propertyChanged: OnValueChanged);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Maximum slider value (default 10).
    /// </summary>
    public static readonly BindableProperty MaxValueProperty =
        BindableProperty.Create(
            nameof(MaxValue),
            typeof(double),
            typeof(DiscomfortSlider),
            10.0,
            propertyChanged: OnValueChanged);

    public double MaxValue
    {
        get => (double)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    /// <summary>
    /// Computed width of the filled track based on current value.
    /// </summary>
    public double FilledWidth
    {
        get
        {
            if (_trackWidth <= 0 || MaxValue <= 0)
                return 0;
            
            var percentage = Value / MaxValue;
            return Math.Max(0, Math.Min(_trackWidth, _trackWidth * percentage));
        }
    }

    #endregion

    public DiscomfortSlider()
    {
        InitializeComponent();
    }

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

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DiscomfortSlider slider)
        {
            slider.OnPropertyChanged(nameof(FilledWidth));
        }
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        _trackWidth = Width;
        OnPropertyChanged(nameof(FilledWidth));
    }
}
