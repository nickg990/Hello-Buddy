using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;

namespace CaninePhysioApp.Controls
{
    public partial class SineWaveBlock : ContentView
    {
        // BlockHeight property - controls the layout height of the wave block
        public static readonly BindableProperty BlockHeightProperty =
            BindableProperty.Create(nameof(BlockHeight), typeof(double), typeof(SineWaveBlock), 160.0,
                propertyChanged: OnWavePropertyChanged);

        public double BlockHeight
        {
            get => (double)GetValue(BlockHeightProperty);
            set => SetValue(BlockHeightProperty, value);
        }

        // Wave Amplitude property - controls the depth of the wave dip
        public static readonly BindableProperty WaveAmplitudeProperty =
            BindableProperty.Create(nameof(WaveAmplitude), typeof(double), typeof(SineWaveBlock), 50.0,
                propertyChanged: OnWavePropertyChanged);

        public double WaveAmplitude
        {
            get => (double)GetValue(WaveAmplitudeProperty);
            set => SetValue(WaveAmplitudeProperty, value);
        }

        public SineWaveBlock()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
        }

        private static void OnWavePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SineWaveBlock block && block.Width > 0)
            {
                block.GenerateWavePath(block.Width, block.WaveAmplitude, block.BlockHeight);
            }
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            if (Width > 0 && BlockHeight > 0)
            {
                GenerateWavePath(Width, WaveAmplitude, BlockHeight);
            }
        }

        /// <summary>
        /// Generates a smooth wave path that creates a single "dip" effect.
        /// Uses cosine formula: y = amplitude * (1 - cos(2π * x / width)) / 2
        /// This yields: y=0 at x=0, y=amplitude at x=width/2, y=0 at x=width
        /// </summary>
        private void GenerateWavePath(double width, double amplitude, double height)
        {
            // Clamp amplitude to avoid distortion (max 90% of height)
            double clampedAmplitude = Math.Min(amplitude, height * 0.9);
            
            // Start at top-left (y=0 baseline)
            var pathFigure = new PathFigure { StartPoint = new Point(0, 0) };
            
            int segments = 100;
            double step = width / segments;
            
            var polyLineSegment = new PolyLineSegment();
            
            // Draw the wave curve using cosine for smooth single dip
            // y = amplitude * (1 - cos(2π * x / width)) / 2
            // This creates: y=0 at edges, y=amplitude at center
            for (int i = 0; i <= segments; i++)
            {
                double x = i * step;
                double y = clampedAmplitude * (1 - Math.Cos(2 * Math.PI * x / width)) / 2;
                polyLineSegment.Points.Add(new Point(x, y));
            }
            
            pathFigure.Segments.Add(polyLineSegment);
            
            // Complete the path - draw down to bottom-right, across bottom, and back up to start
            var lineToBottomRight = new LineSegment { Point = new Point(width, height) };
            var lineAcrossBottom = new LineSegment { Point = new Point(0, height) };
            var lineToStart = new LineSegment { Point = new Point(0, 0) };
            
            pathFigure.Segments.Add(lineToBottomRight);
            pathFigure.Segments.Add(lineAcrossBottom);
            pathFigure.Segments.Add(lineToStart);
            pathFigure.IsClosed = true;
            
            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);
            
            wavePath.Data = pathGeometry;
        }
    }
}
