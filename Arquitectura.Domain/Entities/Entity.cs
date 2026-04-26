namespace Arquitectura.Domain.Entities;

/// <summary>
/// Base entity with common properties for all entities
/// </summary>
public abstract class Entity
{
    public int Id { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;
        
        if (ReferenceEquals(this, other))
            return true;
        
        if (GetType() != other.GetType())
            return false;
        
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null)
            return true;
        
        if (left is null || right is null)
            return false;
        
        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}

/// <summary>
/// Entities that track creation and modification info
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    public void MarkAsDeleted(string deletedBy)
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = deletedBy;
    }
    
    public void MarkAsUpdated(string updatedBy)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}

/// <summary>
/// Soft delete entity with version for optimistic locking
/// </summary>
public abstract class VersionableEntity : AuditableEntity
{
    public byte[]? Version { get; set; }
    
    public bool IsOriginalVersion(byte[] version) => Version?.SequenceEqual(version) ?? false;
}