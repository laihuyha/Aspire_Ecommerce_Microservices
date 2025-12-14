using BuildingBlocks.CQRS.Behaviors;
using Catalog.Api.Filters;
using Catalog.Domain.Aggregates.Product;
using Catalog.Domain.Interfaces;
using Catalog.Infrastructure.Configurations;
using Catalog.Infrastructure.Repositories;
using Catalog.Infrastructure.UnitOfWork;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Catalog.Api.Extensions;

public static class BuilderServiceExtension
{
    public static IServiceCollection AddCatalogServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        // API Controllers and related services
        services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());
        services.AddEndpointsApiExplorer();

        // Swagger configuration
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog API", Version = "v1" });
        });

        // OpenAPI configuration
        services.AddOpenApi("catalog");

        // CORS configuration for development and cross-origin requests
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Register all Application layer services (includes domain, infrastructure, repositories, etc.)
        services.AddApplicationServices(builder.Configuration);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register MediatR from Application assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Catalog.Application.Commands.CreateProductCommand).Assembly));

        // Register pipeline behaviors from BuildingBlocks
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));

        // Register Marten configuration
        services.AddSingleton<MartenRegistry, ProductEntityTypeConfiguration>();

        // Add Marten with database configuration
        services.AddMarten(options =>
        {
            options.Connection(configuration.GetConnectionString("Database")!);
            options.RegisterDocumentType<Product>();
        }).UseLightweightSessions();

        // Register repositories and unit of work
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, MartenUnitOfWork>();

        return services;
    }
}
