# Convertir certificados PEM de Let's Encrypt a PFX para Kestrel .NET
param(
    [string]$Domain = "smartstay.es",
    [string]$Password = "smartstay"
)

$ErrorActionPreference = "Stop"

# Buscar openssl en PATH del sistema
$OpenSsl = (Get-Command openssl -ErrorAction SilentlyContinue).Source
if (-not $OpenSsl) {
    # Buscar en ubicaciones comunes
    $Paths = @(
        "C:\Program Files\Git\usr\bin\openssl.exe",
        "C:\Program Files (x86)\Git\usr\bin\openssl.exe",
        "C:\OpenSSL-Win64\bin\openssl.exe",
        "C:\Program Files\OpenSSL-Win64\bin\openssl.exe"
    )
    foreach ($p in $Paths) {
        if (Test-Path $p) {
            $OpenSsl = $p
            break
        }
    }
}

if (-not $OpenSsl) {
    Write-Host "ERROR: openssl no encontrado en sistema"
    Write-Host "Instala Git o OpenSSL para Windows"
    exit 1
}

# Buscar certificados
$ScriptDir = $PSScriptRoot
$CertPath = $null

# Buscar en varias ubicaciones
$PossiblePaths = @(
    "$ScriptDir\..\docker\certs\live\$Domain\$Domain",
    "$ScriptDir\docker\certs\live\$Domain\$Domain",
    "$ScriptDir\..\..\docker\certs\live\$Domain\$Domain"
)

foreach ($p in $PossiblePaths) {
    if (Test-Path (Join-Path $p "cert.pem")) {
        $CertPath = $p
        break
    }
}

if (-not $CertPath) {
    Write-Host "ERROR: No se encontraron certificados en:"
    $PossiblePaths | ForEach-Object { Write-Host "  - $_" }
    exit 1
}

$CertFile = Join-Path $CertPath "cert.pem"
$KeyFile = Join-Path $CertPath "privkey.pem"
$ChainFile = Join-Path $CertPath "chain.pem"
$OutputPath = Join-Path (Split-Path $CertPath -Parent) "smartstay.pfx"

Write-Host "=== Convertidor PEM -> PFX ==="
Write-Host "Openssl: $OpenSsl"
Write-Host "Origen: $CertPath"

# Ejecutar conversion
& $OpenSsl pkcs12 -export -in $CertFile -inkey $KeyFile -certfile $ChainFile -out $OutputPath -password "pass:$Password" -name "barcelo-cert"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Conversion fallida"
    exit 1
}

$size = (Get-Item $OutputPath).Length
Write-Host ""
Write-Host "=== OK ==="
Write-Host "Archivo: $OutputPath"
Write-Host "Tamano: $size bytes"