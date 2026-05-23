using System.Windows.Input;

namespace Canine_Physio_App.Views
{
    /// <summary>
    /// Hello Buddy page - main landing page for the app.
    /// Displays brand identity and navigation to key app sections.
    /// </summary>
    public partial class HelloBuddyPage : ContentPage
    {
        /// <summary>
        /// Command to navigate to the Information page.
        /// </summary>
        public ICommand NavigateToInfoCommand { get; }

        /// <summary>
        /// Command to navigate to the Settings page.
        /// </summary>
        public ICommand NavigateToSettingsCommand { get; }

        /// <summary>
        /// Command to navigate to the Login page (Start Physio flow).
        /// </summary>
        public ICommand NavigateToStartPhysioCommand { get; }

        /// <summary>
        /// Command to navigate to the Registration page.
        /// </summary>
        public ICommand NavigateToRegistrationCommand { get; }

        public HelloBuddyPage()
        {
            NavigateToInfoCommand = new Command(async () => 
                await Shell.Current.GoToAsync(nameof(InformationPage)));

            NavigateToSettingsCommand = new Command(async () => 
                await Shell.Current.GoToAsync(nameof(SettingsPage)));

            NavigateToStartPhysioCommand = new Command(async () => 
                await Shell.Current.GoToAsync(nameof(LoginPage)));

            NavigateToRegistrationCommand = new Command(async () => 
                await Shell.Current.GoToAsync(nameof(RegistrationPage)));

            BindingContext = this;
            InitializeComponent();
        }
    }
}
