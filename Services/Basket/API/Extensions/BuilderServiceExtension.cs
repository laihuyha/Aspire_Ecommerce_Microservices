using Basket.Domain.Interfaces;
using Basket.Infrastructure;
using Basket.Infrastructure.Repositories;
using BuildingBlocks.CQRS.Behaviors;
using BuildingBlocks.Errors;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Basket.Api.Extensions;

public static class BuilderServiceExtension
{
    public static IServiceCollection AddBasketServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        // API Controllers and related services
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        
        // Global Exception Handler
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Swagger configuration
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Basket API", Version = "v1" });
        });

        // OpenAPI configuration
        services.AddOpenApi("basket");

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

    private static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register MediatR from Application assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Basket.Application.Queries.GetCartByIdQuery).Assembly));

        // Register pipeline behaviors from BuildingBlocks
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));

        // Register EF Core DbContext with PostgreSQL provider
        services.AddDbContext<BasketDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        // Register repositories and unit of work
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
