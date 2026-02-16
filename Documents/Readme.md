# Aspire Ecommerce Microservices - Architecture & Operations Summary

## 1. Purpose & Solution Overview
- Modern e-commerce platform using microservices, Clean Architecture, CQRS (MediatR), and DDD principles.
- Built on .NET 9.0, orchestrated by .NET Aspire (AppHost).
- Each service: API (endpoints), Application (CQRS), Domain (business logic), Infrastructure (persistence/integration).
- Shared BuildingBlocks for CQRS, entities, errors, and specifications.

## 2. Build & Run
- **Development:**
  - `dotnet build Aspire/AppHost.sln`
  - `dotnet run --project Aspire/AppHost/AppHost.csproj`
- **Production:**
  - `aspire deploy -o ./manifests`
  - `docker compose --env-file ./manifests/.env.Production up -d`
  - `docker compose down`
- **Run individual service:**
  - `dotnet run --project Services/<Service>/API/<Service>.API.csproj`

## 3. Solution Structure
```
Aspire/
├── AppHost/           # Orchestration, config, infra setup
├── ServiceDefaults/   # OpenTelemetry, health, resilience
BuildingBlocks/        # CQRS, Entities, Errors, Specs, Middleware
Services/{Service}/    # API, Application, Domain, Infrastructure
```

## 4. Architecture & Patterns
- **Dependency Flow:** API → Application → Domain (outer to inner)
- **CQRS + MediatR:** Commands/Queries/Handlers, behaviors (logging, validation, performance, error handling)
- **Mapster:** DTO ↔ Command/Response mapping
- **Marten (PostgreSQL):** Document/event store, auto schema
- **Redis:** Distributed cache (Basket, etc.)
- **OpenTelemetry:** Traces/metrics/logs
- **FluentValidation:** Request validation
- **ServiceDefaults:** Health endpoints, resilience, telemetry

## 5. Configuration & Package Management
- **Directory.Packages.props:** Centralized NuGet version management
- **Directory.Build.props:** net9.0, explicit using, warnings as errors, XML doc required
- **Config:** AppHost JSON configs + Service appsettings.*.json + environment variables
- **Service-specific overrides:** Each service can have its own config files, merged with global settings

## 6. Environment & Endpoints
- Catalog API: http://localhost:6000 (HTTPS: 6060)
- Basket API: http://localhost:6001 (HTTPS: 6061)
- PostgreSQL: localhost:5432
- Redis: localhost:6379; Redis Commander: http://localhost:7001
- Health endpoints: `/health`, `/alive`

## 7. Adding Features & New Services
- **New API Endpoint:**
  1. Create Command/Query in `Application/Commands/` or `Application/Queries/`
  2. Create Handler in same file or `Application/Handlers/`
  3. Create Validator in `Application/Validators/` (FluentValidation)
  4. Create Request/Response DTOs in `API/Requests/` and `API/Responses/`
  5. Add Controller action using MediatR + Mapster
- **New Microservice:**
  1. Copy `Services/Catalog/` structure as template
  2. Create layered folders: API, Application, Domain, Infrastructure
  3. Create Service Definition in `Aspire/AppHost/Services/` (see main Readme for code sample)
  4. Register in `ServiceRegistry.CreateDefault()`
  5. Add config in service's `API/appsettings.*.json`
  6. Add certificate copy in service's `.csproj`

## 8. Code Rules & Best Practices
- Explicit using at file top (implicit usings disabled)
- Warnings as errors, code must be warning-free
- Mapster preferred for mapping
- Marten auto-creates schema
- Never commit production secrets to source control
- Use InfrastructureFactory for shared resources (DB/Cache)
- All entities inherit from `BaseEntity<TId>` (Id, audit fields)
- Domain logic in aggregates, not anemic models

## 9. DDD Blueprint Example (OrderService)
- Folders: Api, Application, Domain (Aggregates, ValueObjects, Events), Infrastructure, Contracts, tests
- Domain: Aggregate Order, OrderItem, Money VO, domain events
- Application: orchestration, validation, handlers
- Infrastructure: persistence, repository, messaging

## 10. Advanced Configuration & Certificates
- Services can use different database types (PostgreSQL, SQL Server, etc.) via config merging
- HTTPS certificates auto-generated for development, copied to output for Docker
- Certificate config: path, password, allow invalid, enabled (see `https-certificate-setup.md`)

## 11. Operation Notes
- Check config before deployment
- Use AppHost for orchestration
- Ensure version consistency
- Follow code analysis rules
- All configs tracked in git for version control

---
This document summarizes architecture, operation, code rules, and DDD blueprint for Aspire Ecommerce Microservices. For detailed guides, see `adding-new-services.md`, `service-configuration-examples.md`, and `https-certificate-setup.md` in this folder.
