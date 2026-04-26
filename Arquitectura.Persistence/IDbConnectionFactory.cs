using System.Data;
using Microsoft.Data.SqlClient;

namespace Arquitectura.Persistence;

/// <summary>
/// Factory for creating database connections
/// Injected via DI - uses centralized DatabaseConfiguration
/// Returns SqlConnection which has async support
/// </summary>
public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}