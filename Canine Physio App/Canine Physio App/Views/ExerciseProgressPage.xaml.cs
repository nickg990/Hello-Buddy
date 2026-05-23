using System.ComponentModel;

using System.Windows.Input;
using Canine_Physio_App.Helpers;
using Canine_Physio_App.Services;

namespace Canine_Physio_App.Views
{
    /// <summary>
    /// Exercise Progress page for logging exercise completion with discomfort and comments.
    /// Receives reps/sets from ExerciseDetailPage via Shell query parameters.
    /// </summary>
    [QueryProperty(nameof(RepsParam), "reps")]
    [QueryProperty(nameof(SetsParam), "sets")]
    [QueryProperty(nameof(ExerciseKey), "exerciseKey")]
    public partial class ExerciseProgressPage : ContentPage
    {
        #region Private Fields

        private readonly PhysioContentService _physioContentService;
        private string _exerciseKey = string.Empty;
        private string _exerciseTitle = "Update Progress";
        private string _repsText = string.Empty;
        private string _setsText = string.Empty;
        private int _expectedReps = 10;
        private int _expectedSets = 3;
        private double _discomfort = 0;
        private string _comments = string.Empty;
        private bool _isBusy;
        private CancellationTokenSource? _cts;

        #endregion

        #region Query Parameters

        /// <summary>
        /// Reps parameter received from navigation query string.
        /// Sets expected reps only - actual reps starts blank for user input.
        /// </summary>
        public string? RepsParam
        {
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var reps))
                {
                    ExpectedReps = reps;
                }
            }
        }

        /// <summary>
        /// Sets parameter received from navigation query string.
        /// Sets expected sets only - actual sets starts blank for user input.
        /// </summary>
        public string? SetsParam
        {
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var sets))
                {
                    ExpectedSets = sets;
                }
            }
        }

        /// <summary>
        /// Exercise key parameter received from navigation query string.
        /// Setting this loads the exercise title.
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
                    _ = LoadExerciseTitleAsync();
                }
            }
        }

        #endregion

        #region Properties

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
        /// Text for reps entry field (string for Entry binding).
        /// </summary>
        public string RepsText
        {
            get => _repsText;
            set
            {
                if (_repsText != value)
                {
                    _repsText = value;
                    OnPropertyChanged(nameof(RepsText));
                }
            }
        }

        /// <summary>
        /// Parsed reps value for validation. Null if empty or invalid.
        /// </summary>
        public int? Reps => int.TryParse(_repsText, out var reps) ? reps : null;

        /// <summary>
        /// Text for sets entry field (string for Entry binding).
        /// </summary>
        public string SetsText
        {
            get => _setsText;
            set
            {
                if (_setsText != value)
                {
                    _setsText = value;
                    OnPropertyChanged(nameof(SetsText));
                }
            }
        }

        /// <summary>
        /// Parsed sets value for validation. Null if empty or invalid.
        /// </summary>
        public int? Sets => int.TryParse(_setsText, out var sets) ? sets : null;

        /// <summary>
        /// Expected number of repetitions (from exercise data, read-only).
        /// </summary>
        public int ExpectedReps
        {
            get => _expectedReps;
            set
            {
                if (_expectedReps != value)
                {
                    _expectedReps = value;
                    OnPropertyChanged(nameof(ExpectedReps));
                }
            }
        }

        /// <summary>
        /// Expected number of sets (from exercise data, read-only).
        /// </summary>
        public int ExpectedSets
        {
            get => _expectedSets;
            set
            {
                if (_expectedSets != value)
                {
                    _expectedSets = value;
                    OnPropertyChanged(nameof(ExpectedSets));
                }
            }
        }

        /// <summary>
        /// Discomfort level (0-10).
        /// </summary>
        public double Discomfort
        {
            get => _discomfort;
            set
            {
                if (Math.Abs(_discomfort - value) > 0.01)
                {
                    _discomfort = value;
                    OnPropertyChanged(nameof(Discomfort));
                    OnPropertyChanged(nameof(DiscomfortText));
                }
            }
        }

        /// <summary>
        /// Formatted discomfort text for display (e.g., "4/10").
        /// </summary>
        public string DiscomfortText => $"{(int)Math.Round(Discomfort)}/10";

        /// <summary>
        /// User comments about the exercise.
        /// </summary>
        public string Comments
        {
            get => _comments;
            set
            {
                if (_comments != value)
                {
                    _comments = value;
                    OnPropertyChanged(nameof(Comments));
                }
            }
        }

        /// <summary>
        /// Whether the page is in a loading/saving state.
        /// Named IsPageBusy to avoid shadowing base Page.IsBusy.
        /// </summary>
        public bool IsPageBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsPageBusy));
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to navigate to the Information page (via header icon).
        /// </summary>
        public ICommand InfoCommand { get; }

        /// <summary>
        /// Command to navigate back to the previous page.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Command to save progress and navigate back.
        /// </summary>
        public ICommand SaveCommand { get; }

        #endregion

        #region Constructor

        public ExerciseProgressPage(PhysioContentService physioContentService)
        {
            _physioContentService = physioContentService;

            // Initialize commands
            InfoCommand = new Command(async () => await OnInfoAsync());
            BackCommand = new Command(async () => await OnBackAsync());
            SaveCommand = new Command(async () => await OnSaveAsync());

            BindingContext = this;
            InitializeComponent();
        }

        #endregion

        #region Lifecycle

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _cts?.Cancel();
        }

        #endregion

        #region Entry Focus Handlers

        /// <summary>
        /// Handles tap on reps entry border to ensure focus and keyboard appears.
        /// </summary>
        private void OnRepsEntryTapped(object? sender, EventArgs e)
        {
            repsEntry.Focus();
        }

        /// <summary>
        /// Handles tap on sets entry border to ensure focus and keyboard appears.
        /// </summary>
        private void OnSetsEntryTapped(object? sender, EventArgs e)
        {
            setsEntry.Focus();
        }

        #endregion

        #region Data Loading

        /// <summary>
        /// Loads the exercise title from PhysioContentService.
        /// </summary>
        private async Task LoadExerciseTitleAsync()
        {
            if (string.IsNullOrEmpty(_exerciseKey))
                return;

            try
            {
                var exercise = await _physioContentService.GetExerciseByKeyAsync(_exerciseKey, _cts?.Token ?? default);
                if (exercise is not null)
                {
                    ExerciseTitle = StringHelper.ToTitleCase(exercise.Title);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading exercise title: {ex.Message}");
            }
        }

        #endregion

        #region Command Handlers

        /// <summary>
        /// Navigates to the Information page.
        /// </summary>
        private async Task OnInfoAsync()
        {
            await Shell.Current.GoToAsync(nameof(InformationPage));
        }

        /// <summary>
        /// Navigates back to the previous page.
        /// </summary>
        private async Task OnBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        /// <summary>
        /// Saves progress and navigates back to the main exercises page.
        /// Validates that reps, sets, and discomfort are filled in.
        /// Shows a loading spinner during the save operation.
        /// </summary>
        private async Task OnSaveAsync()
        {
            if (IsPageBusy) return;

            // Validate mandatory fields
            if (Reps is null || Sets is null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Required Fields",
                    "Please enter the number of reps and sets completed.",
                    "OK");
                return;
            }

            if (Discomfort <= 0)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Required Fields",
                    "Please indicate the discomfort level using the slider.",
                    "OK");
                return;
            }

            try
            {
                IsPageBusy = true;

                // Simulate save operation with brief delay
                await Task.Delay(300);

                // TODO: Implement actual persistence
                // - Save discomfort level and comments
                // - Update progress tracking

                // Navigate back to MainExercisesPage with completed exercise key
                await Shell.Current.GoToAsync($"../../?completedExerciseKey={_exerciseKey}");
            }
            finally
            {
                IsPageBusy = false;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        // ContentPage already implements INotifyPropertyChanged via BindableObject.
        // Use base.OnPropertyChanged(propertyName) in property setters.

        #endregion
    }
}
