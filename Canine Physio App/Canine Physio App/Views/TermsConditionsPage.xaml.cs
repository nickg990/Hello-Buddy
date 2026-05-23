using System.ComponentModel;
using System.Windows.Input;
using Canine_Physio_App.Helpers;
using Canine_Physio_App.Services;

namespace Canine_Physio_App.Views
{
    /// <summary>
    /// Terms and Conditions page requiring user acceptance before proceeding.
    /// Features collapsible sections for legal content.
    /// </summary>
    public partial class TermsConditionsPage : ContentPage
    {
        private readonly TextContentLoader _contentLoader;
        private readonly PhysioContentService _physioContentService;
        private bool _contentLoaded;
        private CancellationTokenSource? _cts;

        private bool _isAccepted;
        private bool _isLoading;
        private bool _isTermsExpanded;
        private bool _isPrivacyExpanded;
        private bool _isAcceptableUseExpanded;

        private string _termsHeader = "TERMS OF SERVICE";
        private string _termsBody = string.Empty;
        private string _privacyHeader = "PRIVACY POLICY";
        private string _privacyBody = string.Empty;
        private string _acceptableUseHeader = "ACCEPTABLE USE POLICY";
        private string _acceptableUseBody = string.Empty;

        /// <summary>
        /// Whether the user has accepted the terms.
        /// </summary>
        public bool IsAccepted
        {
            get => _isAccepted;
            set
            {
                if (_isAccepted != value)
                {
                    _isAccepted = value;
                    OnPropertyChanged(nameof(IsAccepted));
                }
            }
        }

        /// <summary>
        /// Whether the page is currently loading/transitioning.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        /// <summary>
        /// Whether the Terms of Service section is expanded.
        /// </summary>
        public bool IsTermsExpanded
        {
            get => _isTermsExpanded;
            set
            {
                if (_isTermsExpanded != value)
                {
                    _isTermsExpanded = value;
                    OnPropertyChanged(nameof(IsTermsExpanded));
                    OnPropertyChanged(nameof(TermsExpanderIcon));
                }
            }
        }

        /// <summary>
        /// Whether the Privacy Policy section is expanded.
        /// </summary>
        public bool IsPrivacyExpanded
        {
            get => _isPrivacyExpanded;
            set
            {
                if (_isPrivacyExpanded != value)
                {
                    _isPrivacyExpanded = value;
                    OnPropertyChanged(nameof(IsPrivacyExpanded));
                    OnPropertyChanged(nameof(PrivacyExpanderIcon));
                }
            }
        }

        /// <summary>
        /// Whether the Acceptable Use Policy section is expanded.
        /// </summary>
        public bool IsAcceptableUseExpanded
        {
            get => _isAcceptableUseExpanded;
            set
            {
                if (_isAcceptableUseExpanded != value)
                {
                    _isAcceptableUseExpanded = value;
                    OnPropertyChanged(nameof(IsAcceptableUseExpanded));
                    OnPropertyChanged(nameof(AcceptableUseExpanderIcon));
                }
            }
        }

        // Section headers
        public string TermsHeader
        {
            get => _termsHeader;
            set { _termsHeader = value; OnPropertyChanged(nameof(TermsHeader)); }
        }

        public string TermsBody
        {
            get => _termsBody;
            set { _termsBody = value; OnPropertyChanged(nameof(TermsBody)); }
        }

        public string PrivacyHeader
        {
            get => _privacyHeader;
            set { _privacyHeader = value; OnPropertyChanged(nameof(PrivacyHeader)); }
        }

        public string PrivacyBody
        {
            get => _privacyBody;
            set { _privacyBody = value; OnPropertyChanged(nameof(PrivacyBody)); }
        }

        public string AcceptableUseHeader
        {
            get => _acceptableUseHeader;
            set { _acceptableUseHeader = value; OnPropertyChanged(nameof(AcceptableUseHeader)); }
        }

        public string AcceptableUseBody
        {
            get => _acceptableUseBody;
            set { _acceptableUseBody = value; OnPropertyChanged(nameof(AcceptableUseBody)); }
        }

        // Expander icons (+ when collapsed, − when expanded)
        public string TermsExpanderIcon => IsTermsExpanded ? "−" : "+";
        public string PrivacyExpanderIcon => IsPrivacyExpanded ? "−" : "+";
        public string AcceptableUseExpanderIcon => IsAcceptableUseExpanded ? "−" : "+";

        // Commands
        public ICommand ToggleTermsCommand { get; }
        public ICommand TogglePrivacyCommand { get; }
        public ICommand ToggleAcceptableUseCommand { get; }
        public ICommand NavigateBackCommand { get; }
        public ICommand NavigateNextCommand { get; }

        public TermsConditionsPage(TextContentLoader contentLoader, PhysioContentService physioContentService)
        {
            _contentLoader = contentLoader;
            _physioContentService = physioContentService;

            // Toggle commands for collapsible sections
            ToggleTermsCommand = new Command(() => IsTermsExpanded = !IsTermsExpanded);
            TogglePrivacyCommand = new Command(() => IsPrivacyExpanded = !IsPrivacyExpanded);
            ToggleAcceptableUseCommand = new Command(() => IsAcceptableUseExpanded = !IsAcceptableUseExpanded);

            // Navigation commands
            NavigateBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            NavigateNextCommand = new Command(async () => await OnNavigateNext());

            BindingContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Load content when page appears - more reliable than constructor loading.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            if (_contentLoaded) return;

            // Set flag first to prevent re-entry during async load
            _contentLoaded = true;

            try
            {
                await LoadContentAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                // Reset flag so content load can be retried
                _contentLoaded = false;
            }
        }

        /// <summary>
        /// Loads terms content from the JSON file.
        /// </summary>
        private async Task LoadContentAsync()
        {
            try
            {
                var sections = await _contentLoader.GetSectionsAsync("termsconditions", _cts?.Token ?? default);

                foreach (var section in sections)
                {
                    switch (section.Key.ToLowerInvariant())
                    {
                        case "termsofservice":
                            TermsHeader = section.Header;
                            TermsBody = section.Body;
                            break;
                        case "privacypolicy":
                            PrivacyHeader = section.Header;
                            PrivacyBody = section.Body;
                            break;
                        case "acceptableuse":
                            AcceptableUseHeader = section.Header;
                            AcceptableUseBody = section.Body;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading T&C content: {ex.Message}");
                throw; // Re-throw so OnAppearing can handle it
            }
        }

        /// <summary>
        /// Handles navigation to the main exercises page.
        /// Shows loading spinner while preloading exercise data.
        /// </summary>
        private async Task OnNavigateNext()
        {
            if (!IsAccepted || IsLoading)
                return;

            // Show loading spinner
            IsLoading = true;

            try
            {
                // Save acceptance to preferences
                Preferences.Set("TermsAccepted", true);

                // Preload exercise data while spinner is showing
                // This primes the cache so MainExercisesPage loads instantly
                await Task.WhenAll(
                    _physioContentService.LoadContentAsync(),
                    _contentLoader.GetWarningAsync("exerciseDisclaimer")
                );

                // Navigate to main exercises page (absolute route switches to PostLogin TabBar)
                await Shell.Current.GoToAsync("//MainExercises");
            }
            finally
            {
                // Hide spinner after navigation completes
                IsLoading = false;
            }
        }

        // ContentPage already implements INotifyPropertyChanged via BindableObject.
        // Use base.OnPropertyChanged(propertyName) in property setters.

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _cts?.Cancel();
        }
    }
}
