using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace HelloBuddy.PdfService.Telemetry;

/// <summary>
/// Stamps every telemetry item with a fixed cloud role name so the
/// three Container Apps appear as distinct nodes in the App Insights
/// Application Map and end-to-end transaction view.
/// </summary>
internal sealed class CloudRoleNameInitializer : ITelemetryInitializer
{
    private readonly string _roleName;

    public CloudRoleNameInitializer(string roleName) => _roleName = roleName;

    public void Initialize(ITelemetry telemetry)
    {
        if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
        {
            telemetry.Context.Cloud.RoleName = _roleName;
        }
    }
}
