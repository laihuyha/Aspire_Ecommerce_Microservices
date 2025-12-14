# .NET Aspire Cross-Platform AllowedHosts Fix Tool
# Safe JSON configuration for Windows, Mac, Linux
# Usage: pwsh -ExecutionPolicy Bypass -File tools/fix-allowedhosts.ps1

Write-Host "=== .NET ASPIRE CROSS-PLATFORM FIX TOOL ===" -ForegroundColor Cyan
Write-Host "Auto-configures hostname validation errors" -ForegroundColor Gray
Write-Host ""

# Auto-detect project root from script location
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir

Write-Host "[INFO] Project: $projectRoot" -ForegroundColor Blue
Write-Host "[INFO] Scanning all API services automatically" -ForegroundColor Blue
Write-Host ""

# Safe JSON handling
function Get-JsonContent { param($p)
    try {
        if (Test-Path $p) {
            $c = Get-Content $p -Raw -Encoding UTF8
            return ConvertFrom-Json $c
        }
    } catch {
        Write-Host "[WARN] Invalid JSON at $p" -ForegroundColor Red
    }
    return $null
}

function Set-JsonContent { param($p, $o)
    $d = Split-Path -Parent $p
    if (-not (Test-Path $d)) {
        New-Item -ItemType Directory -Path $d -Force | Out-Null
    }
    ConvertTo-Json $o -Depth 10 | Set-Content $p -Encoding UTF8 -NoNewline
}

# Discover and process all API services
$servicesPath = Join-Path $projectRoot "Services"
if (-not (Test-Path $servicesPath)) {
    Write-Host "[ERROR] Services directory not found at $servicesPath" -ForegroundColor Red
    exit 1
}

$serviceDirs = Get-ChildItem -Path $servicesPath -Directory
$processedCount = 0
$totalServices = $serviceDirs.Count

Write-Host "[SCAN] Found $totalServices service(s) to check..." -ForegroundColor Blue
Write-Host ""

foreach ($serviceDir in $serviceDirs) {
    $serviceName = $serviceDir.Name
    Write-Host "[$serviceName] Processing..." -ForegroundColor Yellow

    $apiPath = Join-Path $serviceDir.FullName "API"
    if (-not (Test-Path $apiPath)) {
        Write-Host "[$serviceName] SKIP: No API directory found" -ForegroundColor Gray
        continue
    }

    $devFile = Join-Path $apiPath "appsettings.Development.json"
    $prodFile = Join-Path $apiPath "appsettings.json"

    # Configure development settings for this service
    $devConfig = Get-JsonContent $devFile
    $updateDev = $false

    if (-not $devConfig) {
        Write-Host "[$serviceName] CREATE: Building development configuration..." -ForegroundColor Magenta
        $devConfig = [ordered]@{
            Logging = [ordered]@{
                LogLevel = [ordered]@{
                    Default = "Information"
                    "Microsoft.AspNetCore" = "Warning"
                }
            }
            AllowedHosts = "*"
        }
        $updateDev = $true
    } elseif (-not $devConfig.AllowedHosts) {
        Write-Host "[$serviceName] UPDATE: Adding AllowedHosts to development..." -ForegroundColor Yellow
        $devConfig | Add-Member -Name "AllowedHosts" -Value "*" -MemberType NoteProperty -Force
        $updateDev = $true
    } else {
        Write-Host "[$serviceName] OK: Development configuration correct" -ForegroundColor Green
    }

    if ($updateDev) {
        Set-JsonContent $devFile $devConfig
        Write-Host "[$serviceName] SUCCESS: Development configuration updated" -ForegroundColor Green
    }

    # Clean production configuration for this service
    if (Test-Path $prodFile) {
        $prodConfig = Get-JsonContent $prodFile
        if ($prodConfig -and $prodConfig.PSObject.Properties["AllowedHosts"]) {
            Write-Host "[$serviceName] CLEAN: Removing AllowedHosts from production..." -ForegroundColor Yellow
            $prodConfig.PSObject.Properties.Remove("AllowedHosts")
            Set-JsonContent $prodFile $prodConfig
            Write-Host "[$serviceName] SUCCESS: Production configuration cleaned" -ForegroundColor Green
        } else {
            Write-Host "[$serviceName] OK: Production configuration already clean" -ForegroundColor Green
        }
    } else {
        Write-Host "[$serviceName] SKIP: No production config file" -ForegroundColor Gray
    }

    $processedCount++
    Write-Host "[$serviceName] Completed" -ForegroundColor Blue
    Write-Host ""
}

Write-Host ""
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "Services processed    : $processedCount / $totalServices" -ForegroundColor White
Write-Host "Ready to test         : https://localhost:6060" -ForegroundColor Green
Write-Host "Security note         : AllowedHosts='*' (dev-safe only)" -ForegroundColor Yellow
Write-Host "==============================================" -ForegroundColor Cyan
