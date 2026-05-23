using CaninePhysioApp.Controls;
using CaninePhysioApp.Models;
using CaninePhysioApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CaninePhysioApp.Views
{
    public partial class MainExercisesPage : ContentPage, INotifyPropertyChanged
    {
        private readonly PhysioContentService _physioContentService;
        private readonly TextContentService _textContentService;

        private string _exerciseSetDescription = string.Empty;
        private string _warningText = string.Empty;
        private List<ExerciseViewModel> _exercises = new();
        private double _progressBarMaxWidth = 280;
        private bool _isNavigating;

        public string ExerciseSetDescription
        {
            get => _exerciseSetDescription;
            set { _exerciseSetDescription = value; OnPropertyChanged(); }
        }

        public string WarningText
        {
            get => _warningText;
            set { _warningText = value; OnPropertyChanged(); }
        }

        public bool IsNavigating
        {
            get => _isNavigating;
            set { _isNavigating = value; OnPropertyChanged(); }
        }

        public int CompletedCount => _exercises.Count(e => e.IsComplete);
        public int TotalCount => _exercises.Count;
        public double ProgressPercentage => TotalCount > 0 ? (double)CompletedCount / TotalCount * 100 : 0;
        public string ProgressPercentageText => $"{ProgressPercentage:0}%";
        public double ProgressBarWidth => TotalCount > 0 ? (_progressBarMaxWidth * CompletedCount / TotalCount) : 0;

        public ICommand NavigateToProgressCommand { get; }
        public ICommand NavigateToSkipCommand { get; }
        public ICommand NavigateToInfoCommand { get; }
        public ICommand NavigateToExerciseCommand { get; }

        public MainExercisesPage(PhysioContentService physioContentService, TextContentService textContentService)
        {
            _physioContentService = physioContentService;
            _textContentService = textContentService;

            NavigateToProgressCommand = new Command(async () => await OnNavigateToProgress());
            NavigateToSkipCommand = new Command(async () => await OnNavigateToSkip());
            NavigateToInfoCommand = new Command(async () => await OnNavigateToInfo());
            NavigateToExerciseCommand = new Command<string>(async (key) => await OnNavigateToExercise(key));

            BindingContext = this;
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Reset spinner when returning to page
            IsNavigating = false;
            
            await LoadContentAsync();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            _progressBarMaxWidth = width - 80;
            OnPropertyChanged(nameof(ProgressBarWidth));
        }

        private async Task LoadContentAsync()
        {
            // Load warning text
            WarningText = await _textContentService.GetWarningAsync("exerciseDisclaimer");

            // Load exercises
            var exerciseSet = await _physioContentService.GetFirstExerciseSetAsync();

            if (exerciseSet is null)
                return;

            ExerciseSetDescription = exerciseSet.Description;

            _exercises = exerciseSet.Exercises.Select((e, index) => new ExerciseViewModel
            {
                Key = e.Key,
                Name = e.Name,
                Image = e.Image,
                IsComplete = index < 2
            }).ToList();

            BuildExerciseGrid();
            UpdateProgress();
        }

        private void BuildExerciseGrid()
        {
            ExerciseGrid.Children.Clear();
            ExerciseGrid.RowDefinitions.Clear();

            int rowCount = (int)Math.Ceiling(_exercises.Count / 2.0);
            for (int i = 0; i < rowCount; i++)
            {
                ExerciseGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int i = 0; i < _exercises.Count; i++)
            {
                var exercise = _exercises[i];
                var tile = new ExerciseTile
                {
                    ExerciseKey = exercise.Key,
                    ExerciseName = exercise.Name,
                    ImageSource = exercise.Image + ".jpg",
                    IsComplete = exercise.IsComplete,
                    Command = NavigateToExerciseCommand
                };

                int row = i / 2;
                int column = i % 2;

                Grid.SetRow(tile, row);
                Grid.SetColumn(tile, column);
                ExerciseGrid.Children.Add(tile);
            }
        }

        private void UpdateProgress()
        {
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(ProgressPercentage));
            OnPropertyChanged(nameof(ProgressPercentageText));
            OnPropertyChanged(nameof(ProgressBarWidth));
        }

        private async Task OnNavigateToProgress()
        {
            await Shell.Current.DisplayAlert("Coming Soon", "Progress page is under development.", "OK");
        }

        private async Task OnNavigateToSkip()
        {
            await Shell.Current.DisplayAlert("Coming Soon", "Skip Session page is under development.", "OK");
        }

        private async Task OnNavigateToInfo()
        {
            await Shell.Current.GoToAsync(nameof(InformationPage));
        }

        private async Task OnNavigateToExercise(string exerciseKey)
        {
            // Show loading spinner
            IsNavigating = true;

            try
            {
                await Shell.Current.GoToAsync($"{nameof(ExerciseDetailPage)}?exerciseKey={exerciseKey}");
            }
            finally
            {
                // Spinner will be hidden in OnAppearing when returning
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ExerciseViewModel
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
    }
}