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
    /// Configures HTTP endpoints with external and internal port mapping.
    /// Fix for Aspire 9.4+: When IsProxied=true, Port and TargetPort cannot be the same.
    /// </summary>
    protected IResourceBuilder<ProjectResource> ConfigureEndpoints(
        IResourceBuilder<ProjectResource> projectBuilder,
        ServicePortOptions portOptions)
    {
        // Calculate external ports (fallback to internal if not specified)
        var httpExternal = portOptions.ExternalHttpPort ?? portOptions.InternalHttpPort;
        var httpsExternal = portOptions.ExternalHttpsPort ?? portOptions.InternalHttpsPort;

        // Only specify targetPort if external != internal to avoid Aspire 9.4+ proxy error
        if (httpExternal == portOptions.InternalHttpPort)
        {
            projectBuilder = projectBuilder.WithHttpEndpoint(httpExternal, name: $"{ServiceName}-http");
        }
        else
        {
            projectBuilder = projectBuilder.WithHttpEndpoint(httpExternal, portOptions.InternalHttpPort, $"{ServiceName}-http");
        }

        if (httpsExternal == portOptions.InternalHttpsPort)
        {
            projectBuilder = projectBuilder.WithHttpsEndpoint(httpsExternal, name: $"{ServiceName}-https");
        }
        else
        {
            projectBuilder = projectBuilder.WithHttpsEndpoint(httpsExternal, portOptions.InternalHttpsPort, $"{ServiceName}-https");
        }

        return projectBuilder;
    }
}
