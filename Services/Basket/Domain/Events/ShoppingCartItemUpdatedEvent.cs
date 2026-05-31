using System;
using BuildingBlocks.Entity;

namespace Basket.Domain.Aggregates.Events
{
    public class ShoppingCartItemUpdatedEvent : DomainEvent
    {
        public ShoppingCartItemUpdatedEvent(Guid cartId, Guid productId, int newQuantity)
        {
            CartId = cartId;
            ProductId = productId;
            NewQuantity = newQuantity;
        }

        public Guid CartId { get; }
        public Guid ProductId { get; }
        public int NewQuantity { get; }
    }
}
