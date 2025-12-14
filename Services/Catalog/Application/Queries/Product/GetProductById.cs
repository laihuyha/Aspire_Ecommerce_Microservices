using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Marten;

namespace Catalog.Application.Queries.Product;

public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdQueryResponse>;

public record GetProductByIdQueryResponse(
    Guid Id,
    string Name,
    IReadOnlyList<CategoryDto> Categories,
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

    public async Task<GetProductByIdQueryResponse> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await _querySession.LoadAsync<Domain.Aggregates.Product.Product>(query.Id, cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(query.Id);
        }

        return new GetProductByIdQueryResponse(
            product.Id,
            product.Name,
            product.Categories.Select(c => new CategoryDto(c.CategoryId, c.CategoryName)).ToList(),
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

public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(Guid productId)
        : base($"Product with ID '{productId}' was not found.")
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}
