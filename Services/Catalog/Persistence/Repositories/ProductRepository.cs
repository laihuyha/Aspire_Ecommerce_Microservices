using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Domain.Aggregates.Product;
using Catalog.Domain.Interfaces;
using Catalog.Domain.Specifications;
using Marten;

namespace Catalog.Persistence.Repositories;

/// <summary>
/// Product-specific repository implementation.
/// </summary>
public class ProductRepository : MartenRepository<Product>, IProductRepository
{
    public ProductRepository(IDocumentSession documentSession) : base(documentSession)
    {
    }

    public async Task<Product> GetProductWithVariantsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // With Marten document database, the product and its variants are stored together
        // So we can simply load the product which will include all variants
        return await _documentSession.LoadAsync<Product>(productId, cancellationToken);
    }

    public async Task<List<Product>> GetProductsInStockAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var specification = new ProductInStockSpecification();

        var skip = (pageNumber - 1) * pageSize;
        var paginatedSpec = new PaginatedSpecification<Product>(specification.Criteria, skip, pageSize);
        paginatedSpec.ApplyOrderBy(specification.OrderBy);

        IQueryable<Product> query = _documentSession.Query<Product>();
        query = MartenSpecificationEvaluator.GetQuery(query, paginatedSpec);
        return (List<Product>)await query.ToListAsync(cancellationToken);
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm, Guid? categoryId, CancellationToken cancellationToken = default)
    {
        var specification = new ProductSearchSpecification(searchTerm, categoryId, null, null, false);

        IQueryable<Product> query = _documentSession.Query<Product>();
        query = MartenSpecificationEvaluator.GetQuery(query, specification);
        return (List<Product>)await query.ToListAsync(cancellationToken);
    }
}
