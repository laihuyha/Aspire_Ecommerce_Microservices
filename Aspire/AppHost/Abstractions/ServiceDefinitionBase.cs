using System;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using AppHost.Options;
using AppHost.Utils;

namespace AppHost.Abstractions;

/// <summary>
/// Base class for service definitions providing common configuration patterns.
/// </summary>
public abstract class ServiceDefinitionBase : IServiceDefinition
{
    public abstract string ServiceName { get; }
    public virtual string DisplayName => $"{ServiceName} Service";

    protected IInfrastructureFactory InfrastructureFactory { get; }

    protected ServiceDefinitionBase(IInfrastructureFactory infrastructureFactory)
    {
        InfrastructureFactory = infrastructureFactory ?? throw new ArgumentNullException(nameof(infrastructureFactory));
    }

    public abstract IResourceBuilder<ProjectResource> Register(IDistributedApplicationBuilder builder);

    /// <summary>
    /// Gets API port options from configuration
    /// </summary>
    protected ServicePortOptions GetPortOptions(IDistributedApplicationBuilder builder)
    {
        var mergedConfig = AppHostConfiguration.GetMergedConfiguration(builder.Configuration);
        return ServiceConfigurationHelper.GetServicePortOptions(mergedConfig, ServiceName)
               ?? new ServicePortOptions();
    }

    /// <summary>
    /// Gets HTTPS certificate options from configuration
    /// </summary>
    protected static HttpsCertificateOptions GetHttpsCertificateOptions(IDistributedApplicationBuilder builder)
    {
        var mergedConfig = AppHostConfiguration.GetMergedConfiguration(builder.Configuration);
        return ServiceConfigurationHelper.GetHttpsCertificateOptions(mergedConfig);
    }

    /// <summary>
    /// Applies common Docker Compose configuration for the service
    /// </summary>
    protected IResourceBuilder<ProjectResource> ConfigureForDocker(
        IResourceBuilder<ProjectResource> projectBuilder,
        ServicePortOptions portOptions,
        HttpsCertificateOptions certOptions)
    {
        return projectBuilder.PublishAsDockerComposeService((resource, service) =>
        {
            service.Name = $"{ServiceName}-api";

            // Port configuration
            service.Environment["HTTP_PORTS"] = portOptions.InternalHttpPort.ToString();
            service.Environment["HTTPS_PORTS"] = portOptions.InternalHttpsPort.ToString();

            // HTTPS certificate configuration
            if (!string.IsNullOrEmpty(certOptions.CertificatePath))
            {
                service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Path"] = certOptions.CertificatePath;
                service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Password"] = certOptions.CertificatePassword;
                service.Environment["ASPNETCORE_Kestrel__Certificates__Default__AllowInvalid"] = certOptions.AllowInvalid.ToString().ToLowerInvariant();
            }
        });
    }

    /// <summary>
    /// Configures HTTP endpoints with external and internal port mapping
    /// </summary>
    protected IResourceBuilder<ProjectResource> ConfigureEndpoints(
        IResourceBuilder<ProjectResource> projectBuilder,
        ServicePortOptions portOptions)
    {
        return projectBuilder
            .WithHttpEndpoint(portOptions.ExternalHttpPort ?? portOptions.InternalHttpPort, portOptions.InternalHttpPort, $"{ServiceName}-http")
            .WithHttpsEndpoint(portOptions.ExternalHttpsPort ?? portOptions.InternalHttpsPort, portOptions.InternalHttpsPort, $"{ServiceName}-https");
    }
}
