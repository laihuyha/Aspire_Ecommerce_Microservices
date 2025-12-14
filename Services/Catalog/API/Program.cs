using BuildingBlocks.Middlewares;
using Catalog.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Register all Catalog services
builder.Services.AddCatalogServices(builder);

WebApplication app = builder.Build();

// Correlation ID middleware
app.UseCorrelationId();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1"));
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS middleware
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
