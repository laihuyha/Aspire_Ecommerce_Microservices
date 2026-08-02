using Basket.Api.Extensions;
using BuildingBlocks.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Register all Basket services
builder.Services.AddBasketServices(builder);

var app = builder.Build();

// Correlation ID middleware
app.UseCorrelationId();

// Global Exception Handling
app.UseExceptionHandler();

app.MapDefaultEndpoints();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket API v1"));
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS middleware
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();