# Aspire Ecommerce Microservices

This repository contains a modular microservices solution for an ecommerce platform, built using .NET Aspire.

## Solution Structure

- **Aspire/**
  - **AppHost/**: Main entry point for hosting and orchestrating microservices.
    - `AppHost.csproj`, `AppHost.cs`: Project and startup logic.
    - `appsettings.json`, `appsettings.Development.json`: Configuration files.
    - `Properties/launchSettings.json`: Launch profiles for development.
  - **ServiceDefaults/**: Shared service configuration and extensions.
    - `Extensions.cs`: Common extension methods.
    - `ServiceDefaults.csproj`: Project file for shared logic.
  - `AppHost.sln`: Solution file for the entire microservices system.
  - `Directory.Build.props`, `Directory.Packages.props`: Centralized build and package management.

- **BuildingBlocks/**: Shared components and utilities used across services.
  - Common abstractions, interfaces, and reusable components.

- **Services/**: Individual microservices that make up the ecommerce platform.
  - **Catalog/**: Product catalog management service.
    - **API/**: RESTful API endpoints and controllers.
    - **Application/**: Application business logic and use cases.
    - **Domain/**: Core domain models and business rules.
    - **Infrastructure/**: Data access, external services integration.
  - **Basket/**: Shopping basket service.
    - **Basket.API/**: API endpoints for basket operations.

## Getting Started

1. **Prerequisites**
   - [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
   - Visual Studio 2022+ or VS Code

2. **Build the Solution**
   ```powershell
   dotnet build Aspire\AppHost.sln
   ```

3. **Run the AppHost**
   ```powershell
   dotnet run --project Aspire\AppHost\AppHost.csproj
   ```

4. **Configuration**
   - Adjust settings in `appsettings.json` as needed for your environment.

## Project Overview

### Core Components

- **AppHost**: 
  - Orchestrates and runs all microservices
  - Manages service discovery and communication
  - Handles configuration and environment setup

- **ServiceDefaults**: 
  - Contains shared configuration and extension methods
  - Implements common service features:
    - Service discovery
    - Resilience patterns
    - Health checks
    - OpenTelemetry integration
    - Standardized HTTP client configuration

### Microservices

1. **Catalog Service**
   - Manages product catalog and inventory
   - **Key Features**:
     - Product information management
     - Category management
     - Search and filtering
     - Brand management
   - **Architecture**:

2. **Basket Service**
   - Handles shopping cart operations
   - **Key Features**:
     - Shopping cart management
     - Item addition/removal
     - Price calculation
     - Temporary storage
   - **Architecture**:

## Technical Features

### Monitoring and Diagnostics

- Health checks endpoints at `/health` and `/alive`
- OpenTelemetry integration for:
  - Distributed tracing
  - Metrics collection
  - Logging
- Support for multiple telemetry exporters:
  - OTLP (OpenTelemetry Protocol)
  - Azure Monitor (optional)

### Resilience and Reliability

- Built-in resilience patterns
- Standard HTTP client configuration with:
  - Automatic retries
  - Circuit breaker
  - Timeout policies
- Service discovery integration

### Security

- Health check endpoints secured in non-development environments
- Configurable service discovery schemes
- Environment-specific settings

## Development Guidelines

1. **Service Development**
   - Follow Clean Architecture principles
   - Implement health checks for monitoring
   - Use OpenTelemetry for observability
   - Implement resilience patterns

2. **Configuration Management**
   - Use environment-specific settings
   - Follow configuration best practices
   - Utilize centralized package management

3. **Deployment**
   - Support for containerization
   - Environment-specific configurations
   - Orchestration using .NET Aspire
