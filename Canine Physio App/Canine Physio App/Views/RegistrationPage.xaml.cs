using System.ComponentModel;
using System.Windows.Input;
using Canine_Physio_App.Helpers;

namespace Canine_Physio_App.Views
{
    /// <summary>
    /// Registration page for new user account creation.
    /// Validates email and password, shows success message on completion.
    /// </summary>
    public partial class RegistrationPage : ContentPage
    {
        #region Private Fields

        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private bool _hasError;
        private bool _hasSuccess;
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
                    // Clear messages when user starts typing
                    ClearMessages();
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
                    // Clear messages when user starts typing
                    ClearMessages();
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
        /// Success message to display after successful registration.
        /// </summary>
        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                if (_successMessage != value)
                {
                    _successMessage = value;
                    OnPropertyChanged(nameof(SuccessMessage));
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
        /// Whether a success message should be displayed.
        /// </summary>
        public bool HasSuccess
        {
            get => _hasSuccess;
            set
            {
                if (_hasSuccess != value)
                {
                    _hasSuccess = value;
                    OnPropertyChanged(nameof(HasSuccess));
                    OnPropertyChanged(nameof(ShowRegisterButton));
                }
            }
        }

        /// <summary>
        /// Whether the register button should be visible (hidden on success).
        /// </summary>
        public bool ShowRegisterButton => !HasSuccess;

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
        /// Command to attempt registration.
        /// </summary>
        public ICommand RegisterCommand { get; }

        /// <summary>
        /// Command to navigate back to previous page.
        /// </summary>
        public ICommand NavigateBackCommand { get; }

        #endregion

        #region Constructor

        public RegistrationPage()
        {
            RegisterCommand = new Command(async () => await OnRegisterAsync());
            NavigateBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            BindingContext = this;
            InitializeComponent();
        }

        #endregion

        #region Command Handlers

        /// <summary>
        /// Handles registration attempt with validation.
        /// Shows loading overlay during API call simulation.
        /// </summary>
        private async Task OnRegisterAsync()
        {
            // Prevent double-tap
            if (IsLoading) return;

            // Reset message states
            HasError = false;
            HasSuccess = false;

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

            // Validate password length
            if (Password.Length < 8)
            {
                ErrorMessage = "Password must be at least 8 characters.";
                HasError = true;
                return;
            }

            try
            {
                // Show loading overlay
                IsLoading = true;

                // TODO: Replace with actual API call to POST /auth/register
                await Task.Delay(500); // Simulate network call

                // Show success message
                SuccessMessage = "Registration successful!";
                HasSuccess = true;
            }
            catch (Exception ex)
            {
                // Handle registration failure
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
                ErrorMessage = "Registration failed. Please try again.";
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Clears error and success messages.
        /// </summary>
        private void ClearMessages()
        {
            if (HasError) HasError = false;
            if (HasSuccess) HasSuccess = false;
        }

        #endregion

        #region INotifyPropertyChanged

        // ContentPage already implements INotifyPropertyChanged via BindableObject.
        // Use base.OnPropertyChanged(propertyName) in property setters.

        #endregion
    }
}
