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

# Or deploy to Docker Compose for production-like environment
cd Aspire/AppHost
aspire deploy -o ./manifests
docker compose --env-file ./manifests/.env.Production up -d
```

## 🧹 Clean up resources

After deploying your application, it's important to clean up resources to avoid incurring unnecessary costs or consuming local system resources.

### Docker Compose
To clean up resources after deploying with Docker Compose, you can stop and remove the running containers using the following command:

```bash
# Aspire CLI - Stop and remove containers
aspire do docker-compose-down-{environmentName}
example: environmentName = "aspire-ecommerce" => aspire do docker-compose-down-aspire-ecommerce || docker compose --env-file .env.{Environment} down
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
