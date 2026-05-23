using System.ComponentModel;
using System.Windows.Input;
using Canine_Physio_App.Helpers;

namespace Canine_Physio_App.Views
{
    /// <summary>
    /// Login page for user authentication.
    /// Validates email and password, navigates to TermsConditionsPage on success.
    /// </summary>
    public partial class LoginPage : ContentPage
    {
        #region Private Fields

        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _hasError;
        private bool _isLoading;

        #endregion

        #region Properties

        /// <summary>
        /// User's email address.
        /// </summary>
        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                    // Clear error when user starts typing
                    if (HasError) HasError = false;
                }
            }
        }

        /// <summary>
        /// User's password.
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                    // Clear error when user starts typing
                    if (HasError) HasError = false;
                }
            }
        }

        /// <summary>
        /// Error message to display when validation fails.
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }

        /// <summary>
        /// Whether an error message should be displayed.
        /// </summary>
        public bool HasError
        {
            get => _hasError;
            set
            {
                if (_hasError != value)
                {
                    _hasError = value;
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        /// <summary>
        /// Whether the page is in a loading state (shows overlay spinner).
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

        #endregion

        #region Commands

        /// <summary>
        /// Command to attempt login.
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// Command to navigate to forgotten password page.
        /// </summary>
        public ICommand ForgotPasswordCommand { get; }

        /// <summary>
        /// Command to navigate back to previous page.
        /// </summary>
        public ICommand NavigateBackCommand { get; }

        #endregion

        #region Constructor

        public LoginPage()
        {
            LoginCommand = new Command(async () => await OnLoginAsync());
            ForgotPasswordCommand = new Command(async () =>
                await Shell.Current.GoToAsync(nameof(ForgottenPasswordPage)));
            NavigateBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            BindingContext = this;
            InitializeComponent();
        }

        #endregion

        #region Command Handlers

        /// <summary>
        /// Handles login attempt with validation.
        /// </summary>
        private async Task OnLoginAsync()
        {
            // Prevent double-tap
            if (IsLoading) return;

            // Reset error state
            HasError = false;

            // Validate email
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required.";
                HasError = true;
                return;
            }

            if (!ValidationHelper.IsValidEmail(Email))
            {
                ErrorMessage = "Please enter a valid email address.";
                HasError = true;
                return;
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password is required.";
                HasError = true;
                return;
            }

            try
            {
                // Show loading overlay
                IsLoading = true;

                // TODO: Replace with actual API call to POST /auth/login
                await Task.Delay(1000); // Simulate network call

                // Simulate successful login - navigate to Terms & Conditions
                await Shell.Current.GoToAsync(nameof(TermsConditionsPage));
            }
            catch (Exception ex)
            {
                // Handle login failure
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                ErrorMessage = "Incorrect email or password.";
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        // ContentPage already implements INotifyPropertyChanged via BindableObject.
        // Use base.OnPropertyChanged(propertyName) in property setters.

        #endregion
    }
}
