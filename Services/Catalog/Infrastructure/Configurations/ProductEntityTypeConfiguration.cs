using System.Linq;
using Catalog.Domain.Aggregates.Product;
using Catalog.Domain.ValueObjects;
using Marten;

namespace Catalog.Infrastructure.Configurations
{
    /// <summary>
    ///     Configuration for Product document in Marten.
    /// </summary>
    public class ProductEntityTypeConfiguration : MartenRegistry
    {
        public ProductEntityTypeConfiguration()
        {
            For<Product>()
                .Identity(x => x.Id)
                // Basic indexes for better query performance
                .Duplicate(x => x.Name)           // Full-text index for searching
                .Index(x => x.CreatedAt)          // Time-based queries
                .Index(x => x.Name)               // Name-based queries

                // Configure search indexes for embedded collections
                // Marten will handle these through GIN indexes for JSON data

                // Configure optimistic concurrency for safe updates
                .UseOptimisticConcurrency(true)

                // Enable document versioning/metadata
                .Metadata(md =>
                {
                    md.Revision.Enabled = true;
                    md.CausationId.Enabled = true;
                    md.CorrelationId.Enabled = true;
                });
        }
    }
}
