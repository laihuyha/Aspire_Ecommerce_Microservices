using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BuildingBlocks.Specifications
{
    /// <summary>
    ///     Base implementation of the specification pattern.
    /// </summary>
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        protected BaseSpecification()
        {
            // Default to tracking enabled
            IsTrackingEnabled = true;
        }

        protected BaseSpecification(Expression<Func<T, bool>> criteria) : this()
        {
            Criteria = criteria;
        }

        public Expression<Func<T, bool>> Criteria { get; } = null!;
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public Expression<Func<T, object>> OrderBy { get; private set; } = null!;
        public Expression<Func<T, object>> OrderByDescending { get; private set; } = null!;
        public List<Expression<Func<T, object>>> ThenBy { get; } = new();
        public List<Expression<Func<T, object>>> ThenByDescending { get; } = new();

        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; }
        public bool IsTrackingEnabled { get; private set; } = true;

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }

        protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
        }

        protected void AddThenBy(Expression<Func<T, object>> thenByExpression)
        {
            ThenBy.Add(thenByExpression);
        }

        protected void AddThenByDescending(Expression<Func<T, object>> thenByDescendingExpression)
        {
            ThenByDescending.Add(thenByDescendingExpression);
        }

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        protected void ApplyTracking(bool tracking)
        {
            IsTrackingEnabled = tracking;
        }
    }
}
