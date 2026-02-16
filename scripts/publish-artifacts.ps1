# ==================== Aspire Artifacts Publishing Script (PowerShell) ====================
# This script generates deployment artifacts using the artifacts workflow.
# Artifacts include docker-compose.yml, manifest.json, and parameter files.
#
# Usage:
#   .\scripts\publish-artifacts.ps1 [OutputDirectory]
#
# Example:
#   .\scripts\publish-artifacts.ps1 artifacts
# ==========================================================================================

param(
    [string]$OutputDir = "artifacts"
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Host "📦 Publishing Aspire artifacts..." -ForegroundColor Cyan
Write-Host "   Project: $ProjectRoot\Aspire\AppHost" -ForegroundColor Gray
Write-Host "   Output: $OutputDir" -ForegroundColor Gray
Write-Host ""

# Navigate to project root
Push-Location $ProjectRoot

try {
    # Publish artifacts
    aspire publish -o $OutputDir

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Artifacts published successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📁 Generated files:" -ForegroundColor Cyan
        Get-ChildItem $OutputDir | Format-Table -AutoSize
        Write-Host ""
        Write-Host "🚀 Next steps:" -ForegroundColor Yellow
        Write-Host "   1. Review artifacts in $OutputDir\" -ForegroundColor Gray
        Write-Host "   2. Deploy using: .\scripts\deploy-from-artifacts.ps1 $OutputDir" -ForegroundColor Gray
        Write-Host "   3. Or manually: docker compose -f $OutputDir\docker-compose.yml up -d" -ForegroundColor Gray
    } else {
        Write-Host ""
        Write-Host "❌ Failed to publish artifacts" -ForegroundColor Red
        exit 1
    }
} finally {
    Pop-Location
}
