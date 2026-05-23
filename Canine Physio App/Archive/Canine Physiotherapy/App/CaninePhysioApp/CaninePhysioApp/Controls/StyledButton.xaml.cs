using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace CaninePhysioApp.Controls
{
    public partial class StyledButton : ContentView
    {
        // Text property
        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(StyledButton), string.Empty);

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // Command property
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(StyledButton), null);

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        // Background Color property
        public static readonly BindableProperty ButtonBackgroundColorProperty =
            BindableProperty.Create(nameof(ButtonBackgroundColor), typeof(Color), typeof(StyledButton), Color.FromArgb("#3D81A9"));

        public Color ButtonBackgroundColor
        {
            get => (Color)GetValue(ButtonBackgroundColorProperty);
            set => SetValue(ButtonBackgroundColorProperty, value);
        }

        // Text Color property
        public static readonly BindableProperty ButtonTextColorProperty =
            BindableProperty.Create(nameof(ButtonTextColor), typeof(Color), typeof(StyledButton), Colors.White);

        public Color ButtonTextColor
        {
            get => (Color)GetValue(ButtonTextColorProperty);
            set => SetValue(ButtonTextColorProperty, value);
        }

        // Border Color property
        public static readonly BindableProperty ButtonBorderColorProperty =
            BindableProperty.Create(nameof(ButtonBorderColor), typeof(Color), typeof(StyledButton), Colors.Transparent);

        public Color ButtonBorderColor
        {
            get => (Color)GetValue(ButtonBorderColorProperty);
            set => SetValue(ButtonBorderColorProperty, value);
        }

        // Border Width property
        public static readonly BindableProperty ButtonBorderWidthProperty =
            BindableProperty.Create(nameof(ButtonBorderWidth), typeof(double), typeof(StyledButton), 0.0);

        public double ButtonBorderWidth
        {
            get => (double)GetValue(ButtonBorderWidthProperty);
            set => SetValue(ButtonBorderWidthProperty, value);
        }

        // Width property - now treated as MINIMUM width (use -1 for no minimum)
        public static readonly BindableProperty ButtonWidthProperty =
            BindableProperty.Create(nameof(ButtonWidth), typeof(double), typeof(StyledButton), -1.0,
                propertyChanged: OnButtonWidthChanged);

        public double ButtonWidth
        {
            get => (double)GetValue(ButtonWidthProperty);
            set => SetValue(ButtonWidthProperty, value);
        }

        // Height property - now treated as MINIMUM height for touch target
        public static readonly BindableProperty ButtonHeightProperty =
            BindableProperty.Create(nameof(ButtonHeight), typeof(double), typeof(StyledButton), 48.0);

        public double ButtonHeight
        {
            get => (double)GetValue(ButtonHeightProperty);
            set => SetValue(ButtonHeightProperty, value);
        }

        // Horizontal Alignment property
        public static readonly BindableProperty ButtonHorizontalAlignmentProperty =
            BindableProperty.Create(nameof(ButtonHorizontalAlignment), typeof(LayoutOptions), typeof(StyledButton), LayoutOptions.Fill);

        public LayoutOptions ButtonHorizontalAlignment
        {
            get => (LayoutOptions)GetValue(ButtonHorizontalAlignmentProperty);
            set => SetValue(ButtonHorizontalAlignmentProperty, value);
        }

        // Margin property
        public static readonly BindableProperty ButtonMarginProperty =
            BindableProperty.Create(nameof(ButtonMargin), typeof(Thickness), typeof(StyledButton), new Thickness(0));

        public Thickness ButtonMargin
        {
            get => (Thickness)GetValue(ButtonMarginProperty);
            set => SetValue(ButtonMarginProperty, value);
        }

        // Corner Radius property
        public static readonly BindableProperty ButtonCornerRadiusProperty =
            BindableProperty.Create(nameof(ButtonCornerRadius), typeof(int), typeof(StyledButton), 12);

        public int ButtonCornerRadius
        {
            get => (int)GetValue(ButtonCornerRadiusProperty);
            set => SetValue(ButtonCornerRadiusProperty, value);
        }

        // Font Size property (increased default from 10 to 11 for better readability)
        public static readonly BindableProperty ButtonFontSizeProperty =
            BindableProperty.Create(nameof(ButtonFontSize), typeof(double), typeof(StyledButton), 11.0);

        public double ButtonFontSize
        {
            get => (double)GetValue(ButtonFontSizeProperty);
            set => SetValue(ButtonFontSizeProperty, value);
        }

        // Font Family property
        public static readonly BindableProperty ButtonFontFamilyProperty =
            BindableProperty.Create(nameof(ButtonFontFamily), typeof(string), typeof(StyledButton), "MontserratSemiBold");

        public string ButtonFontFamily
        {
            get => (string)GetValue(ButtonFontFamilyProperty);
            set => SetValue(ButtonFontFamilyProperty, value);
        }

        // Shadow Color property
        public static readonly BindableProperty ShadowColorProperty =
            BindableProperty.Create(nameof(ShadowColor), typeof(Color), typeof(StyledButton), Color.FromArgb("#33000000"));

        public Color ShadowColor
        {
            get => (Color)GetValue(ShadowColorProperty);
            set => SetValue(ShadowColorProperty, value);
        }

        // Shadow Offset X property
        public static readonly BindableProperty ShadowOffsetXProperty =
            BindableProperty.Create(nameof(ShadowOffsetX), typeof(double), typeof(StyledButton), 2.0);

        public double ShadowOffsetX
        {
            get => (double)GetValue(ShadowOffsetXProperty);
            set => SetValue(ShadowOffsetXProperty, value);
        }

        // Shadow Offset Y property
        public static readonly BindableProperty ShadowOffsetYProperty =
            BindableProperty.Create(nameof(ShadowOffsetY), typeof(double), typeof(StyledButton), 2.0);

        public double ShadowOffsetY
        {
            get => (double)GetValue(ShadowOffsetYProperty);
            set => SetValue(ShadowOffsetYProperty, value);
        }

        // Character Spacing property for letter spacing in button text
        public static readonly BindableProperty ButtonCharacterSpacingProperty =
            BindableProperty.Create(nameof(ButtonCharacterSpacing), typeof(double), typeof(StyledButton), 0.0);

        public double ButtonCharacterSpacing
        {
            get => (double)GetValue(ButtonCharacterSpacingProperty);
            set => SetValue(ButtonCharacterSpacingProperty, value);
        }

        public StyledButton()
        {
            InitializeComponent();
            
            // Apply initial minimum width if set
            ApplyMinimumWidth(ButtonWidth);
        }

        private static void OnButtonWidthChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is StyledButton button && newValue is double width)
            {
                button.ApplyMinimumWidth(width);
            }
        }

        /// <summary>
        /// Applies ButtonWidth as MinimumWidthRequest instead of forced WidthRequest.
        /// This allows buttons to grow if needed while maintaining a consistent minimum size.
        /// </summary>
        private void ApplyMinimumWidth(double width)
        {
            if (width > 0)
            {
                // Apply as minimum width, allowing button to grow if content requires
                MinimumWidthRequest = width;
                MainButton.MinimumWidthRequest = width;
            }
            else
            {
                // No minimum - button sizes to content or fills container
                MinimumWidthRequest = -1;
                MainButton.MinimumWidthRequest = -1;
            }
        }

        private void OnButtonPressed(object sender, EventArgs e)
        {
            // Darken the button to show pressed state
            this.ScaleTo(0.96, 50);
            MainButton.Opacity = 0.7;
        }

        private void OnButtonReleased(object sender, EventArgs e)
        {
            // Restore the button to normal state
            this.ScaleTo(1.0, 50);
            MainButton.Opacity = 1.0;
        }
    }
}
