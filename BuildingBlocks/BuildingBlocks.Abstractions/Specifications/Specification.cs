using System.Linq.Expressions;

namespace BuildingBlocks.Abstractions.Specifications;

/// <summary>
/// Composable predicate over <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type the specification filters.</typeparam>
public abstract class Specification<T>
{
    /// <summary>
    /// Projects the specification into an expression tree.
    /// </summary>
    /// <returns>The predicate expression.</returns>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Evaluates the specification against an instance.
    /// </summary>
    /// <param name="entity">The instance to test.</param>
    /// <returns><c>true</c> when the instance satisfies the specification.</returns>
    public bool IsSatisfiedBy(T entity) => ToExpression().Compile()(entity);

    /// <summary>
    /// Combines this specification with another one using AND.
    /// </summary>
    /// <param name="other">The other specification.</param>
    /// <returns>The combined specification.</returns>
    public Specification<T> And(Specification<T> other) => new AndSpecification<T>(this, other);

    /// <summary>
    /// Combines this specification with another one using OR.
    /// </summary>
    /// <param name="other">The other specification.</param>
    /// <returns>The combined specification.</returns>
    public Specification<T> Or(Specification<T> other) => new OrSpecification<T>(this, other);

    /// <summary>
    /// Negates this specification.
    /// </summary>
    /// <returns>The negated specification.</returns>
    public Specification<T> Not() => new NotSpecification<T>(this);

    /// <summary>
    /// Combines two specifications using AND.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>The combined specification.</returns>
    public static Specification<T> operator &(Specification<T> left, Specification<T> right) => left.And(right);

    /// <summary>
    /// Combines two specifications using OR.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>The combined specification.</returns>
    public static Specification<T> operator |(Specification<T> left, Specification<T> right) => left.Or(right);

    /// <summary>
    /// Negates a specification.
    /// </summary>
    /// <param name="spec">The specification to negate.</param>
    /// <returns>The negated specification.</returns>
    public static Specification<T> operator !(Specification<T> spec) => spec.Not();
}

/// <summary>
/// Alias base class for specifications declared with an explicit name.
/// </summary>
/// <typeparam name="T">The type the specification filters.</typeparam>
public abstract class SpecificationBase<T> : Specification<T>;

/// <summary>
/// Composite specification: both operands must hold.
/// </summary>
/// <typeparam name="T">The type the specification filters.</typeparam>
public sealed class AndSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    /// <inheritdoc />
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = left.ToExpression();
        var rightExpression = right.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "x");

        var body = Expression.AndAlso(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

/// <summary>
/// Composite specification: at least one operand must hold.
/// </summary>
/// <typeparam name="T">The type the specification filters.</typeparam>
public sealed class OrSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    /// <inheritdoc />
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = left.ToExpression();
        var rightExpression = right.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "x");

        var body = Expression.OrElse(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

/// <summary>
/// Composite specification: the operand must not hold.
/// </summary>
/// <typeparam name="T">The type the specification filters.</typeparam>
public sealed class NotSpecification<T>(Specification<T> spec) : Specification<T>
{
    /// <inheritdoc />
    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = spec.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "x");

        var body = Expression.Not(Expression.Invoke(expression, parameter));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

/// <summary>
/// Specification that accepts every instance.
/// </summary>
/// <typeparam name="T">The type the specification filters.</typeparam>
public sealed class NoneSpecification<T> : Specification<T>
{
    /// <inheritdoc />
    public override Expression<Func<T, bool>> ToExpression() => _ => true;
}
