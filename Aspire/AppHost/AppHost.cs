using System;
using AppHost.Extensions;
using Aspire.Hosting;

// Main application host for .NET Aspire orchestration.
// Defines the distributed application with PostgreSQL, Redis, and microservices.

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddAdditionalConfigurationFiles();

// Temporarily removed InfraConfig loading for testing
// var infraConfig = builder.Configuration.GetSection("Infra").Get<InfraConfig>() ?? throw new InvalidOperationException("Infra config not found");

// Configuration variables with default values
int catalogApiPort = 6000;
int catalogApiHttpsPort = 6060;
int catalogApiTargetPort = 8080;
int catalogApiTargetHttpsPort = 8081;

int catalogDbPort = 5432;
string catalogDbName = "CatalogDb";
string catalogDbUser = "postgres";
string catalogDbPassword = "123456";
string catalogRedisPort = "6379";
var catalogConnectionString = $"Server=catalog;Port={catalogDbPort};Database={catalogDbName};User Id={catalogDbUser};Password={catalogDbPassword};Include Error Detail=true";

var postgresUsername = builder.AddParameter(
    "postgresDbUsername",
    value: "postgres");
var postgresPassword = builder.AddParameter(
    "postgresDbPassword",
    value: "123456",
    secret: true);

var catalog = AppHostBuilderExtensions.AddPostgresDatabase(
    builder,
    "catalog",
    null,
    postgresUsername,
    postgresPassword);

builder.AddRedis("redis")
    .WithDataVolume()
    .WithPersistence(
        interval: TimeSpan.FromMinutes(5),
        keysChangedThreshold: 100)
    .WithRedisCommander();

builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ConnectionStrings__CatalogDb", catalogConnectionString)
    .WithEnvironment("ConnectionStrings__Redis", $"redis:{catalogRedisPort}")
    .WithHttpEndpoint(port: catalogApiPort, targetPort: catalogApiTargetPort, name: "catalog-http")
    .WithHttpsEndpoint(port: catalogApiHttpsPort, targetPort: catalogApiTargetHttpsPort, name: "catalog-https")
    .WaitFor(catalog);

builder.Build().Run();
