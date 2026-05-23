using Microsoft.Maui.Controls;

namespace Canine_Physio_App.Components.Controls;

/// <summary>
/// LoadingSpinner - Activity indicator with optional message.
/// 
/// Token-driven styling with responsive sizing.
/// </summary>
public partial class LoadingSpinner : ContentView
{
    #region Constants - Responsive Sizing

    /// <summary>Spinner size for phones.</summary>
    private const double SpinnerSizePhone = 40;

    /// <summary>Spinner size for tablets.</summary>
    private const double SpinnerSizeTablet = 56;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Controls visibility and animation state of the spinner.
    /// </summary>
    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(
            nameof(IsLoading),
            typeof(bool),
            typeof(LoadingSpinner),
            false);

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Optional message displayed below the spinner.
    /// </summary>
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(
            nameof(Message),
            typeof(string),
            typeof(LoadingSpinner),
            "Loading...");

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Whether to show the message label.
    /// </summary>
    public static readonly BindableProperty ShowMessageProperty =
        BindableProperty.Create(
            nameof(ShowMessage),
            typeof(bool),
            typeof(LoadingSpinner),
            true);

    public bool ShowMessage
    {
        get => (bool)GetValue(ShowMessageProperty);
        set => SetValue(ShowMessageProperty, value);
    }

    #endregion

    #region Constructor

    public LoadingSpinner()
    {
        InitializeComponent();
        ApplyResponsiveSizing();
    }

    #endregion

    #region Responsive Sizing

    private void ApplyResponsiveSizing()
    {
        var size = DeviceInfo.Idiom == DeviceIdiom.Tablet 
            ? SpinnerSizeTablet 
            : SpinnerSizePhone;

        activityIndicator.WidthRequest = size;
        activityIndicator.HeightRequest = size;
    }

    #endregion
}
