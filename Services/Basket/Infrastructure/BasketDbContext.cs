using System.Reflection;
using Basket.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class BasketDbContext : DbContext
    {
        public BasketDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            _ = modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    }
}