# Different Database Types Per Service - Configuration Merging Example

## 🎯 The Challenge

Your question: *"If Catalog service uses PostgreSQL but Order service uses SQL Server, how does this work with configuration merging?"*

## ✅ The Solution: Configuration Merging with Service-Specific Overrides

The configuration merging system allows each service to specify its own database preferences while maintaining infrastructure consistency.

## 📁 Configuration Structure

### Global Infrastructure (Aspire/AppHost/infrastructure.json)
```json
{
  "Database": {
    "Type": "PostgreSQL",
    "Host": "localhost",
    "Port": 5432,
    "Username": "postgres",
    "Password": "dev_password",
    "DatabaseName": "appdb",
    "Image": "postgres:16-alpine"
  }
}
```

### Catalog Service (Services/Catalog/API/appsettings.Development.json)
```json
{
  "Database": {
    "Username": "catalog_user",
    "Password": "catalog_dev_password",
    "DatabaseName": "catalogdb",
    "Type": "PostgreSQL"
  }
}
```

### Order Service (Services/Order/API/appsettings.Development.json)
```json
{
  "Database": {
    "Username": "order_user",
    "Password": "order_dev_password",
    "DatabaseName": "orderdb",
    "Type": "SqlServer",  // ← Different database type!
    "Host": "order-sql-server",
    "Port": 1433
  }
}
```

## 🔄 Configuration Merging Process

### 1. AppHost Loads Global Config
```
Database:Type = "PostgreSQL"
Database:Host = "localhost"
Database:Port = 5432
Database:Username = "postgres"
Database:Password = "dev_password"
```

### 2. Configuration Merger Finds Service Configs
- Discovers `Services/Catalog/` and `Services/Order/` directories
- Loads `appsettings*.json` from each service's `API/` folder

### 3. Merges Service-Specific Configs with Prefixes
```
Services:Catalog:Database:Username = "catalog_user"
Services:Catalog:Database:Password = "catalog_dev_password"
Services:Catalog:Database:DatabaseName = "catalogdb"

Services:Order:Database:Type = "SqlServer"
Services:Order:Database:Username = "order_user"
Services:Order:Database:Host = "order-sql-server"
Services:Order:Database:Port = 1433
```

### 4. AppHost Resolves Configurations

#### For Catalog Service:
```csharp
// Configuration extraction moved to InfrastructureExtensions.AddServiceDatabase()
public static IResourceBuilder<PostgresDatabaseResource> AddServiceDatabase(
    this IDistributedApplicationBuilder builder,
    string serviceName,
    string databaseName)
{
    // Extracts service-specific config automatically
    var mergedConfig = builder.Configuration;
    var options = ServiceConfigurationHelper.GetServiceDatabaseOptions(mergedConfig, serviceName);
    // options now contains merged service-specific + global defaults
}

// In AppHost.cs - just calls the method
var catalogDb = builder.AddServiceDatabase("catalog", "Database");
```

#### For Order Service:
```csharp
// Same method handles different service configurations automatically
var orderDb = builder.AddServiceDatabase("order", "Database");
// Method extracts Services:Order:Database config automatically
// Result: Different database settings for Order service
```

## 🔧 Service Method Architecture

### Self-Contained Service Methods

Each service method handles its own configuration and dependency creation:

```csharp
// CatalogServiceExtensions.cs
public static IResourceBuilder<ProjectResource> AddCatalogApi(
    this IDistributedApplicationBuilder builder,
    string serviceName)
{
    // Creates its own dependencies with service-specific configs
    var database = builder.AddServiceDatabase(serviceName, "Database");
    var cache = builder.AddCatalogCache();

    // Handles its own configuration extraction
    var mergedConfig = builder.Configuration;
    var apiOptions = ServiceConfigurationHelper.GetCatalogApiOptions(mergedConfig);
    var httpsCertOptions = ServiceConfigurationHelper.GetHttpsCertificateOptions(mergedConfig);

    // Configures itself completely
    return builder.AddProject<Catalog_API>(...)
        .WithReference(database)
        .WithReference(cache)
        // ... complete service configuration
}
```

### Configuration Helper Methods

Reusable configuration extraction methods:

```csharp
// ServiceConfigurationHelper.cs
public static DatabaseOptions GetServiceDatabaseOptions(IConfiguration config, string serviceName)
{
    return config.GetSection($"Services:{serviceName}:Database").Get<DatabaseOptions>() ??
           config.GetSection("Database").Get<DatabaseOptions>() ??
           new DatabaseOptions();
}

public static CatalogApiOptions GetCatalogApiOptions(IConfiguration config)
{
    return config.GetSection("Services:Catalog:CatalogApi").Get<CatalogApiOptions>() ??
           config.GetSection("CatalogApi").Get<CatalogApiOptions>() ??
           new CatalogApiOptions();
}
```

## 🚀 How Services Use Different Databases

### Current Limitation
Aspire currently only supports PostgreSQL out-of-the-box, but the configuration system is ready for expansion.

### For PostgreSQL Services (Current)
```csharp
// Each service gets its own PostgreSQL instance with different configs
var catalogDb = builder.AddServiceDatabase("catalog", "Database", catalogDbOptions);
// Creates: catalog-postgres container with catalog-specific credentials

var orderDb = builder.AddServiceDatabase("order", "Database", orderDbOptions);
// Would create: order-postgres container (if service existed)
```

### Future Expansion for Different Database Types
```csharp
// When Aspire adds support for SQL Server, MySQL, etc.:
var orderDb = builder.AddServiceDatabase("order", "Database", orderDbOptions);

// The method would check orderDbOptions.Type and create:
// - SQL Server container if Type = "SqlServer"
// - MySQL container if Type = "MySQL"
// - MongoDB container if Type = "MongoDB"
```

## 📊 Configuration Resolution Priority

1. **Service + Environment Specific**: `Services:Order:appsettings.Development.json`
2. **Service Base**: `Services:Order:appsettings.json`
3. **Global + Environment**: `infrastructure.Development.json`
4. **Global Base**: `infrastructure.json`

## 🎯 Benefits

- ✅ **Service Autonomy**: Each service specifies its database preferences
- ✅ **Infrastructure Consistency**: Global defaults ensure consistency
- ✅ **Easy Migration**: Services can change database types without AppHost changes
- ✅ **Environment Flexibility**: Different databases per environment
- ✅ **Future-Proof**: Ready for when Aspire supports more database types

## 🔧 Practical Implementation

### Service-Specific Database Configuration
```csharp
// In Order service's Program.cs
builder.Services.Configure<DatabaseOptions>(
    ServiceConfiguration.GetServiceConfig(builder.Configuration, "Order", "Database"));

builder.Services.AddDbContext<OrderDbContext>((serviceProvider, options) =>
{
    var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    options.UseSqlServer(dbOptions.GetConnectionString());
    // Even though Aspire doesn't support SQL Server yet, the service can configure for it
});
```

### AppHost Infrastructure Setup
```csharp
// In AppHost.cs - when multiple database types are supported
var catalogDb = catalogDbOptions.Type switch
{
    DatabaseType.PostgreSQL => builder.AddPostgres("catalog-postgres", ...),
    DatabaseType.SqlServer => builder.AddSqlServer("catalog-sqlserver", ...),
    // etc.
};
```

This configuration merging approach solves your question by allowing each service to specify its preferred database type and connection details, while the AppHost infrastructure can adapt to provide the appropriate database containers.
