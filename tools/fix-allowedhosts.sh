#!/usr/bin/env bash
set -euo pipefail

# =============================================================================
# .NET Aspire Cross-Platform AllowedHosts Fix Tool (Bash)
# =============================================================================
# Automatically configures AllowedHosts = "*" in appsettings.Development.json
# and removes it from appsettings.json for all API services.
#
# Works on: Linux, macOS, Windows (Git Bash / WSL)
#
# Usage:
#   chmod +x tools/fix-allowedhosts.sh
#   ./tools/fix-allowedhosts.sh

echo "=== .NET ASPIRE CROSS-PLATFORM FIX TOOL ===" | sed ''  # Colored via echo only (cross-platform)
echo "Auto-configures hostname validation errors"
echo ""

# Auto-detect project root from script location
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "[INFO] Project: $PROJECT_ROOT"
echo "[INFO] Scanning all API services automatically"
echo ""

# Check if jq is available (required for safe JSON editing)
if ! command -v jq &> /dev/null; then
    echo "❌ jq is required but not installed."
    echo "   Install it:"
    echo "     macOS: brew install jq"
    echo "     Ubuntu/Debian: sudo apt install jq"
    echo "     Fedora: sudo dnf install jq"
    echo "     Windows (Git Bash): download from https://jqlang.github.io/jq/download/"
    exit 1
fi

SERVICES_PATH="$PROJECT_ROOT/Services"
if [[ ! -d "$SERVICES_PATH" ]]; then
    echo "[ERROR] Services directory not found at $SERVICES_PATH" >&2
    exit 1
fi

SERVICE_DIRS=("$SERVICES_PATH"/*/)
TOTAL_SERVICES=${#SERVICE_DIRS[@]}
PROCESSED_COUNT=0

echo "[SCAN] Found $TOTAL_SERVICES service(s) to check..."
echo ""

for SERVICE_DIR in "${SERVICE_DIRS[@]}"; do
    SERVICE_DIR="${SERVICE_DIR%/}"  # Remove trailing slash
    SERVICE_NAME=$(basename "$SERVICE_DIR")

    echo "[$SERVICE_NAME] Processing..."

    API_PATH="$SERVICE_DIR/API"
    if [[ ! -d "$API_PATH" ]]; then
        echo "[$SERVICE_NAME] SKIP: No API directory found"
        echo ""
        continue
    fi

    DEV_FILE="$API_PATH/appsettings.Development.json"
    PROD_FILE="$API_PATH/appsettings.json"

    # === Handle Development config ===
    UPDATE_DEV=false

    if [[ ! -f "$DEV_FILE" ]]; then
        echo "[$SERVICE_NAME] CREATE: Building development configuration..."
        cat > "$DEV_FILE" << 'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
EOF
        UPDATE_DEV=true
    elif ! jq -e '.AllowedHosts' "$DEV_FILE" > /dev/null 2>&1; then
        echo "[$SERVICE_NAME] UPDATE: Adding AllowedHosts to development..."
        jq '. + { "AllowedHosts": "*" }' "$DEV_FILE" > "$DEV_FILE.tmp" && mv "$DEV_FILE.tmp" "$DEV_FILE"
        UPDATE_DEV=true
    else
        echo "[$SERVICE_NAME] OK: Development configuration correct"
    fi

    if $UPDATE_DEV; then
        echo "[$SERVICE_NAME] SUCCESS: Development configuration updated"
    fi

    # === Handle Production config ===
    if [[ -f "$PROD_FILE" ]]; then
        if jq -e '.AllowedHosts' "$PROD_FILE" > /dev/null 2>&1; then
            echo "[$SERVICE_NAME] CLEAN: Removing AllowedHosts from production..."
            jq 'del(.AllowedHosts)' "$PROD_FILE" > "$PROD_FILE.tmp" && mv "$PROD_FILE.tmp" "$PROD_FILE"
            echo "[$SERVICE_NAME] SUCCESS: Production configuration cleaned"
        else
            echo "[$SERVICE_NAME] OK: Production configuration already clean"
        fi
    else
        echo "[$SERVICE_NAME] SKIP: No production config file"
    fi

    ((PROCESSED_COUNT++))
    echo "[$SERVICE_NAME] Completed"
    echo ""
done

echo ""
echo "=============================================="
echo "COMPLETED SUCCESSFULLY!"
echo "Services processed    : $PROCESSED_COUNT / $TOTAL_SERVICES"
echo "Ready to test         : https://localhost:6060"
echo "Security note         : AllowedHosts='*' (dev-safe only)"
echo "=============================================="