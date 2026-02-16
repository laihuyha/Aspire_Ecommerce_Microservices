#!/bin/bash
# ==================== Aspire Artifacts Publishing Script ====================
# This script generates deployment artifacts using the artifacts workflow.
# Artifacts include docker-compose.yml, manifest.json, and parameter files.
#
# Usage:
#   ./scripts/publish-artifacts.sh [output-directory]
#
# Example:
#   ./scripts/publish-artifacts.sh artifacts/
# ============================================================================

set -e  # Exit on error

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
OUTPUT_DIR="${1:-artifacts}"

echo "📦 Publishing Aspire artifacts..."
echo "   Project: $PROJECT_ROOT/Aspire/AppHost"
echo "   Output: $OUTPUT_DIR"
echo ""

# Navigate to project root
cd "$PROJECT_ROOT"

# Publish artifacts
aspire publish -o "$OUTPUT_DIR"

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Artifacts published successfully!"
    echo ""
    echo "📁 Generated files:"
    ls -lh "$OUTPUT_DIR"
    echo ""
    echo "🚀 Next steps:"
    echo "   1. Review artifacts in $OUTPUT_DIR/"
    echo "   2. Deploy using: ./scripts/deploy-from-artifacts.sh $OUTPUT_DIR"
    echo "   3. Or manually: docker compose -f $OUTPUT_DIR/docker-compose.yml up -d"
else
    echo ""
    echo "❌ Failed to publish artifacts"
    exit 1
fi
