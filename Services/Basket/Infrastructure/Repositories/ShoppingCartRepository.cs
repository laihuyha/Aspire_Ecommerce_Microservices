using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Basket.Domain.Aggregates;
using Basket.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly BasketDbContext _context;

        public ShoppingCartRepository()
        {
        }

        public ShoppingCartRepository(BasketDbContext context)
        {
            this._context = context;
        }

        public async Task AddAsync(ShoppingCart entity, CancellationToken cancellationToken = default)
        {
            await _context.ShoppingCarts.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(ShoppingCart entity, CancellationToken cancellationToken = default)
        {
            _context.ShoppingCarts.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ShoppingCarts.FindAsync(id, cancellationToken);
            if (entity != null)
            {
                _context.ShoppingCarts.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<ShoppingCart> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ShoppingCarts.FindAsync(id, cancellationToken);
        }

        public async Task<ShoppingCart> GetShoppingCartsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        }

        public async Task<ShoppingCart> GetShoppingCartWithItemsAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            return await _context.ShoppingCarts.FindAsync(cartId, cancellationToken);
        }

        public async Task UpdateAsync(ShoppingCart entity, CancellationToken cancellationToken = default)
        {
            _context.ShoppingCarts.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}