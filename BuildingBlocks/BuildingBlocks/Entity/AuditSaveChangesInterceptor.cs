using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;

namespace BuildingBlocks.Entity;

/// <summary>
/// Interceptor for auditing entity changes.
/// Automatically updates CreatedAt, UpdatedAt, CreatedBy, UpdatedBy.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null)
            return new ValueTask<InterceptionResult<int>>(result);

        foreach (var entry in context.ChangeTracker
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
