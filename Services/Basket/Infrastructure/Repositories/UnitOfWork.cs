using System;
using System.Threading;
using System.Threading.Tasks;
using Basket.Domain.Aggregates;
using Basket.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Basket.Infrastructure.Repositories
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

        public async Task<bool> CommitTransactionAsync(CancellationToken cancellationToken = default)
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
            return true;
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
