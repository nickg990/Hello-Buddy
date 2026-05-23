using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;

namespace CaninePhysioApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Window != null)
        {
            // Enable edge-to-edge with transparent bars
            WindowCompat.SetDecorFitsSystemWindows(Window, false);
            
            // Make status bar transparent so app content shows through
            Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
            
            // Make navigation bar transparent
            Window.SetNavigationBarColor(Android.Graphics.Color.Transparent);

            // Set dark icons for visibility on white background
            var windowInsetsController = WindowCompat.GetInsetsController(Window, Window.DecorView);
            if (windowInsetsController != null)
            {
                windowInsetsController.AppearanceLightStatusBars = true;
                windowInsetsController.AppearanceLightNavigationBars = true;
            }
        }
    }
}
