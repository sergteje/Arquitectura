namespace BuildingBlocks.Abstractions.Domain;

/// <summary>
/// Marker for entities.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the entity identifier.
    /// </summary>
    int Id { get; }
}

/// <summary>
/// Base entity with identity-based equality.
/// </summary>
public abstract class Entity : IEntity
{
    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public int Id { get; set; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType() && Id == other.Id;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Compares two entities by identity.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns><c>true</c> when both are the same entity.</returns>
    public static bool operator ==(Entity? left, Entity? right) => left is null ? right is null : left.Equals(right);

    /// <summary>
    /// Compares two entities by identity.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns><c>true</c> when they are different entities.</returns>
    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}

/// <summary>
/// An entity that tracks creation and modification metadata.
/// </summary>
public abstract class AuditableEntity : Entity
{
    /// <summary>
    /// Gets or sets the creation instant (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the last modification instant (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets who last modified the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Marks the entity as soft-deleted.
    /// </summary>
    /// <param name="deletedBy">Who deleted the entity.</param>
    public void MarkAsDeleted(string deletedBy)
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = deletedBy;
    }

    /// <summary>
    /// Records who modified the entity and when.
    /// </summary>
    /// <param name="updatedBy">Who modified the entity.</param>
    public void MarkAsUpdated(string updatedBy)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}

/// <summary>
/// An auditable entity with a row version for optimistic concurrency.
/// </summary>
public abstract class VersionableEntity : AuditableEntity
{
    /// <summary>
    /// Gets or sets the row version.
    /// </summary>
    public byte[]? Version { get; set; }

    /// <summary>
    /// Checks whether <paramref name="version"/> matches the loaded row version.
    /// </summary>
    /// <param name="version">The version to compare.</param>
    /// <returns><c>true</c> when the versions match.</returns>
    public bool IsOriginalVersion(byte[] version) => Version?.SequenceEqual(version) ?? false;
}
