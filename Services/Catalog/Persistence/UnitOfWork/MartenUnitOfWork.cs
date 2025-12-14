using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Domain.Aggregates.Product;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Catalog.Domain.Specifications;
using Catalog.Persistence.Repositories;
using Marten;

namespace Catalog.Persistence.UnitOfWork;

/// <summary>
/// Marten-based Unit of Work implementation.
/// </summary>
public class MartenUnitOfWork : IUnitOfWork
{
    private readonly IDocumentSession _documentSession;
    private bool _disposed;

    // Repository cache
    private readonly Dictionary<Type, object> _repositories = new();

    // Specific repositories
    private ProductRepository _productRepository;

    public MartenUnitOfWork(IDocumentSession documentSession)
    {
        _documentSession = documentSession ?? throw new ArgumentNullException(nameof(documentSession));
    }

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);

        if (!_repositories.TryGetValue(type, out object value))
        {
            var repositoryType = typeof(MartenRepository<>).MakeGenericType(type);
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

    public async Task<T> GetSingleBySpecAsync<T>(ISpecification<T> specification, CancellationToken cancellationToken = default) where T : class
    {
        IQueryable<T> query = _documentSession.Query<T>();
        query = MartenSpecificationEvaluator.GetQuery(query, specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<T>> GetListBySpecAsync<T>(ISpecification<T> specification, CancellationToken cancellationToken = default) where T : class
    {
        IQueryable<T> query = _documentSession.Query<T>();
        query = MartenSpecificationEvaluator.GetQuery(query, specification);
        return (List<T>)await query.ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResult<T>> GetPaginatedBySpecAsync<T>(ISpecification<T> specification, int pageNumber, int pageSize, CancellationToken cancellationToken = default) where T : class
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

        // Get total count without paging
        var totalCount = await CountAsync(specification, cancellationToken);

        // Get paged results
        var items = await GetListBySpecAsync(paginatedSpec, cancellationToken);

        return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<int> CountAsync<T>(ISpecification<T> specification, CancellationToken cancellationToken = default) where T : class
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

    public async Task<Product> GetProductWithVariantsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await Products.GetProductWithVariantsAsync(productId, cancellationToken);
    }

    public async Task UpdateProductVariantsAsync(Product product, CancellationToken cancellationToken = default)
    {
        _documentSession.Store(product);
        await _documentSession.SaveChangesAsync(cancellationToken);
    }

    public async Task<Variant> GetVariantWithDetailsAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        // Since Variant doesn't have its own repository, query directly
        return await _documentSession.LoadAsync<Variant>(variantId, cancellationToken);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _documentSession?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Helper specification for pagination.
/// </summary>
internal sealed class PaginatedSpecification<T> : BaseSpecification<T>
{
    public PaginatedSpecification(System.Linq.Expressions.Expression<System.Func<T, bool>> criteria, int skip, int take) : base(criteria)
    {
        AddPaging(skip, take);
    }
}
