using System.Reflection;
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

        // MediatR configuration with all behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.Load("Catalog.Application"));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));
        });

        // Marten configuration
        services.AddSingleton<MartenRegistry, ProductEntityTypeConfiguration>();
        services.AddMarten(options =>
        {
            options.Connection(builder.Configuration.GetConnectionString("Database"));
            options.RegisterDocumentType<Product>();
        }).UseLightweightSessions();

        // Infrastructure/Persistence services registration
        services.AddScoped<IUnitOfWork, MartenUnitOfWork>();
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
