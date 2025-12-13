using System;
using AppHost;
using Aspire.Hosting;

// AppHost - Microservices orchestration with .NET Aspire

var builder = DistributedApplication.CreateBuilder(args);

var catalogDb = builder.AddCatalogDatabase("catalogDb");
var catalogCache = builder.AddCatalogCache();
var catalogApi = builder.AddCatalogApi("catalog", catalogCache, catalogDb);

builder.Build().Run();
