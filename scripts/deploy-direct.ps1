# ==================== Direct Deployment Script (PowerShell) ====================
# This script deploys the application using the direct workflow.
# This is a single-step deployment that builds and starts containers.
#
# Usage:
#   .\scripts\deploy-direct.ps1
#
# This is equivalent to:
#   aspire deploy -o .\
# ================================================================================

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Host "🚀 Direct deployment (integrated build + deploy)..." -ForegroundColor Cyan
Write-Host "   Project: $ProjectRoot\Aspire\AppHost" -ForegroundColor Gray
Write-Host ""

# Navigate to project root
Push-Location $ProjectRoot

try {
    # Deploy directly
    aspire deploy -o .\

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Deployment successful!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📊 Container status:" -ForegroundColor Cyan
        docker compose ps
        Write-Host ""
        Write-Host "📝 Useful commands:" -ForegroundColor Yellow
        Write-Host "   View logs: docker compose logs -f" -ForegroundColor Gray
        Write-Host "   Stop: docker compose down" -ForegroundColor Gray
        Write-Host "   Restart: docker compose restart" -ForegroundColor Gray
    } else {
        Write-Host ""
        Write-Host "❌ Deployment failed" -ForegroundColor Red
        exit 1
    }
} finally {
    Pop-Location
}
