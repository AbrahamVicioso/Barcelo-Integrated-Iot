# Convertir certificados PEM de Let's Encrypt a PFX para Kestrel .NET
# Uso: .\scripts\convert-certs.ps1 [-Domain smartstay.es]

param(
    [string]$Domain = "smartstay.es",
    [string]$OutputFile = "smartstay.pfx",
    [string]$Password = "smartstay"
)

$CertsPath = "docker\certs\live\$Domain"

$ErrorActionPreference = "Stop"

# Rutas
$SourcePath = Join-Path $PSScriptRoot "..\$CertsPath"
$OutputPath = Join-Path $PSScriptRoot "..\$CertsPath\$OutputFile"

# Verificar que existen los certificados PEM
$CertFile = Join-Path $SourcePath "cert.pem"
$KeyFile = Join-Path $SourcePath "privkey.pem"
$ChainFile = Join-Path $SourcePath "chain.pem"

Write-Host "=== Convertidor Certificados PEM -> PFX ===" -ForegroundColor Cyan
Write-Host "Dominio: $Domain"
Write-Host "Ruta origen: $SourcePath"
Write-Host ""

if (-not (Test-Path $CertFile)) {
    Write-Error "No encontrado: $CertFile"
    exit 1
}
if (-not (Test-Path $KeyFile)) {
    Write-Error "No encontrado: $KeyFile"
    exit 1
}

# Buscar openssl
$OpenSsl = $null
$OpenSslPaths = @(
    "C:\Program Files\Git\usr\bin\openssl.exe",
    "C:\Program Files (x86)\Git\usr\bin\openssl.exe",
    "C:\Program Files\OpenSSL-Win64\bin\openssl.exe",
    "C:\OpenSSL-Win64\bin\openssl.exe"
)

foreach ($path in $OpenSslPaths) {
    if (Test-Path $path) {
        $OpenSsl = $path
        break
    }
}

# Si no se encuentra, usar contenedor Docker con openssl
if (-not $OpenSsl) {
    Write-Host "Openssl no encontrado en sistema, usando contenedor Docker..." -ForegroundColor Yellow

    # Crear contenedor temporal con openssl
    $containerName = "barcelo-cert-converter"

    # Verificar si la red existe
    $networkExists = docker network ls --format "{{.Name}}" | Where-Object { $_ -eq "barcelo-iot" }
    if (-not $networkExists) {
        docker network create barcelo-iot 2>$null
    }

    # Ejecutar conversión en contenedor
    docker run --rm -it `
        --name $containerName `
        --network barcelo-iot `
        -v "$((Resolve-Path $SourcePath).Path):/certs:ro" `
        alpine/openssl:latest pkcs12 `
        -export `
        -in /certs/cert.pem `
        -inkey /certs/privkey.pem `
        -certfile /certs/chain.pem `
        -out /certs/$OutputFile `
        -password "pass:$Password" `
        -name "barcelo-cert"

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al convertir certificados con Docker"
        exit 1
    }
} else {
    Write-Host "Usando: $OpenSsl" -ForegroundColor Green

    # Convertir a PFX usando openssl
    & $OpenSsl pkcs12 `
        -export `
        -in $CertFile `
        -inkey $KeyFile `
        -certfile $ChainFile `
        -out $OutputPath `
        -password "pass:$Password" `
        -name "barcelo-cert"

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al convertir certificados"
        exit 1
    }
}

# Verificar que se creó el archivo
if (Test-Path $OutputPath) {
    $fileInfo = Get-Item $OutputPath
    Write-Host ""
    Write-Host "=== ✓ Certificado convertido exitosamente ===" -ForegroundColor Green
    Write-Host "Archivo: $OutputPath"
    Write-Host "Tamaño: $($fileInfo.Length) bytes"
    Write-Host ""
    Write-Host "Configuración para appsettings:" -ForegroundColor Cyan
    Write-Host '  "Certificate": {' -ForegroundColor Gray
    Write-Host '    "Path": "/https/smartstay.pfx",' -ForegroundColor Gray
    Write-Host '    "Password": "smartstay"' -ForegroundColor Gray
    Write-Host '  }' -ForegroundColor Gray
} else {
    Write-Error "No se pudo crear el archivo PFX"
    exit 1
}