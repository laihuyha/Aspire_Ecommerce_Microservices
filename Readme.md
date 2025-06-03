# Aspire Ecommerce Microservices

A modular, production-grade microservices solution for ecommerce, built with .NET Aspire. This repository demonstrates scalable architecture, clean code principles, and modern cloud-native patterns.

---

## Table of Contents

- [Solution Structure](#solution-structure)
- [Getting Started](#getting-started)
- [Architecture Overview](#architecture-overview)
- [Microservices Breakdown](#microservices-breakdown)
- [Technical Features](#technical-features)
- [Development Guidelines](#development-guidelines)
- [Extending the Platform](#extending-the-platform)
- [Contributing](#contributing)
- [License](#license)

---

## Solution Structure

```
Aspire_Ecommerce_Microservices/
│
├── Aspire/
│   ├── AppHost/                # Main entry point, orchestration, and configuration
│   ├── ServiceDefaults/        # Shared service configuration and extensions
│   ├── AppHost.sln             # Solution file
│   ├── Directory.Build.props   # Centralized build settings
│   └── Directory.Packages.props# Centralized package management
│
├── BuildingBlocks/             # Shared abstractions, interfaces, and utilities
│
└── Services/
    ├── Catalog/                # Product catalog microservice
    │   ├── API/                # REST API endpoints
    │   ├── Application/        # Business logic and use cases
    │   ├── Domain/             # Domain models and rules
    │   ├── Infrastructure/     # Data access and integrations
    │   └── Persistence/        # Database context and migrations
    └── Basket/                 # Shopping basket microservice
        └── Basket.API/         # API endpoints for basket operations
```

---

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ or VS Code

### Build and Run

1. **Clone the repository**
   ```powershell
   git clone <your-repo-url>
   cd Aspire_Ecommerce_Microservices
   ```

2. **Build the solution**
   ```powershell
   dotnet build Aspire\AppHost.sln
   ```

3. **Run the AppHost**
   ```powershell
   dotnet run --project Aspire\AppHost\AppHost.csproj
   ```

4. **Configuration**
   - Adjust `appsettings.json` and environment-specific files as needed.

---

## Architecture Overview

- **AppHost**: Orchestrates and runs all microservices, manages service discovery, configuration, and environment setup.
- **ServiceDefaults**: Provides shared configuration, extension methods, and implements cross-cutting concerns (resilience, health checks, telemetry).
- **BuildingBlocks**: Contains reusable abstractions, interfaces, and utilities to promote DRY and clean architecture.
- **Services**: Each microservice follows Clean Architecture, with clear separation of API, Application, Domain, Infrastructure, and (for Catalog) Persistence layers.

---

## Microservices Breakdown

### Catalog Service

- **Purpose**: Manages product catalog and inventory.
- **Key Features**:
  - Product and category management
  - Search and filtering
  - Brand management
- **Layers**:
  - **API**: RESTful endpoints
  - **Application**: Business logic, use cases
  - **Domain**: Core models, rules
  - **Infrastructure**: Data access, integrations
  - **Persistence**: Database context and migrations

### Basket Service

- **Purpose**: Handles shopping cart operations.
- **Key Features**:
  - Shopping cart CRUD
  - Item addition/removal
  - Price calculation
  - Temporary storage

---

## Technical Features

### Monitoring and Diagnostics

- Health check endpoints: `/health`, `/alive`
- OpenTelemetry integration for:
  - Distributed tracing
  - Metrics collection
  - Structured logging
- Multiple telemetry exporters supported:
  - OTLP (OpenTelemetry Protocol)
  - Azure Monitor (optional)

### Resilience and Reliability

- Built-in resilience patterns (retry, circuit breaker, timeout)
- Standardized HTTP client configuration
- Service discovery integration

### Security

- Health check endpoints secured in non-development environments
- Configurable service discovery schemes
- Environment-specific settings

---

## Development Guidelines

### Service Development

- Follow Clean Architecture principles
- Implement health checks and OpenTelemetry for observability
- Use resilience patterns for all outbound calls

### Configuration Management

- Use environment-specific settings
- Centralize package and build management
- Store secrets securely (do not commit secrets to source control)

### Deployment

- Containerization support (add Dockerfiles as needed)
- Environment-specific configuration
- Orchestration via .NET Aspire or external orchestrators (Kubernetes, Docker Compose, etc.)

---

## Extending the Platform

- **Add a new microservice**: Use the existing structure as a template. Implement API, Application, Domain, Infrastructure, and (if needed) Persistence layers.
- **Add shared logic**: Place reusable code in `BuildingBlocks` or `ServiceDefaults`.
- **Integrate new tools**: Leverage .NET Aspire's extensibility for monitoring, resilience, and configuration.

---

## Contributing

Contributions are welcome! Please open issues or submit pull requests for improvements, bug fixes, or new features.

---

## License

This project is licensed under the MIT License.

---
