using CaninePhysioApp.Views;

namespace CaninePhysioApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register routes for navigation
            Routing.RegisterRoute(nameof(InformationPage), typeof(InformationPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(ForgottenPasswordPage), typeof(ForgottenPasswordPage));
            Routing.RegisterRoute(nameof(TermsConditionsPage), typeof(TermsConditionsPage));
            Routing.RegisterRoute(nameof(MainExercisesPage), typeof(MainExercisesPage));
            Routing.RegisterRoute(nameof(ExerciseDetailPage), typeof(ExerciseDetailPage));
        }
    }
}
