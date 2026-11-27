namespace BuildingBlocks.Abstractions.Domain;

/// <summary>
/// Something that happened inside a module's domain. Modules publish these in-process only.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the instant the event happened (UTC).
    /// </summary>
    DateTime OccurredOn { get; }
}

/// <summary>
/// Aggregate root: the only entry point to a cluster of related entities.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Gets the domain events raised by the aggregate.
    /// </summary>
    /// <returns>The pending domain events.</returns>
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();

    /// <summary>
    /// Clears the domain events raised by the aggregate.
    /// </summary>
    void ClearDomainEvents();
}

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Collection of domain events.
/// </summary>
public sealed class DomainEvents : List<IDomainEvent>
{
    /// <summary>
    /// Gets an empty collection.
    /// </summary>
    public static DomainEvents None => [];
}
