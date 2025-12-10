using System;
using BuildingBlocks.Entity;

namespace BuildingBlocks.Contracts.Events;

/// <summary>
/// Integration event published when a product is created in Catalog service.
/// Other services can subscribe to this event.
/// </summary>
public class ProductCreatedIntegrationEvent : IntegrationEvent
{
    public Guid ProductId { get; }

    public ProductCreatedIntegrationEvent(Guid productId)
    {
        ProductId = productId;
    }
}
