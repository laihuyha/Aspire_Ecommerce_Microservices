using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Common;
using BuildingBlocks.Specifications;
using Catalog.Domain.Aggregates.Product;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Catalog.Infrastructure.Repositories;
using Catalog.Infrastructure.Specifications;
using Marten;

namespace Catalog.Infrastructure.UnitOfWork
{
    /// <summary>
    ///     Marten-based Unit of Work implementation.
    /// </summary>
    public class MartenUnitOfWork : IUnitOfWork
    {
        private readonly IDocumentSession _documentSession;

        // Repository cache
        private readonly Dictionary<Type, object> _repositories = new();
        private bool _disposed;

        // Specific repositories
        private ProductRepository _productRepository;

        public MartenUnitOfWork(IDocumentSession documentSession)
        {
            _documentSession = documentSession ?? throw new ArgumentNullException(nameof(documentSession));
        }

        public IRepository<T> Repository<T>() where T : class
        {
            Type type = typeof(T);

            if (!_repositories.TryGetValue(type, out object value))
            {
                Type repositoryType = typeof(MartenRepository<>).MakeGenericType(type);
                value = Activator.CreateInstance(repositoryType, _documentSession);
                _repositories[type] = value;
            }

            return (IRepository<T>)value;
        }

        public IProductRepository Products
        {
            get
            {
                if (_productRepository == null)
                {
                    _productRepository = new ProductRepository(_documentSession);
                }

                return _productRepository;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _documentSession.SaveChangesAsync(cancellationToken);
            return 1; // Marten doesn't return count like EF Core
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            await _documentSession.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<T> GetSingleBySpecAsync<T>(ISpecification<T> specification,
            CancellationToken cancellationToken = default) where T : class
        {
            IQueryable<T> query = _documentSession.Query<T>();
            query = MartenSpecificationEvaluator.GetQuery(query, specification);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<T>> GetListBySpecAsync<T>(ISpecification<T> specification,
            CancellationToken cancellationToken = default) where T : class
        {
            IQueryable<T> query = _documentSession.Query<T>();
            query = MartenSpecificationEvaluator.GetQuery(query, specification);
            return (List<T>)await query.ToListAsync(cancellationToken);
        }

        public async Task<PaginatedResult<T>> GetPaginatedBySpecAsync<T>(ISpecification<T> specification,
            int pageNumber, int pageSize, CancellationToken cancellationToken = default) where T : class
        {
            int totalCount = await CountAsync(specification, cancellationToken);

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

        public async Task<int> CountAsync<T>(ISpecification<T> specification,
            CancellationToken cancellationToken = default) where T : class
        {
            IQueryable<T> query = _documentSession.Query<T>();
            query = MartenSpecificationEvaluator.GetQuery(query, specification);
            return await query.CountAsync(cancellationToken);
        }

        public async Task<T> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : class
        {
            return await _documentSession.LoadAsync<T>(id, cancellationToken);
        }

        public async Task<List<T>> GetAllAsync<T>(CancellationToken cancellationToken = default) where T : class
        {
            return (List<T>)await _documentSession.Query<T>().ToListAsync(cancellationToken);
        }

        public async Task<Product> GetProductWithVariantsAsync(Guid productId,
            CancellationToken cancellationToken = default)
        {
            return await Products.GetProductWithVariantsAsync(productId, cancellationToken);
        }

        public async Task UpdateProductVariantsAsync(Product product, CancellationToken cancellationToken = default)
        {
            _documentSession.Store(product);
            await _documentSession.SaveChangesAsync(cancellationToken);
        }

        public async Task<Variant> GetVariantWithDetailsAsync(Guid variantId,
            CancellationToken cancellationToken = default)
        {
            // Since Variant doesn't have its own repository, query directly
            return await _documentSession.LoadAsync<Variant>(variantId, cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _documentSession?.Dispose();
                }

                _disposed = true;
            }
        }

        ~MartenUnitOfWork()
        {
            Dispose(false);
        }
    }

}
