using BuildingBlocks.Infrastructure.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Auth.Contracts;
using Modules.Auth.Infrastructure.Auth;
using Modules.Auth.Infrastructure.Persistence;
using Modules.Auth.Persistence;
using Modules.Auth.Services;

namespace Modules.Auth.Infrastructure.DependencyInjection;

/// <summary>
/// Composition root for the Auth module.
/// </summary>
public static class AuthModuleExtensions
{
    /// <summary>
    /// Registers the Auth module: handlers, validators, persistence, JWT issuing and its options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var moduleAssembly = typeof(LoginCommand).Assembly;

        services.AddRequestHandlersFromAssembly(moduleAssembly);
        services.AddValidatorsFromAssembly(moduleAssembly);

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Key),
                "The 'Jwt:Key' setting is required. Provide it through configuration or the 'Jwt__Key' environment variable.")
            .ValidateOnStart();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
