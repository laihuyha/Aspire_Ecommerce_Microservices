using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace AppHost.Abstractions;

/// <summary>
/// Defines a microservice that can be orchestrated by Aspire AppHost.
/// Implement this interface to create a new service definition.
/// </summary>
public interface IServiceDefinition
{
    /// <summary>
    /// Unique identifier for the service (e.g., "catalog", "basket", "order")
    /// </summary>
    string ServiceName { get; }

    /// <summary>
    /// Display name for logging and dashboard
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Registers and configures all resources for this service
    /// </summary>
    /// <param name="builder">The distributed application builder</param>
    /// <returns>The configured API resource builder</returns>
    IResourceBuilder<ProjectResource> Register(IDistributedApplicationBuilder builder);
}

/// <summary>
/// Marker interface for services that require a database
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// The type of database required
    /// </summary>
    DatabaseRequirement DatabaseRequirement { get; }
}

/// <summary>
/// Marker interface for services that require caching
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Whether a dedicated cache is required or shared cache is acceptable
    /// </summary>
    bool RequiresDedicatedCache { get; }
}

/// <summary>
/// Specifies database requirements for a service
/// </summary>
public sealed class DatabaseRequirement
{
    public string DatabaseName { get; init; } = "Database";
    public bool RequiresDedicatedInstance { get; init; }

    public static DatabaseRequirement Shared(string databaseName = "Database") =>
        new() { DatabaseName = databaseName, RequiresDedicatedInstance = false };

    public static DatabaseRequirement Dedicated(string databaseName = "Database") =>
        new() { DatabaseName = databaseName, RequiresDedicatedInstance = true };
}
