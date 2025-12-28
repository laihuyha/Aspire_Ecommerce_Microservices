using System;
using System.IO;
using System.Linq;
using AppHost;
using AppHost.Options;
using AppHost.PathConstants;
using AppHost.Utils;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// AppHost - Microservices orchestration with .NET Aspire

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Collect and merge service configurations
string servicesPath = Constants.ServicesPath;
string[] serviceDirectories = Directory.Exists(servicesPath)
    ? Directory.GetDirectories(servicesPath).Select(Path.GetFileName).ToArray()
    : Array.Empty<string>();

// Create configuration builder with all AppHost config files
var apphostConfigBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("infrastructure.json", optional: true)
    .AddJsonFile("validation.json", optional: true)
    .AddJsonFile("appsettings.Development.json", optional: true);

// Merge service configurations with the AppHost configuration
var mergedConfigBuilder = ConfigurationMerger.MergeServiceConfigurations(
    new ConfigurationBuilder().AddConfiguration(apphostConfigBuilder.Build()),
    Path.GetDirectoryName(servicesPath),
    serviceDirectories);

// Use the merged configuration for options configuration
var mergedConfig = mergedConfigBuilder.Build();

// Configure options from merged configuration
builder.Services.Configure<CertificateSetupOptions>(
    mergedConfig.GetSection("CertificateSetup"));
builder.Services.Configure<AllowedHostsValidationOptions>(
    mergedConfig.GetSection("AllowedHostsValidation"));
builder.Services.Configure<DatabaseOptions>(
    mergedConfig.GetSection("Database"));
builder.Services.Configure<CacheOptions>(
    mergedConfig.GetSection("Cache"));
builder.Services.Configure<HttpsCertificateOptions>(
    mergedConfig.GetSection("HttpsCertificate"));

// Validate allowed hosts for all services
var allowedHostsOptions = mergedConfig.GetSection("AllowedHostsValidation").Get<AllowedHostsValidationOptions>() ??
                          new AllowedHostsValidationOptions();
AllowedHostsValidator.ValidateAllServices(Constants.ServicesPath, allowedHostsOptions);

// Setup HTTPS certificates if enabled in configuration
string projectRoot = PathHelper.GetProjectRootPathFromBaseDirectory();

// Get the configured options and setup certificates
var certificateOptions = mergedConfig.GetSection("CertificateSetup").Get<CertificateSetupOptions>() ??
                         new CertificateSetupOptions();
SelfSignCertificateSetup.SetupIfEnabled(certificateOptions, projectRoot);

var databaseOptions = mergedConfig.GetSection("Database").Get<DatabaseOptions>() ?? new DatabaseOptions();
var cacheOptions = mergedConfig.GetSection("Cache").Get<CacheOptions>() ?? new CacheOptions();

// Example: Different services can have different database configurations
// Catalog service gets its database config (may be overridden by Services:Catalog:Database)
var catalogDbOptions = mergedConfig.GetSection("Services:Catalog:Database").Get<DatabaseOptions>() ??
                      mergedConfig.GetSection("Database").Get<DatabaseOptions>() ??
                      new DatabaseOptions();

IResourceBuilder<PostgresDatabaseResource> catalogDb = builder.AddServiceDatabase("catalog", "Database", catalogDbOptions);
IResourceBuilder<RedisResource> catalogCache = builder.AddCatalogCache(cacheOptions);

IResourceBuilder<ProjectResource> catalogApi = builder.AddCatalogApi("catalog", catalogCache, catalogDb);

builder.Build().Run();
