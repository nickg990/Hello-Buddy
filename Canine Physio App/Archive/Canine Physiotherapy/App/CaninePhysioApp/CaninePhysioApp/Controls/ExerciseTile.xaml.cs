using System.Windows.Input;

namespace CaninePhysioApp.Controls
{
    public partial class ExerciseTile : ContentView
    {
        public static readonly BindableProperty ExerciseKeyProperty =
            BindableProperty.Create(nameof(ExerciseKey), typeof(string), typeof(ExerciseTile), string.Empty);

        public static readonly BindableProperty ExerciseNameProperty =
            BindableProperty.Create(nameof(ExerciseName), typeof(string), typeof(ExerciseTile), string.Empty);

        public static readonly BindableProperty ImageSourceProperty =
            BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(ExerciseTile), null);

        public static readonly BindableProperty IsCompleteProperty =
            BindableProperty.Create(nameof(IsComplete), typeof(bool), typeof(ExerciseTile), false);

        public static readonly BindableProperty TileWidthProperty =
            BindableProperty.Create(nameof(TileWidth), typeof(double), typeof(ExerciseTile), 150.0);

        public static readonly BindableProperty TileHeightProperty =
            BindableProperty.Create(nameof(TileHeight), typeof(double), typeof(ExerciseTile), 100.0);

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ExerciseTile), null);

        public string ExerciseKey
        {
            get => (string)GetValue(ExerciseKeyProperty);
            set => SetValue(ExerciseKeyProperty, value);
        }

        public string ExerciseName
        {
            get => (string)GetValue(ExerciseNameProperty);
            set => SetValue(ExerciseNameProperty, value);
        }

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public bool IsComplete
        {
            get => (bool)GetValue(IsCompleteProperty);
            set => SetValue(IsCompleteProperty, value);
        }

        public double TileWidth
        {
            get => (double)GetValue(TileWidthProperty);
            set => SetValue(TileWidthProperty, value);
        }

        public double TileHeight
        {
            get => (double)GetValue(TileHeightProperty);
            set => SetValue(TileHeightProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public ExerciseTile()
        {
            InitializeComponent();
        }
    }
}
