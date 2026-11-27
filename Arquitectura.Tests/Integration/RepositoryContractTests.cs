using FluentAssertions;
using Microsoft.Data.SqlClient;
using Modules.Weather.Infrastructure.Persistence;
using Modules.Weather.Persistence;
using Xunit;

namespace Arquitectura.Tests.Integration;

public class RepositoryContractTests
{
    [Fact]
    public void WeatherForecastRepository_ImplementsItsPort()
    {
        typeof(IWeatherForecastRepository)
            .IsAssignableFrom(typeof(WeatherForecastRepository))
            .Should().BeTrue();
    }

    [Fact]
    public void WeatherForecastRepositoryPort_ExposesTheExpectedOperations()
    {
        var methods = typeof(IWeatherForecastRepository).GetMethods().Select(method => method.Name);

        methods.Should().Contain([
            "GetAllAsync",
            "GetByIdAsync",
            "CreateAsync",
            "UpdateAsync",
            "DeleteAsync"
        ]);
    }
}

public class ConnectionStringTests
{
    [Fact]
    public void DockerComposeConnectionString_ParsesToExpectedEndpoint()
    {
        const string connectionString =
            "Server=sqlserver;Database=Arquitectura;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True";

        var builder = new SqlConnectionStringBuilder(connectionString);

        builder.DataSource.Should().Be("sqlserver");
        builder.InitialCatalog.Should().Be("Arquitectura");
        builder.TrustServerCertificate.Should().BeTrue();
    }
}
