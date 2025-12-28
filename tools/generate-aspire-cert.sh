#!/bin/bash
set -e  # Exit immediately if a command exits with a non-zero status

# ==================== CONFIGURATION ====================
PASSWORD="AspireSecure2024!"        # Change to a stronger password if desired
DAYS=3650                           # 10 years validity
KEY_SIZE=4096
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
CERTS_DIR="$PROJECT_ROOT/certs"

# Subject Alternative Names (SAN)
SAN_LIST=(
    "DNS:localhost"
    "DNS:*.localhost"
    "DNS:catalogapi"
    "DNS:catalog-api"
    "DNS:orderapi"
    "DNS:order-api"
    "DNS:basketapi"
    "DNS:basket-api"
    "DNS:identityapi"
    "DNS:identity-api"
    "DNS:paymentapi"
    "DNS:payment-api"
    "DNS:notificationapi"
    "DNS:notification-api"
    "DNS:api-gateway"
    "DNS:gateway"
    "IP:127.0.0.1"
    "IP:::1"
)

# Convert array to comma-separated string for OpenSSL config
SAN_STRING=$(IFS=,; echo "${SAN_LIST[*]}")

# ==================== CHECK FOR OPENSSL ====================
if ! command -v openssl &> /dev/null; then
    echo "❌ OpenSSL not found!"
    echo "   Please install OpenSSL before running this script:"
    echo "     - macOS: brew install openssl"
    echo "     - Ubuntu/Debian: sudo apt update && sudo apt install openssl"
    echo "     - Fedora: sudo dnf install openssl"
    echo "     - Alpine: sudo apk add openssl"
    echo "     - Windows (Git Bash): Install OpenSSL or use WSL"
    echo "     - Windows (PowerShell): choco install openssl"
    exit 1
fi

echo "✅ OpenSSL is available"

# ==================== DETECT OS AND PROVIDE WARNINGS ====================
if [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "win32" ]]; then
    echo "⚠️  Windows detected. Make sure you're running this in Git Bash or WSL for best compatibility."
    echo "   If you encounter issues, consider using WSL or Linux subsystem."
fi

# ==================== CREATE DIRECTORIES ====================
mkdir -p "$CERTS_DIR"

PFX_FILE="$CERTS_DIR/aspnetapp.pfx"

if [ -f "$PFX_FILE" ]; then
    echo "⚠️  Certificate already exists at: $PFX_FILE"
    read -p "Do you want to overwrite it? (y/N): " answer
    if [[ ! "$answer" =~ ^[Yy]$ ]]; then
        echo "Operation cancelled."
        exit 0
    fi
fi

# ==================== CREATE OPENSSL CONFIG ====================
CONFIG_FILE="$CERTS_DIR/openssl-san.cnf"
cat > "$CONFIG_FILE" <<EOF
[req]
distinguished_name = req_dn
req_extensions = v3_req
prompt = no

[req_dn]
CN = AspireMicroservicesDev

[v3_req]
subjectAltName = $SAN_STRING
extendedKeyUsage = serverAuth
keyUsage = digitalSignature,keyEncipherment
EOF

# ==================== GENERATE CERTIFICATE ====================
KEY_FILE="$CERTS_DIR/server.key"
CERT_FILE="$CERTS_DIR/server.crt"

echo "🔑 Generating private key..."
if ! openssl genrsa -out "$KEY_FILE" $KEY_SIZE; then
    echo "❌ Failed to generate private key"
    exit 1
fi

echo "📜 Generating self-signed certificate with SAN..."
if ! openssl req -new -x509 -days $DAYS \
    -key "$KEY_FILE" \
    -out "$CERT_FILE" \
    -config "$CONFIG_FILE" \
    -extensions v3_req; then
    echo "❌ Failed to generate certificate"
    rm -f "$KEY_FILE" "$CONFIG_FILE"
    exit 1
fi

echo "📦 Exporting to PFX format (for Kestrel)..."
if ! openssl pkcs12 -export \
    -out "$PFX_FILE" \
    -inkey "$KEY_FILE" \
    -in "$CERT_FILE" \
    -passout pass:"$PASSWORD"; then
    echo "❌ Failed to export PFX certificate"
    rm -f "$KEY_FILE" "$CERT_FILE" "$CONFIG_FILE"
    exit 1
fi

# ==================== CLEANUP TEMP FILES ====================
rm -f "$KEY_FILE" "$CERT_FILE" "$CONFIG_FILE"

echo "🎉 Certificate generated successfully!"
echo "   Location: $PFX_FILE"
echo "   Password: $PASSWORD"

# ==================== COPY TO API PROJECTS ====================
echo ""
echo "📤 Copying certificate to API projects..."

# List of API project directory names (must match actual directory names)
PROJECT_NAMES=(
    "Catalog"
    "Order"
    "Basket"
    "Identity"
    "Payment"
    "Notification"
)

COPIED_COUNT=0

for proj in "${PROJECT_NAMES[@]}"; do
    # Possible locations for the project
    POSSIBLE_PATHS=(
        "$PROJECT_ROOT/Services/$proj"
        "$PROJECT_ROOT/$proj"
        "$PROJECT_ROOT/Services/$proj/API"
        "$PROJECT_ROOT/$proj/API"
    )

    TARGET_DIR=""
    for path in "${POSSIBLE_PATHS[@]}"; do
        if [ -d "$path" ]; then
            TARGET_DIR="$path/certs"
            break
        fi
    done

    if [ -n "$TARGET_DIR" ]; then
        mkdir -p "$TARGET_DIR"
        cp "$PFX_FILE" "$TARGET_DIR/"
        echo "   ✅ Copied → $TARGET_DIR/aspnetapp.pfx"
        ((COPIED_COUNT++))
    fi
done

echo ""
echo "Done! Certificate copied to $COPIED_COUNT project(s)."
echo ""
echo "🚀 Next step – Add to your AppHost Program.cs (inside PublishAsDockerComposeService):"
echo ""
echo 'service.Environment["ASPNETCORE_URLS"] = "https://+;http://+";'
echo 'service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Path"] = "/app/certs/aspnetapp.pfx";'
echo "service.Environment[\"ASPNETCORE_Kestrel__Certificates__Default__Password\"] = \"$PASSWORD\";"
echo ""
echo "To regenerate: run the script again and choose 'y' when prompted to overwrite."
