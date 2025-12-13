using AppHost.Configs;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace AppHost.Extensions;

public static class AppHostBuilderExtensions
{
    // Get default port for postgres services
    private static int GetDefaultPort(string name) => name switch
    {
        "catalog" => 5433,
        "basket" => 5434,
        _ => 5432
    };

    // Helper method to reduce duplication in postgres database configuration
    public static IResourceBuilder<PostgresDatabaseResource> AddPostgresDatabase(
        this IDistributedApplicationBuilder builder,
        string name,
        PostgresInst config,
        IResourceBuilder<ParameterResource> username,
        IResourceBuilder<ParameterResource> password)
    {
        return builder.AddPostgres(name)
            .WithImage(config?.Image ?? "postgres:16.4")
            .WithUserName(username)
            .WithPassword(password)
            .WithPgAdmin()
            .WithEndpoint(
                port: config?.Port ?? GetDefaultPort(name),
                targetPort: config?.TargetPort ?? 5432,
                name: $"{name}Db")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithVolume(config?.Volume ?? $"{name}-data", "/var/lib/postgresql/data")
            .AddDatabase(config?.Db ?? $"{name}Db");
    }
}
