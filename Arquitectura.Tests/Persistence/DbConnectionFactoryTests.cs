using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arquitectura.Tests.Persistence;

public class DbConnectionFactoryTests
{
    [Fact]
    public void CreateConnection_WithConnectionString_ReturnsClosedConnection()
    {
        var factory = new DbConnectionFactory(Options.Create(new DatabaseOptions
        {
            ConnectionString = "Server=test;Database=test;TrustServerCertificate=True"
        }));

        using var connection = factory.CreateConnection();

        connection.ConnectionString.Should().Contain("test");
        connection.State.Should().Be(System.Data.ConnectionState.Closed);
    }

    [Fact]
    public void CreateConnection_WithoutConnectionString_Throws()
    {
        var factory = new DbConnectionFactory(Options.Create(new DatabaseOptions
        {
            ConnectionString = string.Empty
        }));

        factory.Invoking(x => x.CreateConnection())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*ConnectionStrings:DefaultConnection*");
    }
}
