using Foundation;
using UIKit;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace Canine_Physio_App.Platforms.iOS;

/// <summary>
/// Custom Shell renderer that handles tab reselection on iOS.
/// When the user taps the tab they're already on, pops the navigation
/// stack to root so they return to the tab's root page.
/// </summary>
public class CustomShellRenderer : ShellRenderer
{
    protected override IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker()
    {
        return base.CreateTabBarAppearanceTracker();
    }

    protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
    {
        return new CustomShellItemRenderer(this)
        {
            ShellItem = shellItem
        };
    }
}

/// <summary>
/// Custom ShellItem renderer that installs a UITabBarController delegate
/// to intercept tab reselection on iOS.
/// </summary>
public class CustomShellItemRenderer : ShellItemRenderer
{
    public CustomShellItemRenderer(IShellContext context) : base(context) { }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Install our delegate to intercept tab reselection
        if (TabBar != null)
        {
            Delegate = new TabBarDelegate();
        }
    }
}

/// <summary>
/// UITabBarController delegate that pops to root when the current tab is re-tapped.
/// </summary>
public class TabBarDelegate : UITabBarControllerDelegate
{
    public override bool ShouldSelectViewController(
        UITabBarController tabBarController,
        UIViewController viewController)
    {
        // If the user tapped the tab they're already on, pop to root
        if (tabBarController.SelectedViewController == viewController)
        {
            var shell = Shell.Current;
            var navigation = shell?.CurrentItem?.CurrentItem?.Navigation;

            if (navigation?.NavigationStack?.Count > 1)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await navigation.PopToRootAsync(animated: false);
                });
            }

            // Return false to prevent default behavior (we handled it)
            return false;
        }

        return true;
    }
}
