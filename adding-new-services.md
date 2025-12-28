# Service Configuration Setup Guide

## How to Configure a New Service

When adding a new service to your microservices architecture, follow these steps to configure it properly.

## 🚀 Step 1: Create Service Directory Structure

```
Services/
└── NewService/
    ├── API/
    │   ├── appsettings.json
    │   ├── appsettings.Development.json
    │   └── appsettings.Production.json
    ├── Domain/
    ├── Application/
    ├── Infrastructure/
    └── Persistence/
```

## 🚀 Step 2: Configure Service-Specific Settings

### Services/NewService/API/appsettings.json (base settings)

```json
{
  "Database": {
    "Username": "newservice_user",
    "Password": "newservice_password",
    "VolumeName": "newservice_data"
  },
  "Cache": {
    "MaxMemory": "128mb"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "NewService": "Debug"
    }
  }
}
```

### Services/NewService/API/appsettings.Development.json (dev overrides)

```json
{
  "Database": {
    "Password": "newservice_dev_password"
  },
  "Cache": {
    "MaxMemory": "256mb"
  },
  "CertificateSetup": {
    "Enabled": true
  }
}
```

### Services/NewService/API/appsettings.Production.json (prod overrides)

```json
{
  "Database": {
    "Username": "newservice_prod_user",
    "Password": "newservice_prod_secure_password"
  },
  "Cache": {
    "MaxMemory": "512mb"
  }
}
```

## 🚀 Step 3: Access Configuration in Service Code

### Program.cs Configuration Setup

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AppHost.Options; // If using shared options

var builder = WebApplication.CreateBuilder(args);

// Configure service-specific options with fallback
builder.Services.Configure<DatabaseOptions>(
    ServiceConfiguration.GetServiceConfig(builder.Configuration, "NewService", "Database"));

builder.Services.Configure<CacheOptions>(
    ServiceConfiguration.GetServiceConfig(builder.Configuration, "NewService", "Cache"));

// Or configure directly
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Services:NewService:Database") ??
    builder.Configuration.GetSection("Database"));

builder.Services.Configure<CacheOptions>(
    builder.Configuration.GetSection("Services:NewService:Cache") ??
    builder.Configuration.GetSection("Cache"));
```

### ServiceConfiguration Helper Class

```csharp
public static class ServiceConfiguration
{
    public static IConfigurationSection GetServiceConfig(
        IConfiguration configuration,
        string serviceName,
        string sectionName)
    {
        // Try service-specific first
        var serviceSpecific = configuration.GetSection($"Services:{serviceName}:{sectionName}");
        if (serviceSpecific.Exists())
            return serviceSpecific;

        // Fall back to global
        return configuration.GetSection(sectionName);
    }
}
```

## 🚀 Step 4: Use Configuration in Service Classes

### Example: Database Configuration Usage

```csharp
public class NewServiceDbContext : DbContext
{
    private readonly DatabaseOptions _dbOptions;

    public NewServiceDbContext(IOptions<DatabaseOptions> dbOptions)
    {
        _dbOptions = dbOptions.Value;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql($"Host=localhost;Database={_dbOptions.DatabaseName};Username={_dbOptions.Username};Password={_dbOptions.Password}");
    }
}
```

### Example: Cache Configuration Usage

```csharp
public class NewServiceCacheService
{
    private readonly CacheOptions _cacheOptions;

    public NewServiceCacheService(IOptions<CacheOptions> cacheOptions)
    {
        _cacheOptions = cacheOptions.Value;
    }

    public void ConfigureCache()
    {
        // Use _cacheOptions.MaxMemory, _cacheOptions.PersistenceInterval, etc.
    }
}
```

## 🚀 Step 5: Update AppHost.cs (if needed)

If your service needs special AppHost configuration, add it:

```csharp
// In AppHost.cs
IResourceBuilder<ProjectResource> newServiceApi = builder.AddProject<NewService_API>(
    "newservice-api",
    project => project
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
        .WithReference(database)
        .WithReference(cache)
        .WithHttpEndpoint(5000, 8080, "newservice-http")
        .WithHttpsEndpoint(5443, 8443, "newservice-https")
        .WithBakedInHttpsCertificate());
```

## 📋 Configuration Resolution Examples

### For NewService in Development:
```
Services:NewService:Database:Username = "newservice_user"        ← Service-specific
Services:NewService:Database:Password = "newservice_dev_password" ← Service + env specific
Services:NewService:Cache:MaxMemory = "256mb"                   ← Service + env specific
Database:Username = "postgres"                                  ← Global fallback
Cache:MaxMemory = "128mb"                                       ← Global fallback
```

### For NewService in Production:
```
Services:NewService:Database:Username = "newservice_prod_user"     ← Service + env specific
Services:NewService:Database:Password = "newservice_prod_secure_password" ← Service + env specific
Services:NewService:Cache:MaxMemory = "512mb"                     ← Service + env specific
Database:Username = "postgres"                                    ← Global fallback
```

## 🎯 Best Practices

### 1. Environment-Specific Configurations
- Put environment-agnostic settings in `appsettings.json`
- Put environment-specific overrides in `appsettings.{Environment}.json`

### 2. Sensitive Data
- Never put passwords or secrets in config files
- Use environment variables or Azure Key Vault for secrets
- Use placeholders in config and resolve at runtime

### 3. Configuration Naming
- Use consistent naming across services
- Document configuration options in service README
- Version configuration changes with service releases

### 4. Fallback Strategy
- Always provide sensible global fallbacks
- Service-specific configs should only override when necessary
- Use the `ServiceConfiguration.GetServiceConfig()` helper for clean fallback logic

This approach ensures each service maintains its autonomy while benefiting from global defaults and infrastructure consistency.
