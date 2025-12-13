param(
    [string]$InfraConfigPath = "$PSScriptRoot/../infra-config.json",
    [string]$OutputDir = "$PSScriptRoot/.."
)

Write-Host "Reading infra config from: $InfraConfigPath" -ForegroundColor Green

if (!(Test-Path $InfraConfigPath)) {
    Write-Error "Infra config file not found at: $InfraConfigPath"
    exit 1
}

# Read and parse JSON
$content = Get-Content $InfraConfigPath -Raw
$config = ConvertFrom-Json $content

Write-Host "Generating .env file..." -ForegroundColor Yellow

# Generate .env content
$envContent = @"
# Environment configuration - Auto-generated from infra-config.json
# Do not edit manually - changes will be overwritten

# ASP.NET Core Environment
ASPNETCORE_ENVIRONMENT=$($config.environment)

# Database Configuration
POSTGRES_DB=$($config.services.postgres.catalog.db)
POSTGRES_USER=$($config.services.postgres.catalog.user)
POSTGRES_PASSWORD=$($config.services.postgres.catalog.password)
POSTGRES_PORT=$($config.services.postgres.catalog.port)

# Redis Configuration
REDIS_CONNECTION=distributedcache:$($config.services.redis.port)
REDIS_PORT=$($config.services.redis.port)

# Redis Commander (Web UI for Redis)
REDIS_COMMANDER_USER=$($config.services.redisCommander.httpUser)
REDIS_COMMANDER_PASSWORD=$($config.services.redisCommander.httpPassword)
REDIS_COMMANDER_PORT=$($config.services.redisCommander.port)

# API Ports
CATALOG_HTTP_PORT=$($config.apis.catalog.httpPort)
CATALOG_HTTPS_PORT=$($config.apis.catalog.httpsPort)

# Docker Registry (optional)
DOCKER_REGISTRY=$($config.docker.registry)
TAG=$($config.docker.tag)
"@

# Write .env
$envPath = Join-Path $OutputDir ".env"
$envContent | Out-File -FilePath $envPath -Encoding UTF8 -Force
Write-Host "Generated .env at: $envPath" -ForegroundColor Green

Write-Host "Generating docker-compose.yml..." -ForegroundColor Yellow

# Generate docker-compose.yml content
$dockerCompose = @"
# Auto-generated docker-compose configuration from infra-config.json
# Do not edit manually - changes will be overwritten

services:
  catalog.api:
    image: `${DOCKER_REGISTRY:-$($config.docker.registry)}/catalog.api:`${TAG:-$($config.docker.tag)}
    build:
      context: .
      dockerfile: Services/Catalog/API/Dockerfile
      target: final
    depends_on:
      catalog.db:
        condition: service_healthy
    environment:
      - ASPNETCORE_ENVIRONMENT=`${ASPNETCORE_ENVIRONMENT:-$($config.apis.catalog.environment)}
      - ASPNETCORE_HTTP_PORTS=8080
      - ASPNETCORE_HTTPS_PORTS=8081
      - ConnectionStrings__CatalogDb=Server=catalog.db;Port=5432;Database=`${POSTGRES_DB:-$($config.services.postgres.catalog.db)};User Id=`${POSTGRES_USER:-$($config.services.postgres.catalog.user)};Password=`${POSTGRES_PASSWORD};Include Error Detail=true
      - ConnectionStrings__Redis=`${REDIS_CONNECTION:-distributedcache:$($config.services.redis.port)}
    ports:
      - "`${CATALOG_HTTP_PORT:-$($config.apis.catalog.httpPort)}:8080"
      - "`${CATALOG_HTTPS_PORT:-$($config.apis.catalog.httpsPort)}:8081"
    healthcheck:
      test: ["CMD-SHELL", "$($config.apis.catalog.healthcheck.test)"]
      interval: $($config.apis.catalog.healthcheck.interval)
      timeout: $($config.apis.catalog.healthcheck.timeout)
      retries: $($config.apis.catalog.healthcheck.retries)
      start_period: $($config.apis.catalog.healthcheck.startPeriod)
    deploy:
      resources:
        limits:
          memory: $($config.apis.catalog.resources.limits.memory)
          cpus: '$($config.apis.catalog.resources.limits.cpus)'
        reservations:
          memory: $($config.apis.catalog.resources.reservations.memory)
          cpus: '$($config.apis.catalog.resources.reservations.cpus)'
    restart: unless-stopped
    networks:
      - $($config.docker.network.name)

  catalog.db:
    image: $($config.services.postgres.catalog.image)
    environment:
      - POSTGRES_DB=`${POSTGRES_DB:-$($config.services.postgres.catalog.db)}
      - POSTGRES_USER=`${POSTGRES_USER:-$($config.services.postgres.catalog.user)}
      - POSTGRES_PASSWORD=`${POSTGRES_PASSWORD}
    volumes:
      - $($config.services.postgres.catalog.volume):/var/lib/postgresql/data
      - ./db/init:/docker-entrypoint-initdb.d
    ports:
      - "`${POSTGRES_PORT:-$($config.services.postgres.catalog.port)}:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U `${POSTGRES_USER:-$($config.services.postgres.catalog.user)} -d `${POSTGRES_DB:-$($config.services.postgres.catalog.db)}"]
      interval: $($config.services.postgres.catalog.healthcheck.interval)
      timeout: $($config.services.postgres.catalog.healthcheck.timeout)
      retries: $($config.services.postgres.catalog.healthcheck.retries)
      start_period: $($config.services.postgres.catalog.healthcheck.startPeriod)
    deploy:
      resources:
        limits:
          memory: $($config.services.postgres.catalog.resources.limits.memory)
        reservations:
          memory: $($config.services.postgres.catalog.resources.reservations.memory)
    restart: unless-stopped
    networks:
      - $($config.docker.network.name)

  distributedcache:
    image: $($config.services.redis.image)
    command: $($config.services.redis.command)
    ports:
      - "`${REDIS_PORT:-$($config.services.redis.port)}:$($config.services.redis.targetPort)"
    volumes:
      - $($config.services.redis.volume):/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: $($config.services.redis.healthcheck.interval)
      timeout: $($config.services.redis.healthcheck.timeout)
      retries: $($config.services.redis.healthcheck.retries)
      start_period: $($config.services.redis.healthcheck.startPeriod)
    deploy:
      resources:
        limits:
          memory: $($config.services.redis.resources.limits.memory)
        reservations:
          memory: $($config.services.redis.resources.reservations.memory)
    restart: unless-stopped
    networks:
      - $($config.docker.network.name)

  redis-commander:
    image: $($config.services.redisCommander.image)
    environment:
      - REDIS_HOSTS=$($config.services.redisCommander.redisHosts)
      - HTTP_USER=`${REDIS_COMMANDER_USER:-$($config.services.redisCommander.httpUser)}
      - HTTP_PASSWORD=`${REDIS_COMMANDER_PASSWORD}
    ports:
      - "`${REDIS_COMMANDER_PORT:-$($config.services.redisCommander.port)}:$($config.services.redisCommander.targetPort)"
    depends_on:
      - distributedcache
    deploy:
      resources:
        limits:
          memory: $($config.services.redisCommander.resources.limits.memory)
        reservations:
          memory: $($config.services.redisCommander.resources.reservations.memory)
    restart: unless-stopped
    networks:
      - $($config.docker.network.name)

volumes:
  $($config.services.postgres.catalog.volume):
    driver: local
  $($config.services.redis.volume):
    driver: local

networks:
  $($config.docker.network.name):
    driver: $($config.docker.network.driver)
    ipam:
      config:
        - subnet: $($config.docker.network.subnet)
"@

# Write docker-compose.yml
$composePath = Join-Path $OutputDir "docker-compose.yml"
$dockerCompose | Out-File -FilePath $composePath -Encoding UTF8 -Force
Write-Host "Generated docker-compose.yml at: $composePath" -ForegroundColor Green

Write-Host "Config generation completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Review the generated files" -ForegroundColor White
Write-Host "2. Run 'docker-compose up' to test production environment" -ForegroundColor White
Write-Host "3. Run Aspire AppHost to test development environment" -ForegroundColor White</result>
</write_to_file>
