# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Microservices-based e-commerce platform built with .NET 10.0, .NET Aspire, Clean Architecture, and CQRS pattern using MediatR. Currently implements a Catalog service with plans for Basket and Order services.

## Build and Run Commands

### Basic Build

```powershell
# Build the entire solution
dotnet build Aspire\AppHost.sln

# Run with .NET Aspire (orchestrates all services and infrastructure)
dotnet run --project Aspire\AppHost\AppHost.csproj

# Build specific service
dotnet build Services\Catalog\API\Catalog.API.csproj

# Format code (run before committing)
dotnet format Aspire\AppHost.sln

# Deploy to Docker Compose
cd Aspire\AppHost
aspire deploy -o ./manifests
docker compose --env-file ./manifests/.env.Production up -d
```

No test projects exist yet. When added, run with: `dotnet test Aspire\AppHost.sln`

## Architecture

### Solution Structure

```
Aspire/
├── AppHost/
│   ├── Abstractions/      # IServiceDefinition, IInfrastructureFactory, ServiceDefinitionBase
│   ├── Infrastructure/    # InfrastructureFactory (singleton: shares DB/cache across services)
│   ├── Services/          # CatalogServiceDefinition, ServiceRegistry
│   ├── Extensions/        # Fluent API: WithDefaultConfiguration(), WithServices()
│   ├── Options/           # Strongly-typed config classes (ports, DB, cache, certs)
│   └── AppHost.cs         # 3-line entry point using fluent builder
└── ServiceDefaults/       # OpenTelemetry, health checks, HTTP resilience (Extensions.cs)

BuildingBlocks/            # Shared cross-cutting concerns (single .csproj)
├── CQRS/                  # ICommand<T>, IQuery<T>, ICommandHandler, IQueryHandler, Behaviors/
├── Common/                # Result<T> pattern
├── Entity/                # BaseEntity<TId>, IAggregateRoot, DomainEvent, IntegrationEvent, ValueObject
├── Errors/                # DomainException, NotFoundException, ValidationException, GlobalExceptionHandler
├── Specifications/        # ISpecification<T>, BaseSpecification<T>
└── Middlewares/           # CorrelationIdMiddleware

Services/{ServiceName}/
├── API/                   # Controllers, Requests, Responses, Filters, Extensions, Program.cs
├── Application/           # Commands, Queries, Validators, Handlers, Exceptions
├── Domain/                # Aggregates, Entities, ValueObjects, Interfaces, Specifications
└── Infrastructure/        # Repositories, UnitOfWork, Configurations
```

### Clean Architecture Dependency Rules

**Allowed:**
```
API → Application + Infrastructure
Application → Domain + BuildingBlocks
Infrastructure → Domain + BuildingBlocks
Domain → BuildingBlocks
```

**Forbidden:**
```
Domain → Application
Infrastructure → API
API → Domain directly (must go through Application)
```

### CQRS Pattern Flow

1. Controller receives Request DTO → maps to Command/Query via Mapster → dispatches via `IMediator.Send()`
2. MediatR pipeline executes behaviors in order: `LoggingBehavior` → `ValidationBehavior` → `PerformanceBehavior` → `ErrorHandlingBehavior`
3. Handler uses `IUnitOfWork` (Domain interface) for aggregate access and persistence
4. Returns Response DTO

```csharp
// Command + Handler co-located in Application/Commands/
public record CreateProductCommand(string Name, decimal? BasePrice) : ICommand<CreateProductCommandResponse>;
public record CreateProductCommandResponse(Guid ProductId);

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<CreateProductCommandResponse> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var product = Product.Create(command.Name, command.BasePrice);
        await _unitOfWork.Repository<Product>().AddAsync(product, ct);
        await _unitOfWork.SaveEntitiesAsync(ct);
        return new CreateProductCommandResponse(product.Id);
    }
}
```

## Build Configuration

**Directory.Build.props** (applies to all projects):
- `ImplicitUsings`: **disabled** — always write explicit `using` statements
- `Nullable`: **disabled** — do not enable nullable reference types in new code
- `TreatWarningsAsErrors`: **enabled** — code must be warning-free to build
- `EnforceCodeStyleInBuild`: **enabled** — style violations are build errors
- **Certificate auto-copy**: projects under `Services/` automatically copy `certs/aspnetapp.pfx` to output directory on build/publish — no per-project configuration needed

**Directory.Packages.props:** Centralized NuGet version management — always add packages here, never with a version in `.csproj`.

## Marten as Document Database

Catalog uses **Marten** (document DB on PostgreSQL), not EF Core. Key implications:

- `IDocumentSession` is the Marten equivalent of `DbContext` — injected into `MartenUnitOfWork`
- Documents are stored as JSON; no migrations needed — Marten auto-creates schema
- The `IUnitOfWork` interface lives in **Domain** (`Catalog.Domain.Interfaces`), not Application
- `IUnitOfWork` exposes both a generic `Repository<T>()` factory and a typed `Products` property
- `SaveEntitiesAsync()` flushes all pending changes; `SaveChangesAsync()` is equivalent
- Specifications use `MartenSpecificationEvaluator` (Catalog-specific, in `Domain/Specifications/`)

```csharp
// IUnitOfWork (defined in Domain, implemented in Infrastructure)
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }                   // typed repository
    IRepository<T> Repository<T>() where T : class;       // generic repository
    Task<bool> SaveEntitiesAsync(CancellationToken ct);
    Task<T> GetSingleBySpecAsync<T>(ISpecification<T> spec, CancellationToken ct) where T : class;
    Task<PaginatedResult<T>> GetPaginatedBySpecAsync<T>(ISpecification<T> spec, int page, int size, CancellationToken ct) where T : class;
}
```

## Key Patterns

### Domain Entities
All aggregates inherit from `BaseEntity<TId>` (provides `Id`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`). Aggregate roots implement `IAggregateRoot` and use `AddDomainEvent()` to raise `DomainEvent` (in-process) or `IntegrationEvent` (cross-service).

### Rich Domain Models
Domain logic in aggregate methods, not anemic data containers:
```csharp
public class Product : BaseEntity<Guid>, IAggregateRoot
{
    public void AddVariant(Variant variant)
    {
        if (_variants.Any(v => v.SKU == variant.SKU))
            throw new DomainException("Duplicate SKU");
        _variants.Add(variant);
    }
}
```

### Specifications
`ISpecification<T>` / `BaseSpecification<T>` in BuildingBlocks define the contract. Catalog Domain has its own `MartenSpecificationEvaluator` that translates specifications into Marten LINQ queries. Use `IUnitOfWork.GetListBySpecAsync<T>()` rather than querying Marten directly from handlers.

### Error Handling
- `DomainException` — business rule violations (returns 400)
- `NotFoundException` — resource not found (returns 404)
- `ValidationException` — FluentValidation failures via `ValidationBehavior` (returns 422)
- `GlobalExceptionFilter` on controllers + `GlobalExceptionHandler` in BuildingBlocks handle all unhandled exceptions

### Result Pattern
`BuildingBlocks.Common.Result<T>` is available for operations that can succeed or fail without throwing. Prefer exceptions for domain violations; use `Result<T>` for expected failures in Application layer queries.

## Adding New Features

### New API Endpoint

1. Create Command/Query record + Handler class in same file under `Application/Commands/` or `Application/Queries/`
2. Create Validator in `Application/Validators/` using FluentValidation
3. Create Request/Response DTOs in `API/Requests/` and `API/Responses/`
4. Add controller action in `API/Controllers/` — map Request → Command/Query via `Adapt<>()`, dispatch via `_mediator.Send()`

### New Microservice

1. Copy `Services/Catalog/` as template; adjust namespaces (`Catalog` → `{ServiceName}`)
2. Create `Aspire/AppHost/Services/{ServiceName}ServiceDefinition.cs`:
   ```csharp
   public sealed class NewServiceDefinition : ServiceDefinitionBase, IDatabaseService, ICacheService
   {
       public override string ServiceName => "newservice";
       public override string DisplayName => "NewService API";
       public DatabaseRequirement DatabaseRequirement => DatabaseRequirement.Shared("Database");
       public bool RequiresDedicatedCache => false;

       public NewServiceDefinition() : base(InfrastructureFactory.Instance) { }

       public override IResourceBuilder<ProjectResource> Register(IDistributedApplicationBuilder builder)
       {
           var database = InfrastructureFactory.GetOrCreateDatabase(builder, ServiceName, DatabaseRequirement.DatabaseName);
           var cache = InfrastructureFactory.GetOrCreateCache(builder);
           return builder.AddProject<Projects.NewService_API>($"{ServiceName}-api")
               .WithReference(database).WithReference(cache).WaitFor(database);
       }
   }
   ```
3. Register in `ServiceRegistry.CreateDefault()`:
   ```csharp
   return new ServiceRegistry().Add<CatalogServiceDefinition>().Add<NewServiceDefinition>();
   ```
4. Add `appsettings.json` and `appsettings.Development.json` in `API/`
5. Register service in the service's `Program.cs` — call `builder.AddServiceDefaults()`, wire up `app.UseCorrelationId()` and `app.MapDefaultEndpoints()`

## Service Registration (per service Program.cs)

```csharp
builder.AddServiceDefaults();               // OpenTelemetry, health checks, resilience
builder.Services.AddCatalogServices(builder); // service-specific DI (MediatR, Marten, etc.)
app.UseCorrelationId();
app.MapDefaultEndpoints();                  // /health and /alive
```

## Service Endpoints (Development)

- Catalog API: http://localhost:6000 (HTTPS: 6060)
- PostgreSQL: localhost:5432
- Redis: localhost:6379
- Aspire Dashboard: launched automatically when running AppHost
- Swagger UI: http://localhost:6000/swagger (development only)

## Technology Stack

- .NET Aspire 9.4.2 — cloud-native orchestration
- MediatR 12.5.0 — CQRS messaging + pipeline behaviors
- Marten 8.11.0 — document database on PostgreSQL
- Mapster 7.4.0 — object mapping (Request → Command, aggregate → Response)
- FluentValidation 12.1.1 — request validation via `ValidationBehavior`
- OpenTelemetry 1.12.0 — traces, metrics, logs (OTLP exporter)
