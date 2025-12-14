using AppHost;
using AppHost.PathConstants;
using AppHost.Utils;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

// AppHost - Microservices orchestration with .NET Aspire

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> catalogDb = builder.AddCatalogDatabase("Database");
IResourceBuilder<RedisResource> catalogCache = builder.AddCatalogCache();

AllowedHostsValidator.ValidateAllServices(Constants.ServicesPath);

IResourceBuilder<ProjectResource> catalogApi = builder.AddCatalogApi("catalog", catalogCache, catalogDb);

builder.Build().Run();
