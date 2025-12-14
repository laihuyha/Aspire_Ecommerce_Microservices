using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BuildingBlocks.Specifications
{
    /// <summary>
    ///     Interface for the specification pattern.
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
    }
}
