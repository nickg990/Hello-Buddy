using Microsoft.Maui.Controls;

namespace CaninePhysioApp.Controls
{
    public partial class LoadingSpinner : ContentView
    {
        // IsLoading property
        public static readonly BindableProperty IsLoadingProperty =
            BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(LoadingSpinner), false);

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        // Message property
        public static readonly BindableProperty MessageProperty =
            BindableProperty.Create(nameof(Message), typeof(string), typeof(LoadingSpinner), "Loading...");

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        // ShowMessage property
        public static readonly BindableProperty ShowMessageProperty =
            BindableProperty.Create(nameof(ShowMessage), typeof(bool), typeof(LoadingSpinner), true);

        public bool ShowMessage
        {
            get => (bool)GetValue(ShowMessageProperty);
            set => SetValue(ShowMessageProperty, value);
        }

        // SpinnerColor property
        public static readonly BindableProperty SpinnerColorProperty =
            BindableProperty.Create(nameof(SpinnerColor), typeof(Color), typeof(LoadingSpinner), Color.FromArgb("#3D81A9"));

        public Color SpinnerColor
        {
            get => (Color)GetValue(SpinnerColorProperty);
            set => SetValue(SpinnerColorProperty, value);
        }

        // SpinnerSize property
        public static readonly BindableProperty SpinnerSizeProperty =
            BindableProperty.Create(nameof(SpinnerSize), typeof(double), typeof(LoadingSpinner), 40.0);

        public double SpinnerSize
        {
            get => (double)GetValue(SpinnerSizeProperty);
            set => SetValue(SpinnerSizeProperty, value);
        }

        // MessageColor property
        public static readonly BindableProperty MessageColorProperty =
            BindableProperty.Create(nameof(MessageColor), typeof(Color), typeof(LoadingSpinner), Color.FromArgb("#3CB371"));

        public Color MessageColor
        {
            get => (Color)GetValue(MessageColorProperty);
            set => SetValue(MessageColorProperty, value);
        }

        // MessageFontFamily property
        public static readonly BindableProperty MessageFontFamilyProperty =
            BindableProperty.Create(nameof(MessageFontFamily), typeof(string), typeof(LoadingSpinner), "MontserratSemiBold");

        public string MessageFontFamily
        {
            get => (string)GetValue(MessageFontFamilyProperty);
            set => SetValue(MessageFontFamilyProperty, value);
        }

        // MessageFontSize property
        public static readonly BindableProperty MessageFontSizeProperty =
            BindableProperty.Create(nameof(MessageFontSize), typeof(double), typeof(LoadingSpinner), 14.0);

        public double MessageFontSize
        {
            get => (double)GetValue(MessageFontSizeProperty);
            set => SetValue(MessageFontSizeProperty, value);
        }

        public LoadingSpinner()
        {
            InitializeComponent();
        }
    }
}