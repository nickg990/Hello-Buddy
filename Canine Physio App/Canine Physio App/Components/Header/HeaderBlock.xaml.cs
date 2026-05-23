using System.Windows.Input;
using Canine_Physio_App.Helpers;

namespace Canine_Physio_App.Components.Header
{
    /// <summary>
    /// A reusable page header component displaying subtitle, title, and optional icon button.
    /// Uses design tokens for consistent styling across the app.
    /// </summary>
    public partial class HeaderBlock : ContentView
    {
        /// <summary>
        /// The main title text displayed in the header.
        /// </summary>
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(
                nameof(Title),
                typeof(string),
                typeof(HeaderBlock),
                "Hello Buddy");

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// The subtitle text displayed above the title (e.g., "CANINE PHYSIOTHERAPY").
        /// </summary>
        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create(
                nameof(Subtitle),
                typeof(string),
                typeof(HeaderBlock),
                "CANINE PHYSIOTHERAPY");

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        /// <summary>
        /// Whether to show the subtitle text.
        /// </summary>
        public static readonly BindableProperty ShowSubtitleProperty =
            BindableProperty.Create(
                nameof(ShowSubtitle),
                typeof(bool),
                typeof(HeaderBlock),
                true);

        public bool ShowSubtitle
        {
            get => (bool)GetValue(ShowSubtitleProperty);
            set => SetValue(ShowSubtitleProperty, value);
        }

        /// <summary>
        /// Whether to show the icon button on the right side.
        /// </summary>
        public static readonly BindableProperty ShowIconProperty =
            BindableProperty.Create(
                nameof(ShowIcon),
                typeof(bool),
                typeof(HeaderBlock),
                false);

        public bool ShowIcon
        {
            get => (bool)GetValue(ShowIconProperty);
            set => SetValue(ShowIconProperty, value);
        }

        /// <summary>
        /// The image source for the icon button.
        /// </summary>
        public static readonly BindableProperty IconSourceProperty =
            BindableProperty.Create(
                nameof(IconSource),
                typeof(ImageSource),
                typeof(HeaderBlock),
                null);

        public ImageSource? IconSource
        {
            get => (ImageSource?)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        /// <summary>
        /// The command to execute when the icon button is tapped.
        /// </summary>
        public static readonly BindableProperty IconCommandProperty =
            BindableProperty.Create(
                nameof(IconCommand),
                typeof(ICommand),
                typeof(HeaderBlock),
                null);

        public ICommand? IconCommand
        {
            get => (ICommand?)GetValue(IconCommandProperty);
            set => SetValue(IconCommandProperty, value);
        }

        /// <summary>
        /// Target width percentage for subtitle character spacing (0.0 to 1.0).
        /// </summary>
        private const double SubtitleWidthPercent = 0.90;

        public HeaderBlock()
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
                Loaded += OnLoaded;
                SizeChanged += OnSizeChanged;
                rootGrid.SizeChanged += OnRootGridSizeChanged;
            }
        }

        protected override void OnHandlerChanging(HandlerChangingEventArgs args)
        {
            base.OnHandlerChanging(args);
            
            if (args.OldHandler != null)
            {
                // Unsubscribe when handler is detaching to prevent memory leaks
                Loaded -= OnLoaded;
                SizeChanged -= OnSizeChanged;
                rootGrid.SizeChanged -= OnRootGridSizeChanged;
            }
        }

        #endregion

        private void OnLoaded(object? sender, EventArgs e)
        {
            // Delay slightly to ensure layout is complete
            Dispatcher.Dispatch(() => UpdateSubtitleSpacing());
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            UpdateSubtitleSpacing();
        }

        private void OnRootGridSizeChanged(object? sender, EventArgs e)
        {
            UpdateSubtitleSpacing();
        }

        /// <summary>
        /// Calculates and applies character spacing to make the subtitle fill 90% of available width.
        /// Uses CharacterSpacingHelper for consistent calculation across components.
        /// </summary>
        private void UpdateSubtitleSpacing()
        {
            // Check if we have valid dimensions
            double containerWidth = rootGrid.Width;
            if (containerWidth <= 0)
                containerWidth = this.Width;
            if (containerWidth <= 0)
                return;
            
            if (!ShowSubtitle || string.IsNullOrEmpty(Subtitle))
                return;

            // Get available width (container width × target percentage)
            double availableWidth = containerWidth * SubtitleWidthPercent;

            // Get font size from the label
            double fontSize = subtitleLabel.FontSize;
            if (fontSize <= 0)
                fontSize = 11; // Fallback to TextXs default

            // Calculate spacing using shared helper
            string text = Subtitle.ToUpperInvariant();
            subtitleLabel.CharacterSpacing = CharacterSpacingHelper.Calculate(
                text,
                availableWidth,
                fontSize);
        }
    }
}
