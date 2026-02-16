# ==================== Deploy from Artifacts Script (PowerShell) ====================
# This script deploys the application using pre-published artifacts.
# Part of the artifacts workflow (2-step deployment).
#
# Usage:
#   .\scripts\deploy-from-artifacts.ps1 [ArtifactsDir] [Environment]
#
# Examples:
#   .\scripts\deploy-from-artifacts.ps1 artifacts dev
#   .\scripts\deploy-from-artifacts.ps1 artifacts staging
#   .\scripts\deploy-from-artifacts.ps1 artifacts prod
# ====================================================================================

param(
    [string]$ArtifactsDir = "artifacts",
    [string]$Environment = "dev"
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Host "🚀 Deploying from artifacts..." -ForegroundColor Cyan
Write-Host "   Artifacts: $ArtifactsDir" -ForegroundColor Gray
Write-Host "   Environment: $Environment" -ForegroundColor Gray
Write-Host ""

# Navigate to project root
Push-Location $ProjectRoot

try {
    # Check if artifacts directory exists
    if (-not (Test-Path $ArtifactsDir)) {
        Write-Host "❌ Artifacts directory not found: $ArtifactsDir" -ForegroundColor Red
        Write-Host "   Run .\scripts\publish-artifacts.ps1 first" -ForegroundColor Yellow
        exit 1
    }

    # Check if docker-compose.yml exists
    if (-not (Test-Path "$ArtifactsDir\docker-compose.yml")) {
        Write-Host "❌ docker-compose.yml not found in $ArtifactsDir" -ForegroundColor Red
        Write-Host "   Run .\scripts\publish-artifacts.ps1 first" -ForegroundColor Yellow
        exit 1
    }

    # Build compose file arguments
    $ComposeFiles = @("-f", "$ArtifactsDir\docker-compose.yml")

    # Add environment-specific override if exists
    if (Test-Path "$ArtifactsDir\docker-compose.$Environment.yml") {
        Write-Host "   Using environment override: docker-compose.$Environment.yml" -ForegroundColor Gray
        $ComposeFiles += @("-f", "$ArtifactsDir\docker-compose.$Environment.yml")
    }

    # Add override file if exists
    if (Test-Path "$ArtifactsDir\docker-compose.override.yml") {
        Write-Host "   Using override: docker-compose.override.yml" -ForegroundColor Gray
        $ComposeFiles += @("-f", "$ArtifactsDir\docker-compose.override.yml")
    }

    Write-Host ""
    Write-Host "🐳 Starting Docker Compose deployment..." -ForegroundColor Cyan

    # Deploy with docker compose
    $ComposeArgs = $ComposeFiles + @("up", "-d", "--build")
    & docker compose @ComposeArgs

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Deployment successful!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📊 Container status:" -ForegroundColor Cyan
        & docker compose @ComposeFiles ps
        Write-Host ""
        Write-Host "📝 Useful commands:" -ForegroundColor Yellow
        Write-Host "   View logs: docker compose $($ComposeFiles -join ' ') logs -f" -ForegroundColor Gray
        Write-Host "   Stop: docker compose $($ComposeFiles -join ' ') down" -ForegroundColor Gray
        Write-Host "   Restart: docker compose $($ComposeFiles -join ' ') restart" -ForegroundColor Gray
    } else {
        Write-Host ""
        Write-Host "❌ Deployment failed" -ForegroundColor Red
        exit 1
    }
} finally {
    Pop-Location
}
