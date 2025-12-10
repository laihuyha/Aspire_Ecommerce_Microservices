using System;
using BuildingBlocks.Entity;

namespace Catalog.Domain.Aggregates.Product.Events;

/// <summary>
/// Domain event raised when a product is created.
/// </summary>
public class ProductCreatedDomainEvent : DomainEvent
{
    public Guid ProductId { get; }

    public ProductCreatedDomainEvent(Guid productId)
    {
        ProductId = productId;
    }
}
