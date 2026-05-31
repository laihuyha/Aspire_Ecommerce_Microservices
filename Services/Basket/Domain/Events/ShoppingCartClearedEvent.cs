using System;
using BuildingBlocks.Entity;

namespace Basket.Domain.Aggregates.Events
{
    public class ShoppingCartClearedEvent : DomainEvent
    {
        public ShoppingCartClearedEvent(Guid cartId)
        {
            CartId = cartId;
        }

        public Guid CartId { get; }
    }
}
