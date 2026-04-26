using System.Data;
using Microsoft.Data.SqlClient;

namespace Arquitectura.Persistence;

/// <summary>
/// Implementation of IDbConnectionFactory using centralized DatabaseConfiguration
/// Returns SqlConnection which supports async operations
/// </summary>
public class DbConnectionFactory : IDbConnectionFactory
{
    public SqlConnection CreateConnection()
    {
        return DatabaseConfiguration.CreateConnection();
    }
}