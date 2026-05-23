using System.Windows.Input;
using Canine_Physio_App.Services;

namespace Canine_Physio_App.Components.Layout
{
    /// <summary>
    /// A reusable page layout template providing consistent structure across the app.
    /// Composes HeaderBlock (fixed), hero stage + wave + body (scrollable), and optional footer (fixed).
    /// Handles safe area insets for edge-to-edge display.
    /// </summary>
    public partial class AppPageTemplate : ContentView
    {
        private readonly ISafeAreaService? _safeAreaService;

        #region Responsive Sizing Constants

        // Width breakpoints (in device-independent pixels)
        private const double SmallPhoneMaxWidth = 360;
        private const double TabletMinWidth = 600;

        // HeroHeight values for each screen size category (as ratio of screen height)
        // These ratios ensure the hero area scales proportionally with screen height
        private const double HeroHeightRatio_SmallPhone = 0.24;
        private const double HeroHeightRatio_Phone = 0.26;
        private const double HeroHeightRatio_Tablet = 0.28;

        // Maximum safe area inset to prevent excessive spacing on some emulators
        // Matched to Samsung S22 physical device (27dp status bar)
        private const double MaxTopInset = 27;
        private const double MaxBottomInset = 48;

        private enum ScreenSizeCategory
        {
            SmallPhone,
            Phone,
            Tablet
        }

        #endregion

        #region Screen Size Properties

        /// <summary>
        /// Indicates whether the current screen is tablet-sized (600dp+ width).
        /// Use this for responsive layouts that need larger content on tablets.
        /// </summary>
        public static readonly BindableProperty IsTabletProperty =
            BindableProperty.Create(
                nameof(IsTablet),
                typeof(bool),
                typeof(AppPageTemplate),
                false);

        public bool IsTablet
        {
            get => (bool)GetValue(IsTabletProperty);
            private set => SetValue(IsTabletProperty, value);
        }

        /// <summary>
        /// Indicates whether the current screen is phone-sized (under 600dp width).
        /// Includes both regular phones and small phones.
        /// </summary>
        public static readonly BindableProperty IsPhoneProperty =
            BindableProperty.Create(
                nameof(IsPhone),
                typeof(bool),
                typeof(AppPageTemplate),
                true);

        public bool IsPhone
        {
            get => (bool)GetValue(IsPhoneProperty);
            private set => SetValue(IsPhoneProperty, value);
        }

        #endregion

        #region Header Properties

        /// <summary>
        /// The main title displayed in the header.
        /// </summary>
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(
                nameof(Title),
                typeof(string),
                typeof(AppPageTemplate),
                "Hello Buddy");

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// The subtitle displayed below the title.
        /// </summary>
        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create(
                nameof(Subtitle),
                typeof(string),
                typeof(AppPageTemplate),
                "CANINE PHYSIOTHERAPY");

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        /// <summary>
        /// Whether to show the subtitle.
        /// </summary>
        public static readonly BindableProperty ShowSubtitleProperty =
            BindableProperty.Create(
                nameof(ShowSubtitle),
                typeof(bool),
                typeof(AppPageTemplate),
                true);

        public bool ShowSubtitle
        {
            get => (bool)GetValue(ShowSubtitleProperty);
            set => SetValue(ShowSubtitleProperty, value);
        }

        /// <summary>
        /// Whether to show the header icon button.
        /// </summary>
        public static readonly BindableProperty ShowHeaderIconProperty =
            BindableProperty.Create(
                nameof(ShowHeaderIcon),
                typeof(bool),
                typeof(AppPageTemplate),
                false);

        public bool ShowHeaderIcon
        {
            get => (bool)GetValue(ShowHeaderIconProperty);
            set => SetValue(ShowHeaderIconProperty, value);
        }

        /// <summary>
        /// The image source for the header icon.
        /// </summary>
        public static readonly BindableProperty HeaderIconSourceProperty =
            BindableProperty.Create(
                nameof(HeaderIconSource),
                typeof(ImageSource),
                typeof(AppPageTemplate),
                null);

        public ImageSource? HeaderIconSource
        {
            get => (ImageSource?)GetValue(HeaderIconSourceProperty);
            set => SetValue(HeaderIconSourceProperty, value);
        }

        /// <summary>
        /// The command for the header icon button.
        /// </summary>
        public static readonly BindableProperty HeaderIconCommandProperty =
            BindableProperty.Create(
                nameof(HeaderIconCommand),
                typeof(ICommand),
                typeof(AppPageTemplate),
                null);

        public ICommand? HeaderIconCommand
        {
            get => (ICommand?)GetValue(HeaderIconCommandProperty);
            set => SetValue(HeaderIconCommandProperty, value);
        }

        #endregion

        #region Hero Properties

        /// <summary>
        /// The content to display in the hero stage area (e.g., logo disc, illustrations).
        /// </summary>
        public static readonly BindableProperty HeroContentProperty =
            BindableProperty.Create(
                nameof(HeroContent),
                typeof(View),
                typeof(AppPageTemplate),
                null,
                propertyChanged: OnHeroContentChanged);

        public View? HeroContent
        {
            get => (View?)GetValue(HeroContentProperty);
            set => SetValue(HeroContentProperty, value);
        }

        private static void OnHeroContentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AppPageTemplate template)
            {
                template.UpdateHeroContent();
            }
        }

        #endregion

        #region Wave Properties

        /// <summary>
        /// Optional content to overlay on the sine wave transition area (e.g., session date label).
        /// Aligned to the bottom of the wave block so it sits on the Surface-coloured curve.
        /// </summary>
        public static readonly BindableProperty WaveContentProperty =
            BindableProperty.Create(
                nameof(WaveContent),
                typeof(View),
                typeof(AppPageTemplate),
                null,
                propertyChanged: OnWaveContentChanged);

        public View? WaveContent
        {
            get => (View?)GetValue(WaveContentProperty);
            set => SetValue(WaveContentProperty, value);
        }

        private static void OnWaveContentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AppPageTemplate template)
            {
                template.UpdateWaveContent();
            }
        }

        #endregion

        #region Body Properties

        /// <summary>
        /// The content to display in the body region (standard mode).
        /// </summary>
        public static readonly BindableProperty BodyContentProperty =
            BindableProperty.Create(
                nameof(BodyContent),
                typeof(View),
                typeof(AppPageTemplate),
                null,
                propertyChanged: OnBodyContentChanged);

        public View? BodyContent
        {
            get => (View?)GetValue(BodyContentProperty);
            set => SetValue(BodyContentProperty, value);
        }

        private static void OnBodyContentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AppPageTemplate template)
            {
                template.UpdateBodyContent();
            }
        }

        /// <summary>
        /// The content to display in full body mode (transparent overlay spanning hero+wave+body).
        /// Used when ShowFullBody="True".
        /// </summary>
        public static readonly BindableProperty FullBodyContentProperty =
            BindableProperty.Create(
                nameof(FullBodyContent),
                typeof(View),
                typeof(AppPageTemplate),
                null,
                propertyChanged: OnFullBodyContentChanged);

        public View? FullBodyContent
        {
            get => (View?)GetValue(FullBodyContentProperty);
            set => SetValue(FullBodyContentProperty, value);
        }

        private static void OnFullBodyContentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AppPageTemplate template)
            {
                template.UpdateFullBodyContent();
            }
        }

        /// <summary>
        /// Whether to use full body mode (transparent overlay) instead of standard hero+body slots.
        /// When true, FullBodyContent is shown as a transparent scrollable overlay.
        /// When false (default), HeroContent and BodyContent are shown with the wave between them.
        /// </summary>
        public static readonly BindableProperty ShowFullBodyProperty =
            BindableProperty.Create(
                nameof(ShowFullBody),
                typeof(bool),
                typeof(AppPageTemplate),
                false,
                propertyChanged: OnShowFullBodyChanged);

        public bool ShowFullBody
        {
            get => (bool)GetValue(ShowFullBodyProperty);
            set => SetValue(ShowFullBodyProperty, value);
        }

        private static void OnShowFullBodyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AppPageTemplate template)
            {
                template.UpdateFullBodyVisibility();
            }
        }

        /// <summary>
        /// Whether scrolling is enabled for the page content.
        /// When false, the ScrollView is disabled (content should fit).
        /// </summary>
        public static readonly BindableProperty IsBodyScrollableProperty =
            BindableProperty.Create(
                nameof(IsBodyScrollable),
                typeof(bool),
                typeof(AppPageTemplate),
                true,
                propertyChanged: OnIsBodyScrollableChanged);

        public bool IsBodyScrollable
        {
            get => (bool)GetValue(IsBodyScrollableProperty);
            set => SetValue(IsBodyScrollableProperty, value);
        }

        private static void OnIsBodyScrollableChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AppPageTemplate template)
            {
                template.UpdateScrollEnabled();
            }
        }

        /// <summary>
        /// Inverse of IsBodyScrollable for binding visibility (kept for API compatibility).
        /// </summary>
        public bool IsBodyStatic => !IsBodyScrollable;

        #endregion

        #region Footer Properties

        /// <summary>
        /// Whether to show the footer with navigation buttons.
        /// </summary>
        public static readonly BindableProperty ShowFooterProperty =
            BindableProperty.Create(
                nameof(ShowFooter),
                typeof(bool),
                typeof(AppPageTemplate),
                false);

        public bool ShowFooter
        {
            get => (bool)GetValue(ShowFooterProperty);
            set => SetValue(ShowFooterProperty, value);
        }

        /// <summary>
        /// Text for the primary button (e.g., "Next", "Save").
        /// </summary>
        public static readonly BindableProperty PrimaryButtonTextProperty =
            BindableProperty.Create(
                nameof(PrimaryButtonText),
                typeof(string),
                typeof(AppPageTemplate),
                string.Empty);

        public string PrimaryButtonText
        {
            get => (string)GetValue(PrimaryButtonTextProperty);
            set => SetValue(PrimaryButtonTextProperty, value);
        }

        /// <summary>
        /// Command for the primary button.
        /// </summary>
        public static readonly BindableProperty PrimaryCommandProperty =
            BindableProperty.Create(
                nameof(PrimaryCommand),
                typeof(ICommand),
                typeof(AppPageTemplate),
                null);

        public ICommand? PrimaryCommand
        {
            get => (ICommand?)GetValue(PrimaryCommandProperty);
            set => SetValue(PrimaryCommandProperty, value);
        }

        /// <summary>
        /// Text for the secondary button (e.g., "Back", "Cancel").
        /// </summary>
        public static readonly BindableProperty SecondaryButtonTextProperty =
            BindableProperty.Create(
                nameof(SecondaryButtonText),
                typeof(string),
                typeof(AppPageTemplate),
                string.Empty);

        public string SecondaryButtonText
        {
            get => (string)GetValue(SecondaryButtonTextProperty);
            set => SetValue(SecondaryButtonTextProperty, value);
        }

        /// <summary>
        /// Command for the secondary button.
        /// </summary>
        public static readonly BindableProperty SecondaryCommandProperty =
            BindableProperty.Create(
                nameof(SecondaryCommand),
                typeof(ICommand),
                typeof(AppPageTemplate),
                null);

        public ICommand? SecondaryCommand
        {
            get => (ICommand?)GetValue(SecondaryCommandProperty);
            set => SetValue(SecondaryCommandProperty, value);
        }

        /// <summary>
        /// Whether the primary button should be visible.
        /// </summary>
        public bool HasPrimaryButton => !string.IsNullOrEmpty(PrimaryButtonText);

        /// <summary>
        /// Whether the secondary button should be visible.
        /// </summary>
        public bool HasSecondaryButton => !string.IsNullOrEmpty(SecondaryButtonText);

        /// <summary>
        /// Whether the primary button is enabled. Useful for conditional enabling
        /// such as requiring terms acceptance before proceeding.
        /// </summary>
        public static readonly BindableProperty PrimaryButtonIsEnabledProperty =
            BindableProperty.Create(
                nameof(PrimaryButtonIsEnabled),
                typeof(bool),
                typeof(AppPageTemplate),
                true);

        public bool PrimaryButtonIsEnabled
        {
            get => (bool)GetValue(PrimaryButtonIsEnabledProperty);
            set => SetValue(PrimaryButtonIsEnabledProperty, value);
        }

        /// <summary>
        /// Text for the optional tertiary button (e.g., "SKIP").
        /// Appears between secondary and primary buttons when provided.
        /// </summary>
        public static readonly BindableProperty TertiaryButtonTextProperty =
            BindableProperty.Create(
                nameof(TertiaryButtonText),
                typeof(string),
                typeof(AppPageTemplate),
                string.Empty,
                propertyChanged: OnTertiaryButtonTextChanged);

        public string TertiaryButtonText
        {
            get => (string)GetValue(TertiaryButtonTextProperty);
            set => SetValue(TertiaryButtonTextProperty, value);
        }

        private static void OnTertiaryButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AppPageTemplate template)
            {
                template.UpdateFooterLayout();
            }
        }

        /// <summary>
        /// Command for the tertiary button.
        /// </summary>
        public static readonly BindableProperty TertiaryCommandProperty =
            BindableProperty.Create(
                nameof(TertiaryCommand),
                typeof(ICommand),
                typeof(AppPageTemplate),
                null);

        public ICommand? TertiaryCommand
        {
            get => (ICommand?)GetValue(TertiaryCommandProperty);
            set => SetValue(TertiaryCommandProperty, value);
        }

        /// <summary>
        /// Whether the tertiary button should be visible.
        /// </summary>
        public bool HasTertiaryButton => !string.IsNullOrEmpty(TertiaryButtonText);

        #endregion

        #region Overlay Properties

        /// <summary>
        /// Whether to show the loading overlay with spinner.
        /// Bind to a ViewModel's IsLoading property for loading states.
        /// </summary>
        public static readonly BindableProperty ShowOverlayProperty =
            BindableProperty.Create(
                nameof(ShowOverlay),
                typeof(bool),
                typeof(AppPageTemplate),
                false);

        public bool ShowOverlay
        {
            get => (bool)GetValue(ShowOverlayProperty);
            set => SetValue(ShowOverlayProperty, value);
        }

        #endregion

        public AppPageTemplate()
        {
            InitializeComponent();

            // Get safe area service from DI
            _safeAreaService = Application.Current?.Handler?.MauiContext?.Services
                .GetService<ISafeAreaService>();
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
            }
        }

        #endregion

        private void OnLoaded(object? sender, EventArgs e)
        {
            ConfigureSafeAreas();
            UpdateScrollEnabled();
            UpdateFullBodyVisibility();
            ApplyResponsiveSizing(Width);
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            ApplyResponsiveSizing(Width);
        }

        /// <summary>
        /// Configures safe area row heights based on platform insets.
        /// Uses SafeAreaService with fallback to tested defaults if service unavailable.
        /// </summary>
        private void ConfigureSafeAreas()
        {
            // Default values - Android bottom is 0 (nav bar handles itself), iOS needs bottom inset
            const double defaultTopInset = 27;
#if ANDROID
            const double defaultBottomInset = 0;
#else
            const double defaultBottomInset = 24;
#endif
            
            // Try to get actual insets from service, fall back to defaults
            double topInset = defaultTopInset;
            double bottomInset = defaultBottomInset;
            
            if (_safeAreaService != null)
            {
                _safeAreaService.Refresh();
                
                // Use service values if they're reasonable (non-zero, within bounds)
                if (_safeAreaService.TopInset > 0 && _safeAreaService.TopInset <= MaxTopInset)
                {
                    topInset = _safeAreaService.TopInset;
                }
                if (_safeAreaService.BottomInset > 0 && _safeAreaService.BottomInset <= MaxBottomInset)
                {
                    bottomInset = _safeAreaService.BottomInset;
                }
            }
            
            safeAreaTopRow.Height = new GridLength(topInset);
            safeAreaBottomSpacer.HeightRequest = bottomInset;
        }

        /// <summary>
        /// Updates the hero content host with the provided HeroContent.
        /// </summary>
        private void UpdateHeroContent()
        {
            heroContentHost.Content = HeroContent;
        }

        /// <summary>
        /// Updates the wave content overlay with the provided WaveContent.
        /// </summary>
        private void UpdateWaveContent()
        {
            waveContentHost.Content = WaveContent;
            waveContentHost.IsVisible = WaveContent != null;
        }

        /// <summary>
        /// Updates the body content host with the provided BodyContent.
        /// </summary>
        private void UpdateBodyContent()
        {
            bodyContentHost.Content = BodyContent;
        }

        /// <summary>
        /// Updates the footer grid layout when a tertiary button is added or removed.
        /// Switches between 2-column (secondary + primary) and 3-column (secondary + tertiary + primary).
        /// </summary>
        private void UpdateFooterLayout()
        {
            OnPropertyChanged(nameof(HasTertiaryButton));

            if (HasTertiaryButton)
            {
                footerGrid.ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                };
                Grid.SetColumn(primaryButton, 2);
            }
            else
            {
                footerGrid.ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                };
                Grid.SetColumn(primaryButton, 1);
            }
        }

        /// <summary>
        /// Updates the full body content host with the provided FullBodyContent.
        /// </summary>
        private void UpdateFullBodyContent()
        {
            fullBodyContentHost.Content = FullBodyContent;
        }

        /// <summary>
        /// Updates visibility of standard content slots vs full body overlay.
        /// When ShowFullBody is true:
        /// - FullBodyContent overlay is visible (transparent, scrollable)
        /// - HeroContent and BodyContent hosts are hidden (backgrounds remain)
        /// When ShowFullBody is false:
        /// - Standard HeroContent + BodyContent are visible
        /// - FullBodyContent overlay is hidden
        /// </summary>
        private void UpdateFullBodyVisibility()
        {
            bool showFull = ShowFullBody;
            
            // Toggle overlay visibility
            fullBodyScroll.IsVisible = showFull;
            
            // Hide/show the content inside hero and body hosts (backgrounds remain)
            heroContentHost.IsVisible = !showFull;
            bodyContentHost.IsVisible = !showFull;
            
            // Control which ScrollView handles scrolling
            pageScroll.VerticalScrollBarVisibility = showFull 
                ? ScrollBarVisibility.Never 
                : (IsBodyScrollable ? ScrollBarVisibility.Default : ScrollBarVisibility.Never);
        }

        /// <summary>
        /// Updates the ScrollView scrolling behavior based on IsBodyScrollable.
        /// Note: We control VerticalScrollBarVisibility instead of IsEnabled,
        /// because IsEnabled=false would disable all child controls (buttons, etc.).
        /// </summary>
        private void UpdateScrollEnabled()
        {
            // Use scroll bar visibility to indicate scrollability
            // Never set IsEnabled=false as it disables child interactivity
            pageScroll.VerticalScrollBarVisibility = IsBodyScrollable 
                ? ScrollBarVisibility.Default 
                : ScrollBarVisibility.Never;
        }

        /// <summary>
        /// Applies responsive sizing based on screen dimensions.
        /// Uses screen height to calculate hero height proportionally,
        /// ensuring content fits on screens of varying heights.
        /// Also updates IsTablet/IsPhone properties for responsive content.
        /// </summary>
        private void ApplyResponsiveSizing(double width)
        {
            if (width <= 0 || Height <= 0) return;

            // Determine screen size category based on width
            ScreenSizeCategory category;
            if (width < SmallPhoneMaxWidth)
                category = ScreenSizeCategory.SmallPhone;
            else if (width >= TabletMinWidth)
                category = ScreenSizeCategory.Tablet;
            else
                category = ScreenSizeCategory.Phone;

            // Update screen size properties for responsive content
            IsTablet = category == ScreenSizeCategory.Tablet;
            IsPhone = category != ScreenSizeCategory.Tablet;

            // Calculate hero height as a ratio of screen height
            double heroRatio = category switch
            {
                ScreenSizeCategory.SmallPhone => HeroHeightRatio_SmallPhone,
                ScreenSizeCategory.Tablet => HeroHeightRatio_Tablet,
                _ => HeroHeightRatio_Phone
            };

            double heroHeight = Height * heroRatio;
            heroStage.HeightRequest = heroHeight;
        }

        protected override void OnPropertyChanged(string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            // Update computed properties when dependencies change
            if (propertyName == nameof(IsBodyScrollable))
            {
                OnPropertyChanged(nameof(IsBodyStatic));
            }
            else if (propertyName == nameof(PrimaryButtonText))
            {
                OnPropertyChanged(nameof(HasPrimaryButton));
            }
            else if (propertyName == nameof(SecondaryButtonText))
            {
                OnPropertyChanged(nameof(HasSecondaryButton));
            }
            else if (propertyName == nameof(TertiaryButtonText))
            {
                OnPropertyChanged(nameof(HasTertiaryButton));
            }
        }
    }
}
