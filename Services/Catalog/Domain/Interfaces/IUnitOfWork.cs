using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Domain.Aggregates.Product;
using Catalog.Domain.Entities;
using Catalog.Domain.Specifications;

namespace Catalog.Domain.Interfaces;

/// <summary>
/// Unit of Work interface for managing database operations and transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Generic repository access
    IRepository<T> Repository<T>() where T : class;

    // Specific aggregate repositories (if needed for complex operations)
    IProductRepository Products { get; }

    // Transaction management
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);

    // Specification queries for aggregates
    Task<T> GetSingleBySpecAsync<T>(ISpecification<T> specification, CancellationToken cancellationToken = default) where T : class;
    Task<System.Collections.Generic.List<T>> GetListBySpecAsync<T>(ISpecification<T> specification, CancellationToken cancellationToken = default) where T : class;
    Task<PaginatedResult<T>> GetPaginatedBySpecAsync<T>(ISpecification<T> specification, int pageNumber, int pageSize, CancellationToken cancellationToken = default) where T : class;
    Task<int> CountAsync<T>(ISpecification<T> specification, CancellationToken cancellationToken = default) where T : class;

    // Basic CRUD operations (for when you don't need specifications)
    Task<T> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : class;
    Task<System.Collections.Generic.List<T>> GetAllAsync<T>(CancellationToken cancellationToken = default) where T : class;

    // Product-specific operations
    Task<Product> GetProductWithVariantsAsync(Guid productId, CancellationToken cancellationToken = default);
    Task UpdateProductVariantsAsync(Product product, CancellationToken cancellationToken = default);

    // Variant-specific operations
    Task<Variant> GetVariantWithDetailsAsync(Guid variantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic repository interface.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<System.Collections.Generic.List<T>> GetListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    Task<PaginatedResult<T>> GetPaginatedAsync(ISpecification<T> specification, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Product-specific repository for complex operations.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<Product> GetProductWithVariantsAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<System.Collections.Generic.List<Product>> GetProductsInStockAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<System.Collections.Generic.List<Product>> SearchProductsAsync(string searchTerm, Guid? categoryId, CancellationToken cancellationToken = default);
}
