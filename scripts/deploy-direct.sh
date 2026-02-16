#!/bin/bash
# ==================== Direct Deployment Script ====================
# This script deploys the application using the direct workflow.
# This is a single-step deployment that builds and starts containers.
#
# Usage:
#   ./scripts/deploy-direct.sh
#
# This is equivalent to:
#   aspire deploy -o .\
# ===================================================================

set -e  # Exit on error

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🚀 Direct deployment (integrated build + deploy)..."
echo "   Project: $PROJECT_ROOT/Aspire/AppHost"
echo ""

# Navigate to project root
cd "$PROJECT_ROOT"

# Deploy directly
aspire deploy -o .\\

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Deployment successful!"
    echo ""
    echo "📊 Container status:"
    docker compose ps
    echo ""
    echo "📝 Useful commands:"
    echo "   View logs: docker compose logs -f"
    echo "   Stop: docker compose down"
    echo "   Restart: docker compose restart"
else
    echo ""
    echo "❌ Deployment failed"
    exit 1
fi
