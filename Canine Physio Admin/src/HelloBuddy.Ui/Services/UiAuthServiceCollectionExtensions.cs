using HelloBuddy.Application.Auth;
using HelloBuddy.Infrastructure.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace HelloBuddy.Ui.Services;

/// <summary>
/// Registers UI auth services (ILoginService, IPractitionerAdminService) into DI.
/// </summary>
public static class UiAuthServiceCollectionExtensions
{
    public static IServiceCollection AddUiAuthServices(this IServiceCollection services)
    {
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IPractitionerAdminService, PractitionerAdminService>();
        return services;
    }
}
