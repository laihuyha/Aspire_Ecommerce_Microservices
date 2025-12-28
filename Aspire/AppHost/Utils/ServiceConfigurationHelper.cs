using System;
using AppHost.Options;
using Microsoft.Extensions.Configuration;

namespace AppHost.Utils;

/// <summary>
/// Helper class for extracting service-specific configuration options.
/// Provides a unified way to access configuration with fallback patterns.
/// </summary>
public static class ServiceConfigurationHelper
{
    /// <summary>
    /// Gets port options for a specific service.
    /// Lookup order: Services:{serviceName}:Ports -> {serviceName}Api -> Default
    /// </summary>
    public static ServicePortOptions GetServicePortOptions(IConfiguration config, string serviceName)
    {
        var options = config.GetSection($"Services:{serviceName}:Ports").Get<ServicePortOptions>()
                      ?? config.GetSection($"{serviceName}Api").Get<ServicePortOptions>()
                      ?? GetServicePortOptionsFromLegacy(config, serviceName)
                      ?? new ServicePortOptions();

        options.Validate(serviceName);
        return options;
    }

    /// <summary>
    /// Backward compatibility: converts CatalogApiOptions to ServicePortOptions
    /// </summary>
    private static ServicePortOptions GetServicePortOptionsFromLegacy(IConfiguration config, string serviceName)
    {
        if (!serviceName.Equals("catalog", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var legacyOptions = config.GetSection("CatalogApi").Get<CatalogApiOptions>();
        if (legacyOptions == null)
        {
            return null;
        }

        return new ServicePortOptions
        {
            ExternalHttpPort = legacyOptions.ExternalHttpPort,
            ExternalHttpsPort = legacyOptions.ExternalHttpsPort,
            InternalHttpPort = legacyOptions.InternalHttpPort,
            InternalHttpsPort = legacyOptions.InternalHttpsPort
        };
    }

    /// <summary>
    /// Gets CatalogApi options from merged configuration.
    /// </summary>
    [Obsolete("Use GetServicePortOptions instead for new services.")]
    public static CatalogApiOptions GetCatalogApiOptions(IConfiguration mergedConfig)
    {
        return mergedConfig.GetSection("Services:Catalog:CatalogApi").Get<CatalogApiOptions>()
               ?? mergedConfig.GetSection("CatalogApi").Get<CatalogApiOptions>()
               ?? new CatalogApiOptions();
    }

    /// <summary>
    /// Gets database options for a specific service.
    /// Lookup order: Services:{serviceName}:Database -> Database -> Default
    /// </summary>
    public static DatabaseOptions GetServiceDatabaseOptions(IConfiguration config, string serviceName)
    {
        return config.GetSection($"Services:{serviceName}:Database").Get<DatabaseOptions>()
               ?? config.GetSection("Database").Get<DatabaseOptions>()
               ?? new DatabaseOptions();
    }

    /// <summary>
    /// Gets cache options from merged configuration.
    /// </summary>
    public static CacheOptions GetCacheOptions(IConfiguration config)
    {
        return config.GetSection("Cache").Get<CacheOptions>() ?? new CacheOptions();
    }

    /// <summary>
    /// Gets HTTPS certificate options from merged configuration.
    /// </summary>
    public static HttpsCertificateOptions GetHttpsCertificateOptions(IConfiguration config)
    {
        var options = config.GetSection("HttpsCertificate").Get<HttpsCertificateOptions>()
                      ?? new HttpsCertificateOptions();
        options.Validate();
        return options;
    }

    /// <summary>
    /// Gets certificate setup options from merged configuration.
    /// </summary>
    public static CertificateSetupOptions GetCertificateSetupOptions(IConfiguration config)
    {
        return config.GetSection("CertificateSetup").Get<CertificateSetupOptions>()
               ?? new CertificateSetupOptions();
    }

    /// <summary>
    /// Gets allowed hosts validation options from merged configuration.
    /// </summary>
    public static AllowedHostsValidationOptions GetAllowedHostsValidationOptions(IConfiguration config)
    {
        return config.GetSection("AllowedHostsValidation").Get<AllowedHostsValidationOptions>()
               ?? new AllowedHostsValidationOptions();
    }
}
