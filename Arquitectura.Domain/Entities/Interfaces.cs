namespace Arquitectura.Domain.Entities;

/// <summary>
/// Marker interface for entities
/// </summary>
public interface IEntity
{
    int Id { get; }
}

/// <summary>
/// Aggregate root - entry point for a cluster of related entities
/// </summary>
public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}

/// <summary>
/// Domain events for internal pub/sub
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

/// <summary>
/// Base class for domain events
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId = Guid.NewGuid();
}

/// <summary>
/// Collection of domain events
/// </summary>
public class DomainEvents : List<IDomainEvent>
{
    public static DomainEvents None => new();
}