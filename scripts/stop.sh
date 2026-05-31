#!/bin/bash
# ==================== Stop / Manage Containers ====================
# Handles stopping containers regardless of whether they were started via
# 'aspire deploy' (project name has a hash) or 'docker compose up' (fixed name).
#
# Usage:
#   ./scripts/stop.sh              # Stop and remove all containers
#   ./scripts/stop.sh --logs       # Follow container logs
#   ./scripts/stop.sh --restart    # Restart all containers
#   ./scripts/stop.sh --status     # Show container status
# ==================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
COMPOSE_FILE="$PROJECT_ROOT/Aspire/docker-compose.yaml"
PROJECT_FILE="$PROJECT_ROOT/Aspire/.aspire-project"

ACTION="${1:-down}"

# ── Resolve project name ──────────────────────────────────────────────────────
# 1. Try to find from running containers via Docker label
PROJECT_NAME=$(docker ps --filter "label=com.docker.compose.service=catalog-api" \
    --format "{{.Labels}}" 2>/dev/null | head -1 | \
    sed -n 's/.*com\.docker\.compose\.project=\([^,]*\).*/\1/p')

# 2. Fall back to saved file from last deploy
if [ -z "$PROJECT_NAME" ] && [ -f "$PROJECT_FILE" ]; then
    PROJECT_NAME=$(cat "$PROJECT_FILE" | tr -d '[:space:]')
    echo "   (project name from .aspire-project: $PROJECT_NAME)"
fi

if [ -z "$PROJECT_NAME" ]; then
    echo "❌ No running Aspire project found."
    echo "   Make sure containers are running or re-run ./scripts/deploy-direct.sh"
    exit 1
fi

COMPOSE_ARGS="-p $PROJECT_NAME -f $COMPOSE_FILE"
echo "   Project: $PROJECT_NAME"

# ── Actions ───────────────────────────────────────────────────────────────────
case "$ACTION" in
    --logs)
        echo "📋 Following logs (Ctrl+C to exit)..."
        docker compose $COMPOSE_ARGS logs -f
        ;;

    --restart)
        echo "🔄 Restarting containers..."
        docker compose $COMPOSE_ARGS restart
        if [ $? -eq 0 ]; then
            echo "✅ Restarted"
            docker compose $COMPOSE_ARGS ps
        fi
        ;;

    --status)
        echo "📊 Container status:"
        docker compose $COMPOSE_ARGS ps
        ;;

    *)
        # Default: stop and remove
        echo "🛑 Stopping containers (project: $PROJECT_NAME)..."
        docker compose $COMPOSE_ARGS down

        if [ $? -eq 0 ]; then
            echo "✅ Stopped"
            # Clean up saved project name since containers are gone
            [ -f "$PROJECT_FILE" ] && rm -f "$PROJECT_FILE"
        else
            echo "❌ Failed to stop containers"
            exit 1
        fi
        ;;
esac
