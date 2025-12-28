using System;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using AppHost.Abstractions;
using AppHost.Infrastructure;
using AppHost.Options;
using AppHost.Utils;
using Projects;

namespace AppHost.Services;

/// <summary>
/// Catalog service definition implementing the service registration pattern.
/// Encapsulates all Catalog-specific infrastructure and configuration.
/// </summary>
public sealed class CatalogServiceDefinition : ServiceDefinitionBase, IDatabaseService, ICacheService
{
    public override string ServiceName => "catalog";
    public override string DisplayName => "Catalog API";

    public DatabaseRequirement DatabaseRequirement => DatabaseRequirement.Shared("Database");
    public bool RequiresDedicatedCache => false;

    public CatalogServiceDefinition() : base(Infrastructure.InfrastructureFactory.Instance) { }

    public CatalogServiceDefinition(IInfrastructureFactory infrastructureFactory) : base(infrastructureFactory) { }

    public override IResourceBuilder<ProjectResource> Register(IDistributedApplicationBuilder builder)
    {
        // Get infrastructure resources using the base class property
        var database = base.InfrastructureFactory.GetOrCreateDatabase(builder, ServiceName, DatabaseRequirement.DatabaseName);
        var cache = base.InfrastructureFactory.GetOrCreateCache(builder);

        // Get configuration options
        var portOptions = GetPortOptions(builder);
        var certOptions = GetHttpsCertificateOptions(builder);

        // Build the service
        var catalogApi = builder.AddProject<Catalog_API>($"{ServiceName}-api")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", GetEnvironment(builder))
            .WithReference(database)
            .WithReference(cache)
            .WaitFor(database);

        // Configure endpoints
        catalogApi = ConfigureEndpoints(catalogApi, portOptions);

        // Configure for Docker deployment
        catalogApi = ConfigureForDocker(catalogApi, portOptions, certOptions);

        return catalogApi;
    }

    private static string GetEnvironment(IDistributedApplicationBuilder builder)
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    }
}
