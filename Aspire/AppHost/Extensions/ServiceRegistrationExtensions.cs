using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using AppHost.Services;

namespace AppHost.Extensions;

/// <summary>
/// Fluent extension methods for registering individual services with Aspire.
/// Note: Prefer using WithServices() for registering all services at once.
/// </summary>
public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// Registers the Catalog service with all its dependencies.
    /// </summary>
    public static IResourceBuilder<ProjectResource> AddCatalogService(
        this IDistributedApplicationBuilder builder)
    {
        var registry = ServiceRegistry.CreateDefault();
        registry.RegisterAll(builder);
        return registry.GetResource("catalog");
    }
}
