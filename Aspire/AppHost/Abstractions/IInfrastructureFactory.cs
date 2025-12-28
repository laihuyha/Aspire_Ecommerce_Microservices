using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace AppHost.Abstractions;

/// <summary>
/// Factory for creating shared infrastructure resources.
/// Implements singleton pattern for shared resources.
/// </summary>
public interface IInfrastructureFactory
{
    /// <summary>
    /// Gets or creates a PostgreSQL database for the specified service
    /// </summary>
    IResourceBuilder<PostgresDatabaseResource> GetOrCreateDatabase(
        IDistributedApplicationBuilder builder,
        string serviceName,
        string databaseName);

    /// <summary>
    /// Gets or creates a Redis cache instance
    /// </summary>
    IResourceBuilder<RedisResource> GetOrCreateCache(
        IDistributedApplicationBuilder builder,
        string cacheName = "distributedcache");
}
