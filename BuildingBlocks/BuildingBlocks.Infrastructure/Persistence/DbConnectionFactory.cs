using System.Data.Common;
using BuildingBlocks.Abstractions.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Creates SQL Server connections from the configured connection string.
/// </summary>
/// <param name="options">The database options.</param>
public sealed class DbConnectionFactory(IOptions<DatabaseOptions> options) : IDbConnectionFactory
{
    private readonly string _connectionString = options.Value.ConnectionString;

    /// <inheritdoc />
    public DbConnection CreateConnection()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                "The database connection string is not configured. Set 'ConnectionStrings:DefaultConnection' " +
                "in configuration or the 'ConnectionStrings__DefaultConnection' environment variable.");
        }

        return new SqlConnection(_connectionString);
    }
}
