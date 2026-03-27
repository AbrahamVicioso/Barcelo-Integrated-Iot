#!/bin/bash
# Genera el certificado de desarrollo para HTTPS/gRPC en Docker
# Requiere: openssl
# Uso: bash docker/generate-dev-cert.sh

set -e

CERT_DIR="$(dirname "$0")/certs"
mkdir -p "$CERT_DIR"

CERT_NAME="barcelo-dev"
CERT_PASSWORD="barcelo-dev"

echo "Generando certificado de desarrollo para Barcelo IoT..."

# Generar clave privada + certificado autofirmado con SANs para todos los servicios Docker
openssl req -x509 \
  -newkey rsa:4096 \
  -sha256 \
  -days 3650 \
  -nodes \
  -keyout "$CERT_DIR/$CERT_NAME.key" \
  -out    "$CERT_DIR/$CERT_NAME.crt" \
  -subj   "/CN=barcelo-dev/O=Barcelo IoT Dev" \
  -addext "subjectAltName=DNS:localhost,DNS:auth-api,DNS:usuarios-api,DNS:reservas-api,DNS:dispositivos-api,DNS:api-gateway,IP:127.0.0.1"

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
