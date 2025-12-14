using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Domain.Aggregates.Product;
using Marten;

namespace Catalog.Application.Queries
{
    public record GetProductsQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string Category = null) : IQuery<GetProductsQueryResponse>;

    public record GetProductsQueryResponse(
        IReadOnlyList<ProductDto> Products,
        int TotalCount,
        int PageNumber,
        int PageSize);

    public record ProductDto(
        Guid Id,
        string Name,
        IReadOnlyList<CategorySummaryDto> Categories,
        string Description,
        string ImageUrl,
        decimal EffectivePrice,
        bool IsInStock,
        int TotalStockQuantity);

    public record CategorySummaryDto(
        Guid CategoryId,
        string CategoryName);

    public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, GetProductsQueryResponse>
    {
        private readonly IQuerySession _querySession;

        public GetProductsQueryHandler(IQuerySession querySession)
        {
            _querySession = querySession;
        }

        public async Task<GetProductsQueryResponse> Handle(GetProductsQuery query, CancellationToken cancellationToken)
        {
            if (query.PageNumber < 1)
            {
                throw new ArgumentException("Page number must be greater than zero.", nameof(query));
            }

            if (query.PageSize < 1 || query.PageSize > 100)
            {
                throw new ArgumentException("Page size must be between 1 and 100.", nameof(query));
            }

            IQueryable<Product> productsQuery = _querySession.Query<Product>();

            if (!string.IsNullOrWhiteSpace(query.Category))
            {
                productsQuery =
                    productsQuery.Where(p => p.Categories.Any(c => c.CategoryName.Contains(query.Category)));
            }

            int totalCount = await productsQuery.CountAsync(cancellationToken);

            IReadOnlyList<Product> products = await productsQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            List<ProductDto> productDtos = products.Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Categories.Select(c => new CategorySummaryDto(c.CategoryId, c.CategoryName)).ToList(),
                p.Description,
                p.ImageUrl,
                p.GetEffectivePrice(),
                p.IsInStock(),
                p.GetTotalStockQuantity())).ToList();

            return new GetProductsQueryResponse(productDtos, totalCount, query.PageNumber, query.PageSize);
        }
    }
}
