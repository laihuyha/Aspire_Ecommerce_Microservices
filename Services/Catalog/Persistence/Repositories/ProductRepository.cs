/*
 * DOMAIN REPOSITORY PATTERN - ProductRepository
 *
 * WHY DOMAIN REPOSITORY (not just MartenRepository):
 * ✅ Contains PRODUCT-SPECIFIC business logic queries
 * ✅ Implements DOMAIN INTERFACES (IProductRepository)
 * ✅ Handles PRODUCT AGGREGATE complex operations
 * ✅ Uses PRODUCT SPECIFICATIONS for filtering
 *
 * WHEN TO USE THIS PATTERN:
 * - Aggregate-specific queries (variants, categories)
 * - Business logic filtering (in stock, by category)
 * - Statistical/analytics queries for domain
 * - Complex joins within aggregate boundaries
 *
 * DIFFERENCE FROM MARTEN REPO:
 * MartenRepo: Generic Add/Update/Delete for any entity
 * ProductRepo: Business-focused methods for Product domain
 *
 * EXAMPLES OF DOMAIN METHODS:
 * - GetProductWithVariantsAsync() - loads full aggregate
 * - GetProductsInStockAsync() - business rule: "in stock"
 * - SearchProductsAsync() - complex multi-criteria search
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Domain.Aggregates.Product;
using Catalog.Domain.Interfaces;
using Catalog.Domain.Specifications;
using Marten;

namespace Catalog.Persistence.Repositories
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

            int skip = (pageNumber - 1) * pageSize;
            PaginatedSpecification<Product> paginatedSpec = new(specification.Criteria, skip, pageSize);
            paginatedSpec.ApplyOrderBy(specification.OrderBy);

            IQueryable<Product> query = _documentSession.Query<Product>();
            query = MartenSpecificationEvaluator.GetQuery(query, paginatedSpec);
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
