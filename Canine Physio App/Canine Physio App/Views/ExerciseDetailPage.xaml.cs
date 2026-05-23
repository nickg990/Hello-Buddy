using Canine_Physio_App.Models;
using Canine_Physio_App.Services;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using Canine_Physio_App.Helpers;
using System.Windows.Input;

namespace Canine_Physio_App.Views
{
    /// <summary>
    /// Exercise detail page displaying summary, video thumbnail, instructions,
    /// and reps/sets for a specific exercise. Video plays fullscreen on tap.
    /// </summary>
    [QueryProperty(nameof(ExerciseKey), "exerciseKey")]
    public partial class ExerciseDetailPage : ContentPage
    {
        private readonly PhysioContentService _physioContentService;
        private readonly SessionStateService _sessionStateService;

        private string _exerciseKey = string.Empty;
        private string _exerciseTitle = string.Empty;
        private string _summary = string.Empty;
        private ImageSource? _thumbnailImage;
        private string _videoFileName = string.Empty;
        private bool _isVideoPlaying;
        private int _reps;
        private int _sets;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Collection of numbered instruction steps for the exercise.
        /// </summary>
        public ObservableCollection<InstructionStep> InstructionSteps { get; } = new();

        #region Bindable Properties

        /// <summary>
        /// The exercise key passed via navigation query parameter.
        /// Setting this triggers loading of exercise data.
        /// </summary>
        public string ExerciseKey
        {
            get => _exerciseKey;
            set
            {
                if (_exerciseKey != value)
                {
                    _exerciseKey = value;
                    OnPropertyChanged(nameof(ExerciseKey));
                    _ = LoadExerciseAsync();
                }
            }
        }

        /// <summary>
        /// The exercise title displayed in the header.
        /// </summary>
        public string ExerciseTitle
        {
            get => _exerciseTitle;
            set
            {
                if (_exerciseTitle != value)
                {
                    _exerciseTitle = value;
                    OnPropertyChanged(nameof(ExerciseTitle));
                }
            }
        }

        /// <summary>
        /// Summary description of the exercise.
        /// </summary>
        public string Summary
        {
            get => _summary;
            set
            {
                if (_summary != value)
                {
                    _summary = value;
                    OnPropertyChanged(nameof(Summary));
                }
            }
        }

        /// <summary>
        /// Thumbnail image for the exercise video.
        /// </summary>
        public ImageSource? ThumbnailImage
        {
            get => _thumbnailImage;
            set
            {
                if (_thumbnailImage != value)
                {
                    _thumbnailImage = value;
                    OnPropertyChanged(nameof(ThumbnailImage));
                }
            }
        }

        /// <summary>
        /// Whether the fullscreen video overlay is visible.
        /// </summary>
        public bool IsVideoPlaying
        {
            get => _isVideoPlaying;
            set
            {
                if (_isVideoPlaying != value)
                {
                    _isVideoPlaying = value;
                    OnPropertyChanged(nameof(IsVideoPlaying));
                }
            }
        }

        /// <summary>
        /// Number of repetitions for the exercise.
        /// </summary>
        public int Reps
        {
            get => _reps;
            set
            {
                if (_reps != value)
                {
                    _reps = value;
                    OnPropertyChanged(nameof(Reps));
                }
            }
        }

        /// <summary>
        /// Number of sets for the exercise.
        /// </summary>
        public int Sets
        {
            get => _sets;
            set
            {
                if (_sets != value)
                {
                    _sets = value;
                    OnPropertyChanged(nameof(Sets));
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to navigate back to the previous page.
        /// </summary>
        public ICommand NavigateBackCommand { get; }

        /// <summary>
        /// Command to mark the exercise as complete.
        /// </summary>
        public ICommand MarkCompleteCommand { get; }

        /// <summary>
        /// Command to play the exercise video fullscreen.
        /// </summary>
        public ICommand PlayVideoCommand { get; }

        /// <summary>
        /// Command to close the fullscreen video overlay.
        /// </summary>
        public ICommand CloseVideoCommand { get; }

        /// <summary>
        /// Command to navigate to the information page (via header icon).
        /// </summary>
        public ICommand NavigateToInfoCommand { get; }

        #endregion

        #region Responsive Properties

        private double _responsiveTextSize;
        /// <summary>Responsive font size for body text (TextMd on tablet, TextSm on phone).</summary>
        public double ResponsiveTextSize
        {
            get => _responsiveTextSize;
            private set
            {
                if (Math.Abs(_responsiveTextSize - value) > 0.01)
                {
                    _responsiveTextSize = value;
                    OnPropertyChanged(nameof(ResponsiveTextSize));
                }
            }
        }

        private double _instructionColumnSpacing = 8;
        /// <summary>Responsive column spacing for instruction rows.</summary>
        public double InstructionColumnSpacing
        {
            get => _instructionColumnSpacing;
            private set
            {
                if (Math.Abs(_instructionColumnSpacing - value) > 0.01)
                {
                    _instructionColumnSpacing = value;
                    OnPropertyChanged(nameof(InstructionColumnSpacing));
                }
            }
        }

        private double _instructionNumberMinWidth = 20;
        /// <summary>Responsive minimum width for instruction step numbers.</summary>
        public double InstructionNumberMinWidth
        {
            get => _instructionNumberMinWidth;
            private set
            {
                if (Math.Abs(_instructionNumberMinWidth - value) > 0.01)
                {
                    _instructionNumberMinWidth = value;
                    OnPropertyChanged(nameof(InstructionNumberMinWidth));
                }
            }
        }

        #endregion

        public ExerciseDetailPage(PhysioContentService physioContentService,
            SessionStateService sessionStateService)
        {
            _physioContentService = physioContentService;
            _sessionStateService = sessionStateService;

            // Initialize commands
            NavigateBackCommand = new Command(async () => await OnNavigateBack());
            MarkCompleteCommand = new Command(async () => await OnMarkComplete());
            PlayVideoCommand = new Command(OnPlayVideo);
            CloseVideoCommand = new Command(OnCloseVideo);
            NavigateToInfoCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(InformationPage)));

            BindingContext = this;
            InitializeComponent();
        }

        #region Data Loading

        /// <summary>
        /// Loads exercise data from the PhysioContentService.
        /// Called when ExerciseKey is set via navigation.
        /// </summary>
        private async Task LoadExerciseAsync()
        {
            if (string.IsNullOrEmpty(_exerciseKey))
                return;

            try
            {
                var exercise = await _physioContentService.GetExerciseByKeyAsync(_exerciseKey, _cts?.Token ?? default);

                if (exercise is null)
                {
                    await Shell.Current.DisplayAlertAsync("Error", "Exercise not found.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // Populate properties
                ExerciseTitle = StringHelper.ToTitleCase(exercise.Title);
                Summary = exercise.Summary;
                Reps = exercise.Reps;
                Sets = exercise.Sets;

                // Load thumbnail image - MauiImage strips extension, so use name only
                ThumbnailImage = ImageSource.FromFile(exercise.Image);
                _videoFileName = exercise.VideoName;

                // Build numbered instruction steps
                InstructionSteps.Clear();
                for (int i = 0; i < exercise.Instructions.Count; i++)
                {
                    InstructionSteps.Add(new InstructionStep
                    {
                        Number = i + 1,
                        Text = exercise.Instructions[i]
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading exercise: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Error", "Failed to load exercise details.", "OK");
            }
        }

        #endregion

        #region Video Playback

        /// <summary>
        /// Starts fullscreen video playback.
        /// </summary>
        private async void OnPlayVideo()
        {
            if (string.IsNullOrEmpty(_videoFileName))
            {
                await Shell.Current.DisplayAlertAsync("Error", "Video not available.", "OK");
                return;
            }

            VideoPlayer.Source = MediaSource.FromResource(_videoFileName);
            IsVideoPlaying = true;
        }

        /// <summary>
        /// Stops video playback and closes the overlay.
        /// </summary>
        private void OnCloseVideo()
        {
            VideoPlayer.Stop();
            VideoPlayer.Source = null;
            IsVideoPlaying = false;
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Navigates back to the previous page.
        /// </summary>
        private async Task OnNavigateBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        /// <summary>
        /// Navigates to the Update Progress page with current reps/sets.
        /// </summary>
        private async Task OnMarkComplete()
        {
            await Shell.Current.GoToAsync(
                $"{nameof(ExerciseProgressPage)}?reps={Reps}&sets={Sets}&exerciseKey={ExerciseKey}");
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Subscribes to template property changes for responsive sizing.
        /// </summary>
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            if (Handler != null)
            {
                pageTemplate.PropertyChanged += OnTemplatePropertyChanged;
                ApplyResponsiveContentStyles();
            }
        }

        /// <summary>
        /// Unsubscribes from events when the handler is being removed.
        /// </summary>
        protected override void OnHandlerChanging(HandlerChangingEventArgs args)
        {
            base.OnHandlerChanging(args);
            if (args.OldHandler != null)
            {
                pageTemplate.PropertyChanged -= OnTemplatePropertyChanged;
            }
        }

        private void OnTemplatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsTablet")
            {
                ApplyResponsiveContentStyles();
            }
        }

        /// <summary>
        /// Applies responsive sizing to content elements based on screen width category.
        /// Replaces duplicate phone/tablet XAML layouts with a single adaptive layout.
        /// </summary>
        private void ApplyResponsiveContentStyles()
        {
            if (contentLayout == null) return;

            bool isTablet = pageTemplate.IsTablet;

            // Font sizes for body text and value labels
            ResponsiveTextSize = GetResourceDouble(isTablet ? "TextMd" : "TextSm");
            double valueSize = GetResourceDouble(isTablet ? "TextLg" : "TextMd");

            // Bindable properties for DataTemplate bindings
            InstructionColumnSpacing = GetResourceDouble(isTablet ? "Space12" : "Space8");
            InstructionNumberMinWidth = isTablet ? 24 : 20;

            // Named element sizing
            summaryLabel.FontSize = ResponsiveTextSize;
            contentLayout.Spacing = GetResourceDouble(isTablet ? "SpaceStackMd" : "SpaceStackSm");
            instructionsList.Spacing = GetResourceDouble(isTablet ? "SpaceStackSm" : "Space8");
            repsSetsGrid.ColumnSpacing = GetResourceDouble(isTablet ? "Space48" : "Space32");
            repsStack.Spacing = GetResourceDouble(isTablet ? "Space8" : "Space4");
            setsStack.Spacing = GetResourceDouble(isTablet ? "Space8" : "Space4");
            repsLabel.FontSize = ResponsiveTextSize;
            repsValue.FontSize = valueSize;
            setsLabel.FontSize = ResponsiveTextSize;
            setsValue.FontSize = valueSize;
        }

        /// <summary>
        /// Retrieves a double value from the application's merged resource dictionaries.
        /// </summary>
        private static double GetResourceDouble(string key)
        {
            if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is double d)
                return d;
            return 0;
        }

        /// <summary>
        /// Ensures video is stopped when leaving the page.
        /// Sets the current exercise key for the Skip tab to detect.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            // Set current exercise context so Skip tab knows which exercise to skip
            if (!string.IsNullOrEmpty(_exerciseKey))
            {
                _sessionStateService.CurrentExerciseKey = _exerciseKey;
            }
        }

        /// <summary>
        /// Ensures video is stopped when leaving the page.
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _cts?.Cancel();
            OnCloseVideo();
        }

        #endregion

    }
}
