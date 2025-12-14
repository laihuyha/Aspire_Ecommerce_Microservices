# Aspire Ecommerce Microservices - Architecture & Infrastructure Summary

## 1. Overview

This is a microservices-based e-commerce platform built with **.NET Aspire**, **DDD (Domain-Driven Design)**, and *
*Clean Architecture** principles. The project follows CQRS pattern using MediatR, with each service implementing a
layered architecture. Currently implements a Catalog service, with plans for Basket and Order services.

Application layer handles commands/queries, Domain holds business logic, Infrastructure implements persistence and
integrations using Marten (document database on PostgreSQL).

------------------------------------------------------------------------

## 2. Application Layer Summary

Contains coordinators of domain logic for each microservice: - Commands / Queries with MediatR - Handlers -
Request/Response DTOs - Validation using FluentValidation - Interfaces - Mappings using Mapster

------------------------------------------------------------------------

## 3. Domain Layer Summary

Contains core business rules for each microservice: - Aggregates - Entities - Value Objects - Domain Events - Domain
Services - Specifications (future)

------------------------------------------------------------------------

## 4. Infrastructure Layer Summary

Implements technical details for each microservice: - Marten document database configuration - Audit interceptors -
Repositories (future) - Messaging (MassTransit / RabbitMQ - future) - External API clients - Cache (Redis)

------------------------------------------------------------------------

## 4. Dependency Flow (Clean Architecture)

```
Aspire/AppHost/             # Orchestrates all services
├── Services/               # Business Microservices
│
BuildingBlocks/            # SHARED CROSS-CUTTING CODE
│
Aspire_Ecommerce_Microservices/
│
├── API (Presentation Layer)
│   ├── Controllers/       # Thin controllers, handle HTTP
│   ├── DTOs/              # Request/Response models
│   ├── Program.cs         # Service entry point
│   └── Middleware/        # Cross-cutting concerns
│
├── Application (Use Cases Layer)
│   ├── Commands/          # Write operations (CQRS)
│   ├── Queries/           # Read operations (CQRS)
│   ├── Handlers/          # Command/Query handlers
│   ├── Validators/        # FluentValidation rules
│   └── Specifications/    # Business query rules
│
├── Domain (Business Logic Layer)
│   ├── Aggregates/        # Aggregate roots
│   ├── Entities/          # Domain entities
│   ├── ValueObjects/      # Value objects
│   ├── Events/            # Domain events
│   └── Interfaces/        # Repository contracts
│
├── Infrastructure (External Concerns Layer)
│   ├── Configurations/    # Database configurations
│   ├── Repositories/      # Data access implementations
│   ├── Services/          # External service integrations
│   └── UnitOfWork/        # Transaction management
│
└── BuildingBlocks (Cross-Cutting Abstractions)
    ├── CQRS/             # Command/Query interfaces
    ├── Entity/           # Base entity patterns
    ├── Errors/           # Exception handling
    ├── Logging/          # Structured logging
    └── Behaviors/        # MediatR pipeline behaviors
```

### Clean Architecture Dependency Rules

**✅ ALLOWED Dependencies:**
```
API → Application + Infrastructure
Application → Domain + Infrastructure + BuildingBlocks
Infrastructure → Domain + BuildingBlocks
Domain → BuildingBlocks
BuildingBlocks → (no dependencies)
```

**❌ FORBIDDEN Dependencies:**
```
Domain → Application (inversion via interfaces in Domain)
Infrastructure → API (data flows outward, dependencies inward)
API → Domain (bypass Application layer)
Application → Infrastructure (via contracts, not direct coupling)
```

### Current Project Flow

```
Catalog.API → Catalog.Application + Catalog.Infrastructure
Catalog.Application → Catalog.Domain + Catalog.Infrastructure + BuildingBlocks
Catalog.Infrastructure → Catalog.Domain + BuildingBlocks
Catalog.Domain → BuildingBlocks
BuildingBlocks → (independent)
```

------------------------------------------------------------------------

## 5. Full Directory Structure (DDD + Clean Architecture)

    Aspire_Ecommerce_Microservices/
     ├── Aspire/
     │   ├── AppHost/
     │   │   ├── AppHost.cs              # Service orchestration
     │   │   ├── Extensions/              # Infrastructure setup
     │   │   └── *.json                   # Configuration files
     │   └── ServiceDefaults/             # Shared configurations
     │
     ├── BuildingBlocks/
     │   ├── CQRS/                       # Command/Query interfaces
     │   ├── Entity/                     # Base entity with auditing
     │   └── Contracts/                  # Shared DTOs and contracts
     │
     ├── Services/
     │   └── Catalog/                    # Currently implemented service
     │       ├── API/
     │       │   ├── Controllers/        # REST endpoints
     │       │   ├── Filters/            # Exception filters
     │       │   └── Program.cs          # Service entry point
     │       │
     │       ├── Application/
     │       │   ├── Commands/           # CRUD commands
     │       │   ├── Queries/            # Read queries
     │       │   ├── Handlers/           # MediatR handlers
     │       │   └── Validators/         # FluentValidation
     │       │
     │       ├── Domain/
     │       │   ├── Aggregates/
     │       │   │   └── Product/        # Product aggregate
     │       │   │       ├── Product.cs
     │       │   │       └── Events/
     │       │   │           └── ProductCreatedDomainEvent.cs
     │       │   └── Common/             # Shared domain concepts
     │       │
     │       └── Infrastructure/
     │           ├── Configurations/     # Marten entity configs
     │           ├── Repositories/       # Repository implementations
     │           └── UnitOfWork/         # Transaction management
     │
     ├── db/                             # Database scripts/init
     ├── tools/                          # Development tools
     └── .env*                           # Environment configs

------------------------------------------------------------------------

## 6. Current Implementation Status

### Implemented Services

- **Catalog Service**: Full CRUD product management with CQRS
    - API: RESTful endpoints with Swagger
    - Application: MediatR commands/queries
    - Domain: Product aggregate with domain events
    - Persistence: Marten document storage

### Planned Services

- **Basket Service**: Shopping cart operations
- **Order Service**: Order processing and lifecycle

### Cross-Cutting Concerns (BuildingBlocks)

- CQRS abstractions
- Base entity with audit fields
- Shared contracts (DTOs, events)

### Infrastructure (.NET Aspire)

- AppHost orchestration
- PostgreSQL database
- Redis cache
- Service discovery
- Health checks with OpenTelemetry

------------------------------------------------------------------------

This file serves as your portable "architecture blueprint" for the Aspire Ecommerce Microservices project. Follow these
patterns when adding new services or features.
