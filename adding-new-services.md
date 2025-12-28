# Adding New Services Guide

This guide explains how to add new microservices to the Aspire e-commerce platform using the Service Definition pattern.

## Architecture Overview

The AppHost uses several design patterns for maintainability and extensibility:

```
Aspire/AppHost/
├── Abstractions/
│   ├── IServiceDefinition.cs      # Interface for service definitions
│   ├── IInfrastructureFactory.cs  # Factory interface for infrastructure
│   └── ServiceDefinitionBase.cs   # Base class with common logic
├── Infrastructure/
│   └── InfrastructureFactory.cs   # Singleton factory for DB/Cache resources
├── Services/
│   ├── CatalogServiceDefinition.cs # Example service implementation
│   └── ServiceRegistry.cs          # Central registry for all services
├── Extensions/
│   └── DistributedApplicationBuilderExtensions.cs  # Fluent API
└── Options/
    ├── ServicePortOptions.cs       # Port configuration
    └── HttpsCertificateOptions.cs  # HTTPS certificate config
```

## Step 1: Create Service Directory Structure

```
Services/
└── NewService/
    ├── API/
    │   ├── NewService.API.csproj
    │   ├── Program.cs
    │   ├── appsettings.json
    │   └── appsettings.Development.json
    ├── Application/
    ├── Domain/
    └── Infrastructure/
```

## Step 2: Configure appsettings.json

### Services/NewService/API/appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "NewService": "Information"
    }
  },
  "Database": {
    "Username": "newservice_user",
    "Password": "newservice_dev_password",
    "DatabaseName": "newservicedb",
    "Type": "PostgreSQL"
  },
  "Cache": {
    "MaxMemory": "256mb",
    "PersistenceKeys": 200
  },
  "CertificateSetup": {
    "Enabled": true,
    "AutoSetup": true,
    "ForceRegenerate": false
  },
  "NewServiceApi": {
    "ExternalHttpPort": 7000,
    "ExternalHttpsPort": 7060,
    "InternalHttpPort": 8080,
    "InternalHttpsPort": 8081
  }
}
```

## Step 3: Create Service Definition

### Aspire/AppHost/Services/NewServiceDefinition.cs

```csharp
using System;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using AppHost.Abstractions;
using AppHost.Options;
using Projects;
using InfraFactory = AppHost.Infrastructure.InfrastructureFactory;

namespace AppHost.Services;

/// <summary>
/// Service definition for the NewService microservice.
/// </summary>
public sealed class NewServiceDefinition : ServiceDefinitionBase, IDatabaseService, ICacheService
{
    public override string ServiceName => "newservice";
    public override string DisplayName => "NewService API";

    public DatabaseRequirement DatabaseRequirement => DatabaseRequirement.Shared("Database");
    public bool RequiresDedicatedCache => false;

    public NewServiceDefinition() : base(InfraFactory.Instance) { }

    public NewServiceDefinition(IInfrastructureFactory infrastructureFactory)
        : base(infrastructureFactory) { }

    public override IResourceBuilder<ProjectResource> Register(IDistributedApplicationBuilder builder)
    {
        // Get infrastructure resources
        var database = base.InfrastructureFactory.GetOrCreateDatabase(
            builder, ServiceName, DatabaseRequirement.DatabaseName);
        var cache = base.InfrastructureFactory.GetOrCreateCache(builder);

        // Get configuration options
        var portOptions = GetPortOptions(builder);
        var certOptions = GetHttpsCertificateOptions(builder);

        // Build the service
        var api = builder.AddProject<NewService_API>($"{ServiceName}-api")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", GetEnvironment())
            .WithReference(database)
            .WithReference(cache)
            .WaitFor(database);

        // Configure endpoints
        api = ConfigureEndpoints(api, portOptions);

        // Configure for Docker deployment
        api = ConfigureForDocker(api, portOptions, certOptions);

        return api;
    }

    private static string GetEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    }
}
```

## Step 4: Register in ServiceRegistry

### Aspire/AppHost/Services/ServiceRegistry.cs

Add your service to the `CreateDefault()` method:

```csharp
public static ServiceRegistry CreateDefault()
{
    return new ServiceRegistry()
        .Add<CatalogServiceDefinition>()
        .Add<NewServiceDefinition>();  // Add your new service here
}
```

## Step 5: Add Certificate Copy to .csproj

### Services/NewService/API/NewService.API.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
    </PropertyGroup>

    <!-- Copy HTTPS certificates to output for Docker deployment -->
    <ItemGroup>
        <None Include="$(MSBuildThisFileDirectory)..\..\..\certs\aspnetapp.pfx"
              Condition="Exists('$(MSBuildThisFileDirectory)..\..\..\certs\aspnetapp.pfx')"
              CopyToOutputDirectory="PreserveNewest"
              Link="certs\aspnetapp.pfx"/>
    </ItemGroup>
</Project>
```

## Step 6: Add Port Options (Optional)

If your service needs custom port configuration, add to `ServiceConfigurationHelper.cs`:

```csharp
public static ServicePortOptions GetNewServicePortOptions(IConfiguration config)
{
    var options = config.GetSection("Services:NewService:Ports").Get<ServicePortOptions>()
                  ?? config.GetSection("NewServiceApi").Get<ServicePortOptions>()
                  ?? new ServicePortOptions();

    options.Validate("newservice");
    return options;
}
```

## Configuration Resolution Order

The configuration system uses a fallback pattern:

1. `Services:{ServiceName}:Ports` - Service-specific in merged config
2. `{ServiceName}Api` - Legacy format support
3. Default values from `ServicePortOptions`

### Example Resolution:

```
Configuration lookup for "newservice":
1. Services:NewService:Ports          ← Preferred (service-specific)
2. NewServiceApi                       ← Legacy fallback
3. ServicePortOptions defaults         ← Final fallback
```

## AppHost Entry Point

The refactored `AppHost.cs` is clean and simple:

```csharp
IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder
    .WithDefaultConfiguration()  // Docker Compose, config merge, validation, certs
    .WithServices();             // Register all services from ServiceRegistry

builder.Build().Run();
```

## Best Practices

### 1. Service Definition

- Inherit from `ServiceDefinitionBase` for common functionality
- Implement `IDatabaseService` if service needs a database
- Implement `ICacheService` if service needs caching
- Use `base.InfrastructureFactory` for shared resources

### 2. Configuration

- Put base settings in `appsettings.json`
- Put environment-specific overrides in `appsettings.{Environment}.json`
- Never commit production secrets to source control

### 3. Infrastructure Sharing

- Use `InfrastructureFactory.GetOrCreateDatabase()` for database resources
- Use `InfrastructureFactory.GetOrCreateCache()` for cache resources
- Factory ensures resources are created only once and shared

### 4. Naming Conventions

- Service name: lowercase, no hyphens (e.g., `newservice`)
- Resource names: `{serviceName}-api`, `{serviceName}-postgres`
- Configuration sections: `NewServiceApi`, `Services:NewService:Ports`

## Testing Your New Service

```powershell
# Build the solution
dotnet build Aspire\AppHost.sln

# Run with Aspire
dotnet run --project Aspire\AppHost\AppHost.csproj

# Your service should be available at:
# HTTP:  http://localhost:7000
# HTTPS: https://localhost:7060
```

## Troubleshooting

### Service not registering

- Ensure `NewServiceDefinition` is added to `ServiceRegistry.CreateDefault()`
- Check that the project reference exists in `AppHost.csproj`

### Certificate errors

- Run `dotnet dev-certs https --export-path certs/aspnetapp.pfx --password 'AspireSecure2024!' --trust`
- Ensure certificate is copied via `.csproj` configuration

### Port conflicts

- Check that `ExternalHttpPort` and `ExternalHttpsPort` are unique
- Use environment variables to override: `NEWSERVICE_HTTP_PORT`, `NEWSERVICE_HTTPS_PORT`
