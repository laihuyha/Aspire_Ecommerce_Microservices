# ==================== Stop / Manage Containers (PowerShell) ====================
# Handles stopping containers regardless of whether they were started via
# 'aspire deploy' (project name has a hash) or 'docker compose up' (fixed name).
#
# Usage:
#   .\scripts\stop.ps1             # Stop and remove all containers
#   .\scripts\stop.ps1 -Logs       # Follow container logs
#   .\scripts\stop.ps1 -Restart    # Restart all containers
#   .\scripts\stop.ps1 -Status     # Show container status
# ================================================================================

param(
    [switch]$Logs,
    [switch]$Restart,
    [switch]$Status
)

$ErrorActionPreference = "Stop"

$ScriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$ComposeFile = "$ProjectRoot\Aspire\docker-compose.yaml"
$ProjectFile = "$ProjectRoot\Aspire\.aspire-project"

# ── Resolve project name ──────────────────────────────────────────────────────
# 1. Try to find from running containers via Docker label
$labels = docker ps --filter "label=com.docker.compose.service=catalog-api" `
    --format "{{.Labels}}" 2>$null | Select-Object -First 1
$ProjectName = if ($labels -match 'com\.docker\.compose\.project=([^,]+)') { $Matches[1] } else { $null }

# 2. Fall back to saved file from last deploy
if (-not $ProjectName -and (Test-Path $ProjectFile)) {
    $ProjectName = (Get-Content $ProjectFile -First 1).Trim()
    Write-Host "   (project name from .aspire-project: $ProjectName)" -ForegroundColor DarkGray
}

if (-not $ProjectName) {
    Write-Host "❌ No running Aspire project found." -ForegroundColor Red
    Write-Host "   Make sure containers are running or re-run .\scripts\deploy-direct.ps1" -ForegroundColor Yellow
    exit 1
}

$ComposeArgs = @("-p", $ProjectName, "-f", $ComposeFile)
Write-Host "   Project: $ProjectName" -ForegroundColor DarkGray

# ── Actions ───────────────────────────────────────────────────────────────────
if ($Logs) {
    Write-Host "📋 Following logs (Ctrl+C to exit)..." -ForegroundColor Cyan
    & docker compose @ComposeArgs logs -f

} elseif ($Restart) {
    Write-Host "🔄 Restarting containers..." -ForegroundColor Cyan
    & docker compose @ComposeArgs restart
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Restarted" -ForegroundColor Green
        & docker compose @ComposeArgs ps
    }

} elseif ($Status) {
    Write-Host "📊 Container status:" -ForegroundColor Cyan
    & docker compose @ComposeArgs ps

} else {
    # Default: stop and remove
    Write-Host "🛑 Stopping containers (project: $ProjectName)..." -ForegroundColor Cyan
    & docker compose @ComposeArgs down

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Stopped" -ForegroundColor Green
        # Clean up saved project name since containers are gone
        if (Test-Path $ProjectFile) { Remove-Item $ProjectFile }
    } else {
        Write-Host "❌ Failed to stop containers" -ForegroundColor Red
        exit 1
    }
}
