#!/bin/bash
# ==================== Deploy from Artifacts Script ====================
# This script deploys the application using pre-published artifacts.
# Part of the artifacts workflow (2-step deployment).
#
# Usage:
#   ./scripts/deploy-from-artifacts.sh [artifacts-directory] [environment]
#
# Examples:
#   ./scripts/deploy-from-artifacts.sh artifacts/ dev
#   ./scripts/deploy-from-artifacts.sh artifacts/ staging
#   ./scripts/deploy-from-artifacts.sh artifacts/ prod
# =======================================================================

set -e  # Exit on error

ARTIFACTS_DIR="${1:-artifacts}"
ENVIRONMENT="${2:-dev}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🚀 Deploying from artifacts..."
echo "   Artifacts: $ARTIFACTS_DIR"
echo "   Environment: $ENVIRONMENT"
echo ""

# Navigate to project root
cd "$PROJECT_ROOT"

# Check if artifacts directory exists
if [ ! -d "$ARTIFACTS_DIR" ]; then
    echo "❌ Artifacts directory not found: $ARTIFACTS_DIR"
    echo "   Run ./scripts/publish-artifacts.sh first"
    exit 1
fi

# Check if docker-compose.yml exists
if [ ! -f "$ARTIFACTS_DIR/docker-compose.yml" ]; then
    echo "❌ docker-compose.yml not found in $ARTIFACTS_DIR"
    echo "   Run ./scripts/publish-artifacts.sh first"
    exit 1
fi

# Build compose file arguments
COMPOSE_FILES="-f $ARTIFACTS_DIR/docker-compose.yml"

# Add environment-specific override if exists
if [ -f "$ARTIFACTS_DIR/docker-compose.$ENVIRONMENT.yml" ]; then
    echo "   Using environment override: docker-compose.$ENVIRONMENT.yml"
    COMPOSE_FILES="$COMPOSE_FILES -f $ARTIFACTS_DIR/docker-compose.$ENVIRONMENT.yml"
fi

# Add override file if exists
if [ -f "$ARTIFACTS_DIR/docker-compose.override.yml" ]; then
    echo "   Using override: docker-compose.override.yml"
    COMPOSE_FILES="$COMPOSE_FILES -f $ARTIFACTS_DIR/docker-compose.override.yml"
fi

echo ""
echo "🐳 Starting Docker Compose deployment..."

# Deploy with docker compose
docker compose $COMPOSE_FILES up -d --build

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Deployment successful!"
    echo ""
    echo "📊 Container status:"
    docker compose $COMPOSE_FILES ps
    echo ""
    echo "📝 Useful commands:"
    echo "   View logs: docker compose $COMPOSE_FILES logs -f"
    echo "   Stop: docker compose $COMPOSE_FILES down"
    echo "   Restart: docker compose $COMPOSE_FILES restart"
else
    echo ""
    echo "❌ Deployment failed"
    exit 1
fi
