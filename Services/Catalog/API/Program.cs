using System.Reflection;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers(); // Add this line to register controllers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Catalog API", Version = "v1" });
});

builder.Services.AddOpenApi("catalog");

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("Catalog.Application")));

builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("database"));
}).UseLightweightSessions();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1"));
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization(); // Add this line for authorization

app.MapControllers(); // Add this line to map controller endpoints

app.Run();
