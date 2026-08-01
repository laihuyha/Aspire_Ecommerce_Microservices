using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Common;
using BuildingBlocks.Specifications;
using Catalog.Domain.Interfaces;
using Catalog.Infrastructure.Specifications;
using Marten;

namespace Catalog.Infrastructure.Repositories
{
    /// <summary>
    ///     Generic repository implementation using Marten.
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

        public async Task<List<T>> GetListAsync(ISpecification<T> specification,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _documentSession.Query<T>();
            query = MartenSpecificationEvaluator.GetQuery(query, specification);
            return (List<T>)await query.ToListAsync(cancellationToken);
        }

        public async Task<PaginatedResult<T>> GetPaginatedAsync(ISpecification<T> specification, int pageNumber,
            int pageSize, CancellationToken cancellationToken = default)
        {
            IQueryable<T> countQuery = _documentSession.Query<T>();
            if (specification.Criteria != null)
                countQuery = countQuery.Where(specification.Criteria);
            int totalCount = await countQuery.CountAsync(cancellationToken);

            IQueryable<T> query = _documentSession.Query<T>();
            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);
            if (specification.OrderBy != null)
                query = query.OrderBy(specification.OrderBy);
            else if (specification.OrderByDescending != null)
                query = query.OrderByDescending(specification.OrderByDescending);

            int skip = (pageNumber - 1) * pageSize;
            query = query.Skip(skip).Take(pageSize);
            List<T> items = (List<T>)await query.ToListAsync(cancellationToken);

            return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<int> CountAsync(ISpecification<T> specification,
            CancellationToken cancellationToken = default)
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

}
