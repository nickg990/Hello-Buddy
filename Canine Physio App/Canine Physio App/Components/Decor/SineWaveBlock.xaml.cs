using Microsoft.Maui.Controls.Shapes;

namespace Canine_Physio_App.Components.Decor
{
    /// <summary>
    /// A decorative sine wave block used as a visual transition between page sections.
    /// Generates a smooth cosine curve that creates a single "dip" effect.
    /// </summary>
    public partial class SineWaveBlock : ContentView
    {
        /// <summary>
        /// The fill color of the wave shape.
        /// Default: BrandPrimaryLight from theme.
        /// </summary>
        public static readonly BindableProperty FillColorProperty =
            BindableProperty.Create(
                nameof(FillColor),
                typeof(Color),
                typeof(SineWaveBlock),
                null, // Default set in constructor from resources
                propertyChanged: OnVisualPropertyChanged);

        public Color FillColor
        {
            get => (Color)GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }

        /// <summary>
        /// The overall height of the wave block.
        /// Default: WaveHeight token from Sizing.xaml.
        /// </summary>
        public static readonly BindableProperty WaveHeightProperty =
            BindableProperty.Create(
                nameof(WaveHeight),
                typeof(double),
                typeof(SineWaveBlock),
                140.0, // Fallback if token not available
                propertyChanged: OnVisualPropertyChanged);

        public double WaveHeight
        {
            get => (double)GetValue(WaveHeightProperty);
            set => SetValue(WaveHeightProperty, value);
        }

        /// <summary>
        /// Which edge the wave attaches to. Controls curve orientation.
        /// Bottom (default): curve dips downward into body.
        /// Top: curve flips to dip upward into header.
        /// </summary>
        public static readonly BindableProperty WaveEdgeProperty =
            BindableProperty.Create(
                nameof(WaveEdge),
                typeof(WaveEdge),
                typeof(SineWaveBlock),
                WaveEdge.Bottom,
                propertyChanged: OnVisualPropertyChanged);

        public WaveEdge WaveEdge
        {
            get => (WaveEdge)GetValue(WaveEdgeProperty);
            set => SetValue(WaveEdgeProperty, value);
        }

        /// <summary>
        /// Amplitude ratio - the wave dip depth as a percentage of height.
        /// ~30% matches the archive visual proportions.
        /// </summary>
        private const double AmplitudeRatio = 0.30;

        public SineWaveBlock()
        {
            InitializeComponent();

            // Set default FillColor from theme resources
            if (Application.Current?.Resources.TryGetValue("BrandPrimaryLight", out var colorValue) == true
                && colorValue is Color themeColor)
            {
                FillColor = themeColor;
            }
            else
            {
                // Fallback if resource not found
                FillColor = Color.FromArgb("#B3CDD6");
            }

            // Set default WaveHeight from theme resources
            if (Application.Current?.Resources.TryGetValue("WaveHeight", out var heightValue) == true)
            {
                if (heightValue is double h)
                {
                    WaveHeight = h;
                }
                else if (heightValue is OnIdiom<double> idiomHeight)
                {
                    // OnIdiom needs to be resolved
                    WaveHeight = idiomHeight;
                }
            }
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

        private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SineWaveBlock block && block.Width > 0)
            {
                block.RegenerateWave();
            }
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            if (Width > 0 && WaveHeight > 0)
            {
                RegenerateWave();
            }
        }

        /// <summary>
        /// Regenerates the wave path based on current properties.
        /// </summary>
        private void RegenerateWave()
        {
            double width = Width;
            double height = WaveHeight;
            double amplitude = height * AmplitudeRatio;

            // Clamp amplitude to avoid distortion (max 90% of height)
            amplitude = Math.Min(amplitude, height * 0.9);

            GenerateWavePath(width, amplitude, height, WaveEdge);

            // Update fill color
            wavePath.Fill = new SolidColorBrush(FillColor);
        }

        /// <summary>
        /// Generates a smooth wave path using sine formula.
        /// Formula: y = amplitude * (1 + sin(2π * x / width)) / 2
        /// This yields: trough at x=w/4, peak at x=3w/4, mid-height at edges.
        /// </summary>
        private void GenerateWavePath(double width, double amplitude, double height, WaveEdge edge)
        {
            if (width <= 0 || height <= 0)
                return;

            var pathFigure = new PathFigure();
            int segments = 100;
            double step = width / segments;

            if (edge == WaveEdge.Bottom)
            {
                // Wave at bottom: trough on left quarter, peak on right three-quarter
                // Start at top-left (y = amplitude/2, mid-height, descending toward trough)
                double startY = amplitude / 2;
                pathFigure.StartPoint = new Point(0, startY);

                var polyLineSegment = new PolyLineSegment();

                // Draw the wave curve using sine: trough left, peak right, end mid
                for (int i = 0; i <= segments; i++)
                {
                    double x = i * step;
                    double y = amplitude * (1 + Math.Sin(2 * Math.PI * x / width)) / 2;
                    polyLineSegment.Points.Add(new Point(x, y));
                }

                pathFigure.Segments.Add(polyLineSegment);

                // Complete the path: down to bottom-right, across bottom, back to start
                pathFigure.Segments.Add(new LineSegment { Point = new Point(width, height) });
                pathFigure.Segments.Add(new LineSegment { Point = new Point(0, height) });
                pathFigure.Segments.Add(new LineSegment { Point = new Point(0, startY) });
            }
            else // WaveEdge.Top
            {
                // Wave at top: inverted - peak on left quarter, trough on right three-quarter
                // Start at bottom-left (y = height - amplitude/2, mid-height)
                double startY = height - amplitude / 2;
                pathFigure.StartPoint = new Point(0, startY);

                var polyLineSegment = new PolyLineSegment();

                // Draw the wave curve inverted
                for (int i = 0; i <= segments; i++)
                {
                    double x = i * step;
                    // Invert: subtract from height
                    double y = height - amplitude * (1 + Math.Sin(2 * Math.PI * x / width)) / 2;
                    polyLineSegment.Points.Add(new Point(x, y));
                }

                pathFigure.Segments.Add(polyLineSegment);

                // Complete the path: up to top-right, across top, back to start
                pathFigure.Segments.Add(new LineSegment { Point = new Point(width, 0) });
                pathFigure.Segments.Add(new LineSegment { Point = new Point(0, 0) });
                pathFigure.Segments.Add(new LineSegment { Point = new Point(0, startY) });
            }

            pathFigure.IsClosed = true;

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            wavePath.Data = pathGeometry;
        }
    }
}
