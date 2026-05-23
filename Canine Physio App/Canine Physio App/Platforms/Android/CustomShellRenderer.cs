using Android.Content;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace Canine_Physio_App.Platforms.Android;

/// <summary>
/// Custom Shell renderer that handles tab reselection on Android.
/// When the user taps the tab they're already on, pops the navigation
/// stack to root so they return to the tab's root page.
/// </summary>
public class CustomShellRenderer : ShellRenderer
{
    public CustomShellRenderer(Context context) : base(context) { }

    protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
    {
        return new CustomShellItemRenderer(this);
    }
}

/// <summary>
/// Custom ShellItem renderer that intercepts tab reselection via the
/// built-in OnTabReselected virtual method.
/// </summary>
public class CustomShellItemRenderer : ShellItemRenderer
{
    public CustomShellItemRenderer(IShellContext shellContext) : base(shellContext) { }

    /// <summary>
    /// Called when the user taps the tab they're already on.
    /// Pops the navigation stack to root to return to the tab's root page.
    /// </summary>
    protected override void OnTabReselected(ShellSection shellSection)
    {
        var navigation = shellSection.Navigation;

        if (navigation?.NavigationStack?.Count > 1)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await navigation.PopToRootAsync(animated: false);
            });
        }
    }
}
