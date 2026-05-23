using System.Collections.ObjectModel;
using System.Windows.Input;
using CaninePhysioApp.Models;
using CaninePhysioApp.Services;

namespace CaninePhysioApp.Views
{
    public partial class InformationPage : ContentPage
    {
        private readonly TextContentService _contentService;

        public ICommand NavigateBackCommand { get; }
        public ObservableCollection<ContentSection> Sections { get; } = new();

        public InformationPage(TextContentService contentService)
        {
            _contentService = contentService;

            NavigateBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            BindingContext = this;
            InitializeComponent();

            LoadContentAsync();
        }

        private async void LoadContentAsync()
        {
            var sections = await _contentService.GetSectionsAsync("information");

            foreach (var section in sections)
            {
                Sections.Add(section);
            }
        }
    }
}