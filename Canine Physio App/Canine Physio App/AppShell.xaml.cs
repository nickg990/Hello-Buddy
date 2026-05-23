using Canine_Physio_App.Services;
using Canine_Physio_App.Views;

namespace Canine_Physio_App
{
    public partial class AppShell : Shell
    {
        /// <summary>Tracks the previously active tab section for stack cleanup.</summary>
        private ShellSection? _previousSection;

        /// <summary>Lazy-resolved reference to the session state service.</summary>
        private SessionStateService? _sessionStateService;

        /// <summary>
        /// The route of the tab the user was on before switching to the current tab.
        /// Used as a fallback when the section reference is unavailable.
        /// </summary>
        public static string? PreviousTabRoute { get; private set; }

        /// <summary>
        /// The actual ShellSection (Tab) the user was on before switching.
        /// Setting CurrentItem to this preserves the tab's navigation stack.
        /// </summary>
        public static ShellSection? PreviousTabSection { get; private set; }

        /// <summary>
        /// When true, the next tab switch will NOT pop the Exercises tab stack.
        /// Set by BACK buttons before switching tabs so the user returns to
        /// the exact child page they were on.
        /// </summary>
        public static bool PreserveExercisesStack { get; set; }

        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute(nameof(InformationPage), typeof(InformationPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute("StyleGuide", typeof(StyleGuidePage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(ForgottenPasswordPage), typeof(ForgottenPasswordPage));
            Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
            Routing.RegisterRoute(nameof(TermsConditionsPage), typeof(TermsConditionsPage));
            Routing.RegisterRoute(nameof(ExerciseDetailPage), typeof(ExerciseDetailPage));
            Routing.RegisterRoute(nameof(ExerciseProgressPage), typeof(ExerciseProgressPage));

            _previousSection = CurrentItem?.CurrentItem;
        }

        /// <summary>
        /// When the user switches tabs, captures the previous tab route,
        /// manages tab bar visibility, skip icon, and pops stale child
        /// pages from the Exercises tab when arriving at it directly.
        /// </summary>
        protected override async void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            var location = args.Current?.Location?.OriginalString ?? string.Empty;
            var isOnProgressTab = location.Contains("ProgressTab");

            // Swap Skip tab icon based on exercise context:
            // >  (tab_skip_exercise) when an exercise is active (ExerciseDetailPage or exercise-skip page)
            // >> (tab_skip) otherwise — indicates session skip
            _sessionStateService ??= Handler?.MauiContext?.Services.GetService<SessionStateService>();
            var hasExerciseContext = !string.IsNullOrEmpty(_sessionStateService?.CurrentExerciseKey);
            skipTab.Icon = hasExerciseContext
                ? "tab_skip_exercise.svg"
                : "tab_skip.svg";

            // Hide the entire tab bar on ProgressTabPage.
            // ProgressTabPage is a tab root that manages its own full-screen
            // layout; the tab bar is hidden via XAML + FlyoutItemIsVisible.
            if (isOnProgressTab)
            {
                foreach (var item in CurrentItem.Items)
                {
                    item.FlyoutItemIsVisible = false;
                }
                Shell.SetTabBarIsVisible(CurrentPage, false);
            }
            else
            {
                foreach (var item in CurrentItem.Items)
                {
                    item.FlyoutItemIsVisible = true;
                }
            }

            if (args.Source == ShellNavigationSource.ShellSectionChanged)
            {
                // Capture the previous tab's route for back navigation
                if (_previousSection != null)
                {
                    PreviousTabSection = _previousSection;
                    var route = _previousSection.Route;
                    if (!string.IsNullOrEmpty(route))
                    {
                        PreviousTabRoute = $"//{route}";
                    }
                }

                // Pop stale child pages when arriving at the Exercises tab
                // via a direct tab tap (not via a BACK button).
                // The pop runs AFTER arrival so the tab's navigation container
                // is active on-screen, ensuring PopToRootAsync works reliably.
                if (!PreserveExercisesStack)
                {
                    var currentSection = CurrentItem?.CurrentItem;
                    if (currentSection == exercisesTab)
                    {
                        var nav = currentSection.Navigation;
                        if (nav?.NavigationStack?.Count > 1)
                        {
                            await nav.PopToRootAsync(false);
                        }
                    }
                }
                PreserveExercisesStack = false;
            }

            _previousSection = CurrentItem?.CurrentItem;
        }
    }
}
