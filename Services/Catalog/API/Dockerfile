# syntax=docker/dockerfile:1

# Base stage with common settings
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS base
WORKDIR /src
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

# Dependencies stage - restore packages
FROM base AS dependencies
# Copy solution and project files for efficient layer caching
COPY ["Aspire/AppHost.sln", "Aspire/AppHost.sln"]
COPY ["Directory.Packages.props", "Directory.Build.props", "./"]
COPY ["Aspire/AppHost/AppHost.csproj", "Aspire/AppHost/"]
COPY ["Aspire/ServiceDefaults/ServiceDefaults.csproj", "Aspire/ServiceDefaults/"]
COPY ["BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/BuildingBlocks/"]
COPY ["Services/Catalog/Domain/Catalog.Domain.csproj", "Services/Catalog/Domain/"]
COPY ["Services/Catalog/Persistence/Catalog.Persistence.csproj", "Services/Catalog/Persistence/"]
COPY ["Services/Catalog/Infrastructure/Catalog.Infrastructure.csproj", "Services/Catalog/Infrastructure/"]
COPY ["Services/Catalog/Application/Catalog.Application.csproj", "Services/Catalog/Application/"]
COPY ["Services/Catalog/API/Catalog.API.csproj", "Services/Catalog/API/"]

# Restore packages
RUN dotnet restore "Aspire/AppHost.sln"

# Build stage - compile application
FROM dependencies AS build
# Copy source code
COPY ["Aspire/ServiceDefaults/", "Aspire/ServiceDefaults/"]
COPY ["BuildingBlocks/", "BuildingBlocks/"]
COPY ["Services/Catalog/", "Services/Catalog/"]

WORKDIR "/src/Services/Catalog/API"
# Build with optimizations - build the API project which will build dependencies automatically
RUN dotnet build "Catalog.API.csproj" -c Release --no-restore

# Publish stage - create optimized output with all dependencies
FROM build AS publish
WORKDIR "/src/Services/Catalog/API"
RUN dotnet publish "Catalog.API.csproj" -c Release -o /app/publish \
    /p:GenerateDocumentationFile=false \
    /p:DebugType=None \
    /p:DebugSymbols=false \
    --self-contained false

# Development stage - includes dev tools and certificates
FROM base AS development
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Create directory for HTTPS certificates
RUN mkdir -p /https

# Generate development certificates (trust command is not applicable in containers)
RUN dotnet dev-certs https --clean && \
    dotnet dev-certs https -ep /https/aspnetapp.pfx -p "devcertpass"

EXPOSE 8080 8081

# Set certificate environment variables
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=devcertpass
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "Catalog.API.dll"]

# Production stage - optimized for production
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
# Create a non-root user
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser

WORKDIR /app

# Copy published application
COPY --from=publish --chown=appuser:appgroup /app/publish .

# Create directory for HTTPS certificates (to be mounted in production)
RUN mkdir -p /https && \
    chown -R appuser:appgroup /https

EXPOSE 8080 8081

# Switch to non-root user for security
USER appuser

# Health check for production - check if dotnet runtime is available
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD dotnet --version

ENTRYPOINT ["dotnet", "Catalog.API.dll"]
