# OrderService DDD Structure Summary

## 1. Overview

You are designing **OrderService** using **DDD + Clean Architecture**.\
Application layer handles commands/queries, Domain holds business logic,
Infrastructure implements persistence and integrations.

------------------------------------------------------------------------

## 2. Application Layer Summary

Contains coordinators of domain logic: - Commands / Queries - Handlers -
DTOs - Validation - Interfaces - Mappings

------------------------------------------------------------------------

## 3. Domain Layer Summary

Contains core business rules: - Aggregates - Entities - Value Objects -
Domain Events - Domain Services - Specifications

------------------------------------------------------------------------

## 4. Infrastructure Layer Summary

Implements technical details: - EF Core DbContext - Interceptors -
Repositories - Messaging (MassTransit / RabbitMQ) - External clients -
Cache (Redis)

------------------------------------------------------------------------

## 5. Full Directory Structure (DDD + Clean Architecture)

    OrderService/
     ├── src/
     │   ├── OrderService.Api/
     │   │    ├── Controllers/
     │   │    ├── Endpoints/
     │   │    └── Filters/
     │   │
     │   ├── OrderService.Application/
     │   │    ├── Commands/
     │   │    ├── Queries/
     │   │    ├── Handlers/
     │   │    ├── Validators/
     │   │    ├── DTO/
     │   │    ├── Interfaces/
     │   │    └── Mappings/
     │   │
     │   ├── OrderService.Domain/
     │   │    ├── Aggregates/
     │   │    │     └── Order/
     │   │    │           ├── Order.cs
     │   │    │           ├── OrderItem.cs
     │   │    │           ├── ValueObjects/
     │   │    │           │     ├── OrderId.cs
     │   │    │           │     ├── Money.cs
     │   │    │           └── Events/
     │   │    │                 └── OrderCreatedDomainEvent.cs
     │   │    ├── Services/
     │   │    ├── Specifications/
     │   │    └── Common/
     │   │
     │   ├── OrderService.Infrastructure/
     │   │    ├── EF/
     │   │    │     ├── OrderDbContext.cs
     │   │    │     ├── Configurations/
     │   │    │     └── Interceptors/
     │   │    ├── Repositories/
     │   │    ├── Messaging/
     │   │    ├── External/
     │   │    └── Cache/
     │   │
     │   └── OrderService.Contracts/
     │        ├── Requests/
     │        ├── Responses/
     │        └── Events/
     │
     └── tests/
          ├── UnitTests/
          ├── IntegrationTests/
          └── DomainTests/

------------------------------------------------------------------------

This file serves as your portable "architecture blueprint" for
OrderService.
