using System;
using AppHost.Configs;
using AppHost.Extensions;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;

// Main application host for .NET Aspire orchestration.
// Defines the distributed application with PostgreSQL, Redis, and microservices.

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddAdditionalConfigurationFiles();

// Load centralized infrastructure config
var infraConfig = builder.Configuration.GetSection("Infra").Get<InfraConfig>() ?? throw new InvalidOperationException("Infra config not found");

var postgresUsername = builder.AddParameter(
    "postgresDbUsername",
    value: infraConfig.Services?.Postgres?["catalog"]?.User ?? "postgres");
var postgresPassword = builder.AddParameter(
    "postgresDbPassword",
    value: infraConfig.Services?.Postgres?["catalog"]?.Password ?? "123456",
    secret: true);

var catalog = AddPostgresDatabase(
    builder,
    "catalog",
    infraConfig.Services?.Postgres?["catalog"],
    postgresUsername,
    postgresPassword);

var basket = AddPostgresDatabase(
    builder,
    "basket",
    infraConfig.Services?.Postgres?["basket"],
    postgresUsername,
    postgresPassword);

var redis = builder.AddContainer("distributedcache", infraConfig.Services?.Redis?.Image ?? "redis:latest")
    .WithEndpoint(port: infraConfig.Services?.Redis?.Port ?? 6379, targetPort: infraConfig.Services?.Redis?.TargetPort ?? 6379, name: "redis")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithVolume(infraConfig.Services?.Redis?.Volume ?? "redis-data", "/data");

builder.AddContainer("redis-commander", infraConfig.Services?.RedisCommander?.Image ?? "rediscommander/redis-commander:latest")
    .WithEnvironment("REDIS_HOSTS", infraConfig.Services?.RedisCommander?.RedisHosts ?? "local:distributedcache:6379")
    .WithEnvironment("HTTP_USER", infraConfig.Services?.RedisCommander?.HttpUser ?? "admin")
    .WithEnvironment("HTTP_PASSWORD", infraConfig.Services?.RedisCommander?.HttpPassword ?? "devpassword123")
    .WithEndpoint(port: infraConfig.Services?.RedisCommander?.Port ?? 7001, targetPort: infraConfig.Services?.RedisCommander?.TargetPort ?? 8081, name: "redis-commander")
    .WaitFor(redis);

int catalogApiPort = infraConfig.Apis?.Catalog?.HttpPort ?? 6000;
int catalogApiHttpsPort = infraConfig.Apis?.Catalog?.HttpsPort ?? 6060;
int catalogApiTargetPort = infraConfig.Apis?.Catalog?.TargetHttpPort ?? 8080;
int catalogApiTargetHttpsPort = infraConfig.Apis?.Catalog?.TargetHttpsPort ?? 8081;

int catalogDbPort = infraConfig.Services?.Postgres?["catalog"]?.TargetPort ?? 5432;
string catalogDbName = infraConfig.Services?.Postgres?["catalog"]?.Db ?? "CatalogDb";
string catalogDbUser = infraConfig.Services?.Postgres?["catalog"]?.User ?? "postgres";
string catalogDbPassword = infraConfig.Services?.Postgres?["catalog"]?.Password ?? "123456";
string catalogRedisPort = infraConfig.Services?.Redis?.TargetPort.ToString() ?? "6379";

var catalogConnectionString = $"Server=catalog;Port={catalogDbPort};Database={catalogDbName};User Id={catalogDbUser};Password={catalogDbPassword};Include Error Detail=true";
builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", infraConfig.Apis?.Catalog?.Environment ?? "Development")
    .WithEnvironment("ConnectionStrings__CatalogDb", catalogConnectionString)
    .WithEnvironment("ConnectionStrings__Redis", $"distributedcache:{catalogRedisPort}")
    .WithHttpEndpoint(port: catalogApiPort, targetPort: catalogApiTargetPort, name: "catalog-http")
    .WithHttpsEndpoint(port: catalogApiHttpsPort, targetPort: catalogApiTargetHttpsPort, name: "catalog-https")
    .WaitFor(catalog);

// Get default port for postgres services
static int GetDefaultPort(string name) => name switch
{
    "catalog" => 5433,
    "basket" => 5434,
    _ => 5432
};

// Helper method to reduce duplication in postgres database configuration
static IResourceBuilder<PostgresDatabaseResource> AddPostgresDatabase(
    IDistributedApplicationBuilder builder,
    string name,
    PostgresInst? config,
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

builder.Build().Run();
