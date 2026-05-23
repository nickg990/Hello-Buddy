using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace CaninePhysioApp.Views
{
    public partial class HelloBuddyPage : ContentPage
    {
        public ICommand NavigateToInformationCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateToRegistrationCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public HelloBuddyPage()
        {
            NavigateToInformationCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(InformationPage)));
            NavigateToSettingsCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(SettingsPage)));
            NavigateToRegistrationCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(RegistrationPage)));
            NavigateToLoginCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(LoginPage)));
            
            BindingContext = this;
            InitializeComponent();
        }
    }
}
