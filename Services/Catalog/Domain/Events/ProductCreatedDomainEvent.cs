using System;
using BuildingBlocks.Entity;

namespace Catalog.Domain.Aggregates.Product.Events
{
    /// <summary>
    ///     Domain event raised when a product is created.
    /// </summary>
    public class ProductCreatedDomainEvent : DomainEvent
    {
        public ProductCreatedDomainEvent(Guid productId)
        {
            ProductId = productId;
        }

        public Guid ProductId { get; }
    }
}
