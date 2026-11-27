using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.DependencyInjection;

/// <summary>
/// Registers the shared technical infrastructure: mediator, Dapper connection factory, unit of work
/// and the Redis distributed cache. Background job hosting and web concerns belong to the host.
/// </summary>
public static class BuildingBlocksServiceCollectionExtensions
{
    /// <summary>
    /// Registers every shared technical service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddBuildingBlocksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

        services.Configure<DatabaseOptions>(options => options.ConnectionString = connectionString);
        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();
        services.TryAddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();

        services.Configure<RedisOptions>(options =>
            configuration.GetSection(RedisOptions.SectionName).Bind(options));

        services.TryAddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            var redisOptions = serviceProvider.GetRequiredService<IOptions<RedisOptions>>().Value;

            var configurationOptions = ConfigurationOptions.Parse(redisOptions.Connection);
            configurationOptions.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        var redisConnection = configuration[$"{RedisOptions.SectionName}:Connection"] ?? "localhost:6379";

        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
        services.TryAddScoped<IDistributedCacheService, RedisCacheService>();

        services.AddMediator();

        return services;
    }
}
