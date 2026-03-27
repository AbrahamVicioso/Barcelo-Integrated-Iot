# Genera el certificado de desarrollo para HTTPS/gRPC en Docker
# Requiere: OpenSSL (incluido en Git for Windows o WSL)
# Uso: .\docker\generate-dev-cert.ps1

$ErrorActionPreference = "Stop"

$certDir = Join-Path $PSScriptRoot "certs"
New-Item -ItemType Directory -Force -Path $certDir | Out-Null

$certName = "barcelo-dev"
$certPassword = "barcelo-dev"

Write-Host "Generando certificado de desarrollo para Barcelo IoT..." -ForegroundColor Cyan

# Verificar que openssl esta disponible
if (-not (Get-Command openssl -ErrorAction SilentlyContinue)) {
    Write-Error "openssl no encontrado. Instala Git for Windows o usa WSL para ejecutar generate-dev-cert.sh"
    exit 1
}

$keyFile  = Join-Path $certDir "$certName.key"
$crtFile  = Join-Path $certDir "$certName.crt"
$pfxFile  = Join-Path $certDir "$certName.pfx"

# Generar clave + certificado con SANs
openssl req -x509 `
    -newkey rsa:4096 `
    -sha256 `
    -days 3650 `
    -nodes `
    -keyout $keyFile `
    -out    $crtFile `
    -subj   "/CN=barcelo-dev/O=Barcelo IoT Dev" `
    -addext "subjectAltName=DNS:localhost,DNS:auth-api,DNS:usuarios-api,DNS:reservas-api,DNS:dispositivos-api,DNS:api-gateway,IP:127.0.0.1"

# Empaquetar como PFX
openssl pkcs12 -export `
    -out     $pfxFile `
    -inkey   $keyFile `
    -in      $crtFile `
    -passout "pass:$certPassword"

Write-Host ""
Write-Host "Certificado generado en: $certDir" -ForegroundColor Green
Write-Host "  - $certName.pfx  (para Kestrel / servidores gRPC)"
Write-Host "  - $certName.crt  (para clientes)"
Write-Host ""
Write-Host "Password del PFX: $certPassword" -ForegroundColor Yellow
