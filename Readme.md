# Aspire Ecommerce Microservices

A modular, production-grade microservices solution for ecommerce, built with .NET Aspire. This repository demonstrates scalable architecture, clean code principles, CQRS pattern implementation, and modern cloud-native patterns. Currently implements a Catalog microservice with plans for expansion.

## 🚀 Quick Start

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docker.com) and Docker Compose
- Visual Studio 2022+ or VS Code

### Run the Application

```bash
# Clone the repository
git clone <your-repo-url>
cd Aspire_Ecommerce_Microservices

# Run with .NET Aspire (recommended for development)
dotnet run --project Aspire/AppHost/AppHost.csproj
```

### Deployment Options

The project supports two deployment workflows to accommodate different use cases:

#### Option 1: Direct Mode (Default) - One-Step Deployment

**Best for:** Local development, rapid iteration, quick testing

```powershell
# Windows (PowerShell)
.\scripts\deploy-direct.ps1

# Linux/macOS (Bash)
./scripts/deploy-direct.sh

# Or with Aspire CLI
aspire deploy -o .\
```

This will build and deploy all services in one integrated step.

#### Option 2: Artifacts Mode - Two-Step Deployment

**Best for:** CI/CD pipelines, multiple environments, production deployment

**Step 1: Publish Artifacts**
```powershell
# Windows (PowerShell)
.\scripts\publish-artifacts.ps1 artifacts

# Linux/macOS (Bash)
./scripts/publish-artifacts.sh artifacts

# Or with Aspire CLI
aspire publish -o artifacts/
```

This generates deployment artifacts including `docker-compose.yml`, manifests, and configuration files.

**Step 2: Deploy from Artifacts**
```powershell
# Windows (PowerShell)
.\scripts\deploy-from-artifacts.ps1 artifacts dev

# Linux/macOS (Bash)
./scripts/deploy-from-artifacts.sh artifacts dev

# Or with Docker Compose directly
docker compose -f artifacts/docker-compose.yml up -d --build
```

**Switching Modes:** Edit `Aspire/AppHost/validation.json` and set `"DeploymentMode": { "Mode": "artifacts" }`

## 🧹 Clean up Resources

After deploying your application, clean up resources to free up system resources:

### Stop and Remove Containers

```bash
# For direct mode deployment
docker compose down

# For artifacts mode deployment
docker compose -f artifacts/docker-compose.yml down

# Remove volumes and networks as well
docker compose down -v

# View running containers
docker compose ps
```

## 📁 Solution Structure

```
Aspire_Ecommerce_Microservices/
├── Aspire/AppHost/                     # Service orchestration & configuration
│   ├── Extensions/                     # Infrastructure setup
│   │   ├── InfrastructureExtensions.cs # Reusable components (databases)
│   │   └── CatalogServiceExtensions.cs # Service-specific implementations
│   ├── Utils/                          # Configuration helpers
│   │   ├── ServiceConfigurationHelper.cs # Option extraction methods
│   │   └── ConfigurationMerger.cs      # Service config merging
│   ├── Options/                        # Configuration models
│   └── AppHost.cs                      # Pure orchestration (9 lines)
├── BuildingBlocks/                     # Shared components (CQRS, Entities, etc.)
├── Services/Catalog/                   # Product catalog microservice
├── tools/                              # Development utilities
└── docs/                               # Detailed documentation
```

## 🏗️ Architecture

- **Clean Architecture**: Clear separation of concerns with API, Application, Domain, and Infrastructure layers
- **CQRS Pattern**: Command Query Responsibility Segregation with MediatR
- **Event Sourcing**: Domain events with Marten document database
- **Cloud-Native**: .NET Aspire orchestration with Docker Compose deployment

## 📚 Documentation

### Core Documentation
- **[HTTPS Certificate Setup](https-certificate-setup.md)** - Complete guide for production-like HTTPS with Docker
- **[Service Configuration Examples](service-configuration-examples.md)** - How services configure themselves
- **[Adding New Services](adding-new-services.md)** - Step-by-step guide for new microservices
- **[Multi-Database Architecture](multi-database-architecture.md)** - Different database types per service

### Key Features
- **Configuration Merging**: Services can override global settings while inheriting defaults
- **Options Pattern**: Strongly-typed configuration with validation
- **Multi-Database Support**: PostgreSQL, SQL Server, MySQL, MongoDB ready
- **Docker Integration**: Production-ready container deployments
- **Health Checks**: Comprehensive monitoring and diagnostics

## 🛠️ Technical Stack

| Component | Version | Purpose |
|-----------|---------|---------|
| **.NET Aspire** | 9.4.2 | Cloud-native orchestration |
| **ASP.NET Core** | 9.0 | Web API framework |
| **MediatR** | 12.5.0 | CQRS messaging |
| **Marten** | 8.11.0 | Document DB & Event Store |
| **PostgreSQL** | 16.4 | Primary database |
| **Redis** | 7.x | Distributed cache |
| **OpenTelemetry** | 1.12.0 | Observability |

## 🔧 Current Services

### Catalog Service ✅
- Product management (CRUD operations)
- CQRS with MediatR
- Marten document database
- Redis caching
- RESTful API with OpenAPI

## 🚀 Development Workflow

1. **Local Development**: Use .NET Aspire for hot-reload and debugging
2. **Configuration**: Services define their own settings, merged with globals
3. **Testing**: Unit tests for domain logic, integration tests for APIs
4. **Deployment**: Docker Compose for production-like environments

## 🤝 Contributing

We welcome contributions! Please see our detailed guides for:
- Adding new microservices
- Configuration management
- Deployment strategies
- Testing patterns

---

*Built with ❤️ using .NET Aspire for modern cloud-native microservices*
