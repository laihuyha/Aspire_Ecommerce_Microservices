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
/// Basket service definition implementing the service registration pattern.
/// Encapsulates all Basket-specific infrastructure and configuration.
/// </summary>
public sealed class BasketServiceDefinition : ServiceDefinitionBase, IDatabaseService, ICacheService
{
    public override string ServiceName => "basket";
    public override string DisplayName => "Basket API";

    public DatabaseRequirement DatabaseRequirement => DatabaseRequirement.Shared("Database");
    public bool RequiresDedicatedCache => false;

    public BasketServiceDefinition() : base(Infrastructure.InfrastructureFactory.Instance) { }

    public BasketServiceDefinition(IInfrastructureFactory infrastructureFactory) : base(infrastructureFactory) { }

    public override IResourceBuilder<ProjectResource> Register(IDistributedApplicationBuilder builder)
    {
        // Get infrastructure resources using the base class property
        var database = base.InfrastructureFactory.GetOrCreateDatabase(builder, ServiceName, DatabaseRequirement.DatabaseName);
        var cache = base.InfrastructureFactory.GetOrCreateCache(builder);

        // Get configuration options
        var portOptions = GetPortOptions(builder);
        var certOptions = GetHttpsCertificateOptions(builder);

        // Build the service
        var basketApi = builder.AddProject<Basket_API>($"{ServiceName}-api")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", GetEnvironment(builder))
            .WithReference(database)
            .WithReference(cache)
            .WaitFor(database);

        // Configure endpoints
        basketApi = ConfigureEndpoints(basketApi, portOptions);

        // Configure for Docker deployment
        basketApi = ConfigureForDocker(basketApi, portOptions, certOptions);

        return basketApi;
    }

    private static string GetEnvironment(IDistributedApplicationBuilder builder)
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    }
}
