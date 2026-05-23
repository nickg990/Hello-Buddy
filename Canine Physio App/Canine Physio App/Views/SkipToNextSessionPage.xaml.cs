using System.ComponentModel;
using System.Windows.Input;
using Canine_Physio_App.Services;

namespace Canine_Physio_App.Views
{
    /// <summary>
    /// Context-aware skip page:
    /// - Exercise mode: when SessionStateService.CurrentExerciseKey is set
    ///   (user came from ExerciseDetailPage), skips that exercise.
    /// - Session mode: when no exercise context exists
    ///   (user came from MainExercisesPage), skips the whole session.
    /// Uses SessionStateService for session skip persistence.
    /// </summary>
    public partial class SkipToNextSessionPage : ContentPage
    {
        #region Private Fields

        private readonly SessionStateService _sessionStateService;
        private readonly PhysioContentService _physioContentService;
        private string _comments = string.Empty;
        private bool _isBusy;
        private bool _isConfirmEnabled = true;
        private string _statusMessage = string.Empty;
        private string _pageTitle = "Skip to Next Session";
        private string _commentsPlaceholder = "Why are you skipping this session?";
        private bool _isExerciseSkipMode;
        private string? _exerciseKeyToSkip;
        private CancellationTokenSource? _cts;

        #endregion

        #region Properties

        /// <summary>
        /// Optional comments explaining why the user is skipping this session.
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
        /// Indicates whether an operation is in progress (shows loading overlay).
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

        /// <summary>
        /// Whether the CONFIRM button is enabled.
        /// Disabled when the active session already targets tomorrow.
        /// </summary>
        public bool IsConfirmEnabled
        {
            get => _isConfirmEnabled;
            set
            {
                if (_isConfirmEnabled != value)
                {
                    _isConfirmEnabled = value;
                    OnPropertyChanged(nameof(IsConfirmEnabled));
                }
            }
        }

        /// <summary>
        /// Status message shown when skip is unavailable.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                    OnPropertyChanged(nameof(HasStatusMessage));
                }
            }
        }

        /// <summary>
        /// Whether the status message area (with revert button) should be visible.
        /// Only relevant in session-skip mode.
        /// </summary>
        public bool HasStatusMessage => !string.IsNullOrEmpty(_statusMessage) && !_isExerciseSkipMode;

        /// <summary>
        /// The page title shown in the header.
        /// Changes based on skip mode.
        /// </summary>
        public string PageTitle
        {
            get => _pageTitle;
            set
            {
                if (_pageTitle != value)
                {
                    _pageTitle = value;
                    OnPropertyChanged(nameof(PageTitle));
                }
            }
        }

        /// <summary>
        /// Placeholder text for the comments editor.
        /// </summary>
        public string CommentsPlaceholder
        {
            get => _commentsPlaceholder;
            set
            {
                if (_commentsPlaceholder != value)
                {
                    _commentsPlaceholder = value;
                    OnPropertyChanged(nameof(CommentsPlaceholder));
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to navigate back to the previous page.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Command to confirm skipping and navigate to the next session.
        /// </summary>
        public ICommand ConfirmCommand { get; }

        /// <summary>
        /// Command to revert an active skip targeting tomorrow.
        /// </summary>
        public ICommand RevertSkipCommand { get; }

        #endregion

        #region Constructor

        public SkipToNextSessionPage(SessionStateService sessionStateService,
            PhysioContentService physioContentService)
        {
            _sessionStateService = sessionStateService;
            _physioContentService = physioContentService;

            // Initialize commands BEFORE setting BindingContext
            BackCommand = new Command(async () => await NavigateBackAsync());
            ConfirmCommand = new Command(async () => await ConfirmSkipAsync());
            RevertSkipCommand = new Command(async () => await RevertSkipAsync());

            BindingContext = this;
            InitializeComponent();
        }

        #endregion

        #region Lifecycle

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            // Determine mode based on exercise context
            _exerciseKeyToSkip = _sessionStateService.CurrentExerciseKey;
            _isExerciseSkipMode = !string.IsNullOrEmpty(_exerciseKeyToSkip);

            try
            {
                if (_isExerciseSkipMode)
                {
                    await ConfigureExerciseSkipMode();
                }
                else
                {
                    ConfigureSessionSkipMode();
                    await EvaluateSkipAvailability();
                }
            }
            catch (OperationCanceledException)
            {
                // Page left before data loaded; state will refresh on next appearance
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _cts?.Cancel();
        }

        /// <summary>
        /// Configures the page for exercise-skip mode.
        /// Loads the exercise title and updates hero text.
        /// </summary>
        private async Task ConfigureExerciseSkipMode()
        {
            PageTitle = "Skip Exercise";
            CommentsPlaceholder = "Why are you skipping this exercise?";
            IsConfirmEnabled = true;
            StatusMessage = string.Empty;

            heroMainText.Text = "Skipping this exercise will mark it as incomplete.";
            heroSubText.Text = "You can optionally add comments below to explain why you're skipping.";

            // Try to load exercise title for the header
            try
            {
                var exercise = await _physioContentService.GetExerciseByKeyAsync(_exerciseKeyToSkip!, _cts?.Token ?? default);
                if (exercise is not null)
                {
                    PageTitle = Helpers.StringHelper.ToTitleCase(exercise.Title);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading exercise title: {ex.Message}");
            }
        }

        /// <summary>
        /// Configures the page for session-skip mode (default).
        /// </summary>
        private void ConfigureSessionSkipMode()
        {
            PageTitle = "Skip to Next Session";
            CommentsPlaceholder = "Why are you skipping this session?";

            heroMainText.Text = "Skipping this session will mark all missed exercises as incomplete.";
            heroSubText.Text = "You can optionally add comments below to explain why you're skipping.";
        }

        /// <summary>
        /// Checks whether skipping is currently allowed and updates the CONFIRM button state.
        /// Skip is disabled when the active session already targets a future date.
        /// </summary>
        private async Task EvaluateSkipAvailability()
        {
            var programme = await _physioContentService.GetProgrammeAsync(_cts?.Token ?? default);
            if (programme is null || programme.DailySessions.Count == 0)
            {
                IsConfirmEnabled = false;
                StatusMessage = "No sessions available.";
                return;
            }

            var dailySessions = programme.DailySessions;
            var (activeDate, _) = _sessionStateService.GetActiveSession(dailySessions);

            if (activeDate > DateTime.Today)
            {
                IsConfirmEnabled = false;
                StatusMessage = $"You've already skipped to {activeDate:dd MMM yyyy}. Revert the skip to re-enable.";
            }
            else
            {
                IsConfirmEnabled = true;
                StatusMessage = string.Empty;
            }
        }

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Navigates back to the tab the user came from.
        /// Falls back to Exercises if no previous tab is recorded.
        /// </summary>
        private async Task NavigateBackAsync()
        {
            // Switch tabs via CurrentItem to preserve the source tab's navigation stack.
            // Set flag so AppShell.OnNavigated does NOT pop the Exercises stack.
            var section = AppShell.PreviousTabSection;
            if (section != null && Shell.Current.CurrentItem is TabBar tabBar)
            {
                AppShell.PreserveExercisesStack = true;
                tabBar.CurrentItem = section;
            }
            else
            {
                await Shell.Current.GoToAsync("//MainExercises");
            }
        }

        /// <summary>
        /// Confirms skipping the current session or exercise.
        /// In exercise mode: navigates back to MainExercises with skippedExerciseKey.
        /// In session mode: calls SessionStateService.TrySkip then navigates to Exercises tab.
        /// </summary>
        private async Task ConfirmSkipAsync()
        {
            try
            {
                IsPageBusy = true;

                if (_isExerciseSkipMode && !string.IsNullOrEmpty(_exerciseKeyToSkip))
                {
                    // Exercise skip — navigate back to Exercises with skipped key.
                    // Set flag so AppShell doesn't intercept the absolute-route navigation.
                    _sessionStateService.CurrentExerciseKey = null;
                    AppShell.PreserveExercisesStack = true;
                    await Shell.Current.GoToAsync($"//MainExercises?skippedExerciseKey={_exerciseKeyToSkip}");
                    return;
                }

                // Session skip
                var programme = await _physioContentService.GetProgrammeAsync();
                if (programme is null || programme.DailySessions.Count == 0)
                {
                    await Shell.Current.DisplayAlertAsync("Error", "No sessions available.", "OK");
                    return;
                }

                var dailySessions = programme.DailySessions;
                var (currentDate, currentPeriod) = _sessionStateService.GetActiveSession(dailySessions);
                var result = _sessionStateService.TrySkip(currentDate, currentPeriod, dailySessions);

                if (!result.IsBlocked)
                {
                    // Skip succeeded — navigate to Exercises tab which will detect locked state.
                    // Set flag so AppShell doesn't intercept the absolute-route navigation.
                    AppShell.PreserveExercisesStack = true;
                    await Shell.Current.GoToAsync("//MainExercises");
                }
            }
            finally
            {
                IsPageBusy = false;
            }
        }

        /// <summary>
        /// Reverts an active skip and navigates to Exercises tab showing today's session.
        /// </summary>
        private async Task RevertSkipAsync()
        {
            var programme = await _physioContentService.GetProgrammeAsync();
            if (programme is null || programme.DailySessions.Count == 0) return;

            _sessionStateService.RevertSkip(programme.DailySessions);
            AppShell.PreserveExercisesStack = true;
            await Shell.Current.GoToAsync("//MainExercises");
        }

        #endregion

        #region INotifyPropertyChanged

        // ContentPage already implements INotifyPropertyChanged via BindableObject.
        // Use base.OnPropertyChanged(propertyName) in property setters.

        #endregion
    }
}
