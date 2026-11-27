using System.Reflection;
using BuildingBlocks.Abstractions.Mediator;
using FluentAssertions;
using Xunit;

namespace Arquitectura.Tests.Architecture;

/// <summary>
/// Enforces the modular-monolith dependency rules at the assembly level.
/// </summary>
public class ModuleBoundaryTests
{
    private static IEnumerable<string> ReferencedAssemblies(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(name => name.Name!)
            .Where(name => name is not null);

    private static readonly Assembly AuthContracts = typeof(global::Modules.Auth.Contracts.LoginCommand).Assembly;
    private static readonly Assembly WeatherContracts = typeof(global::Modules.Weather.Contracts.GetWeatherForecastQuery).Assembly;
    private static readonly Assembly DataTableContracts = typeof(global::Modules.DataTable.Contracts.GetDataTableConfigQuery).Assembly;
    private static readonly Assembly AuthApi = typeof(global::Modules.Auth.Api.AuthEndpoints).Assembly;
    private static readonly Assembly WeatherApi = typeof(global::Modules.Weather.Api.WeatherEndpoints).Assembly;
    private static readonly Assembly DataTableApi = typeof(global::Modules.DataTable.Api.DataTableEndpoints).Assembly;
    private static readonly Assembly Abstractions = typeof(IRequest<>).Assembly;
    private static readonly Assembly AspNetCoreBlocks = typeof(BuildingBlocks.AspNetCore.ResultStatus).Assembly;
    private static readonly Assembly BuildingBlocksInfrastructure =
        typeof(BuildingBlocks.Infrastructure.Mediator.Mediator).Assembly;

    [Fact]
    public void AuthModule_DoesNotReferenceOtherModules()
    {
        var references = ReferencedAssemblies(AuthContracts);

        references.Should().NotContain(name => name.StartsWith("Modules.Weather", StringComparison.Ordinal));
        references.Should().NotContain(name => name.StartsWith("Modules.DataTable", StringComparison.Ordinal));
    }

    [Fact]
    public void WeatherModule_DoesNotReferenceOtherModules()
    {
        var references = ReferencedAssemblies(WeatherContracts);

        references.Should().NotContain(name => name.StartsWith("Modules.Auth", StringComparison.Ordinal));
        references.Should().NotContain(name => name.StartsWith("Modules.DataTable", StringComparison.Ordinal));
    }

    [Fact]
    public void DataTableModule_DoesNotReferenceOtherModules()
    {
        var references = ReferencedAssemblies(DataTableContracts);

        references.Should().NotContain(name => name.StartsWith("Modules.Auth", StringComparison.Ordinal));
        references.Should().NotContain(name => name.StartsWith("Modules.Weather", StringComparison.Ordinal));
    }

    [Fact]
    public void Abstractions_DoNotReferenceModulesOrLegacyProjects()
    {
        var references = ReferencedAssemblies(Abstractions);

        references.Should().NotContain(name => name.StartsWith("Modules.", StringComparison.Ordinal));
        references.Should().NotContain(name => name.StartsWith("Arquitectura.", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildingBlocksInfrastructure_DoesNotReferenceModulesOrLegacyProjects()
    {
        var references = ReferencedAssemblies(BuildingBlocksInfrastructure);

        references.Should().NotContain(name => name.StartsWith("Modules.", StringComparison.Ordinal));
        references.Should().NotContain(name => name.StartsWith("Arquitectura.", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildingBlocksAspNetCore_DoesNotReferenceModules()
    {
        var references = ReferencedAssemblies(AspNetCoreBlocks);

        references.Should().NotContain(name => name.StartsWith("Modules.", StringComparison.Ordinal));
        references.Should().NotContain(name => name.StartsWith("Arquitectura.", StringComparison.Ordinal));
    }

    [Fact]
    public void AuthApi_DoesNotReferenceOtherModules()
    {
        var references = ReferencedAssemblies(AuthApi);

        references.Should().NotContain(name => name.StartsWith("Modules.Weather", StringComparison.Ordinal));
        references.Should().NotContain(name => name.StartsWith("Modules.DataTable", StringComparison.Ordinal));
    }

    [Fact]
    public void WeatherApi_DoesNotReferenceOtherModules()
    {
        var references = ReferencedAssemblies(WeatherApi);

        references.Should().NotContain(name => name.StartsWith("Modules.Auth", StringComparison.Ordinal));
        references.Should().NotContain(name => name.StartsWith("Modules.DataTable", StringComparison.Ordinal));
    }

    [Fact]
    public void DataTableApi_DoesNotReferenceOtherModules()
    {
        var references = ReferencedAssemblies(DataTableApi);

        references.Should().NotContain(name => name.StartsWith("Modules.Auth", StringComparison.Ordinal));
        references.Should().NotContain(name => name.StartsWith("Modules.Weather", StringComparison.Ordinal));
    }
}
