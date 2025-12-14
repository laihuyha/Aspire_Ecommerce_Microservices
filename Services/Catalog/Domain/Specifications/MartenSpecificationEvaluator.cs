using System;
using System.Linq;
using System.Linq.Expressions;

namespace Catalog.Domain.Specifications
{
    /// <summary>
    ///     Evaluates specifications against Marten queries.
    /// </summary>
    public static class MartenSpecificationEvaluator
    {
        public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification)
            where T : class
        {
            IQueryable<T> query = inputQuery;

            // Apply criteria (where clause)
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // Apply ordering
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            // Apply then by ordering
            foreach (Expression<Func<T, object>> thenBy in specification.ThenBy)
            {
                query = ((IOrderedQueryable<T>)query).ThenBy(thenBy);
            }

            foreach (Expression<Func<T, object>> thenByDesc in specification.ThenByDescending)
            {
                query = ((IOrderedQueryable<T>)query).ThenByDescending(thenByDesc);
            }

            // Apply paging
            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip).Take(specification.Take);
            }

            return query;
        }
    }
}
