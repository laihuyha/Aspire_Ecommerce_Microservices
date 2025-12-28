using System;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Docker;
using AppHost.Options;
using AppHost.Utils;
using AppHost.Extensions;
using Projects;

namespace AppHost.Extensions
{
    /// <summary>
    /// Extensions for Catalog service components
    /// </summary>
    public static class CatalogServiceExtensions
    {
        public static IResourceBuilder<RedisResource> AddCatalogCache(
            this IDistributedApplicationBuilder builder)
        {
            // Get configuration options for the cache
            var mergedConfig = AppHostConfiguration.GetMergedConfiguration(builder.Configuration);
            var options = ServiceConfigurationHelper.GetCacheOptions(mergedConfig);

            return builder.AddRedis("distributedcache")
                .WithImage(options.Image)
                .WithDataVolume(options.VolumeName)
                .WithPersistence(options.PersistenceInterval, options.PersistenceKeys)
                .WithArgs("--maxmemory", options.MaxMemory, "--maxmemory-policy", options.MaxMemoryPolicy)
                .WithRedisCommander()
                .PublishAsDockerComposeService((resource, service) => { service.Name = "distributedcache"; });
        }

        public static IResourceBuilder<ProjectResource> AddCatalogApi(
            this IDistributedApplicationBuilder builder,
            string serviceName)
        {
            // Service methods handle their own configuration - create dependencies too
            var database = builder.AddServiceDatabase(serviceName, "Database");
            var cache = builder.AddCatalogCache();

            // Get configuration options for this service
            var mergedConfig = AppHostConfiguration.GetMergedConfiguration(builder.Configuration);
#pragma warning disable CS0618 // Type or member is obsolete - keeping for backward compatibility
            var apiOptions = ServiceConfigurationHelper.GetCatalogApiOptions(mergedConfig);
#pragma warning restore CS0618
            var httpsCertOptions = ServiceConfigurationHelper.GetHttpsCertificateOptions(mergedConfig);

            // External ports: what host exposes (default from env or parameter)
            int httpPort = apiOptions.ExternalHttpPort ?? int.Parse(Environment.GetEnvironmentVariable("CATALOG_HTTP_PORT") ?? "6000");
            int httpsPort = apiOptions.ExternalHttpsPort ?? int.Parse(Environment.GetEnvironmentVariable("CATALOG_HTTPS_PORT") ?? "6060");

            return builder.AddProject<Catalog_API>($"{serviceName}-api")
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                .WithReference(database)
                .WithReference(cache)
                .WithHttpEndpoint(httpPort, apiOptions.InternalHttpPort, $"{serviceName}-http")
                .WithHttpsEndpoint(httpsPort, apiOptions.InternalHttpsPort, $"{serviceName}-https")
                .WaitFor(database)
                .PublishAsDockerComposeService((resource, service) =>
                {
                    service.Name = $"{serviceName}-api";
                    // Fix duplicate port issue: use single internal port value
                    service.Environment["HTTP_PORTS"] = apiOptions.InternalHttpPort.ToString();
                    service.Environment["HTTPS_PORTS"] = apiOptions.InternalHttpsPort.ToString();

                    // Configure HTTPS certificate from options
                    service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Path"] = httpsCertOptions.CertificatePath;
                    service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Password"] = httpsCertOptions.CertificatePassword;
                    service.Environment["ASPNETCORE_Kestrel__Certificates__Default__AllowInvalid"] = httpsCertOptions.AllowInvalid.ToString().ToLowerInvariant();
                })
                .WithBakedInHttpsCertificate(httpsCertOptions); // Use configured certificate options
        }
    }
}
