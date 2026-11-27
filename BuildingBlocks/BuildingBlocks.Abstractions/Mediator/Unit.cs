namespace BuildingBlocks.Abstractions.Mediator;

/// <summary>
/// The success payload of a request that produces no value.
/// </summary>
/// <remarks>
/// Commands such as <c>Delete</c> or <c>Logout</c> still have a success state; this type is that
/// state without inventing a domain value for it.
/// </remarks>
public readonly record struct Unit
{
    /// <summary>Gets the single <see cref="Unit"/> value.</summary>
    public static Unit Value => default;
}
