using System;
using System.Threading;
using System.Threading.Tasks;
using Basket.Domain.Aggregates;
using Basket.Domain.Interfaces;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BasketDbContext _context;
        private bool _disposed;
        private IDbContextTransaction _currentTransaction;

        private ShoppingCartRepository _shoppingCartRepository;

        public UnitOfWork(BasketDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IShoppingCartRepository ShoppingCarts =>
            _shoppingCartRepository ??= new ShoppingCartRepository(_context);

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
                return;

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No active transaction to commit.");

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task<ShoppingCart> AddShoppingCartAsync(ShoppingCart shoppingCart, CancellationToken cancellationToken = default)
        {
            await _context.ShoppingCarts.AddAsync(shoppingCart, cancellationToken);
            return shoppingCart;
        }

        public Task<ShoppingCart> UpdateShoppingCartAsync(ShoppingCart shoppingCart, CancellationToken cancellationToken = default)
        {
            _context.ShoppingCarts.Update(shoppingCart);
            return Task.FromResult(shoppingCart);
        }

        public async Task<ShoppingCart> GetShoppingCartByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new ArgumentException($"Invalid user ID format: '{userId}'.", nameof(userId));

            return await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userGuid, cancellationToken);
        }

        public async Task DeleteShoppingCartAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new ArgumentException($"Invalid user ID format: '{userId}'.", nameof(userId));

            ShoppingCart cart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userGuid, cancellationToken);

            if (cart != null)
                _context.ShoppingCarts.Remove(cart);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _currentTransaction?.Dispose();
            _context.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
