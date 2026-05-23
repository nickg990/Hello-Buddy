using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace CaninePhysioApp.Views
{
    public partial class ForgottenPasswordPage : ContentPage
    {
        private string _email = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _hasError;
        private bool _hasSuccess;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool HasError
        {
            get => _hasError;
            set { _hasError = value; OnPropertyChanged(); }
        }

        public bool HasSuccess
        {
            get => _hasSuccess;
            set 
            { 
                _hasSuccess = value; 
                OnPropertyChanged();
            }
        }

        public ICommand SendResetCommand { get; }
        public ICommand NavigateBackCommand { get; }

        public ForgottenPasswordPage()
        {
            SendResetCommand = new Command(async () => await OnSendReset());
            NavigateBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            BindingContext = this;
            InitializeComponent();
        }

        private async Task OnSendReset()
        {
            HasError = false;

            // Validation
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required.";
                HasError = true;
                return;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Please enter a valid email address.";
                HasError = true;
                return;
            }

            try
            {
                // TODO: POST /auth/forgot API call
                // For now, simulate success
                await Task.Delay(500); // Simulate network call

                HasSuccess = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to send reset email. Please try again.";
                HasError = true;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
