using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Exceptions;
using Catalog.Domain.Aggregates.Product;
using Marten;

namespace Catalog.Application.Queries
{
    public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdQueryResponse>;

    public record GetProductByIdQueryResponse(
        Guid Id,
        string Name,
        IReadOnlyList<ProductCategoryDto> Categories,
        IList<VariantDto> Variants,
        IList<ProductAttributeDto> Attributes,
        string Description,
        string ImageUrl,
        decimal? BasePrice,
        decimal EffectivePrice,
        bool IsInStock,
        int TotalStockQuantity);

    public record VariantDto(
        Guid VariantId,
        string Name,
        string SKU,
        decimal Price,
        int StockQuantity,
        bool IsActive,
        IList<ProductAttributeDto> Attributes);

    public record ProductCategoryDto(
        Guid CategoryId,
        string CategoryName);

    public record ProductAttributeDto(
        string Name,
        string Value);

    public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, GetProductByIdQueryResponse>
    {
        private readonly IQuerySession _querySession;

        public GetProductByIdQueryHandler(IQuerySession querySession)
        {
            _querySession = querySession;
        }

        public async Task<GetProductByIdQueryResponse> Handle(GetProductByIdQuery query,
            CancellationToken cancellationToken)
        {
            Product product = await _querySession.LoadAsync<Product>(query.Id, cancellationToken);

            if (product is null)
            {
                throw new ProductNotFoundException(query.Id);
            }

            return new GetProductByIdQueryResponse(
                product.Id,
                product.Name,
                product.Categories.Select(c => new ProductCategoryDto(c.CategoryId, c.CategoryName)).ToList(),
                product.Variants.Select(v => new VariantDto(
                    v.Id,
                    v.Name,
                    v.SKU,
                    v.Price,
                    v.StockQuantity,
                    v.IsActive,
                    v.Attributes.Select(a => new ProductAttributeDto(a.Name, a.Value)).ToList()
                )).ToList(),
                product.Attributes.Select(a => new ProductAttributeDto(a.Name, a.Value)).ToList(),
                product.Description,
                product.ImageUrl,
                product.BasePrice,
                product.GetEffectivePrice(),
                product.IsInStock(),
                product.GetTotalStockQuantity());
        }
    }
}
