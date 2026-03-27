#!/bin/bash
# Genera el certificado de desarrollo para HTTPS/gRPC en Docker
# Requiere: openssl (LibreSSL nativo de macOS o Homebrew openssl)
# Uso: bash docker/generate-dev-cert-macos.sh

set -e

CERT_DIR="$(dirname "$0")/certs"
mkdir -p "$CERT_DIR"

CERT_NAME="barcelo-dev"
CERT_PASSWORD="barcelo-dev"

echo "Generando certificado de desarrollo para Barcelo IoT (macOS)..."

# macOS incluye LibreSSL que no soporta -addext.
# Se usa un archivo de configuracion temporal con las SANs definidas.
OPENSSL_CNF=$(mktemp /tmp/barcelo-openssl.XXXXXX.cnf)
trap 'rm -f "$OPENSSL_CNF"' EXIT

cat > "$OPENSSL_CNF" <<EOF
[req]
default_bits       = 4096
prompt             = no
default_md         = sha256
distinguished_name = dn
x509_extensions    = v3_req

[dn]
CN = barcelo-dev
O  = Barcelo IoT Dev

[v3_req]
subjectAltName = @alt_names

[alt_names]
DNS.1 = localhost
DNS.2 = auth-api
DNS.3 = usuarios-api
DNS.4 = reservas-api
DNS.5 = dispositivos-api
DNS.6 = api-gateway
IP.1  = 127.0.0.1
EOF

# Generar clave privada + certificado autofirmado con SANs
openssl req -x509 \
  -newkey rsa:4096 \
  -sha256 \
  -days 3650 \
  -nodes \
  -keyout "$CERT_DIR/$CERT_NAME.key" \
  -out    "$CERT_DIR/$CERT_NAME.crt" \
  -config "$OPENSSL_CNF"

# Empaquetar como PFX para Kestrel
openssl pkcs12 -export \
  -out     "$CERT_DIR/$CERT_NAME.pfx" \
  -inkey   "$CERT_DIR/$CERT_NAME.key" \
  -in      "$CERT_DIR/$CERT_NAME.crt" \
  -passout "pass:$CERT_PASSWORD"

echo ""
echo "Certificado generado en: $CERT_DIR/"
echo "  - $CERT_NAME.pfx  (para Kestrel / servidores gRPC)"
echo "  - $CERT_NAME.crt  (para clientes que necesiten confiar en el cert)"
echo ""
echo "Password del PFX: $CERT_PASSWORD"
