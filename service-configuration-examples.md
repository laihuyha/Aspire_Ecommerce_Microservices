# Service-Specific Configuration Examples

## How Services Can Configure Themselves

With the new configuration merging system, each service can have its own configuration files that override or extend the global AppHost settings.

## 📁 Service Configuration Structure

```
Services/
├── Catalog/
│   ├── API/
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── appsettings.Production.json
│   └── appsettings.json
├── Order/
│   ├── API/
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   └── appsettings.json
└── Basket/
    ├── API/
    │   ├── appsettings.json
    │   └── appsettings.Development.json
    └── appsettings.json
```

## 🎯 Configuration Priority Order

1. **Service-Specific**: `Services:{ServiceName}:{Section}:{Key}` (highest priority)
2. **Global AppHost**: `{Section}:{Key}` from AppHost configs
3. **Environment Variables**: `SERVICENAME_SECTION_KEY`
4. **Defaults**: Hardcoded defaults in option classes

## 📋 Service Configuration Examples

### Catalog Service (Services/Catalog/API/appsettings.Development.json)

```json
{
  "Database": {
    "Username": "catalog_user",
    "Password": "catalog_dev_password",
    "VolumeName": "catalog_dev_data"
  },
  "Cache": {
    "MaxMemory": "256mb",
    "PersistenceKeys": 200
  },
  "CertificateSetup": {
    "Enabled": true,
    "ForceRegenerate": false
  },
  "Logging": {
    "LogLevel": {
      "Catalog": "Debug"
    }
  }
}
```

### Order Service (Services/Order/API/appsettings.Development.json)

```json
{
  "Database": {
    "Username": "order_user",
    "Password": "order_dev_password",
    "VolumeName": "order_dev_data"
  },
  "Cache": {
    "MaxMemory": "128mb",
    "MaxMemoryPolicy": "volatile-lru"
  },
  "HttpsCertificate": {
    "CertificatePassword": "OrderSpecificPassword123!"
  }
}
```

### Basket Service (Services/Basket/API/appsettings.Development.json)

```json
{
  "Database": {
    "Username": "basket_user"
    // Inherits password from global config
  },
  "Cache": {
    "MaxMemory": "64mb",
    "PersistenceIntervalMinutes": 10
  }
}
```

## 🔧 How It Works in AppHost

The `ConfigurationMerger` automatically:

1. **Discovers** all service directories in `Services/`
2. **Collects** configurations from each service's `appsettings*.json` files
3. **Prefixes** service-specific configs with `Services:{ServiceName}:`
4. **Merges** with global AppHost configurations
5. **Provides** merged config to dependency injection

## 🚀 Service-Specific Configuration Access

### In Service Code (Program.cs or Startup.cs):

```csharp
// In Catalog service - gets catalog-specific database config
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Services:Catalog:Database"));

// Or with fallback to global
var dbConfig = builder.Configuration.GetSection("Services:Catalog:Database") ??
               builder.Configuration.GetSection("Database");
builder.Services.Configure<DatabaseOptions>(dbConfig);
```

### Configuration Resolution Helper:

```csharp
// Helper method for service-specific config resolution
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

// Usage in service:
var dbConfig = ServiceConfiguration.GetServiceConfig(builder.Configuration, "Catalog", "Database");
builder.Services.Configure<DatabaseOptions>(dbConfig);
```

## 🔐 Certificate Handling

### Automatic Certificate Copying

Certificates are automatically copied to each service's output directory during build via `Directory.Build.props`:

```xml
<!-- Certificate copying for Docker deployment (for all projects in Services directory) -->
<Target Name="CopyCertificate" AfterTargets="Build"
        Condition="'$(MSBuildProjectDirectory.Contains(\Services\))'">
    <PropertyGroup>
        <ProjectRoot>$([System.IO.Path]::GetFullPath('$(MSBuildProjectDirectory)\..\..\..'))</ProjectRoot>
        <CertSourcePath>$(ProjectRoot)\certs\aspnetapp.pfx</CertSourcePath>
    </PropertyGroup>
    <Copy SourceFiles="$(CertSourcePath)"
          DestinationFolder="$(OutputPath)"
          SkipUnchangedFiles="true"
          Condition="Exists('$(CertSourcePath)')" />
</Target>
```

### Certificate Path in Docker

The certificate is available in Docker containers at `/app/aspnetapp.pfx` for all API services.

## 📊 Configuration Resolution Examples

### Database Username Resolution:
```
Services:Catalog:Database:Username = "catalog_user"  ← Service-specific (used)
Services:Order:Database:Username = "order_user"     ← Service-specific (used)
Database:Username = "postgres"                      ← Global default
```

### Cache MaxMemory Resolution:
```
Services:Catalog:Cache:MaxMemory = "256mb"          ← Service-specific (used)
Cache:MaxMemory = "128mb"                           ← Global default (fallback)
```

## 🎯 Benefits for Services

- ✅ **Service Autonomy**: Each service controls its own configuration
- ✅ **Environment Flexibility**: Different settings per environment
- ✅ **Global Fallbacks**: Sensible defaults from AppHost
- ✅ **Override Capability**: Services can override any global setting
- ✅ **Version Control**: All configurations tracked in git
- ✅ **Team Independence**: Teams can modify their service configs without affecting others
