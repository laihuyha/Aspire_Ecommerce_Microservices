using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Basket.Domain.Aggregates;
using BuildingBlocks.Interfaces;

namespace Basket.Domain.Interfaces
{
    /// <summary>
    ///     Shopping cart repository for operations.
    /// </summary>
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        Task<ShoppingCart> GetShoppingCartWithItemsAsync(Guid cartId, CancellationToken cancellationToken = default);

        Task<ShoppingCart> GetShoppingCartsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
