using System;
using System.IO;
using System.Linq;
using AppHost.PathConstants;
using AppHost.Services;
using AppHost.Utils;
using Aspire.Hosting;
using Microsoft.Extensions.Configuration;

namespace AppHost.Extensions;

/// <summary>
/// Extension methods for IDistributedApplicationBuilder providing fluent configuration.
/// </summary>
public static class DistributedApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the application with merged service configurations.
    /// </summary>
    public static IDistributedApplicationBuilder WithMergedConfiguration(this IDistributedApplicationBuilder builder)
    {
        string servicesPath = Constants.ServicesPath;
        string[] serviceDirectories = Directory.Exists(servicesPath)
            ? Directory.GetDirectories(servicesPath).Select(Path.GetFileName).ToArray()
            : Array.Empty<string>();

        // Create configuration builder with all AppHost config files
        var apphostConfigBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("infrastructure.json", optional: true)
            .AddJsonFile("validation.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true);

        // Merge service configurations with the AppHost configuration
        ConfigurationMerger.MergeServiceConfigurations(
            new ConfigurationBuilder().AddConfiguration(apphostConfigBuilder.Build()),
            Path.GetDirectoryName(servicesPath),
            serviceDirectories);

        return builder;
    }

    /// <summary>
    /// Validates allowed hosts configuration for all services.
    /// </summary>
    public static IDistributedApplicationBuilder WithAllowedHostsValidation(this IDistributedApplicationBuilder builder)
    {
        var options = ServiceConfigurationHelper.GetAllowedHostsValidationOptions(builder.Configuration);
        AllowedHostsValidator.ValidateAllServices(Constants.ServicesPath, options);
        return builder;
    }

    /// <summary>
    /// Sets up HTTPS certificates if enabled in configuration.
    /// </summary>
    public static IDistributedApplicationBuilder WithHttpsCertificateSetup(this IDistributedApplicationBuilder builder)
    {
        string projectRoot = PathHelper.GetProjectRootPathFromBaseDirectory();
        var certificateOptions = ServiceConfigurationHelper.GetCertificateSetupOptions(builder.Configuration);
        SelfSignCertificateSetup.SetupIfEnabled(certificateOptions, projectRoot);
        return builder;
    }

    /// <summary>
    /// Registers all services using the service registry.
    /// </summary>
    public static IDistributedApplicationBuilder WithServices(
        this IDistributedApplicationBuilder builder,
        Action<ServiceRegistry> configure = null)
    {
        var registry = ServiceRegistry.CreateDefault();
        configure?.Invoke(registry);
        registry.RegisterAll(builder);
        return builder;
    }

    /// <summary>
    /// Adds Docker Compose environment for deployment.
    /// </summary>
    public static IDistributedApplicationBuilder WithDockerComposeSupport(
        this IDistributedApplicationBuilder builder,
        string environmentName = "aspire-ecommerce")
    {
        builder.AddDockerComposeEnvironment(environmentName);
        return builder;
    }

    /// <summary>
    /// Applies all default configurations in the recommended order.
    /// </summary>
    public static IDistributedApplicationBuilder WithDefaultConfiguration(this IDistributedApplicationBuilder builder)
    {
        return builder
            .WithDockerComposeSupport()
            .WithMergedConfiguration()
            .WithAllowedHostsValidation()
            .WithHttpsCertificateSetup();
    }
}
