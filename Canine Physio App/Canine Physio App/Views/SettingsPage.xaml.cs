using System.ComponentModel;
using System.Windows.Input;

namespace Canine_Physio_App.Views
{
    /// <summary>
    /// Settings page for configuring app preferences.
    /// Persists settings using MAUI Preferences API.
    /// </summary>
    public partial class SettingsPage : ContentPage
    {
        // Preference keys
        private const string DownloadVideosKey = "downloadVideosEnabled";
        private const string OfflineCachingKey = "offlineCachingEnabled";
        private const string NotificationsKey = "notificationsEnabled";
        private const string NotificationTimeKey = "notificationTime";
        private const string ProgramCodeKey = "programCode";

        /// <summary>
        /// Command to navigate back to the previous page.
        /// </summary>
        public ICommand NavigateBackCommand { get; }

        private string _programCode = string.Empty;
        /// <summary>
        /// The user's unique program identifier.
        /// </summary>
        public string ProgramCode
        {
            get => _programCode;
            set
            {
                if (_programCode != value)
                {
                    _programCode = value;
                    OnPropertyChanged(nameof(ProgramCode));
                }
            }
        }

        private bool _downloadVideosEnabled;
        /// <summary>
        /// Whether to download exercise videos for offline use.
        /// </summary>
        public bool DownloadVideosEnabled
        {
            get => _downloadVideosEnabled;
            set
            {
                if (_downloadVideosEnabled != value)
                {
                    _downloadVideosEnabled = value;
                    OnPropertyChanged(nameof(DownloadVideosEnabled));
                    Preferences.Set(DownloadVideosKey, value);
                }
            }
        }

        private bool _offlineCachingEnabled;
        /// <summary>
        /// Whether to cache exercise data for offline use.
        /// </summary>
        public bool OfflineCachingEnabled
        {
            get => _offlineCachingEnabled;
            set
            {
                if (_offlineCachingEnabled != value)
                {
                    _offlineCachingEnabled = value;
                    OnPropertyChanged(nameof(OfflineCachingEnabled));
                    Preferences.Set(OfflineCachingKey, value);
                }
            }
        }

        private bool _notificationsEnabled;
        /// <summary>
        /// Whether to show exercise reminder notifications.
        /// </summary>
        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set
            {
                if (_notificationsEnabled != value)
                {
                    _notificationsEnabled = value;
                    OnPropertyChanged(nameof(NotificationsEnabled));
                    Preferences.Set(NotificationsKey, value);
                }
            }
        }

        private TimeSpan _notificationTime;
        /// <summary>
        /// The time of day to show notification reminders.
        /// </summary>
        public TimeSpan NotificationTime
        {
            get => _notificationTime;
            set
            {
                if (_notificationTime != value)
                {
                    _notificationTime = value;
                    OnPropertyChanged(nameof(NotificationTime));
                    Preferences.Set(NotificationTimeKey, value.ToString());
                }
            }
        }

        public SettingsPage()
        {
            NavigateBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            LoadPreferences();

            BindingContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Loads saved preferences from storage.
        /// </summary>
        private void LoadPreferences()
        {
            // Load program code (default to placeholder if not set)
            _programCode = Preferences.Get(ProgramCodeKey, "UIUKThiRxd");
            
            // Load toggle states
            _downloadVideosEnabled = Preferences.Get(DownloadVideosKey, false);
            _offlineCachingEnabled = Preferences.Get(OfflineCachingKey, false);
            _notificationsEnabled = Preferences.Get(NotificationsKey, false);

            // Load notification time (default to 9:00 AM)
            var timeString = Preferences.Get(NotificationTimeKey, "09:00:00");
            if (TimeSpan.TryParse(timeString, out var time))
            {
                _notificationTime = time;
            }
            else
            {
                _notificationTime = new TimeSpan(9, 0, 0);
            }
        }

        // ContentPage already implements INotifyPropertyChanged via BindableObject.
        // Use base.OnPropertyChanged(propertyName) in property setters.
    }
}
