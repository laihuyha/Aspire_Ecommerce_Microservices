using AppHost.Extensions;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddAdditionalConfigurationFiles();

builder.AddProject<Projects.Catalog_API>("catalog-api").ConfigCatalogEnvironment();

builder.Build().Run();
