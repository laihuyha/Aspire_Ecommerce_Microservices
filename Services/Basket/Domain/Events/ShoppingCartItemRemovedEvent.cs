using System;
using BuildingBlocks.Entity;

namespace Basket.Domain.Aggregates.Events
{
    public class ShoppingCartItemRemovedEvent : DomainEvent
    {
        public ShoppingCartItemRemovedEvent(Guid cartId, Guid productId)
        {
            CartId = cartId;
            ProductId = productId;
        }

        public Guid CartId { get; }
        public Guid ProductId { get; }
    }
}
