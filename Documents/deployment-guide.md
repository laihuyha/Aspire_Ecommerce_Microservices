# Deployment Guide - Aspire Ecommerce Microservices

This guide explains the deployment workflows available for the Aspire Ecommerce Microservices platform.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Deployment Workflows](#deployment-workflows)
  - [Direct Mode (One-Step)](#direct-mode-one-step)
  - [Artifacts Mode (Two-Step)](#artifacts-mode-two-step)
- [Configuration](#configuration)
- [Environment Management](#environment-management)
- [CI/CD Integration](#cicd-integration)
- [Troubleshooting](#troubleshooting)

## Overview

The project supports two deployment workflows:

| Workflow | Steps | Use Case |
|----------|-------|----------|
| **Direct Mode** | 1 step | Local development, rapid iteration |
| **Artifacts Mode** | 2 steps | CI/CD, multiple environments, production |

Both workflows use Docker Compose as the deployment target but differ in how artifacts are generated and deployed.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Aspire CLI](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)
- [Docker](https://docker.com) and Docker Compose
- Git Bash (Windows only, for running .sh scripts)

## Deployment Workflows

### Direct Mode (One-Step)

**Best for:**
- Local development
- Rapid prototyping
- Quick testing
- Single developer workflow

**How it works:**
The `aspire deploy` command builds all services and starts Docker containers in one integrated step. No intermediate artifacts are created.

**Commands:**

```powershell
# Windows (PowerShell)
.\scripts\deploy-direct.ps1

# Linux/macOS (Bash)
./scripts/deploy-direct.sh

# Or directly with Aspire CLI
aspire deploy -o .\
```

**Flow:**
```
Source Code → Build → Docker Images → Running Containers
```

**Advantages:**
- ✅ Fast iteration cycle
- ✅ Simple - one command
- ✅ No artifact management
- ✅ Integrated build and deploy

**Disadvantages:**
- ❌ No reusable artifacts
- ❌ Harder to customize deployment
- ❌ Not suitable for CI/CD
- ❌ Less portable

### Artifacts Mode (Two-Step)

**Best for:**
- CI/CD pipelines
- Multiple environments (dev, staging, production)
- Team collaboration
- Production deployments
- Infrastructure as Code (IaC)

**How it works:**
First, `aspire publish` generates deployment artifacts (docker-compose.yml, manifests, etc.). Then, these artifacts are deployed using Docker Compose.

**Step 1: Publish Artifacts**

```powershell
# Windows (PowerShell)
.\scripts\publish-artifacts.ps1 artifacts

# Linux/macOS (Bash)
./scripts/publish-artifacts.sh artifacts

# Or directly with Aspire CLI
aspire publish -o artifacts/
```

This generates:
```
artifacts/
├── docker-compose.yml           # Base deployment configuration
├── docker-compose.override.yml  # Optional overrides
├── aspire-manifest.json         # Deployment manifest
├── .env                         # Environment variables
└── parameters.json              # Deployment parameters
```

**Step 2: Deploy from Artifacts**

```powershell
# Windows (PowerShell)
.\scripts\deploy-from-artifacts.ps1 artifacts dev

# Linux/macOS (Bash)
./scripts/deploy-from-artifacts.sh artifacts dev

# Or directly with Docker Compose
docker compose -f artifacts/docker-compose.yml up -d --build
```

**Flow:**
```
Source Code → Build → Artifacts → Review/Customize → Deploy → Running Containers
```

**Advantages:**
- ✅ Reusable artifacts
- ✅ Version control artifacts (git)
- ✅ CI/CD friendly
- ✅ Environment-specific configurations
- ✅ Parameter management
- ✅ Can deploy to multiple targets

**Disadvantages:**
- ❌ Two-step process
- ❌ Manage artifact directory
- ❌ Slower iteration cycle

## Configuration

### Switching Between Modes

Edit `Aspire/AppHost/validation.json`:

```json
{
  "DeploymentMode": {
    "Mode": "direct",    // or "artifacts"
    "Description": "Deployment workflow mode"
  }
}
```

**Note:** This is primarily for documentation and future extensibility. Both modes work regardless of this setting.

### Certificate Setup

The project uses self-signed certificates for HTTPS. Configuration is in `validation.json`:

```json
{
  "CertificateSetup": {
    "Enabled": true,         // Enable automatic certificate setup
    "AutoSetup": true,       // Generate certificate on startup if missing
    "ForceRegenerate": false // Regenerate even if exists
  }
}
```

Manual certificate generation:

```bash
# Generate HTTPS certificate
./tools/generate-aspire-cert.sh
```

## Environment Management

### Environment-Specific Configurations

When using artifacts mode, you can create environment-specific override files:

```bash
artifacts/
├── docker-compose.yml              # Base configuration
├── docker-compose.dev.yml          # Development overrides
├── docker-compose.staging.yml      # Staging overrides
└── docker-compose.prod.yml         # Production overrides
```

**Deployment Example:**

```powershell
# Deploy to development
.\scripts\deploy-from-artifacts.ps1 artifacts dev

# Deploy to staging
.\scripts\deploy-from-artifacts.ps1 artifacts staging

# Deploy to production
.\scripts\deploy-from-artifacts.ps1 artifacts prod
```

**Manual Deployment with Overrides:**

```bash
# Development
docker compose -f artifacts/docker-compose.yml \
               -f artifacts/docker-compose.dev.yml \
               up -d --build

# Production
docker compose -f artifacts/docker-compose.yml \
               -f artifacts/docker-compose.prod.yml \
               up -d --build
```

### Environment Variables

The `.env` file in the artifacts directory contains environment-specific variables:

```env
# Database configuration
POSTGRES_PASSWORD=your-secure-password

# Redis configuration
REDIS_PASSWORD=your-redis-password

# Certificate password
CERT_PASSWORD=AspireSecure2024!

# Environment
ASPNETCORE_ENVIRONMENT=Production
```

## CI/CD Integration

### GitHub Actions Example

```yaml
# .github/workflows/deploy-staging.yml
name: Deploy to Staging

on:
  push:
    branches: [develop]

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 9.0.x

      - name: Install Aspire CLI
        run: dotnet tool install -g aspire

      - name: Publish artifacts
        run: aspire publish -o artifacts/

      - name: Upload artifacts
        uses: actions/upload-artifact@v3
        with:
          name: deployment-artifacts
          path: artifacts/

  deploy:
    needs: publish
    runs-on: ubuntu-latest
    environment: staging
    steps:
      - name: Download artifacts
        uses: actions/download-artifact@v3
        with:
          name: deployment-artifacts
          path: artifacts/

      - name: Deploy to staging
        run: |
          docker compose \
            -f artifacts/docker-compose.yml \
            -f artifacts/docker-compose.staging.yml \
            up -d --build

      - name: Health check
        run: |
          sleep 10
          curl -f http://staging.example.com/health || exit 1
```

### Azure DevOps Example

```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
      - develop
      - main

pool:
  vmImage: 'ubuntu-latest'

stages:
  - stage: Publish
    jobs:
      - job: PublishArtifacts
        steps:
          - task: UseDotNet@2
            inputs:
              version: '9.0.x'

          - script: dotnet tool install -g aspire
            displayName: 'Install Aspire CLI'

          - script: aspire publish -o $(Build.ArtifactStagingDirectory)/artifacts
            displayName: 'Publish Aspire Artifacts'

          - task: PublishBuildArtifacts@1
            inputs:
              pathToPublish: '$(Build.ArtifactStagingDirectory)/artifacts'
              artifactName: 'deployment-artifacts'

  - stage: Deploy
    dependsOn: Publish
    jobs:
      - deployment: DeployToStaging
        environment: 'staging'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: Docker@2
                  inputs:
                    command: 'compose'
                    arguments: '-f $(Pipeline.Workspace)/deployment-artifacts/docker-compose.yml up -d --build'
```

## Troubleshooting

### Issue: Certificate Not Found

**Error:**
```
Unable to configure HTTPS endpoint. No server certificate was specified
```

**Solution:**
```bash
# Regenerate certificate
./tools/generate-aspire-cert.sh

# Or enable auto-setup in validation.json
{
  "CertificateSetup": {
    "Enabled": true,
    "AutoSetup": true
  }
}
```

### Issue: Artifacts Directory Not Found

**Error:**
```
Artifacts directory not found: artifacts
```

**Solution:**
```powershell
# Run publish first
.\scripts\publish-artifacts.ps1 artifacts

# Then deploy
.\scripts\deploy-from-artifacts.ps1 artifacts dev
```

### Issue: Docker Compose Fails to Start

**Error:**
```
Error response from daemon: driver failed programming external connectivity
```

**Solution:**
```bash
# Check if ports are already in use
netstat -ano | findstr :6000
netstat -ano | findstr :5432

# Stop conflicting services
docker compose down

# Or change ports in appsettings.json
```

### Issue: Permission Denied on Scripts (Linux/macOS)

**Error:**
```
bash: ./scripts/deploy-direct.sh: Permission denied
```

**Solution:**
```bash
# Make scripts executable
chmod +x scripts/*.sh

# Run again
./scripts/deploy-direct.sh
```

### View Container Logs

```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f catalog-api

# Last 100 lines
docker compose logs --tail=100 catalog-api
```

### Restart Services

```bash
# Restart all services
docker compose restart

# Restart specific service
docker compose restart catalog-api

# Rebuild and restart
docker compose up -d --build
```

### Clean Up Resources

```bash
# Stop containers
docker compose down

# Stop and remove volumes
docker compose down -v

# Stop and remove images
docker compose down --rmi all

# Nuclear option - remove everything
docker system prune -a --volumes
```

## Best Practices

### For Local Development

1. Use **Direct Mode** for faster iteration
2. Enable auto certificate setup
3. Use hot-reload with `dotnet run`
4. Monitor logs with `docker compose logs -f`

### For Production

1. Use **Artifacts Mode** for controlled deployments
2. Version control your artifacts directory
3. Use environment-specific override files
4. Manage secrets with Azure Key Vault, AWS Secrets Manager, etc.
5. Implement health checks and monitoring
6. Use proper SSL/TLS certificates (Let's Encrypt, commercial CA)

### For CI/CD

1. Separate publish and deploy stages
2. Upload artifacts for traceability
3. Use deployment environments with approvals
4. Implement automated health checks
5. Keep deployment scripts in version control
6. Tag releases and artifacts

## Summary

Choose the deployment workflow that fits your use case:

- **Local Development?** → Use **Direct Mode**
- **CI/CD Pipeline?** → Use **Artifacts Mode**
- **Multiple Environments?** → Use **Artifacts Mode**
- **Quick Testing?** → Use **Direct Mode**
- **Production Deployment?** → Use **Artifacts Mode**

Both workflows are fully supported and can be used interchangeably based on your needs.

---

**Need help?** Check the [troubleshooting section](#troubleshooting) or consult the [Aspire documentation](https://learn.microsoft.com/dotnet/aspire/).
