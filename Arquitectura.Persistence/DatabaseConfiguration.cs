using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Arquitectura.Persistence;

/// <summary>
/// Centralized database configuration
/// </summary>
public static class DatabaseConfiguration
{
    public static string DefaultConnectionString { get; private set; } = string.Empty;
    
    /// <summary>
    /// Configure from IConfiguration
    /// </summary>
    public static void Configure(IConfiguration configuration)
    {
        DefaultConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? GetDefaultConnectionString();
    }
    
    /// <summary>
    /// Configure with custom connection string
    /// </summary>
    public static void Configure(string connectionString)
    {
        DefaultConnectionString = connectionString;
    }
    
    /// <summary>
    /// Get default connection string for development
    /// </summary>
    public static string GetDefaultConnectionString()
    {
        return GetDefaultConnectionString(
            server: "sqlserver",
            database: "Arquitectura",
            user: "sa",
            password: "YourStrong!Passw0rd");
    }
    
    /// <summary>
    /// Build connection string
    /// </summary>
    public static string GetDefaultConnectionString(
        string server = "sqlserver",
        string database = "Arquitectura", 
        string user = "sa",
        string password = "YourStrong!Passw0rd")
    {
        return $"Server={server};Database={database};User Id={user};Password={password};TrustServerCertificate=True";
    }
    
    /// <summary>
    /// Create SqlConnection from centralized config
    /// </summary>
    public static SqlConnection CreateConnection()
    {
        if (string.IsNullOrEmpty(DefaultConnectionString))
        {
            throw new InvalidOperationException(
                "Database not configured. Call Configure() first in Program.cs");
        }
        
        return new SqlConnection(DefaultConnectionString);
    }
    
    /// <summary>
    /// Create SqlConnection with specific connection string
    /// </summary>
    public static SqlConnection CreateConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }
}

/// <summary>
/// Extension for IConfiguration
/// </summary>
public static class ConfigurationExtensions
{
    public static string GetDefaultConnectionString(this IConfiguration configuration)
    {
        return configuration.GetConnectionString("DefaultConnection")
            ?? DatabaseConfiguration.GetDefaultConnectionString();
    }
}