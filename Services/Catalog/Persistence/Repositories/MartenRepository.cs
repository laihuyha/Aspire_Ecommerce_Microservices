using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Domain.Interfaces;
using Catalog.Domain.Specifications;
using Marten;

namespace Catalog.Persistence.Repositories;

/// <summary>
/// Generic repository implementation using Marten.
/// </summary>
public class MartenRepository<T> : IRepository<T> where T : class
{
#pragma warning disable CA1051 // Do not declare visible instance fields
    protected readonly IDocumentSession _documentSession;
#pragma warning restore CA1051 // Do not declare visible instance fields

    public MartenRepository(IDocumentSession documentSession)
    {
        _documentSession = documentSession ?? throw new ArgumentNullException(nameof(documentSession));
    }

    public async Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _documentSession.LoadAsync<T>(id, cancellationToken);
    }

    public async Task<List<T>> GetListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _documentSession.Query<T>();
        query = MartenSpecificationEvaluator.GetQuery(query, specification);
        return (List<T>)await query.ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResult<T>> GetPaginatedAsync(ISpecification<T> specification, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Calculate skip based on page
        var skip = (pageNumber - 1) * pageSize;

        // Create a specification for pagination
        var paginatedSpec = new PaginatedSpecification<T>(specification.Criteria, skip, pageSize);

        // Apply ordering from original specification
        if (specification.OrderBy != null)
        {
            paginatedSpec.ApplyOrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            paginatedSpec.ApplyOrderByDescending(specification.OrderByDescending);
        }

        // Get total count without paging
        IQueryable<T> countQuery = _documentSession.Query<T>();
        if (specification.Criteria != null)
        {
            countQuery = MartenSpecificationEvaluator.GetQuery(countQuery, specification);
        }
        var totalCount = await countQuery.CountAsync(cancellationToken);

        // Get paged results
        IQueryable<T> itemsQuery = _documentSession.Query<T>();
        itemsQuery = MartenSpecificationEvaluator.GetQuery(itemsQuery, paginatedSpec);
        List<T> items = (List<T>)await itemsQuery.ToListAsync(cancellationToken);

        return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _documentSession.Query<T>();
        query = MartenSpecificationEvaluator.GetQuery(query, specification);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _documentSession.Query<T>();
        query = MartenSpecificationEvaluator.GetQuery(query, specification);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        _documentSession.Store((object)entity);
        await Task.CompletedTask; // Marten stores don't need async operation here
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _documentSession.Store((object)entity);
        await Task.CompletedTask; // Marten stores don't need async operation here
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _documentSession.Delete(entity);
        await Task.CompletedTask; // Marten deletes don't need async operation here
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _documentSession.Delete<T>(id);
        await Task.CompletedTask; // Marten deletes don't need async operation here
    }
}

/// <summary>
/// Helper specification for pagination in repository.
/// </summary>
internal sealed class PaginatedSpecification<T> : BaseSpecification<T>
{
    public PaginatedSpecification(System.Linq.Expressions.Expression<System.Func<T, bool>> criteria, int skip, int take) : base(criteria)
    {
        AddPaging(skip, take);
    }
}
