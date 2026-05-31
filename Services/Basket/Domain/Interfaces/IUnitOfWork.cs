using System;
using System.Threading;
using System.Threading.Tasks;
using Basket.Domain.Aggregates;
using Basket.Domain.Interfaces;

namespace Domain.Interfaces
{
    /// <summary>
    ///     Unit of Work interface for managing database operations and transactions.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IShoppingCartRepository ShoppingCarts { get; }

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task<ShoppingCart> AddShoppingCartAsync(ShoppingCart shoppingCart, CancellationToken cancellationToken = default);
        Task<ShoppingCart> UpdateShoppingCartAsync(ShoppingCart shoppingCart, CancellationToken cancellationToken = default);
        Task<ShoppingCart> GetShoppingCartByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task DeleteShoppingCartAsync(string userId, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
    }
}
