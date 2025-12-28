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
