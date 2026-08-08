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

        /// <summary>
        ///     Loads a cart with its items under EF Core change tracking, so item removals
        ///     are detected and deleted on save. Use for write flows; use
        ///     <see cref="GetWithItemsByIdAsync"/> for read-only queries.
        /// </summary>
        Task<ShoppingCart> GetTrackedWithItemsByIdAsync(Guid cartId, CancellationToken cancellationToken = default);

        Task<ShoppingCart> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
