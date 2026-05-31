using AppHost.Extensions;
using Aspire.Hosting;

// =============================================================================
// AppHost - Microservices Orchestration with .NET Aspire
// =============================================================================
// This is the entry point for the Aspire application host.
// It configures infrastructure, services, and deployment options.
//
// Architecture:
//   - Services are defined in AppHost/Services/ using IServiceDefinition
//   - Infrastructure is managed by InfrastructureFactory (singleton pattern)
//   - Configuration is merged from all service appsettings files
//
// Deployment Workflows:
//   1. DIRECT MODE (default) - One-step deployment:
//      - Command: aspire deploy -o .\ (or ./scripts/deploy-direct.ps1)
//      - Use case: Local development, rapid iteration
//      - Flow: Build → Deploy in one step
//
//   2. ARTIFACTS MODE - Two-step deployment:
//      - Commands:
//        Step 1: aspire publish -o artifacts/ (or ./scripts/publish-artifacts.ps1)
//        Step 2: docker compose -f artifacts/docker-compose.yml up -d
//                (or ./scripts/deploy-from-artifacts.ps1)
//      - Use case: CI/CD, multiple environments, production
//      - Flow: Generate artifacts → Review/customize → Deploy
//      - Configure in validation.json: "DeploymentMode": { "Mode": "artifacts" }
//
// To add a new service:
//   1. Create a new ServiceDefinition in AppHost/Services/
//   2. Register it in ServiceRegistry.CreateDefault() or via WithServices()
// =============================================================================

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Configure the application using fluent API
builder
    .WithDefaultConfiguration()  // Docker Compose, merged config, validation, certificates
    .WithServices();             // Register all microservices

builder.Build().Run();
