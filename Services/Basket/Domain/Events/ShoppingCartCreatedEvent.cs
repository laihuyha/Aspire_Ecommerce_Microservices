using System;
using BuildingBlocks.Entity;

namespace Basket.Domain.Aggregates.Events
{
    public class ShoppingCartCreatedEvent : DomainEvent
    {
        public ShoppingCartCreatedEvent(Guid cartId, Guid userId)
        {
            CartId = cartId;
            UserId = userId;
        }

        public Guid CartId { get; }
        public Guid UserId { get; }
    }
}
