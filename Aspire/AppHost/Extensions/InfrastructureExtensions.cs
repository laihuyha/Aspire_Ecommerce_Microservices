using System;
using System.Globalization;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Docker;
using AppHost.Options;
using Projects;
using AppHost.Extensions; // Add using for the extension methods

namespace AppHost
{
    public static class InfrastructureExtensions
    {
        public static IResourceBuilder<PostgresDatabaseResource> AddServiceDatabase(
            this IDistributedApplicationBuilder builder,
            string serviceName,
            string databaseName,
            DatabaseOptions options = null)
        {
            options ??= new DatabaseOptions();

            // For now, only support PostgreSQL as that's what's available in Aspire
            // The configuration merging allows services to specify different connection details
            if (options.Type != DatabaseType.PostgreSQL)
            {
                throw new NotSupportedException($"Database type {options.Type} is not supported. Only PostgreSQL is currently available.");
            }

            IResourceBuilder<ParameterResource> username = builder.AddParameter($"{serviceName}Username",
                Environment.GetEnvironmentVariable($"{serviceName.ToUpperInvariant()}_USERNAME") ?? options.Username);
            IResourceBuilder<ParameterResource> password = builder.AddParameter($"{serviceName}Password",
                Environment.GetEnvironmentVariable($"{serviceName.ToUpperInvariant()}_PASSWORD") ?? options.Password, secret: true);

            return builder.AddPostgres($"{serviceName}-postgres")
                .WithImage(options.Image)
                .WithUserName(username)
                .WithPassword(password)
                .WithPgAdmin()
                .WithVolume(options.VolumeName, options.DataPath)
                .PublishAsDockerComposeService((resource, service) => { service.Name = $"{serviceName}-postgres"; })
                .AddDatabase(databaseName);
        }

        // Backward compatibility method
        public static IResourceBuilder<PostgresDatabaseResource> AddCatalogDatabase(
            this IDistributedApplicationBuilder builder,
            string databaseName)
        {
            var options = new DatabaseOptions { Type = DatabaseType.PostgreSQL };
            return (IResourceBuilder<PostgresDatabaseResource>)AddServiceDatabase(builder, "catalog", databaseName, options);
        }

        public static IResourceBuilder<RedisResource> AddCatalogCache(
            this IDistributedApplicationBuilder builder,
            CacheOptions options = null)
        {
            options ??= new CacheOptions();

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
            string serviceName,
            IResourceBuilder<RedisResource> cache,
            IResourceBuilder<PostgresDatabaseResource> database,
            CatalogApiOptions options = null)
        {
            options ??= new CatalogApiOptions();

            // External ports: what host exposes (default from env or parameter)
            int httpPort = options.ExternalHttpPort ?? int.Parse(Environment.GetEnvironmentVariable("CATALOG_HTTP_PORT") ?? "6000");
            int httpsPort = options.ExternalHttpsPort ?? int.Parse(Environment.GetEnvironmentVariable("CATALOG_HTTPS_PORT") ?? "6060");

            return builder.AddProject<Catalog_API>($"{serviceName}-api")
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                .WithReference(database)
                .WithReference(cache)
                .WithHttpEndpoint(httpPort, options.InternalHttpPort, $"{serviceName}-http")
                .WithHttpsEndpoint(httpsPort, options.InternalHttpsPort, $"{serviceName}-https")
                .WaitFor(database)
                .PublishAsDockerComposeService((resource, service) =>
                {
                    service.Name = $"{serviceName}-api";
                    // Fix duplicate port issue: use single internal port value
                    service.Environment["HTTP_PORTS"] = options.InternalHttpPort.ToString();
                    service.Environment["HTTPS_PORTS"] = options.InternalHttpsPort.ToString();
                })
                .WithBakedInHttpsCertificate(); // Add baked-in HTTPS certificate configuration
        }
    }
}
