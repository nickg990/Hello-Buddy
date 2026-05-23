using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace CaninePhysioApp.Views
{
    public partial class LoginPage : ContentPage
    {
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _hasError;
        private bool _isRedirecting;

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

        public bool IsRedirecting
        {
            get => _isRedirecting;
            set
            {
                _isRedirecting = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotRedirecting));
            }
        }

        public bool IsNotRedirecting => !_isRedirecting;

        public ICommand LoginCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand NavigateBackCommand { get; }

        public LoginPage()
        {
            LoginCommand = new Command(async () => await OnLogin());
            ForgotPasswordCommand = new Command(async () => await OnForgotPassword());
            NavigateBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            BindingContext = this;
            InitializeComponent();
        }

        private async Task OnLogin()
        {
            HasError = false;
            IsRedirecting = false;

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

            try
            {
                // TODO: POST /auth/login API call
                await Task.Delay(500); // Simulate network call

                // Show redirecting spinner
                IsRedirecting = true;
                await Task.Delay(1000);
                IsRedirecting = false;

                // Navigate to Terms & Conditions
                await Shell.Current.GoToAsync(nameof(TermsConditionsPage));
            }
            catch (Exception ex)
            {
                IsRedirecting = false;
                ErrorMessage = "Login failed. Please try again.";
                HasError = true;
            }
        }

        private async Task OnForgotPassword()
        {
            // Navigate to Forgotten Password page
            await Shell.Current.GoToAsync(nameof(ForgottenPasswordPage));
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
