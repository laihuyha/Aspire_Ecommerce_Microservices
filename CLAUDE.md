# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a microservices-based e-commerce platform currently implementing a Catalog service, built with .NET 9.0 and .NET
Aspire. The solution follows Clean Architecture principles with CQRS pattern implementation using MediatR. Services are
independently deployable, containerized, and orchestrated via .NET Aspire. Currently focuses on product catalog
management with plans to expand to Basket and Order services.

## Build and Run Commands

### Using .NET Aspire (Recommended for Development)

```powershell
# Build the entire solution
dotnet build Aspire\AppHost.sln

# Run the AppHost (orchestrates all services and infrastructure)
dotnet run --project Aspire\AppHost\AppHost.csproj

# Build specific service
dotnet build Services\Catalog\API\Catalog.API.csproj
```

### Using Docker Containers (Production-style)

```powershell
# Note: Docker Compose configuration not yet implemented
# Services can be containerized individually using Dockerfiles in each API project
```

### Running Individual Services

```powershell
# Run Catalog API locally (requires PostgreSQL and Redis running)
dotnet run --project Services\Catalog\API\Catalog.API.csproj

# Note: Basket service not yet implemented
```

## Architecture

### Microservices Structure

Each service follows a layered Clean Architecture pattern:

```
Services/{ServiceName}/
├── API/                 # REST endpoints, controllers, DTOs, responses
├── Application/         # CQRS commands/queries, handlers, business logic
├── Domain/             # Domain entities, value objects, domain logic
└── Infrastructure/     # Service implementations, external integrations (Marten, Repos)
```

### Clean Architecture Dependency Flow

**✅ CURRENT Dependencies:**
```
API → Application + Infrastructure
Application → Domain + Infrastructure + BuildingBlocks
Infrastructure → Domain + BuildingBlocks
Domain → BuildingBlocks
BuildingBlocks → (no dependencies)
```

**❌ FORBIDDEN Dependencies:**
```
Domain → Application (inversion via interfaces defined in Domain)
Infrastructure → API (data flows outward, dependencies inward)
API → Domain (must go through Application layer)
```

**Current Project Flow:**
```
Catalog.API → Catalog.Application + Catalog.Infrastructure
Catalog.Application → Catalog.Domain + Catalog.Infrastructure + BuildingBlocks
Catalog.Infrastructure → Catalog.Domain + BuildingBlocks
Catalog.Domain → BuildingBlocks
BuildingBlocks → (independent)
```

### CQRS Pattern Implementation

The codebase uses **Command Query Responsibility Segregation** with MediatR:

- **Commands**: Represent write operations (CreateProduct, UpdateBasket)
- **Queries**: Represent read operations (GetProductById, GetBasket)
- **Handlers**: Process commands/queries (CreateProductCommandHandler)

**Pattern Flow**:

1. Controller receives HTTP request with DTO
2. Maps DTO to Command/Query using Mapster
3. Dispatches to MediatR via `IMediator.Send()`
4. Handler processes request, interacts with domain/persistence
5. Returns response DTO to controller

**Key Interfaces** (defined in BuildingBlocks/CQRS):

- `ICommand<TResponse>` - Marker for commands
- `IQuery<TResponse>` - Marker for queries
- `ICommandHandler<TCommand, TResponse>` - Command processors
- `IQueryHandler<TQuery, TResponse>` - Query processors

### Current Services

**Catalog Service** (Fully Implemented)

- Product catalog management with full CRUD operations
- PostgreSQL + Marten for document storage
- Redis for caching (configured but not yet used in business logic)
- Ports: 6000 (HTTP), 6060 (HTTPS)
- Database: PostgreSQL 16.4 on port 5432

### Shared Components

**BuildingBlocks** (`BuildingBlocks/BuildingBlocks/`)

- CQRS interfaces and abstractions
- Base entity with audit properties (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
- Shared utilities across all services

**ServiceDefaults** (`Aspire/ServiceDefaults/`)

- OpenTelemetry configuration (tracing, metrics, logging)
- HTTP client resilience patterns (retry, circuit breaker, timeout)
- Health check endpoints (`/health`, `/alive`)
- Service discovery configuration

**AppHost** (`Aspire/AppHost/`)

- Service orchestration and composition
- Infrastructure provisioning (PostgreSQL, Redis)
- Configuration file loading from JSON
- Persistent container management with volume handling

## Technology Stack

- **.NET 9.0** - Target framework across all projects
- **ASP.NET Core** - Web API framework
- **MediatR 12.5.0** - CQRS in-process messaging
- **Mapster 7.4.0** - Object-to-object mapping
- **Marten 8.11.0** - Document database & event store for PostgreSQL
- **PostgreSQL 16.4** - Primary database for Catalog service
- **Redis** - Distributed cache (configured for future services)
- **.NET Aspire 9.3.0+** - Cloud-native orchestration framework
- **OpenTelemetry 1.12.0** - Observability (traces, metrics, logs)
- **Swashbuckle** - OpenAPI/Swagger documentation

## Configuration Management

### Centralized Package Management

**Directory.Packages.props** - All NuGet package versions centralized

- Prevents version conflicts across microservices
- Update once, affects all projects
- `ManagePackageVersionsCentrally` enabled

### Build Configuration

**Directory.Build.props** - Shared build settings:

- Target Framework: `net9.0`
- Implicit Usings: **DISABLED** (explicit `using` statements required)
- Nullable: **DISABLED** (nullable annotations not enforced)
- Code Analysis: Latest-recommended with all rules
- Warnings as Errors: **ENABLED**
- Documentation: XML doc generation enforced (suppress 1591 warnings)

**Important**: When writing code, always include explicit `using` statements at the top of files.

### Multi-Level Configuration

1. **AppHost Configuration** (`Aspire/AppHost/*.json`)
    - `catalog-config.Development.json` - Catalog API settings
    - `basket-config.Development.json` - Basket API settings
    - `postgres-config.Development.json` - PostgreSQL config
    - `redis-config.Development.json` - Redis config

2. **Service Configuration** (`Services/{Service}/API/appsettings.*.json`)
    - `appsettings.json` - Production defaults
    - `appsettings.Development.json` - Development overrides
    - Connection strings, logging, service-specific settings

3. **Environment Variables** (Docker/Aspire)
    - `ASPNETCORE_ENVIRONMENT` - Development/Production
    - `ASPNETCORE_HTTP_PORTS` - HTTP port binding
    - `ConnectionStrings__Database` - Database connection
    - `ConnectionStrings__Redis` - Redis connection

## Adding New Features

### Adding a New API Endpoint

1. **Create Command/Query** in `Application/Commands/` or `Application/Queries/`
   ```csharp
   public record CreateProductCommand(string Name, decimal Price) : ICommand<CreateProductResponse>;
   ```

2. **Create Handler** in same folder
   ```csharp
   public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResponse>
   {
       public async Task<CreateProductResponse> Handle(CreateProductCommand command, CancellationToken ct)
       {
           // Implementation
       }
   }
   ```

3. **Create Request/Response DTOs** in `API/Requests/` or `API/Responses/`

4. **Add Controller Action** in `API/Controllers/`
   ```csharp
   [HttpPost]
   public async Task<IActionResult> CreateProduct(ProductRequest request)
   {
       var command = request.Adapt<CreateProductCommand>();
       var response = await mediator.Send(command);
       return Ok(response);
   }
   ```

### Adding a New Microservice

1. **Copy Catalog service structure** as template
2. **Create layered folders**: API, Application, Domain, Infrastructure, Persistence
3. **Register in AppHost.cs**:
   ```csharp
   var newService = builder.AddProject<NewService_API>("new-service")
       .WithHttpEndpoint(port: 6002, targetPort: 8080)
       .WithReference(database);
   ```
4. **Create configuration files** in `Aspire/AppHost/`
5. **Update docker-compose.yml** if using Docker Compose

### Modifying Database Configuration

**For Marten (PostgreSQL Document Store)**:

- Modify Program.cs in API project
- Update connection string in appsettings.Development.json
- Marten auto-creates schema on first run (no migrations needed)

## Infrastructure Details

### .NET Aspire Orchestration

AppHost orchestrates infrastructure using type-safe C# API:

```csharp
var postgres = builder.AddPostgres("catalog")
    .WithEndpoint(port: 5433)
    .AddDatabase("CatalogDb");

var catalog = builder.AddProject<Catalog_API>("catalog-api")
    .WithHttpEndpoint(port: 6000, targetPort: 8080)
    .WaitFor(postgres);
```

**Benefits over Docker Compose**:

- Type-safe service composition
- Automatic service discovery
- Configuration from JSON files
- Better debugging experience in Visual Studio

### Service Endpoints

**Development URLs**:

- Catalog API: http://localhost:6000 (HTTPS: 6060)
- PostgreSQL: localhost:5432
- Redis: localhost:6379
- Redis Commander: http://localhost:7001 (user: root, pass: secret)

**Health Checks**:

- `/health` - Detailed health status
- `/alive` - Simple liveness probe

### Docker Configuration

**Multi-stage Dockerfile** (Services/Catalog/API/Dockerfile):

- Build stage: Layer-by-layer NuGet restore for caching
- Runtime stage: ASP.NET Core 9.0 slim image
- HTTPS certificate setup for development
- Exposed ports: 8080 (HTTP), 8081 (HTTPS)

## Code Patterns and Conventions

### Explicit Usings Required

Always include explicit `using` statements (ImplicitUsings disabled):

```csharp
using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;
```

### Domain Entities

Inherit from `BaseEntity` in BuildingBlocks:

```csharp
public class Product : BaseEntity<Guid>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

BaseEntity provides: `Id`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`

### MediatR Registration

In Program.cs:

```csharp
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly));
```

### Mapster for Mapping

Prefer Mapster over AutoMapper:

```csharp
var command = request.Adapt<CreateProductCommand>();
var response = entity.Adapt<ProductResponse>();
```

## Important Notes

### No Inter-Service Communication Yet

Services are currently independent with no synchronous or asynchronous communication between them. When implementing:

- Use HTTP with resilience patterns (already configured in ServiceDefaults)
- Service discovery is enabled for endpoint resolution
- Consider message queue for async communication

### Code Quality Enforcement

- **All warnings are errors** - Code must be warning-free
- Code analysis runs on build with all rules enabled
- Documentation comments enforced (suppress 1591 for now)

### Database Approach

- **Marten** abstracts PostgreSQL as document database
- No traditional migrations - schema auto-created
- Event sourcing capabilities available but not currently used

### Observability

All services have OpenTelemetry configured:

- Distributed tracing across service calls
- Metrics collection (ASP.NET Core, HTTP, Runtime)
- Structured logging with correlation IDs
- OTLP exporter for telemetry backends

## Recent Changes

Based on git history:

- **Latest**: Dockerized Catalog services with multi-stage builds
- **CQRS Implementation**: Implemented command/query pattern with MediatR
- **Feature Folders**: Reorganized code into feature-based structure
- **Clean Architecture**: Transitioned from simpler structure to layered approach
