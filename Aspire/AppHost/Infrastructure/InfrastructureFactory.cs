using System;
using System.Collections.Generic;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Docker;
using AppHost.Abstractions;
using AppHost.Options;
using AppHost.Utils;

namespace AppHost.Infrastructure;

/// <summary>
/// Factory for creating and managing shared infrastructure resources.
/// Implements singleton pattern to ensure resources are reused across services.
/// </summary>
public sealed class InfrastructureFactory : IInfrastructureFactory
{
    private readonly Dictionary<string, IResourceBuilder<PostgresDatabaseResource>> _databases = new();
    private readonly Dictionary<string, IResourceBuilder<RedisResource>> _caches = new();
    private readonly object _lock = new();

    public static InfrastructureFactory Instance { get; } = new();

    private InfrastructureFactory() { }

    public IResourceBuilder<PostgresDatabaseResource> GetOrCreateDatabase(
        IDistributedApplicationBuilder builder,
        string serviceName,
        string databaseName)
    {
        string key = $"{serviceName}-{databaseName}".ToLowerInvariant();

        lock (_lock)
        {
            if (_databases.TryGetValue(key, out var existing))
            {
                return existing;
            }

            var mergedConfig = AppHostConfiguration.GetMergedConfiguration(builder.Configuration);
            var options = ServiceConfigurationHelper.GetServiceDatabaseOptions(mergedConfig, serviceName);
            ValidateDatabaseOptions(options, serviceName);

            var username = builder.AddParameter($"{serviceName}Username",
                Environment.GetEnvironmentVariable($"{serviceName.ToUpperInvariant()}_USERNAME") ?? options.Username);
            var password = builder.AddParameter($"{serviceName}Password",
                Environment.GetEnvironmentVariable($"{serviceName.ToUpperInvariant()}_PASSWORD") ?? options.Password, secret: true);

            var database = builder.AddPostgres($"{serviceName}-postgres")
                .WithImage(options.Image)
                .WithUserName(username)
                .WithPassword(password)
                .WithPgAdmin()
                .WithVolume(options.VolumeName, options.DataPath)
                .PublishAsDockerComposeService((resource, service) =>
                {
                    service.Name = $"{serviceName}-postgres";
                })
                .AddDatabase(databaseName);

            _databases[key] = database;
            return database;
        }
    }

    public IResourceBuilder<RedisResource> GetOrCreateCache(
        IDistributedApplicationBuilder builder,
        string cacheName = "distributedcache")
    {
        lock (_lock)
        {
            if (_caches.TryGetValue(cacheName, out var existing))
            {
                return existing;
            }

            var mergedConfig = AppHostConfiguration.GetMergedConfiguration(builder.Configuration);
            var options = ServiceConfigurationHelper.GetCacheOptions(mergedConfig);
            ValidateCacheOptions(options);

            var cache = builder.AddRedis(cacheName)
                .WithImage(options.Image)
                .WithDataVolume(options.VolumeName)
                .WithPersistence(options.PersistenceInterval, options.PersistenceKeys)
                .WithArgs("--maxmemory", options.MaxMemory, "--maxmemory-policy", options.MaxMemoryPolicy)
                .WithRedisCommander()
                .PublishAsDockerComposeService((resource, service) =>
                {
                    service.Name = cacheName;
                });

            _caches[cacheName] = cache;
            return cache;
        }
    }

    private static void ValidateDatabaseOptions(DatabaseOptions options, string serviceName)
    {
        if (options.Type != DatabaseType.PostgreSQL)
        {
            throw new NotSupportedException(
                $"Service '{serviceName}' requested database type '{options.Type}' which is not supported. " +
                "Only PostgreSQL is currently available in .NET Aspire.");
        }

        if (string.IsNullOrWhiteSpace(options.Username))
        {
            throw new InvalidOperationException($"Database username is required for service '{serviceName}'.");
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException($"Database password is required for service '{serviceName}'.");
        }
    }

    private static void ValidateCacheOptions(CacheOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Image))
        {
            throw new InvalidOperationException("Cache image is required.");
        }

        if (options.PersistenceKeys < 0)
        {
            throw new InvalidOperationException("Cache persistence keys must be non-negative.");
        }
    }
}
