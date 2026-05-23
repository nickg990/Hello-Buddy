using Microsoft.Maui.Controls;

namespace CaninePhysioApp.Controls
{
    public partial class PageTemplate : ContentView
    {
        // Responsive sizing breakpoints and values
        private const double SmallPhoneMaxWidth = 360;
        private const double TabletMinWidth = 600;
        
        // Header panel minimum heights
        private const double HeaderPanelMinHeight_SmallPhone = 320;
        private const double HeaderPanelMinHeight_Phone = 380;
        private const double HeaderPanelMinHeight_Tablet = 440;
        
        // Wave heights
        private const double WaveHeight_SmallPhone = 140;
        private const double WaveHeight_Phone = 180;
        private const double WaveHeight_Tablet = 220;
        
        // Wave amplitudes
        private const double WaveAmplitude_SmallPhone = 40;
        private const double WaveAmplitude_Phone = 50;
        private const double WaveAmplitude_Tablet = 60;

        // Title property
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(PageTemplate), "Hello Buddy");

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        // Subtitle property
        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(PageTemplate), "Canine Physiotherapy");

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        // Show Subtitle toggle
        public static readonly BindableProperty ShowSubtitleProperty =
            BindableProperty.Create(nameof(ShowSubtitle), typeof(bool), typeof(PageTemplate), true);

        public bool ShowSubtitle
        {
            get => (bool)GetValue(ShowSubtitleProperty);
            set => SetValue(ShowSubtitleProperty, value);
        }

        // Slot 1: Content above the wave (e.g., Logo Disc)
        public static readonly BindableProperty AboveWaveContentProperty =
            BindableProperty.Create(nameof(AboveWaveContent), typeof(View), typeof(PageTemplate), null);

        public View AboveWaveContent
        {
            get => (View)GetValue(AboveWaveContentProperty);
            set => SetValue(AboveWaveContentProperty, value);
        }

        // Slot 2: Content within the wave area (e.g., Buttons)
        public static readonly BindableProperty WaveContentProperty =
            BindableProperty.Create(nameof(WaveContent), typeof(View), typeof(PageTemplate), null);

        public View WaveContent
        {
            get => (View)GetValue(WaveContentProperty);
            set => SetValue(WaveContentProperty, value);
        }

        public PageTemplate()
        {
            InitializeComponent();
            
            // Subscribe to size changes for responsive layout
            SizeChanged += OnSizeChanged;
            
            // Apply initial sizing
            ApplyResponsiveSizing(Width);
        }
        
        private void OnSizeChanged(object sender, EventArgs e)
        {
            ApplyResponsiveSizing(Width);
        }
        
        private void ApplyResponsiveSizing(double width)
        {
            // Determine screen size category
            ScreenSizeCategory category;
            
            if (width < SmallPhoneMaxWidth)
                category = ScreenSizeCategory.SmallPhone;
            else if (width >= TabletMinWidth)
                category = ScreenSizeCategory.Tablet;
            else
                category = ScreenSizeCategory.Phone;
            
            // Apply appropriate values
            switch (category)
            {
                case ScreenSizeCategory.SmallPhone:
                    HeaderPanel.MinimumHeightRequest = HeaderPanelMinHeight_SmallPhone;
                    WaveBlock.BlockHeight = WaveHeight_SmallPhone;
                    WaveBlock.WaveAmplitude = WaveAmplitude_SmallPhone;
                    break;
                    
                case ScreenSizeCategory.Phone:
                    HeaderPanel.MinimumHeightRequest = HeaderPanelMinHeight_Phone;
                    WaveBlock.BlockHeight = WaveHeight_Phone;
                    WaveBlock.WaveAmplitude = WaveAmplitude_Phone;
                    break;
                    
                case ScreenSizeCategory.Tablet:
                    HeaderPanel.MinimumHeightRequest = HeaderPanelMinHeight_Tablet;
                    WaveBlock.BlockHeight = WaveHeight_Tablet;
                    WaveBlock.WaveAmplitude = WaveAmplitude_Tablet;
                    break;
            }
        }
        
        private enum ScreenSizeCategory
        {
            SmallPhone,
            Phone,
            Tablet
        }
    }
}
