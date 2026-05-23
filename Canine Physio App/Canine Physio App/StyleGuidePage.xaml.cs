namespace Canine_Physio_App
{
    /// <summary>
    /// Style Guide page - reference page demonstrating the design token system.
    /// Preserved from original Token Playground for developer reference.
    /// </summary>
    public partial class StyleGuidePage : ContentPage
    {
        public StyleGuidePage()
        {
            InitializeComponent();
        }

        private async void OnSpinnerTestClicked(object sender, EventArgs e)
        {
            // Disable button and show spinner
            spinnerTestButton.IsEnabled = false;
            testSpinner.IsLoading = true;

            // Wait 3 seconds
            await Task.Delay(3000);

            // Hide spinner and re-enable button
            testSpinner.IsLoading = false;
            spinnerTestButton.IsEnabled = true;
        }
    }
}
