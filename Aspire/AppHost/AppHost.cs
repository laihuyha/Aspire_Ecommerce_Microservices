using System;
using AppHost.Configs;
using AppHost.Extensions;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddAdditionalConfigurationFiles();

var rnd = Guid.NewGuid().ToString();

// Load infrastructure configs from configuration
var postgresConfig = builder.Configuration.GetSection("Postgres").Get<PostgresConfig>();
var redisConfig = builder.Configuration.GetSection("Redis").Get<RedisConfig>();
var redisCommanderConfig = builder.Configuration.GetSection("RedisCommander").Get<RedisCommanderConfig>();
var catalogApiConfig = builder.Configuration.GetSection("CatalogApi").Get<CatalogApiConfig>();
var basketApiConfig = builder.Configuration.GetSection("BasketApi").Get<BasketApiConfig>();

var postgresUsername = builder.AddParameter("postgresDbUsername", value: postgresConfig?.User ?? "postgres");
var postgresPassword = builder.AddParameter("postgresDbPassword", value: postgresConfig?.Password ?? "123456", secret: true);

var catalog = builder.AddPostgres("catalog")
    .WithImage("postgres:16.4")
    .WithUserName(postgresUsername)
    .WithPassword(postgresPassword)
    .WithPgAdmin()
    .WithEndpoint(port: 5433, targetPort: 5432, name: "CatalogDb") // Host: localhost:5433
    .WithLifetime(ContainerLifetime.Persistent)
    .WithVolume("catalog-data", "/var/lib/postgresql/data")
    .WithEnvironment("FORCE_RESTART", rnd) // trick: mỗi lần app chạy GUID khác → container restart
    .AddDatabase("CatalogDb");

var basket = builder.AddPostgres("basket")
    .WithImage("postgres:16.4")
    .WithUserName(postgresUsername)
    .WithPassword(postgresPassword)
    .WithPgAdmin()
    .WithEndpoint(port: 5434, targetPort: 5432, name: "BasketDb") // Host: localhost:5434
    .WithLifetime(ContainerLifetime.Persistent)
    .WithVolume("basket-data", "/var/lib/postgresql/data")
    .WithEnvironment("FORCE_RESTART", rnd)
    .AddDatabase("BasketDb");

var redis = builder.AddContainer("distributedcache", redisConfig?.Image ?? "redis:latest")
    .WithEndpoint(port: 6379, targetPort: 6379, name: "redis") // Host: localhost:6379
    .WithLifetime(ContainerLifetime.Persistent)
    .WithVolume("redis-data", "/var/lib/redis/data")
    .WithEnvironment("FORCE_RESTART", rnd);

builder.AddContainer("redis-commander", redisCommanderConfig?.Image ?? "rediscommander/redis-commander:latest")
    .WithEnvironment("REDIS_HOSTS", redisCommanderConfig?.RedisHosts ?? "BasketCache:distributedcache:6379")
    .WithEnvironment("HTTP_USER", redisCommanderConfig?.HttpUser ?? "root")
    .WithEnvironment("HTTP_PASSWORD", redisCommanderConfig?.HttpPassword ?? "secret")
    .WithEndpoint(port: redisCommanderConfig?.Port ?? 7001, targetPort: redisCommanderConfig?.TargetPort ?? 8081, name: "redis-commander")
    .WaitFor(redis);

// Catalog API
builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", catalogApiConfig?.Environment ?? "Development")
    .WithEnvironment("ConnectionStrings__Database", catalogApiConfig?.ConnectionStrings?.Database ?? "")
    .WithHttpEndpoint(port: 6000, targetPort: 8080, name: "catalog-http")
    .WithHttpsEndpoint(port: 6060, targetPort: 8081, name: "catalog-https")
    .WaitFor(catalog);

// Basket API
// builder.AddProject("basket-api", "../../Services/Basket/Basket.API/Basket.API.csproj")
//     .WithEnvironment("ASPNETCORE_ENVIRONMENT", basketApiConfig?.Environment ?? "Development")
//     .WithEnvironment("ASPNETCORE_HTTP_PORTS", basketApiConfig?.HttpPort?.ToString() ?? "8080")
//     .WithEnvironment("ASPNETCORE_HTTPS_PORTS", basketApiConfig?.HttpsPort?.ToString() ?? "8081")
//     .WithEnvironment("ConnectionStrings__Marten", basketApiConfig?.ConnectionStrings?.Marten ?? "")
//     .WithEnvironment("ConnectionStrings__Redis", basketApiConfig?.ConnectionStrings?.Redis ?? "")
//     .WithHttpEndpoint(port: 6001, targetPort: basketApiConfig?.HttpPort ?? 8082, name: "basket-http")
//     .WithHttpsEndpoint(port: 6061, targetPort: basketApiConfig?.HttpsPort ?? 8083, name: "basket-https")
//     .WaitFor(postgres)
//     .WaitFor(redis);

builder.Build().Run();
