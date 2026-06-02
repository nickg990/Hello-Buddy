using FluentValidation;
using HelloBuddy.Application.Programmes;
using HelloBuddy.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace HelloBuddy.Application.Records;

/// <summary>
/// Registers Application-layer services (validators, mapping helpers) with the DI container.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Hello Buddy Application-layer services, including FluentValidation validators
    /// for all <see cref="HelloBuddy.Contracts"/> request types and programme orchestration.
    /// </summary>
    public static IServiceCollection AddHelloBuddyApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SaveOwnerRequestValidator>();
        services.AddScoped<IProgrammeService, ProgrammeService>();
        return services;
    }
}
