using System.Windows.Input;

namespace Canine_Physio_App.Views;

/// <summary>
/// Placeholder progress overview page displayed as a tab.
/// Shows a simple message until full progress tracking is implemented.
/// </summary>
public partial class ProgressTabPage : ContentPage
{
    #region Commands

    /// <summary>
    /// Command to navigate to the Information page (via header icon).
    /// </summary>
    public ICommand NavigateToInfoCommand { get; }

    /// <summary>
    /// Command to navigate back to the tab the user came from.
    /// </summary>
    public ICommand BackCommand { get; }

    #endregion

    public ProgressTabPage()
    {
        NavigateToInfoCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(InformationPage)));

        BackCommand = new Command(async () =>
        {
            // Switch back to the tab the user came from, preserving its stack
            // so they return to the exact child page they were on.
            var section = AppShell.PreviousTabSection;
            if (section != null && Shell.Current.CurrentItem is TabBar tabBar)
            {
                AppShell.PreserveExercisesStack = true;
                tabBar.CurrentItem = section;
            }
            else
            {
                await Shell.Current.GoToAsync("//MainExercises");
            }
        });

        BindingContext = this;
        InitializeComponent();
    }
}
