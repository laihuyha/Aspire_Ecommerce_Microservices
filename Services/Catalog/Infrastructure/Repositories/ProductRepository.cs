using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Domain.Aggregates.Product;
using Catalog.Domain.Interfaces;
using Catalog.Domain.Specifications;
using Catalog.Infrastructure.Specifications;
using Marten;

namespace Catalog.Infrastructure.Repositories
{
    /// <summary>
    ///     Product-specific repository implementation.
    /// </summary>
    public class ProductRepository : MartenRepository<Product>, IProductRepository
    {
        public ProductRepository(IDocumentSession documentSession) : base(documentSession)
        {
        }

        public async Task<Product> GetProductWithVariantsAsync(Guid productId,
            CancellationToken cancellationToken = default)
        {
            // With Marten document database, the product and its variants are stored together
            // So we can simply load the product which will include all variants
            return await _documentSession.LoadAsync<Product>(productId, cancellationToken);
        }

        public async Task<List<Product>> GetProductsInStockAsync(int pageNumber, int pageSize,
            CancellationToken cancellationToken = default)
        {
            ProductInStockSpecification specification = new();

            IQueryable<Product> query = _documentSession.Query<Product>();
            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);
            if (specification.OrderBy != null)
                query = query.OrderBy(specification.OrderBy);
            else if (specification.OrderByDescending != null)
                query = query.OrderByDescending(specification.OrderByDescending);

            int skip = (pageNumber - 1) * pageSize;
            query = query.Skip(skip).Take(pageSize);
            return (List<Product>)await query.ToListAsync(cancellationToken);
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm, Guid? categoryId,
            CancellationToken cancellationToken = default)
        {
            ProductSearchSpecification specification = new(searchTerm, categoryId, null, null, false);

            IQueryable<Product> query = _documentSession.Query<Product>();
            query = MartenSpecificationEvaluator.GetQuery(query, specification);
            return (List<Product>)await query.ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistSkusAsync(IEnumerable<string> skus, CancellationToken cancellationToken = default)
        {
            if (skus == null || !skus.Any())
                return false;

            return await _documentSession.Query<Product>()
                .SelectMany(p => p.Variants.Select(v => v.SKU))
                .AnyAsync(sku => skus.Contains(sku), cancellationToken);
        }
    }
}
