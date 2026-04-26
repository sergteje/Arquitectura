using System.Linq.Expressions;
using Arquitectura.Domain.Specifications;
using FluentAssertions;
using Xunit;

namespace Arquitectura.Tests.Domain;

public class SpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WhenTrue_ShouldReturnTrue()
    {
        // Arrange
        var spec = new TestSpecification(x => x.Name == "test");
        var entity = new TestItem { Name = "test" };

        // Act & Assert
        spec.IsSatisfiedBy(entity).Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WhenFalse_ShouldReturnFalse()
    {
        // Arrange
        var spec = new TestSpecification(x => x.Name == "test");
        var entity = new TestItem { Name = "other" };

        // Act & Assert
        spec.IsSatisfiedBy(entity).Should().BeFalse();
    }

    [Fact]
    public void And_ShouldCombineSpecifications()
    {
        // Arrange
        var spec1 = new TestSpecification(x => x.Name == "test");
        var spec2 = new TestSpecification(x => x.Value > 10);
        var combined = spec1 & spec2;

        // Act & Assert
        combined.IsSatisfiedBy(new TestItem { Name = "test", Value = 20 }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestItem { Name = "test", Value = 5 }).Should().BeFalse();
    }

    [Fact]
    public void Or_ShouldReturnTrueIfAnyMatches()
    {
        // Arrange
        var spec1 = new TestSpecification(x => x.Name == "test");
        var spec2 = new TestSpecification(x => x.Value > 10);
        var combined = spec1 | spec2;

        // Act & Assert
        combined.IsSatisfiedBy(new TestItem { Name = "other", Value = 20 }).Should().BeTrue();
    }

    [Fact]
    public void Not_ShouldInvertSpecification()
    {
        // Arrange
        var spec = new TestSpecification(x => x.Name == "test");
        var inverted = !spec;

        // Act & Assert
        inverted.IsSatisfiedBy(new TestItem { Name = "other" }).Should().BeTrue();
        inverted.IsSatisfiedBy(new TestItem { Name = "test" }).Should().BeFalse();
    }
}

public class PaginatedResultTests
{
    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        // Arrange
        var result = new PaginatedResult<TestItem>
        {
            TotalCount = 50,
            Page = 1,
            PageSize = 10
        };

        // Act & Assert
        result.TotalPages.Should().Be(5);
    }

    [Fact]
    public void HasNextPage_WhenNotLastPage_ShouldBeTrue()
    {
        // Arrange
        var result = new PaginatedResult<TestItem>
        {
            TotalCount = 50,
            Page = 1,
            PageSize = 10
        };

        // Act & Assert
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_WhenLastPage_ShouldBeFalse()
    {
        // Arrange
        var result = new PaginatedResult<TestItem>
        {
            TotalCount = 50,
            Page = 5,
            PageSize = 10
        };

        // Act & Assert
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_WhenFirstPage_ShouldBeFalse()
    {
        // Arrange
        var result = new PaginatedResult<TestItem>
        {
            TotalCount = 50,
            Page = 1,
            PageSize = 10
        };

        // Act & Assert
        result.HasPreviousPage.Should().BeFalse();
    }
}

// Test helpers
internal class TestItem
{
    public string? Name { get; set; }
    public int Value { get; set; }
}

internal class TestSpecification : Specification<TestItem>
{
    private readonly Func<TestItem, bool> _predicate;

    public TestSpecification(Func<TestItem, bool> predicate)
    {
        _predicate = predicate;
    }

    public override Expression<Func<TestItem, bool>> ToExpression()
    {
        return item => _predicate(item);
    }
}