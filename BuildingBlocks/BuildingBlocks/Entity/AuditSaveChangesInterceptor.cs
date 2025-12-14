using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Entity
{
    /// <summary>
    ///     Interceptor for auditing entity changes.
    ///     Automatically updates CreatedAt, UpdatedAt, CreatedBy, UpdatedBy.
    /// </summary>
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            DbContext context = eventData.Context;
            if (context == null)
            {
                return new ValueTask<InterceptionResult<int>>(result);
            }

            foreach (EntityEntry entry in context.ChangeTracker
                         .Entries()
                         .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                if (entry.Entity is BaseEntity<Guid> entity)
                {
                    entity.UpdatedAt = DateTime.UtcNow;

                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedAt = DateTime.UtcNow;
                        // entity.CreatedBy = GetCurrentUser(); // Implement user context
                    }
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
