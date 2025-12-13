using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Marten;

namespace Catalog.Application.Queries.Product;

public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdQueryResponse>;

public record GetProductByIdQueryResponse(
    Guid Id,
    string Name,
    IReadOnlyList<string> Categories,
    string Description,
    string ImageUrl,
    decimal Price);

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
            product.Categories,
            product.Description,
            product.ImageUrl,
            product.Price);
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
