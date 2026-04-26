using FluentAssertions;
using Xunit;
using Microsoft.Data.SqlClient;

namespace Arquitectura.Tests.Integration;

public class RealIntegrationTests
{
    [Fact]
    public void SqlConnectionStringBuilder_ShouldCreateValidConnection()
    {
        // Arrange
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = "localhost",
            InitialCatalog = "master",
            UserID = "sa",
            Password = "YourStrong!Passw0rd",
            TrustServerCertificate = true
        };

        // Assert
        builder.ConnectionString.Should().Contain("localhost");
        builder.InitialCatalog.Should().Be("master");
    }

    [Fact]
    public void SqlConnectionStringBuilder_WithFullOptions_ShouldIncludeAll()
    {
        // Arrange
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = "sqlserver",
            InitialCatalog = "Arquitectura", 
            UserID = "sa",
            Password = "YourStrong!Passw0rd",
            TrustServerCertificate = true,
            ConnectTimeout = 30,
            Encrypt = true
        };

        // Assert
        builder.DataSource.Should().Be("sqlserver");
        builder.InitialCatalog.Should().Be("Arquitectura");
        builder.ConnectTimeout.Should().Be(30);
        Assert.True(builder.Encrypt == true);
    }

    [Fact]
    public void ConnectionString_ForDockerCompose_ShouldMatch()
    {
        // This mimics the docker-compose connection string
        var expected = "Server=sqlserver;Database=Arquitectura;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True";
        
        var builder = new SqlConnectionStringBuilder(expected);
        
        builder.DataSource.Should().Be("sqlserver");
        builder.InitialCatalog.Should().Be("Arquitectura");
        Assert.True(builder.TrustServerCertificate == true);
    }
}

public class RepositoryPatternTests
{
    [Fact]
    public void WeatherForecastRepository_ShouldImplementInterface()
    {
        // Verify the repository pattern is correctly defined
        var repoType = typeof(Arquitectura.Persistence.Repositories.WeatherForecastRepository);
        var interfaceType = typeof(Arquitectura.Domain.Interfaces.IWeatherForecastRepository);
        
        interfaceType.IsAssignableFrom(repoType).Should().BeTrue();
    }

    [Fact]
    public void IWeatherForecastRepository_ShouldHaveCorrectMethods()
    {
        var interfaceType = typeof(Arquitectura.Domain.Interfaces.IWeatherForecastRepository);
        
        var methods = interfaceType.GetMethods();
        
        methods.Should().Contain(m => m.Name == "GetAllAsync");
        methods.Should().Contain(m => m.Name == "GetByIdAsync");
        methods.Should().Contain(m => m.Name == "CreateAsync");
        methods.Should().Contain(m => m.Name == "UpdateAsync");
        methods.Should().Contain(m => m.Name == "DeleteAsync");
    }
}