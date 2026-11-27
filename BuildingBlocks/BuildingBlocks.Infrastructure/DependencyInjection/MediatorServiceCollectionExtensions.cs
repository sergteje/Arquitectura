using System.Reflection;
using BuildingBlocks.Abstractions.Mediator;
using BuildingBlocks.Infrastructure.Behaviors;
using BuildingBlocks.Infrastructure.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Infrastructure.DependencyInjection;

/// <summary>
/// Composition helpers for the first-party mediator and the shared infrastructure primitives.
/// </summary>
public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers the mediator, the handler wrappers and the shared pipeline behaviors.
    /// Behaviors run in this order: validation, logging, resilience, cache invalidation. Validation
    /// runs first so a rejected request does not reach the logging behavior.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddScoped(typeof(RequestHandlerWrapper<,>), typeof(RequestHandlerWrapper<,>));
        services.TryAddScoped<ISender, Mediator.Mediator>();

        services.TryAddEnumerable(ServiceDescriptor.Scoped(
            typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Scoped(
            typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Scoped(
            typeof(IPipelineBehavior<,>), typeof(ResilienceBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Scoped(
            typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>)));

        return services;
    }

    /// <summary>
    /// Scans the given assemblies and registers every <see cref="IRequestHandler{TRequest,TResponse}"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies that contain request handlers.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddRequestHandlersFromAssembly(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        foreach (var implementationType in assemblies
                     .Distinct()
                     .SelectMany(assembly => assembly.GetTypes())
                     .Where(type => type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false }))
        {
            foreach (var serviceType in implementationType.GetInterfaces()
                         .Where(@interface => @interface.IsGenericType
                                              && @interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            {
                services.TryAddScoped(serviceType, implementationType);
            }
        }

        return services;
    }
}
