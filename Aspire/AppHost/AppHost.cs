using AppHost.Configs;
using AppHost.Extensions;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddAdditionalConfigurationFiles();

// Load infrastructure configs from configuration
var mongoConfig = builder.Configuration.GetSection("Mongo").Get<MongoConfig>();
var postgresConfig = builder.Configuration.GetSection("Postgres").Get<PostgresConfig>();
var redisConfig = builder.Configuration.GetSection("Redis").Get<RedisConfig>();
var redisCommanderConfig = builder.Configuration.GetSection("RedisCommander").Get<RedisCommanderConfig>();
var catalogApiConfig = builder.Configuration.GetSection("CatalogApi").Get<CatalogApiConfig>();
var basketApiConfig = builder.Configuration.GetSection("BasketApi").Get<BasketApiConfig>();

var mongoUsername = builder.AddParameter("mongoDbUsername", value: mongoConfig?.RootUsername ?? "root");
var mongoPassword = builder.AddParameter("mongoDbPassword", value: mongoConfig?.RootPassword ?? "123456", secret: true);

var postgresUsername = builder.AddParameter("postgresDbUsername", value: postgresConfig?.User ?? "postgres");
var postgresPassword = builder.AddParameter("postgresDbPassword", value: postgresConfig?.Password ?? "123456", secret: true);

var mongo = builder.AddMongoDB("mongo", mongoConfig?.Port ?? 27017, mongoUsername, mongoPassword).WithImage("mongo:7.0.14")
.WithEndpoint(port: mongoConfig?.Port ?? 27017, targetPort: mongoConfig?.Port ?? 27017, name: "mongodb")
.WithLifetime(ContainerLifetime.Persistent);

var mongoDb = mongo.AddDatabase("mongodb");

var postgres = builder.AddPostgres("postgres").WithImage("postgres:16.4")
.WithUserName(postgresUsername)
.WithPassword(postgresPassword)
.WithPgAdmin()
.WithEndpoint(port: postgresConfig?.Port ?? 5432, targetPort: postgresConfig?.Port ?? 5432, name: "BasketDb") // consider remove all WithEndpoint
.WithLifetime(ContainerLifetime.Persistent)
.AddDatabase("BasketDb");

// Redis distributed cache
var redis = builder.AddContainer("distributedcache", redisConfig?.Image ?? "redis")
    .WithEndpoint(port: redisConfig?.Port ?? 6379, targetPort: redisConfig?.Port ?? 6379, name: "redis")
    .WithLifetime(ContainerLifetime.Persistent);

// Redis Commander
// Example: Instead Add built-in method for Redis, Postgres, Mongo we can use like this to add container.
// Cons: Manual add, more complex.
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
    .WithEnvironment("MongoDb__Host", catalogApiConfig?.MongoDb?.Host ?? "")
    .WithEnvironment("MongoDb__Credentials__UserName", catalogApiConfig?.MongoDb?.Credentials?.UserName ?? "")
    .WithEnvironment("MongoDb__Credentials__Password", catalogApiConfig?.MongoDb?.Credentials?.Password ?? "")
    .WithHttpEndpoint(port: 6000, targetPort: 8080, name: "catalog-http")
    .WithHttpsEndpoint(port: 6060, targetPort: 8081, name: "catalog-https")
    .WaitFor(mongoDb);

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
