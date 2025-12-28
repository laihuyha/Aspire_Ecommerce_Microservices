using System;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Docker;
using AppHost.Options;
using AppHost.Utils;
using Projects;

namespace AppHost
{
    /// <summary>
    /// Reusable infrastructure extensions that can be used by multiple services
    /// </summary>
    public static class InfrastructureExtensions
    {
        public static IResourceBuilder<PostgresDatabaseResource> AddServiceDatabase(
            this IDistributedApplicationBuilder builder,
            string serviceName,
            string databaseName)
        {
            // Get configuration options for this database service
            var mergedConfig = builder.Configuration;
            var options = ServiceConfigurationHelper.GetServiceDatabaseOptions(mergedConfig, serviceName);

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
            return AddServiceDatabase(builder, "catalog", databaseName);
        }
    }
}
