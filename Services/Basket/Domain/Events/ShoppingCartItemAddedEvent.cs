using System;
using BuildingBlocks.Entity;

namespace Basket.Domain.Aggregates.Events
{
    public class ShoppingCartItemAddedEvent : DomainEvent
    {
        public ShoppingCartItemAddedEvent(Guid cartId, Guid productId, int quantity)
        {
            CartId = cartId;
            ProductId = productId;
            Quantity = quantity;
        }

        public Guid CartId { get; }
        public Guid ProductId { get; }
        public int Quantity { get; }
    }
}
