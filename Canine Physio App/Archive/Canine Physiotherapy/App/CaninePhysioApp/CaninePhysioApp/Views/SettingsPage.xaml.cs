using System.ComponentModel;
using System.Windows.Input;

namespace CaninePhysioApp.Views
{
    public partial class SettingsPage : ContentPage, INotifyPropertyChanged
    {
        // Preference keys
        private const string OfflineCachingKey = "offlineEnabled";
        private const string NotificationsKey = "notificationsEnabled";
        private const string NotificationTimeKey = "notificationTime";

        public ICommand NavigateBackCommand { get; }

        private bool _offlineCachingEnabled;
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

        private void LoadPreferences()
        {
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

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}