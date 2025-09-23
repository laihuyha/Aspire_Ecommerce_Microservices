FROM mcr.microsoft.com/dotnet/sdk:9.0.305-bookworm-slim AS build
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# copy all .csproj file and restore as distinct layer
COPY Aspire/AppHost.sln Aspire/AppHost.sln

COPY Aspire/AppHost/AppHost.csproj Aspire/AppHost/AppHost.csproj
COPY Aspire/ServiceDefaults/ServiceDefaults.csproj Aspire/ServiceDefaults/ServiceDefaults.csproj
COPY BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj
COPY Directory.Packages.props ./Directory.Packages.props
COPY Directory.Build.props ./Directory.Build.props

COPY Services/Catalog/Domain/Catalog.Domain.csproj Services/Catalog/Domain/Catalog.Domain.csproj
COPY Services/Catalog/Persistence/Catalog.Persistence.csproj Services/Catalog/Persistence/Catalog.Persistence.csproj
COPY Services/Catalog/Infrastructure/Catalog.Infrastructure.csproj Services/Catalog/Infrastructure/Catalog.Infrastructure.csproj
COPY Services/Catalog/Application/Catalog.Application.csproj Services/Catalog/Application/Catalog.Application.csproj
COPY Services/Catalog/API/Catalog.API.csproj Services/Catalog/API/Catalog.API.csproj

# restore package
RUN dotnet restore Aspire/AppHost.sln

# copy the app folders over
COPY Aspire/ServiceDefaults Aspire/ServiceDefaults

COPY Services/Catalog Services/Catalog

COPY BuildingBlocks/BuildingBlocks BuildingBlocks/BuildingBlocks

WORKDIR /app/Services/Catalog/API

# Fix run https error on docker
# Ref: https://learn.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-8.0
# Generate the development certificate in the build stage
RUN dotnet publish Catalog.API.csproj -c Release -o /app/out \
    && mkdir -p /app/certs \
    && dotnet dev-certs https --clean \
    && dotnet dev-certs https -ep /app/certs/aspnetapp.pfx -p "simplePassword" \
    && dotnet dev-certs https --trust

# build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0.9-bookworm-slim
WORKDIR /app
COPY --from=build /app/out .
COPY --from=build /app/certs/aspnetapp.pfx /root/.aspnet/https/
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/root/.aspnet/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=simplePassword
ENTRYPOINT ["dotnet", "Catalog.API.dll"]