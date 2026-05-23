#if ANDROID
using Android.Views;
using Android.OS;
using AndroidX.Core.View;
#endif

namespace Canine_Physio_App.Services
{
    /// <summary>
    /// Android implementation of ISafeAreaService.
    /// Retrieves WindowInsets for status bar and navigation bar heights.
    /// </summary>
    public class SafeAreaService : ISafeAreaService
    {
        public double TopInset { get; private set; }
        public double BottomInset { get; private set; }
        public double LeftInset { get; private set; }
        public double RightInset { get; private set; }

        public SafeAreaService()
        {
            Refresh();
        }

        public void Refresh()
        {
#if ANDROID
            var activity = Platform.CurrentActivity;
            if (activity?.Window?.DecorView == null)
            {
                // Fallback values if window not available
                TopInset = 24;
                BottomInset = 0; // Android nav bar handles its own space
                return;
            }

            var windowInsets = ViewCompat.GetRootWindowInsets(activity.Window.DecorView);
            if (windowInsets == null)
            {
                TopInset = 24;
                BottomInset = 0;
                return;
            }

            // Get system bars insets (status bar + navigation bar)
            var systemBarsInsets = windowInsets.GetInsets(WindowInsetsCompat.Type.SystemBars());
            
            // Convert from pixels to device-independent pixels
            var density = activity.Resources?.DisplayMetrics?.Density ?? 1f;
            
            // Top inset needed for status bar
            TopInset = systemBarsInsets.Top / density;
            
            // Bottom inset: Set to 0 on Android because:
            // - With 3-button navigation: System reserves space, our content doesn't go under it
            // - With gesture navigation: Nav bar is transparent but minimal, footer padding suffices
            // This avoids double-spacing issues seen on Samsung devices
            BottomInset = 0;
            
            LeftInset = systemBarsInsets.Left / density;
            RightInset = systemBarsInsets.Right / density;
#elif IOS || MACCATALYST
            UIKit.UIWindow? window = null;
            
            // iOS 15+ uses UIWindowScene API (KeyWindow is deprecated in iOS 13+)
            if (OperatingSystem.IsIOSVersionAtLeast(15))
            {
                var connectedScenes = UIKit.UIApplication.SharedApplication?.ConnectedScenes;
                var windowScene = connectedScenes?
                    .OfType<UIKit.UIWindowScene>()
                    .FirstOrDefault(s => s.ActivationState == UIKit.UISceneActivationState.ForegroundActive);
                window = windowScene?.Windows.FirstOrDefault(w => w.IsKeyWindow);
            }
            
            // Fallback for iOS 13-14 or if scene not found
            // Suppress CA1422: intentionally using deprecated API for backwards compatibility
#pragma warning disable CA1422
            window ??= UIKit.UIApplication.SharedApplication?.Windows?.FirstOrDefault(w => w.IsKeyWindow)
                     ?? UIKit.UIApplication.SharedApplication?.Windows?.FirstOrDefault();
#pragma warning restore CA1422
            
            if (window != null)
            {
                var safeArea = window.SafeAreaInsets;
                TopInset = safeArea.Top;
                BottomInset = safeArea.Bottom;
                LeftInset = safeArea.Left;
                RightInset = safeArea.Right;
            }
            else
            {
                TopInset = 44; // Default iOS notch area
                BottomInset = 34; // Default home indicator
            }
#elif WINDOWS
            // Windows typically doesn't have safe area concerns
            TopInset = 0;
            BottomInset = 0;
            LeftInset = 0;
            RightInset = 0;
#else
            TopInset = 0;
            BottomInset = 0;
            LeftInset = 0;
            RightInset = 0;
#endif
        }
    }
}
