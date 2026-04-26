using System.Linq.Expressions;

namespace Arquitectura.Domain.Specifications;

/// <summary>
/// Specification pattern for complex queries
/// </summary>
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();
    
    public bool IsSatisfiedBy(T entity)
    {
        var func = ToExpression().Compile();
        return func(entity);
    }
    
    public Specification<T> And(Specification<T> other) => new AndSpecification<T>(this, other);
    public Specification<T> Or(Specification<T> other) => new OrSpecification<T>(this, other);
    public Specification<T> Not() => new NotSpecification<T>(this);
    
    public static Specification<T> operator &(Specification<T> left, Specification<T> right) => left.And(right);
    public static Specification<T> operator |(Specification<T> left, Specification<T> right) => left.Or(right);
    public static Specification<T> operator !(Specification<T> spec) => spec.Not();
}

public abstract class SpecificationBase<T> : Specification<T> { }

/// <summary>
/// Composite: AND
/// </summary>
public class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;
    
    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }
    
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "x");
        
        var body = Expression.AndAlso(
            Expression.Invoke(leftExpr, parameter),
            Expression.Invoke(rightExpr, parameter));
        
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

/// <summary>
/// Composite: OR
/// </summary>
public class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;
    
    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }
    
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "x");
        
        var body = Expression.OrElse(
            Expression.Invoke(leftExpr, parameter),
            Expression.Invoke(rightExpr, parameter));
        
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

/// <summary>
/// Composite: NOT
/// </summary>
public class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _spec;
    
    public NotSpecification(Specification<T> spec) => _spec = spec;
    
    public override Expression<Func<T, bool>> ToExpression()
    {
        var expr = _spec.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "x");
        
        var body = Expression.Not(Expression.Invoke(expr, parameter));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

/// <summary>
/// Empty specification - returns all
/// </summary>
public class NoneSpecification<T> : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression() => x => true;
}

/// <summary>
/// Paginated specification result
/// </summary>
public class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}