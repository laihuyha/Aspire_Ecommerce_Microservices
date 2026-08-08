using System;
using System.Threading;
using System.Threading.Tasks;
using Basket.Domain.Aggregates;
using Basket.Domain.Interfaces;

namespace Basket.Domain.Interfaces
{
    /// <summary>
    ///     Unit of Work interface for managing database operations and transactions.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IShoppingCartRepository ShoppingCarts { get; }

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task<bool> CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
    }
}
