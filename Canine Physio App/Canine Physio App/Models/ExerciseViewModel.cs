using System.ComponentModel;

namespace Canine_Physio_App.Models;

/// <summary>
/// View model for individual exercises displayed in the exercise grid.
/// Implements INotifyPropertyChanged so tile UI updates when
/// IsComplete or IsSkipped change at runtime.
/// Previously nested inside MainExercisesPage.cs — extracted for maintainability.
/// </summary>
public class ExerciseViewModel : INotifyPropertyChanged
{
    private bool _isComplete;
    private bool _isSkipped;

    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;

    public bool IsComplete
    {
        get => _isComplete;
        set
        {
            if (_isComplete != value)
            {
                _isComplete = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsComplete)));
            }
        }
    }

    public bool IsSkipped
    {
        get => _isSkipped;
        set
        {
            if (_isSkipped != value)
            {
                _isSkipped = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSkipped)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
