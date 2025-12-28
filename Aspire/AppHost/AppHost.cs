using System;
using System.IO;
using System.Linq;
using AppHost;
using AppHost.PathConstants;
using AppHost.Utils;
using AppHost.Extensions;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;

// AppHost - Microservices orchestration with .NET Aspire

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Add Docker Compose environment for deployment
builder.AddDockerComposeEnvironment("aspire-ecommerce");

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

// Setup merged configuration for certificate setup
var mergedConfig = mergedConfigBuilder.Build();

// Validate allowed hosts for all services
var allowedHostsOptions = ServiceConfigurationHelper.GetAllowedHostsValidationOptions(mergedConfig);
AllowedHostsValidator.ValidateAllServices(Constants.ServicesPath, allowedHostsOptions);

// Setup HTTPS certificates if enabled in configuration
string projectRoot = PathHelper.GetProjectRootPathFromBaseDirectory();
var certificateOptions = ServiceConfigurationHelper.GetCertificateSetupOptions(mergedConfig);
SelfSignCertificateSetup.SetupIfEnabled(certificateOptions, projectRoot);

// Service methods handle their own configuration and dependencies
IResourceBuilder<ProjectResource> catalogApi = builder.AddCatalogApi("catalog");

builder.Build().Run();
