using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Catalog.Domain.Specifications
{
    /// <summary>
    ///     Generic specification interface for building queries.
    /// </summary>
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        List<string> IncludeStrings { get; }
        Expression<Func<T, object>> OrderBy { get; }
        Expression<Func<T, object>> OrderByDescending { get; }
        List<Expression<Func<T, object>>> ThenBy { get; }
        List<Expression<Func<T, object>>> ThenByDescending { get; }

        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }
        bool IsTrackingEnabled { get; }

        void ApplyCriteria(Expression<Func<T, bool>> criteria);
        void ApplyInclude(Expression<Func<T, object>> includeExpression);
        void ApplyInclude(string includeString);
        void ApplyOrderBy(Expression<Func<T, object>> orderByExpression);
        void ApplyThenBy(Expression<Func<T, object>> thenByExpression);
        void ApplyPaging(int skip, int take);
        void ApplyTracking(bool tracking);
    }
}
