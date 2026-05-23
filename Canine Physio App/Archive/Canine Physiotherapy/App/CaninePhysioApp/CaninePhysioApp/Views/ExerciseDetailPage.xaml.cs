using CaninePhysioApp.Models;
using CaninePhysioApp.Services;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CaninePhysioApp.Views
{
    [QueryProperty(nameof(ExerciseKey), "exerciseKey")]
    public partial class ExerciseDetailPage : ContentPage, INotifyPropertyChanged
    {
        private readonly PhysioContentService _physioContentService;

        private string _exerciseKey = string.Empty;
        private string _exerciseTitle = string.Empty;
        private string _summary = string.Empty;
        private ImageSource? _thumbnailImage;
        private string _videoFileName = string.Empty;
        private bool _isVideoPlaying;
        private int _reps;
        private int _sets;

        public ObservableCollection<InstructionStep> InstructionSteps { get; } = new();

        public string ExerciseKey
        {
            get => _exerciseKey;
            set
            {
                _exerciseKey = value;
                OnPropertyChanged();
                _ = LoadExerciseAsync();
            }
        }

        public string ExerciseTitle
        {
            get => _exerciseTitle;
            set { _exerciseTitle = value; OnPropertyChanged(); }
        }

        public string Summary
        {
            get => _summary;
            set { _summary = value; OnPropertyChanged(); }
        }

        public ImageSource? ThumbnailImage
        {
            get => _thumbnailImage;
            set { _thumbnailImage = value; OnPropertyChanged(); }
        }

        public bool IsVideoPlaying
        {
            get => _isVideoPlaying;
            set { _isVideoPlaying = value; OnPropertyChanged(); }
        }

        public int Reps
        {
            get => _reps;
            set { _reps = value; OnPropertyChanged(); }
        }

        public int Sets
        {
            get => _sets;
            set { _sets = value; OnPropertyChanged(); }
        }

        public ICommand NavigateBackCommand { get; }
        public ICommand MarkCompleteCommand { get; }
        public ICommand PlayVideoCommand { get; }
        public ICommand CloseVideoCommand { get; }

        public ExerciseDetailPage(PhysioContentService physioContentService)
        {
            _physioContentService = physioContentService;

            NavigateBackCommand = new Command(async () => await OnNavigateBack());
            MarkCompleteCommand = new Command(async () => await OnMarkComplete());
            PlayVideoCommand = new Command(OnPlayVideo);
            CloseVideoCommand = new Command(OnCloseVideo);

            BindingContext = this;
            InitializeComponent();
        }

        private async Task LoadExerciseAsync()
        {
            if (string.IsNullOrEmpty(_exerciseKey))
                return;

            var exercise = await _physioContentService.GetExerciseByKeyAsync(_exerciseKey);

            if (exercise is null)
            {
                await Shell.Current.DisplayAlert("Error", "Exercise not found.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            ExerciseTitle = ToTitleCase(exercise.Title);
            Summary = exercise.Summary;
            Reps = exercise.Reps;
            Sets = exercise.Sets;
            
            // Use ImageSource type with explicit filename - matches ExerciseTile pattern
            ThumbnailImage = ImageSource.FromFile(exercise.Image + ".jpg");
            _videoFileName = exercise.VideoName;

            // Build instruction steps
            InstructionSteps.Clear();
            for (int i = 0; i < exercise.Instructions.Count; i++)
            {
                InstructionSteps.Add(new InstructionStep
                {
                    Number = i + 1,
                    Text = exercise.Instructions[i]
                });
            }
        }

        private void OnPlayVideo()
        {
            VideoPlayer.Source = MediaSource.FromResource(_videoFileName);
            IsVideoPlaying = true;
        }

        private void OnCloseVideo()
        {
            VideoPlayer.Stop();
            VideoPlayer.Source = null;
            IsVideoPlaying = false;
        }

        private static string ToTitleCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }

        private async Task OnNavigateBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        private async Task OnMarkComplete()
        {
            await Shell.Current.DisplayAlert(
                "Exercise Complete",
                $"'{ExerciseTitle}' marked as complete. Navigation to Exercise Progress page coming soon.",
                "OK");

            await Shell.Current.GoToAsync("..");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            OnCloseVideo();
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
