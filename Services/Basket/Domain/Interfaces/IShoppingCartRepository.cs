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
        Task<ShoppingCart> GetWithItemsByIdAsync(Guid cartId, CancellationToken cancellationToken = default);

        Task<ShoppingCart> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
