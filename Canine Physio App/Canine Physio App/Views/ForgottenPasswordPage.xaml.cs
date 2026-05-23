using System.ComponentModel;
using System.Windows.Input;
using Canine_Physio_App.Helpers;

namespace Canine_Physio_App.Views
{
    /// <summary>
    /// Forgotten password page for requesting a password reset.
    /// Validates email and sends reset request to backend.
    /// On success, hides form and shows success message.
    /// </summary>
    public partial class ForgottenPasswordPage : ContentPage
    {
        #region Private Fields

        private string _email = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _hasError;
        private bool _isLoading;
        private bool _isSubmitted;

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

        /// <summary>
        /// Whether the reset request has been successfully submitted.
        /// When true, form is hidden and success message is shown.
        /// </summary>
        public bool IsSubmitted
        {
            get => _isSubmitted;
            set
            {
                if (_isSubmitted != value)
                {
                    _isSubmitted = value;
                    OnPropertyChanged(nameof(IsSubmitted));
                    OnPropertyChanged(nameof(IsNotSubmitted));
                }
            }
        }

        /// <summary>
        /// Inverse of IsSubmitted for binding to IsVisible properties.
        /// </summary>
        public bool IsNotSubmitted => !IsSubmitted;

        #endregion

        #region Commands

        /// <summary>
        /// Command to submit password reset request.
        /// </summary>
        public ICommand SendResetCommand { get; }

        /// <summary>
        /// Command to navigate back to previous page.
        /// </summary>
        public ICommand NavigateBackCommand { get; }

        #endregion

        #region Constructor

        public ForgottenPasswordPage()
        {
            SendResetCommand = new Command(async () => await OnSendResetAsync());
            NavigateBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            BindingContext = this;
            InitializeComponent();
        }

        #endregion

        #region Command Handlers

        /// <summary>
        /// Handles password reset request with validation.
        /// On success, shows success message and hides form.
        /// </summary>
        private async Task OnSendResetAsync()
        {
            // Prevent double-tap or submission after success
            if (IsLoading || IsSubmitted) return;

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

            try
            {
                // Show loading overlay
                IsLoading = true;

                // TODO: Replace with actual API call to POST /auth/forgot-password
                await Task.Delay(500); // Simulate network call

                // Show success message (hides form)
                IsSubmitted = true;
            }
            catch (Exception ex)
            {
                // Handle request failure
                System.Diagnostics.Debug.WriteLine($"Password reset error: {ex.Message}");
                ErrorMessage = "Failed to send reset email. Please try again.";
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
