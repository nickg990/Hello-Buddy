using System.Windows.Input;
using CaninePhysioApp.Services;

namespace CaninePhysioApp.Views
{
    public partial class TermsConditionsPage : ContentPage
    {
        private readonly TextContentService _contentService;

        private bool _isAccepted;
        private bool _isTermsExpanded;
        private bool _isPrivacyExpanded;
        private bool _isAcceptableUseExpanded;
        private bool _isNavigating;

        private string _termsHeader = string.Empty;
        private string _termsBody = string.Empty;
        private string _privacyHeader = string.Empty;
        private string _privacyBody = string.Empty;
        private string _acceptableUseHeader = string.Empty;
        private string _acceptableUseBody = string.Empty;

        public bool IsAccepted
        {
            get => _isAccepted;
            set
            {
                _isAccepted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NextButtonColor));
                ((Command)NavigateNextCommand).ChangeCanExecute();
            }
        }

        public bool IsNavigating
        {
            get => _isNavigating;
            set
            {
                _isNavigating = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotNavigating));
            }
        }

        public bool IsNotNavigating => !IsNavigating;

        public bool IsTermsExpanded
        {
            get => _isTermsExpanded;
            set
            {
                _isTermsExpanded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TermsExpanderIcon));
            }
        }

        public bool IsPrivacyExpanded
        {
            get => _isPrivacyExpanded;
            set
            {
                _isPrivacyExpanded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PrivacyExpanderIcon));
            }
        }

        public bool IsAcceptableUseExpanded
        {
            get => _isAcceptableUseExpanded;
            set
            {
                _isAcceptableUseExpanded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AcceptableUseExpanderIcon));
            }
        }

        public string TermsHeader
        {
            get => _termsHeader;
            set { _termsHeader = value; OnPropertyChanged(); }
        }

        public string TermsBody
        {
            get => _termsBody;
            set { _termsBody = value; OnPropertyChanged(); }
        }

        public string PrivacyHeader
        {
            get => _privacyHeader;
            set { _privacyHeader = value; OnPropertyChanged(); }
        }

        public string PrivacyBody
        {
            get => _privacyBody;
            set { _privacyBody = value; OnPropertyChanged(); }
        }

        public string AcceptableUseHeader
        {
            get => _acceptableUseHeader;
            set { _acceptableUseHeader = value; OnPropertyChanged(); }
        }

        public string AcceptableUseBody
        {
            get => _acceptableUseBody;
            set { _acceptableUseBody = value; OnPropertyChanged(); }
        }

        public string TermsExpanderIcon => IsTermsExpanded ? "?" : "+";
        public string PrivacyExpanderIcon => IsPrivacyExpanded ? "?" : "+";
        public string AcceptableUseExpanderIcon => IsAcceptableUseExpanded ? "?" : "+";

        public Color NextButtonColor => IsAccepted
            ? Color.FromArgb("#3D81A9")
            : Color.FromArgb("#CCCCCC");

        public ICommand ToggleTermsCommand { get; }
        public ICommand TogglePrivacyCommand { get; }
        public ICommand ToggleAcceptableUseCommand { get; }
        public ICommand NavigateBackCommand { get; }
        public ICommand NavigateNextCommand { get; }

        public TermsConditionsPage(TextContentService contentService)
        {
            _contentService = contentService;

            ToggleTermsCommand = new Command(() => IsTermsExpanded = !IsTermsExpanded);
            TogglePrivacyCommand = new Command(() => IsPrivacyExpanded = !IsPrivacyExpanded);
            ToggleAcceptableUseCommand = new Command(() => IsAcceptableUseExpanded = !IsAcceptableUseExpanded);
            NavigateBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            NavigateNextCommand = new Command(async () => await OnNavigateNext(), () => IsAccepted);

            BindingContext = this;
            InitializeComponent();

            LoadContentAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            IsNavigating = false;
        }

        private async void LoadContentAsync()
        {
            var sections = await _contentService.GetSectionsAsync("termsconditions");

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

        private async Task OnNavigateNext()
        {
            if (!IsAccepted)
                return;

            IsNavigating = true;

            Preferences.Set("TermsAccepted", true);
            await Shell.Current.GoToAsync(nameof(MainExercisesPage));
        }
    }
}