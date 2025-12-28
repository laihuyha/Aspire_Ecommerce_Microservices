using System;
using System.Collections.Generic;
using System.Linq;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using AppHost.Abstractions;

namespace AppHost.Services;

/// <summary>
/// Registry for managing and registering all microservices.
/// Provides a centralized way to discover and configure services.
/// </summary>
public sealed class ServiceRegistry
{
    private readonly List<IServiceDefinition> _services = new();
    private readonly Dictionary<string, IResourceBuilder<ProjectResource>> _registeredResources = new();

    /// <summary>
    /// Gets all registered service definitions.
    /// </summary>
    public IReadOnlyList<IServiceDefinition> Services => _services.AsReadOnly();

    /// <summary>
    /// Gets all registered API resources.
    /// </summary>
    public IReadOnlyDictionary<string, IResourceBuilder<ProjectResource>> RegisteredResources =>
        _registeredResources;

    /// <summary>
    /// Registers a service definition.
    /// </summary>
    public ServiceRegistry Add<TService>() where TService : IServiceDefinition, new()
    {
        return Add(new TService());
    }

    /// <summary>
    /// Registers a service definition instance.
    /// </summary>
    public ServiceRegistry Add(IServiceDefinition service)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (_services.Any(s => s.ServiceName.Equals(service.ServiceName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Service '{service.ServiceName}' is already registered.");
        }

        _services.Add(service);
        return this;
    }

    /// <summary>
    /// Registers all services with the distributed application builder.
    /// </summary>
    public ServiceRegistry RegisterAll(IDistributedApplicationBuilder builder)
    {
        foreach (var service in _services)
        {
            Console.WriteLine($"[Aspire] Registering service: {service.DisplayName}");
            var resource = service.Register(builder);
            _registeredResources[service.ServiceName] = resource;
        }

        return this;
    }

    /// <summary>
    /// Gets a registered resource by service name.
    /// </summary>
    public IResourceBuilder<ProjectResource> GetResource(string serviceName)
    {
        if (_registeredResources.TryGetValue(serviceName, out var resource))
        {
            return resource;
        }

        throw new InvalidOperationException($"Service '{serviceName}' is not registered or hasn't been built yet.");
    }

    /// <summary>
    /// Creates a new service registry with default services.
    /// </summary>
    public static ServiceRegistry CreateDefault()
    {
        return new ServiceRegistry()
            .Add<CatalogServiceDefinition>();
    }
}
