using System;
using BuildingBlocks.Errors;

namespace Basket.Application.Exceptions
{
    /// <summary>
    ///     Exception thrown when a cart is not found.
    /// </summary>
    public class CartNotFoundException : NotFoundException
    {
        public CartNotFoundException(Guid cartId)
            : base("Cart", cartId)
        {
        }
    }
}
