using System.Linq.Expressions;
using SharedKernel;

namespace Infrastructure.Database.Specifications;

/// <summary>
/// Provides a base class for defining query specifications for entities.
/// A specification encapsulates a query predicate that can be reused and composed.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to which the specification applies.</typeparam>
public abstract class Specification<TEntity>
    where TEntity : Entity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{TEntity}"/> class with optional criteria.
    /// </summary>
    /// <param name="criteria">The optional criteria expression used to filter entities.</param>
    protected Specification(Expression<Func<TEntity, bool>>? criteria)
    {
        Criteria = criteria;
    }

    /// <summary>
    /// Indicates whether the query should be executed using a split query strategy (useful for large graphs).
    /// </summary>
    public bool IsSplitQuery { get; protected set; }

    /// <summary>
    /// Gets the criteria expression that defines the filter condition for the specification.
    /// </summary>
    public Expression<Func<TEntity, bool>>? Criteria { get; }

    /// <summary>
    /// Gets the list of related entities to include in the query.
    /// </summary>
    public List<Expression<Func<TEntity, object>>> IncludeExpressions { get; set; } = [];

    /// <summary>
    /// Gets the expression used to order the results in ascending order.
    /// </summary>
    public Expression<Func<TEntity, object>>? OrderByExpression { get; set; }

    /// <summary>
    /// Gets the expression used to order the results in descending order.
    /// </summary>
    public Expression<Func<TEntity, object>>? OrderByDescendingExpression { get; set; }

    /// <summary>
    /// Adds a related entity to be included in the query.
    /// </summary>
    /// <param name="includeExpression">The expression representing the related entity to include.</param>
    protected void AddInclude(Expression<Func<TEntity, object>> includeExpression)
    {
        IncludeExpressions.Add(includeExpression);
    }

    /// <summary>
    /// Adds an ordering expression to sort results in ascending order.
    /// </summary>
    /// <param name="orderByExpression">The expression representing the ordering criteria.</param>
    protected void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
    {
        OrderByExpression = orderByExpression;
    }

    /// <summary>
    /// Adds an ordering expression to sort results in descending order.
    /// </summary>
    /// <param name="orderByDescendingExpression">The expression representing the descending ordering criteria.</param>
    protected void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression)
    {
        OrderByDescendingExpression = orderByDescendingExpression;
    }
}
