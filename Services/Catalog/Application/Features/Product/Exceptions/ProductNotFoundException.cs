using System;

namespace Catalog.Application.Features.Product.Exceptions;

/// <summary>
/// Exception thrown when a product is not found.
/// </summary>
public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(Guid productId)
        : base($"Product with ID '{productId}' was not found.")
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}
