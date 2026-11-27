using BuildingBlocks.Infrastructure.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.DataTable.Contracts;
using Modules.DataTable.Infrastructure.Persistence;
using Modules.DataTable.Persistence;

namespace Modules.DataTable.Infrastructure.DependencyInjection;

/// <summary>
/// Composition root for the DataTable module.
/// </summary>
public static class DataTableModuleExtensions
{
    /// <summary>
    /// Registers the DataTable module: handlers, validators and persistence.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddDataTableModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var moduleAssembly = typeof(GetDataTableConfigQuery).Assembly;

        services.AddRequestHandlersFromAssembly(moduleAssembly);
        services.AddValidatorsFromAssembly(moduleAssembly);

        services.AddScoped<IDataTableRepository, DataTableRepository>();

        return services;
    }
}
