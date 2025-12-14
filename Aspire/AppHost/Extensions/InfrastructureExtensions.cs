using System;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Projects;

namespace AppHost
{
    public static class InfrastructureExtensions
    {
        public static IResourceBuilder<PostgresDatabaseResource> AddCatalogDatabase(
            this IDistributedApplicationBuilder builder,
            string databaseName)
        {
            IResourceBuilder<ParameterResource> username = builder.AddParameter("postgresUsername",
                Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "postgres");
            IResourceBuilder<ParameterResource> password = builder.AddParameter("postgresPassword",
                Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "dev_password_123", secret: true);

            return builder.AddPostgres("catalog-postgres")
                .WithImage("postgres:16-alpine")
                .WithUserName(username)
                .WithPassword(password)
                .WithPgAdmin()
                .WithVolume("catalog_data", "/var/lib/postgresql/data")
                .AddDatabase(databaseName);
        }

        public static IResourceBuilder<RedisResource> AddCatalogCache(
            this IDistributedApplicationBuilder builder)
        {
            return builder.AddRedis("distributedcache")
                .WithImage("redis:7-alpine")
                .WithDataVolume("redis_data")
                .WithPersistence(TimeSpan.FromMinutes(5), 100)
                .WithArgs("--maxmemory", "128mb", "--maxmemory-policy", "allkeys-lru")
                .WithRedisCommander();
        }

        public static IResourceBuilder<ProjectResource> AddCatalogApi(
            this IDistributedApplicationBuilder builder,
            string serviceName,
            IResourceBuilder<RedisResource> cache,
            IResourceBuilder<PostgresDatabaseResource> database)
        {
            int httpPort = int.Parse(Environment.GetEnvironmentVariable("CATALOG_HTTP_PORT") ?? "6000");
            int httpsPort = int.Parse(Environment.GetEnvironmentVariable("CATALOG_HTTPS_PORT") ?? "6060");

            return builder.AddProject<Catalog_API>($"{serviceName}-api")
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8080")
                .WithEnvironment("ASPNETCORE_HTTPS_PORTS", "8081")
                .WithReference(database)
                .WithReference(cache)
                .WithHttpEndpoint(httpPort, 8080, $"{serviceName}-http")
                .WithHttpsEndpoint(httpsPort, 8081, $"{serviceName}-https")
                .WaitFor(database);
        }
    }
}
