using System;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace AppHost;

public static class InfrastructureExtensions
{
    public static IResourceBuilder<PostgresDatabaseResource> AddCatalogDatabase(
        this IDistributedApplicationBuilder builder,
        string databaseName)
    {
        var username = builder.AddParameter("postgresUsername",
            Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "postgres");
        var password = builder.AddParameter("postgresPassword",
            Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "dev_password_123", secret: true);

        return builder.AddPostgres("catalog-postgres")
            .WithImage("postgres:16-alpine")
            .WithUserName(username)
            .WithPassword(password)
            .WithPgAdmin()
            .WithEndpoint(port: 5433, targetPort: 5432, name: "catalogDb")
            .WithVolume("catalog_data", "/var/lib/postgresql/data")
            .AddDatabase(databaseName);
    }

    public static IResourceBuilder<RedisResource> AddCatalogCache(
        this IDistributedApplicationBuilder builder)
    {
        return builder.AddRedis("distributedcache")
            .WithImage("redis:7-alpine")
            .WithDataVolume("redis_data")
            .WithPersistence(interval: TimeSpan.FromMinutes(5), keysChangedThreshold: 100)
            .WithEndpoint(port: 6379, targetPort: 6379, name: "redis")
            .WithArgs("--maxmemory", "128mb", "--maxmemory-policy", "allkeys-lru")
            .WithRedisCommander();
    }

    public static IResourceBuilder<ProjectResource> AddCatalogApi(
        this IDistributedApplicationBuilder builder,
        string serviceName,
        IResourceBuilder<RedisResource> cache,
        IResourceBuilder<PostgresDatabaseResource> database)
    {
        var httpPort = int.Parse(Environment.GetEnvironmentVariable("CATALOG_HTTP_PORT") ?? "6000");
        var httpsPort = int.Parse(Environment.GetEnvironmentVariable("CATALOG_HTTPS_PORT") ?? "6060");

        return builder.AddProject<Projects.Catalog_API>($"{serviceName}-api")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8080")
            .WithEnvironment("ASPNETCORE_HTTPS_PORTS", "8081")
            .WithReference(database)
            .WithReference(cache)
            .WithHttpEndpoint(port: httpPort, targetPort: 8080, name: $"{serviceName}-http")
            .WithHttpsEndpoint(port: httpsPort, targetPort: 8081, name: $"{serviceName}-https")
            .WaitFor(database);
    }
}
