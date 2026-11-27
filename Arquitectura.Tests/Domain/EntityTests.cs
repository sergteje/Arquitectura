using BuildingBlocks.Abstractions.Domain;
using FluentAssertions;
using Xunit;

namespace Arquitectura.Tests.Domain;

public class EntityTests
{
    [Fact]
    public void Equals_SameId_SameType_ShouldBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new TestEntity { Id = 1 };

        // Act & Assert
        entity1.Equals(entity2).Should().BeTrue();
        (entity1 == entity2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new TestEntity { Id = 2 };

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentType_ShouldNotBeEqual()
    {
        // Arrange
        var entity = new TestEntity { Id = 1 };
        var other = new OtherEntity { Id = 1 };

        // Act & Assert
        entity.Equals(other).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameId_ShouldBeSame()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 42 };
        var entity2 = new TestEntity { Id = 42 };

        // Act & Assert
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }
}

public class AuditableEntityTests
{
    [Fact]
    public void MarkAsDeleted_ShouldSetDeletedFlag()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act
        entity.MarkAsDeleted("admin");

        // Assert
        entity.IsDeleted.Should().BeTrue();
        entity.UpdatedBy.Should().Be("admin");
        entity.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsUpdated_ShouldSetUpdatedFields()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act
        entity.MarkAsUpdated("admin");

        // Assert
        entity.UpdatedBy.Should().Be("admin");
        entity.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void CreatedAt_ShouldBeSetToUtcNow()
    {
        // Arrange & Act
        var entity = new TestAuditableEntity();

        // Assert
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}

// Test helper classes
internal class TestEntity : Entity
{
    public string? Name { get; set; }
}

internal class OtherEntity : Entity
{
    public string? Name { get; set; }
}

internal class TestAuditableEntity : AuditableEntity
{
    public string? Name { get; set; }
}