using System.ComponentModel;
using System.Windows.Input;
using Canine_Physio_App.Components.Controls;
using Canine_Physio_App.Helpers;
using Canine_Physio_App.Models;
using Canine_Physio_App.Services;

namespace Canine_Physio_App.Views
{
    /// <summary>
    /// Main Exercises page displaying exercise tiles with progress tracking.
    /// Uses AppPageTemplate with single scroll region and fixed footer buttons.
    /// Manages session date/period, skip-to-next-session, and auto-advance.
    /// </summary>
    [QueryProperty(nameof(CompletedExerciseKey), "completedExerciseKey")]
    [QueryProperty(nameof(SkippedExerciseKey), "skippedExerciseKey")]
    public partial class MainExercisesPage : ContentPage
    {
        #region Private Fields

        private readonly TextContentLoader _contentLoader;
        private readonly PhysioContentService _physioContentService;
        private readonly SessionStateService _sessionStateService;

        private string _exerciseSetDescription = string.Empty;
        private string _warningText = string.Empty;
        private string _sessionDateText = string.Empty;
        private List<ExerciseViewModel> _exercises = new();
        private readonly HashSet<string> _completedExerciseKeys = new();
        private readonly HashSet<string> _skippedExerciseKeys = new();
        private double _progressBarMaxWidth = 280;
        private bool _isNavigating;
        private bool _contentLoaded;
        private string? _pendingCompletedKey;
        private string? _pendingSkippedKey;
        private CancellationTokenSource? _cts;

        // Current active session state
        private DateTime _activeDate;
        private string _activePeriod = string.Empty;
        private List<DailySession> _dailySessions = new();
        private bool _isSessionLocked;

        /// <summary>Combined horizontal insets for progress bar width (page padding + card padding on each side).</summary>
        private const double ProgressBarPaddingOffset = 80;

        #endregion

        #region Query Properties

        /// <summary>
        /// Exercise key marked as complete from ExerciseProgressPage.
        /// </summary>
        public string? CompletedExerciseKey
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Store for processing in OnAppearing
                    _pendingCompletedKey = value;
                }
            }
        }

        /// <summary>
        /// Exercise key marked as skipped from SkipExercisePage.
        /// </summary>
        public string? SkippedExerciseKey
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Store for processing in OnAppearing
                    _pendingSkippedKey = value;
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Description of the current exercise set.
        /// </summary>
        public string ExerciseSetDescription
        {
            get => _exerciseSetDescription;
            set
            {
                if (_exerciseSetDescription != value)
                {
                    _exerciseSetDescription = value;
                    OnPropertyChanged(nameof(ExerciseSetDescription));
                }
            }
        }

        /// <summary>
        /// Warning/disclaimer text shown to users.
        /// </summary>
        public string WarningText
        {
            get => _warningText;
            set
            {
                if (_warningText != value)
                {
                    _warningText = value;
                    OnPropertyChanged(nameof(WarningText));
                }
            }
        }

        /// <summary>
        /// Session date/period label displayed above the exercise tiles.
        /// Format: "Session: 09 Mar 2026 - AM" or "Session: 09 Mar 2026" for single-session.
        /// </summary>
        public string SessionDateText
        {
            get => _sessionDateText;
            set
            {
                if (_sessionDateText != value)
                {
                    _sessionDateText = value;
                    OnPropertyChanged(nameof(SessionDateText));
                }
            }
        }

        /// <summary>
        /// Whether the page is navigating (shows loading overlay).
        /// </summary>
        public bool IsNavigating
        {
            get => _isNavigating;
            set
            {
                if (_isNavigating != value)
                {
                    _isNavigating = value;
                    OnPropertyChanged(nameof(IsNavigating));
                }
            }
        }

        /// <summary>
        /// Number of completed exercises.
        /// </summary>
        public int CompletedCount => _exercises.Count(e => e.IsComplete);

        /// <summary>
        /// Total number of exercises.
        /// </summary>
        public int TotalCount => _exercises.Count;

        /// <summary>
        /// Progress as a percentage (0-100).
        /// </summary>
        public double ProgressPercentage => TotalCount > 0 ? (double)CompletedCount / TotalCount * 100 : 0;

        /// <summary>
        /// Progress percentage formatted for display.
        /// </summary>
        public string ProgressPercentageText => $"{ProgressPercentage:0}%";

        /// <summary>
        /// Width of the progress bar fill (in device-independent pixels).
        /// </summary>
        public double ProgressBarWidth => TotalCount > 0 ? (_progressBarMaxWidth * CompletedCount / TotalCount) : 0;

        #endregion

        #region Commands

        /// <summary>
        /// Command to navigate to the progress details page.
        /// </summary>
        public ICommand NavigateToProgressCommand { get; }

        /// <summary>
        /// Command to navigate to the information page (via header icon).
        /// </summary>
        public ICommand NavigateToInfoCommand { get; }

        /// <summary>
        /// Command to navigate to an exercise detail page.
        /// </summary>
        public ICommand NavigateToExerciseCommand { get; }

        #endregion

        #region Constructor

        public MainExercisesPage(TextContentLoader contentLoader,
            PhysioContentService physioContentService,
            SessionStateService sessionStateService)
        {
            _contentLoader = contentLoader;
            _physioContentService = physioContentService;
            _sessionStateService = sessionStateService;

            // Initialize commands
            NavigateToProgressCommand = new Command(async () => await OnNavigateToProgress());
            NavigateToInfoCommand = new Command(async () => await OnNavigateToInfo());
            NavigateToExerciseCommand = new Command<string>(async (key) => await OnNavigateToExercise(key));

            BindingContext = this;
            InitializeComponent();
        }

        #endregion

        #region Lifecycle

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            if (Handler != null)
            {
                SizeChanged += OnSizeChanged;
                pageTemplate.PropertyChanged += OnTemplatePropertyChanged;
            }
        }

        protected override void OnHandlerChanging(HandlerChangingEventArgs args)
        {
            base.OnHandlerChanging(args);
            if (args.OldHandler != null)
            {
                SizeChanged -= OnSizeChanged;
                pageTemplate.PropertyChanged -= OnTemplatePropertyChanged;
            }
        }

        /// <summary>
        /// Update progress bar width when size changes (replaces OnSizeAllocated).
        /// </summary>
        private void OnSizeChanged(object? sender, EventArgs e)
        {
            if (Width > 0 && !double.IsNaN(Width))
            {
                _progressBarMaxWidth = Math.Max(100, Width - ProgressBarPaddingOffset);
                OnPropertyChanged(nameof(ProgressBarWidth));
            }
        }

        /// <summary>
        /// Reacts to AppPageTemplate responsive property changes to update hero styling.
        /// </summary>
        private void OnTemplatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsTablet")
            {
                ApplyResponsiveHeroStyles();
            }
        }

        /// <summary>
        /// Load content when page appears.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            // Reset navigation state when returning to page
            IsNavigating = false;

            // Clear exercise context — no longer viewing an individual exercise
            _sessionStateService.CurrentExerciseKey = null;

            // Process any pending completion from ExerciseProgressPage
            if (!string.IsNullOrEmpty(_pendingCompletedKey))
            {
                MarkExerciseComplete(_pendingCompletedKey);
                _pendingCompletedKey = null;
            }

            // Process any pending skip from SkipExercisePage
            if (!string.IsNullOrEmpty(_pendingSkippedKey))
            {
                MarkExerciseSkipped(_pendingSkippedKey);
                _pendingSkippedKey = null;
            }

            try
            {
                if (_contentLoaded) 
                {
                    // Re-evaluate active session (may have changed via Skip tab or calendar)
                    await RefreshActiveSession();
                    return;
                }

                await LoadContentAsync();
                _contentLoaded = true;
            }
            catch (OperationCanceledException)
            {
                // Page left during async load; will retry on next appearance
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _cts?.Cancel();
        }

        #endregion

        #region Content Loading

        /// <summary>
        /// Loads exercise content, resolves active session, and updates UI.
        /// </summary>
        private async Task LoadContentAsync()
        {
            try
            {
                // Load programme data
                var programme = await _physioContentService.GetProgrammeAsync(_cts?.Token ?? default);

                if (programme is null || programme.DailySessions.Count == 0)
                {
                    ExerciseSetDescription = "No exercises available.";
                    return;
                }

                // Store daily sessions for skip logic
                _dailySessions = programme.DailySessions;

                // Resolve warning text
                if (!string.IsNullOrEmpty(programme.WarningText))
                {
                    WarningText = programme.WarningText;
                }
                else
                {
                    WarningText = await _contentLoader.GetWarningAsync("exerciseDisclaimer", _cts?.Token ?? default);
                }

                // Resolve and load the active session
                await RefreshActiveSession();
            }
            catch (OperationCanceledException)
            {
                throw; // Let caller handle cancellation
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainExercisesPage: Error loading content - {ex.Message}");
            }
        }

        /// <summary>
        /// Re-evaluates the active session from SessionStateService and reloads
        /// if the date/period has changed (e.g., after a skip or calendar catch-up).
        /// </summary>
        private async Task RefreshActiveSession()
        {
            var (activeDate, activePeriod) = _sessionStateService.GetActiveSession(_dailySessions);

            // No change — skip reload to avoid re-triggering auto-advance alerts
            if (activeDate == _activeDate && activePeriod == _activePeriod)
                return;

            _activeDate = activeDate;
            _activePeriod = activePeriod;

            if (_activeDate > DateTime.Today)
            {
                await ShowLockedState();
            }
            else
            {
                _isSessionLocked = false;
                await LoadSessionExercises(_activePeriod);
            }

            UpdateSessionDateLabel();
        }

        /// <summary>
        /// Shows a locked state when the active session is in the future.
        /// Loads exercise tiles visually but blocks navigation — tapping any tile
        /// shows a revert popup instead of opening the exercise.
        /// </summary>
        private async Task ShowLockedState()
        {
            _isSessionLocked = true;
            await LoadSessionExercises(_activePeriod);
        }

        /// <summary>
        /// Loads exercises for a given session period and rebuilds the UI.
        /// </summary>
        private async Task LoadSessionExercises(string period)
        {
            var exerciseSet = await _physioContentService.GetExerciseSetByPeriodAsync(period, _cts?.Token ?? default);

            if (exerciseSet is null)
            {
                ExerciseSetDescription = "No exercises available.";
                return;
            }

            ExerciseSetDescription = exerciseSet.Description;

            // Map exercises to view models using compound completion keys
            _exercises = exerciseSet.Exercises.Select(e =>
            {
                var compoundKey = GetCompletionKey(_activeDate, _activePeriod, e.Key);
                return new ExerciseViewModel
                {
                    Key = e.Key,
                    Name = e.Name,
                    Image = e.Image,
                    IsComplete = _completedExerciseKeys.Contains(compoundKey),
                    IsSkipped = _skippedExerciseKeys.Contains(compoundKey)
                };
            }).ToList();

            BuildExerciseGrid();
            UpdateProgress();
        }

        /// <summary>
        /// Updates the session date label text.
        /// Format: "Session: 09 Mar 2026 - AM" or "Next session: 10 Mar 2026" when locked.
        /// </summary>
        private void UpdateSessionDateLabel()
        {
            var dateStr = _activeDate.ToString("dd MMM yyyy");
            var isMultiSession = _dailySessions.Count > 1;
            var prefix = _isSessionLocked ? "Next session" : "Session";

            SessionDateText = isMultiSession && !string.IsNullOrEmpty(_activePeriod)
                ? $"{prefix}: {dateStr} - {_activePeriod}"
                : $"{prefix}: {dateStr}";
        }

        /// <summary>
        /// Creates a compound key for tracking exercise completion scoped to date+period.
        /// Format: "2026-03-09_AM_baitedBackStretch"
        /// </summary>
        private static string GetCompletionKey(DateTime date, string period, string exerciseKey)
        {
            return $"{date:yyyy-MM-dd}_{period}_{exerciseKey}";
        }

        /// <summary>
        /// Builds the exercise tile grid dynamically.
        /// </summary>
        private void BuildExerciseGrid()
        {
            ExerciseGrid.Children.Clear();
            ExerciseGrid.RowDefinitions.Clear();

            // Calculate number of rows needed (2 columns)
            int rowCount = (int)Math.Ceiling(_exercises.Count / 2.0);
            for (int i = 0; i < rowCount; i++)
            {
                ExerciseGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Add tiles to grid
            for (int i = 0; i < _exercises.Count; i++)
            {
                var exercise = _exercises[i];
                var tile = new ExerciseTile
                {
                    ExerciseKey = exercise.Key,
                    ExerciseName = exercise.Name,
                    ImageSource = exercise.Image + ".jpg",
                    IsComplete = exercise.IsComplete,
                    IsSkipped = exercise.IsSkipped,
                    Command = NavigateToExerciseCommand
                };

                int row = i / 2;
                int column = i % 2;

                Grid.SetRow(tile, row);
                Grid.SetColumn(tile, column);
                ExerciseGrid.Children.Add(tile);
            }
        }

        /// <summary>
        /// Updates progress-related property bindings.
        /// </summary>
        private void UpdateProgress()
        {
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(ProgressPercentage));
            OnPropertyChanged(nameof(ProgressPercentageText));
            OnPropertyChanged(nameof(ProgressBarWidth));
        }

        /// <summary>
        /// Marks an exercise as complete and refreshes the UI.
        /// Called when returning from ExerciseProgressPage after saving.
        /// If the exercise was previously skipped, the skip is removed (cross → tick).
        /// If all exercises in the session are complete or skipped, auto-advances to next session.
        /// </summary>
        /// <param name="exerciseKey">The key of the completed exercise.</param>
        public void MarkExerciseComplete(string exerciseKey)
        {
            if (string.IsNullOrEmpty(exerciseKey)) return;

            // Add to completed set using compound key
            var completionKey = GetCompletionKey(_activeDate, _activePeriod, exerciseKey);
            _completedExerciseKeys.Add(completionKey);

            // Remove from skipped set if previously skipped (cross → tick)
            _skippedExerciseKeys.Remove(completionKey);

            // Update the view model
            var exercise = _exercises.FirstOrDefault(e => e.Key == exerciseKey);
            if (exercise != null)
            {
                exercise.IsComplete = true;
                exercise.IsSkipped = false;
            }

            // Refresh the grid to show the tick
            BuildExerciseGrid();
            UpdateProgress();

            // Auto-advance if all exercises in this session are complete or skipped
            if (_exercises.Count > 0 && _exercises.All(e => e.IsComplete || e.IsSkipped))
            {
                _ = AutoAdvanceToNextSession();
            }
        }

        /// <summary>
        /// Marks an exercise as skipped and refreshes the UI.
        /// Called when returning from SkipExercisePage after confirming.
        /// If all exercises in the session are complete or skipped, auto-advances to next session.
        /// </summary>
        /// <param name="exerciseKey">The key of the skipped exercise.</param>
        public void MarkExerciseSkipped(string exerciseKey)
        {
            if (string.IsNullOrEmpty(exerciseKey)) return;

            // Add to skipped set using compound key
            var skippedKey = GetCompletionKey(_activeDate, _activePeriod, exerciseKey);
            _skippedExerciseKeys.Add(skippedKey);

            // Update the view model
            var exercise = _exercises.FirstOrDefault(e => e.Key == exerciseKey);
            if (exercise != null)
            {
                exercise.IsSkipped = true;
            }

            // Refresh the grid to show the red X
            BuildExerciseGrid();
            UpdateProgress();

            // Auto-advance if all exercises in this session are complete or skipped
            if (_exercises.Count > 0 && _exercises.All(e => e.IsComplete || e.IsSkipped))
            {
                _ = AutoAdvanceToNextSession();
            }
        }

        /// <summary>
        /// Auto-advances to the next session when the current one is fully complete.
        /// Persists the advance via SessionStateService so it survives tab switches.
        /// AM → PM (same day), PM → done for today, single-session → done for today.
        /// </summary>
        private async Task AutoAdvanceToNextSession()
        {
            var isMultiSession = _dailySessions.Count > 1;

            if (isMultiSession
                && _activePeriod.Equals("AM", StringComparison.OrdinalIgnoreCase)
                && _activeDate == DateTime.Today)
            {
                // AM complete → persist advance to PM via session state service
                var result = _sessionStateService.TrySkip(_activeDate, _activePeriod, _dailySessions);

                if (!result.IsBlocked)
                {
                    _activeDate = result.TargetDate;
                    _activePeriod = result.TargetPeriod;

                    await LoadSessionExercises(_activePeriod);
                    UpdateSessionDateLabel();
                }

                await Shell.Current.DisplayAlertAsync(
                    "Session Complete",
                    "Well done! Loading your afternoon exercises.",
                    "OK");
            }
            else
            {
                // PM complete or single-session complete → done for today
                await Shell.Current.DisplayAlertAsync(
                    "All Done!",
                    "Great work! You've completed all exercises for today.",
                    "OK");
            }
        }

        #endregion

        #region Command Handlers

        private async Task OnNavigateToProgress()
        {
            // TODO: Navigate to progress detail page when implemented
            await Shell.Current.DisplayAlertAsync("Coming Soon", "Progress page is under development.", "OK");
        }

        private async Task OnNavigateToInfo()
        {
            await Shell.Current.GoToAsync(nameof(InformationPage));
        }

        private async Task OnNavigateToExercise(string exerciseKey)
        {
            // Prevent double-tap
            if (IsNavigating)
                return;

            // If session is locked (future date), show revert popup instead of navigating
            if (_isSessionLocked)
            {
                var revert = await Shell.Current.DisplayAlertAsync(
                    "Session Not Available",
                    $"These exercises aren't available until {_activeDate:dd MMM yyyy}. Would you like to revert the skip and return to today's exercises?",
                    "Revert Skip",
                    "OK");

                if (revert)
                {
                    var (date, period) = _sessionStateService.RevertSkip(_dailySessions);
                    _activeDate = date;
                    _activePeriod = period;
                    _isSessionLocked = false;

                    // Clear all completion/skip ticks so the reverted session starts fresh
                    _completedExerciseKeys.Clear();
                    _skippedExerciseKeys.Clear();

                    await LoadSessionExercises(_activePeriod);
                    UpdateSessionDateLabel();
                }
                return;
            }

            // Show loading overlay immediately
            IsNavigating = true;

            // Yield to UI thread to render spinner before navigation starts
            await Task.Yield();

            try
            {
                // Navigate to ExerciseDetailPage with exerciseKey parameter
                await Shell.Current.GoToAsync($"{nameof(ExerciseDetailPage)}?exerciseKey={exerciseKey}");
            }
            finally
            {
                // Spinner will be hidden in OnAppearing when returning
                IsNavigating = false;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        // ContentPage already implements INotifyPropertyChanged via BindableObject.
        // Use base.OnPropertyChanged(propertyName) in property setters.

        #endregion

        #region Responsive Hero Styling (Issue #12)

        /// <summary>
        /// Applies responsive sizing to hero card elements based on screen width category.
        /// Replaces duplicate phone/tablet XAML layouts with a single adaptive layout.
        /// </summary>
        private void ApplyResponsiveHeroStyles()
        {
            if (heroCard == null) return;

            bool isTablet = pageTemplate.IsTablet;

            heroCard.Padding = isTablet ? GetResourceThickness("SpaceCardPadding") : new Thickness(12);
            heroStack.Spacing = GetResourceDouble(isTablet ? "SpaceStackSm" : "Space4");
            progressGrid.RowSpacing = GetResourceDouble(isTablet ? "Space4" : "Space2");

            descriptionLabel.FontSize = GetResourceDouble(isTablet ? "TextMd" : "TextSm");
            progressLabel.FontSize = GetResourceDouble(isTablet ? "TextSm" : "TextXs");
            warningLabel.FontSize = GetResourceDouble(isTablet ? "TextSm" : "TextXs");
        }

        private static double GetResourceDouble(string key)
        {
            if (Application.Current?.Resources.TryGetValue(key, out var value) == true)
            {
                if (value is double d) return d;
                if (value is OnIdiom<double> idiom) return idiom;
            }
            return 0;
        }

        private static Thickness GetResourceThickness(string key)
        {
            if (Application.Current?.Resources.TryGetValue(key, out var value) == true)
            {
                if (value is Thickness t) return t;
                if (value is OnIdiom<Thickness> idiom) return idiom;
            }
            return new Thickness(0);
        }

        #endregion
    }
}
