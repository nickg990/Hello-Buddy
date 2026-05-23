using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace CaninePhysioApp.Views
{
    public partial class RegistrationPage : ContentPage
    {
        private string _code = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private bool _hasError;
        private bool _hasSuccess;

        public string Code
        {
            get => _code;
            set { _code = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set { _successMessage = value; OnPropertyChanged(); }
        }

        public bool HasError
        {
            get => _hasError;
            set { _hasError = value; OnPropertyChanged(); }
        }

        public bool HasSuccess
        {
            get => _hasSuccess;
            set { _hasSuccess = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand NavigateBackCommand { get; }
        public ICommand ClearErrorCommand { get; }

        public RegistrationPage()
        {
            RegisterCommand = new Command(async () => await OnRegister());
            NavigateBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            ClearErrorCommand = new Command(() => { HasError = false; HasSuccess = false; });

            BindingContext = this;
            InitializeComponent();
        }

        private async Task OnRegister()
        {
            HasError = false;
            HasSuccess = false;

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

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password is required.";
                HasError = true;
                return;
            }

            if (Password.Length < 8)
            {
                ErrorMessage = "Password must be at least 8 characters.";
                HasError = true;
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                HasError = true;
                return;
            }

            try
            {
                // TODO: POST /auth/register API call
                // For now, simulate success
                await Task.Delay(500); // Simulate network call

                SuccessMessage = "Registration successful!";
                HasSuccess = true;

                // Navigate to Terms & Conditions after successful registration
                await Task.Delay(1000);
                // await Shell.Current.GoToAsync(nameof(TermsAndConditionsPage));
            }
            catch (Exception ex)
            {
                ErrorMessage = "Registration failed. Please contact the administrator.";
                HasError = true;
            }
        }

        private static bool IsValidEmail(string email)
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