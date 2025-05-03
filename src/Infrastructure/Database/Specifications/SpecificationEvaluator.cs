using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Database.Specifications;

/// <summary>
/// Applies a <see cref="Specification{TEntity}"/> to an <see cref="IQueryable{T}"/> sequence,
/// enabling filtering, including navigation properties, ordering, and query shaping.
/// </summary>
public static class SpecificationEvaluator
{
    /// <summary>
    /// Applies the given specification to the provided queryable source.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="inputQueryable">The base queryable source.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>The queryable with the specification applied.</returns>
    public static IQueryable<TEntity> GetQuery<TEntity>(
        IQueryable<TEntity> inputQueryable,
        Specification<TEntity> specification)
        where TEntity : Entity
    {
        IQueryable<TEntity> queryable = inputQueryable;

        if (specification.Criteria is not null)
        {
            queryable = queryable.Where(specification.Criteria);
        }

        queryable = specification.IncludeExpressions.Aggregate(
            queryable,
            (current, includeExpression) =>
                current.Include(includeExpression));

        if (specification.OrderByExpression is not null)
        {
            queryable = queryable.OrderBy(specification.OrderByExpression);
        }
        else if (specification.OrderByDescendingExpression is not null)
        {
            queryable = queryable.OrderByDescending(specification.OrderByDescendingExpression);
        }

        if (specification.IsSplitQuery)
        {
            queryable = queryable.AsSplitQuery();
        }

        return queryable;
    }
}
