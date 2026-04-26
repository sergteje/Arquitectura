using FluentAssertions;
using Xunit;

namespace Arquitectura.Tests.Persistence;

public class UnitOfWorkIntegrationTests
{
    [Fact]
    public void Configure_ShouldSetConnectionString()
    {
        // Arrange
        var connectionString = "Server=test;Database=test";

        // Act
        Arquitectura.Persistence.DatabaseConfiguration.Configure(connectionString);

        // Assert
        Arquitectura.Persistence.DatabaseConfiguration.DefaultConnectionString.Should().Be(connectionString);
    }

    [Fact]
    public void CreateConnection_ShouldReturnSqlConnection()
    {
        // Arrange
        Arquitectura.Persistence.DatabaseConfiguration.Configure("Server=test;Database=test");

        // Act
        var connection = Arquitectura.Persistence.DatabaseConfiguration.CreateConnection();

        // Assert
        connection.Should().NotBeNull();
        connection.ConnectionString.Should().Contain("test");
    }

    [Fact]
    public void GetDefaultConnectionString_ShouldReturnValidString()
    {
        // Act
        var connectionString = Arquitectura.Persistence.DatabaseConfiguration.GetDefaultConnectionString();

        // Assert
        connectionString.Should().NotBeNullOrEmpty();
        connectionString.Should().Contain("Server=");
    }
}