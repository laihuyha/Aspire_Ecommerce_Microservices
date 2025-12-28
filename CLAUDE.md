# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Microservices-based e-commerce platform built with .NET 9.0, .NET Aspire, Clean Architecture, and CQRS pattern using MediatR. Currently implements a Catalog service with plans for Basket and Order services.

## Build and Run Commands

```powershell
# Build the entire solution
dotnet build Aspire\AppHost.sln

# Run with .NET Aspire (orchestrates all services and infrastructure)
dotnet run --project Aspire\AppHost\AppHost.csproj

# Build specific service
dotnet build Services\Catalog\API\Catalog.API.csproj

# Deploy to Docker Compose
cd Aspire\AppHost
aspire deploy -o ./manifests
docker compose --env-file ./manifests/.env.Production up -d
```

## Architecture

### Solution Structure

```
Aspire/
├── AppHost/
│   ├── Abstractions/      # IServiceDefinition, IInfrastructureFactory, ServiceDefinitionBase
│   ├── Infrastructure/    # InfrastructureFactory (singleton for DB/Cache)
│   ├── Services/          # Service definitions (CatalogServiceDefinition, ServiceRegistry)
│   ├── Extensions/        # Fluent API extensions
│   ├── Options/           # Configuration options classes
│   ├── Utils/             # Helpers (ConfigurationMerger, ServiceConfigurationHelper)
│   └── AppHost.cs         # Entry point with fluent configuration
└── ServiceDefaults/       # OpenTelemetry, health checks, resilience patterns

BuildingBlocks/        # Shared cross-cutting concerns
├── CQRS/             # ICommand, IQuery, handlers, behaviors (Logging, Validation, Performance, ErrorHandling)
├── Entity/           # BaseEntity, IAggregateRoot, ValueObject, DomainEvent
├── Errors/           # DomainException, NotFoundException, ValidationException, GlobalExceptionHandler
├── Specifications/   # ISpecification, BaseSpecification
└── Middlewares/      # CorrelationIdMiddleware

Services/{ServiceName}/
├── API/              # Controllers, Requests, Responses, Program.cs
├── Application/      # Commands, Queries, Validators, Handlers
├── Domain/           # Aggregates, Entities, ValueObjects, Events, Interfaces, Specifications
└── Infrastructure/   # Repositories, UnitOfWork, Configurations
```

### Clean Architecture Dependency Rules

**Allowed:**
```
API → Application + Infrastructure
Application → Domain + Infrastructure + BuildingBlocks
Infrastructure → Domain + BuildingBlocks
Domain → BuildingBlocks
```

**Forbidden:**
```
Domain → Application (use interfaces in Domain)
Infrastructure → API
API → Domain (must go through Application)
```

### CQRS Pattern Flow

1. Controller receives HTTP request with Request DTO
2. Maps DTO to Command/Query using Mapster
3. Dispatches via `IMediator.Send()`
4. Handler processes request, interacts with domain/persistence
5. Returns Response DTO

```csharp
// Command definition (Application/Commands/)
public record CreateProductCommand(string Name, decimal Price) : ICommand<CreateProductResponse>;

// Handler (same file or Application/Handlers/)
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand command, CancellationToken ct)
    {
        // Use domain aggregate, repository via IUnitOfWork
    }
}

// Controller (API/Controllers/)
[HttpPost]
public async Task<IActionResult> Create(CreateProductRequest request)
{
    var command = request.Adapt<CreateProductCommand>();
    var response = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
}
```

## Build Configuration

**Directory.Build.props settings:**
- `ImplicitUsings`: **disabled** - always include explicit `using` statements
- `Nullable`: **disabled**
- `TreatWarningsAsErrors`: **enabled** - code must be warning-free
- Target Framework: `net9.0`

**Directory.Packages.props:** Centralized NuGet package version management

## Adding New Features

### New API Endpoint

1. Create Command/Query in `Application/Commands/` or `Application/Queries/`
2. Create Handler in same file or `Application/Handlers/`
3. Create Validator in `Application/Validators/` (uses FluentValidation)
4. Create Request/Response DTOs in `API/Requests/` and `API/Responses/`
5. Add Controller action using MediatR + Mapster

### New Microservice

1. Copy `Services/Catalog/` structure as template
2. Create layered folders: API, Application, Domain, Infrastructure
3. Create Service Definition in `Aspire/AppHost/Services/`:
   ```csharp
   public sealed class NewServiceDefinition : ServiceDefinitionBase, IDatabaseService, ICacheService
   {
       public override string ServiceName => "newservice";
       public override string DisplayName => "NewService API";

       public NewServiceDefinition() : base(InfrastructureFactory.Instance) { }

       public override IResourceBuilder<ProjectResource> Register(IDistributedApplicationBuilder builder)
       {
           var database = base.InfrastructureFactory.GetOrCreateDatabase(builder, ServiceName, "Database");
           var cache = base.InfrastructureFactory.GetOrCreateCache(builder);
           // ... configure and return the project resource
       }
   }
   ```
4. Register in `ServiceRegistry.CreateDefault()`:
   ```csharp
   return new ServiceRegistry()
       .Add<CatalogServiceDefinition>()
       .Add<NewServiceDefinition>();
   ```
5. Add configuration files in service's `API/appsettings.*.json`
6. Add certificate copy in service's `.csproj`:
   ```xml
   <None Include="$(MSBuildThisFileDirectory)..\..\..\certs\aspnetapp.pfx"
         Condition="Exists('...')"
         CopyToOutputDirectory="PreserveNewest"
         Link="certs\aspnetapp.pfx"/>
   ```

## Service Endpoints (Development)

- Catalog API: http://localhost:6000 (HTTPS: 6060)
- PostgreSQL: localhost:5432
- Redis: localhost:6379
- Health checks: `/health`, `/alive`

## Key Patterns

### Domain Entities
All entities inherit from `BaseEntity<TId>` which provides: `Id`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`

### Rich Domain Models
Domain logic in aggregate methods, not anemic data containers:
```csharp
public class Product : BaseEntity<Guid>, IAggregateRoot
{
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0) throw new DomainException("Price must be positive");
        _price = newPrice;
        AddDomainEvent(new ProductPriceChangedEvent(Id, newPrice));
    }
}
```

### Repository + Unit of Work
- `IUnitOfWork` for transaction management
- `IRepository<T>` for aggregate access
- Marten as document database on PostgreSQL (auto-creates schema)

### MediatR Behaviors (registered in order)
1. `LoggingBehavior` - structured logging
2. `ValidationBehavior` - FluentValidation
3. `PerformanceBehavior` - timing
4. `ErrorHandlingBehavior` - exception handling

### Service Registration
Every service Program.cs must include:
```csharp
builder.AddServiceDefaults();
builder.Services.AddCatalogServices(builder); // Service-specific
app.UseCorrelationId();
app.MapDefaultEndpoints();
```

## Technology Stack

- .NET Aspire 9.4.2 - Cloud-native orchestration
- MediatR 12.5.0 - CQRS messaging
- Marten 8.11.0 - Document database on PostgreSQL
- Mapster 7.4.0 - Object mapping
- FluentValidation 12.1.1 - Request validation
- OpenTelemetry 1.12.0 - Observability
