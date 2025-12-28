using AppHost.Options;
using Microsoft.Extensions.Configuration;

namespace AppHost.Utils;

/// <summary>
/// Helper class for extracting service-specific configuration options
/// </summary>
public static class ServiceConfigurationHelper
{
    /// <summary>
    /// Gets CatalogApi options from merged configuration
    /// </summary>
    public static CatalogApiOptions GetCatalogApiOptions(IConfiguration mergedConfig)
    {
        return mergedConfig.GetSection("Services:Catalog:CatalogApi").Get<CatalogApiOptions>() ??
               mergedConfig.GetSection("CatalogApi").Get<CatalogApiOptions>() ??
               new CatalogApiOptions();
    }

    /// <summary>
    /// Gets database options for a specific service
    /// </summary>
    public static DatabaseOptions GetServiceDatabaseOptions(IConfiguration mergedConfig, string serviceName)
    {
        return mergedConfig.GetSection($"Services:{serviceName}:Database").Get<DatabaseOptions>() ??
               mergedConfig.GetSection("Database").Get<DatabaseOptions>() ??
               new DatabaseOptions();
    }

    /// <summary>
    /// Gets cache options from merged configuration
    /// </summary>
    public static CacheOptions GetCacheOptions(IConfiguration mergedConfig)
    {
        return mergedConfig.GetSection("Cache").Get<CacheOptions>() ?? new CacheOptions();
    }

    /// <summary>
    /// Gets HTTPS certificate options from merged configuration
    /// </summary>
    public static HttpsCertificateOptions GetHttpsCertificateOptions(IConfiguration mergedConfig)
    {
        return mergedConfig.GetSection("HttpsCertificate").Get<HttpsCertificateOptions>() ?? new HttpsCertificateOptions();
    }

    /// <summary>
    /// Gets certificate setup options from merged configuration
    /// </summary>
    public static CertificateSetupOptions GetCertificateSetupOptions(IConfiguration mergedConfig)
    {
        return mergedConfig.GetSection("CertificateSetup").Get<CertificateSetupOptions>() ?? new CertificateSetupOptions();
    }

    /// <summary>
    /// Gets allowed hosts validation options from merged configuration
    /// </summary>
    public static AllowedHostsValidationOptions GetAllowedHostsValidationOptions(IConfiguration mergedConfig)
    {
        return mergedConfig.GetSection("AllowedHostsValidation").Get<AllowedHostsValidationOptions>() ?? new AllowedHostsValidationOptions();
    }
}
