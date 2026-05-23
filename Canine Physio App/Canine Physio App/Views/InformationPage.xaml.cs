using System.Collections.ObjectModel;
using System.Windows.Input;
using Canine_Physio_App.Helpers;
using Canine_Physio_App.Models;

namespace Canine_Physio_App.Views
{
    /// <summary>
    /// Information page displaying app details, physiotherapy info,
    /// and guidance on recognising pain in dogs.
    /// </summary>
    public partial class InformationPage : ContentPage
    {
        private readonly TextContentLoader _contentLoader;
        private bool _contentLoaded;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Command to navigate back to the previous page.
        /// </summary>
        public ICommand NavigateBackCommand { get; }

        /// <summary>
        /// Observable collection of content sections to display.
        /// </summary>
        public ObservableCollection<ContentSection> Sections { get; } = new();

        public InformationPage(TextContentLoader contentLoader)
        {
            _contentLoader = contentLoader;

            NavigateBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            BindingContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Load content when page appears. Also detects post-login context
        /// and hides the back button when tabs are available.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            if (_contentLoaded) return;

            try
            {
                var sections = await _contentLoader.GetSectionsAsync("information", _cts?.Token ?? default);

                // Clear and repopulate - we're already on main thread
                Sections.Clear();
                foreach (var section in sections)
                {
                    Sections.Add(section);
                }

                _contentLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading content: {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _cts?.Cancel();
        }
    }
}
