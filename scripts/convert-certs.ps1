# Convertir certificados PEM de Let's Encrypt a PFX para Kestrel .NET
param(
    [string]$Domain = "smartstay.es",
    [string]$OutputFile = "smartstay.pfx",
    [string]$Password = "smartstay"
)

$CertsPath = "docker\certs\live\$Domain"
$ErrorActionPreference = "Stop"

$SourcePath = Join-Path $PSScriptRoot "..\$CertsPath"
$OutputPath = Join-Path $PSScriptRoot "..\$CertsPath\$OutputFile"

$CertFile = Join-Path $SourcePath "cert.pem"
$KeyFile = Join-Path $SourcePath "privkey.pem"
$ChainFile = Join-Path $SourcePath "chain.pem"

Write-Host "=== Convertidor Certificados PEM -> PFX ==="
Write-Host "Dominio: $Domain"
Write-Host "Ruta origen: $SourcePath"

if (-not (Test-Path $CertFile)) {
    Write-Host "ERROR: No encontrado: $CertFile"
    exit 1
}
if (-not (Test-Path $KeyFile)) {
    Write-Host "ERROR: No encontrado: $KeyFile"
    exit 1
}

# Buscar openssl en rutas comunes
$OpenSsl = $null
$OpenSslPaths = @(
    "C:\Program Files\Git\usr\bin\openssl.exe",
    "C:\Program Files (x86)\Git\usr\bin\openssl.exe",
    "C:\Program Files\OpenSSL-Win64\bin\openssl.exe"
)

foreach ($path in $OpenSslPaths) {
    if (Test-Path $path) {
        $OpenSsl = $path
        break
    }
}

if (-not $OpenSsl) {
    Write-Host "Openssl no encontrado, usando contenedor Docker..."

    # Verificar si la red existe
    $networkExists = docker network ls --format "{{.Name}}" | Where-Object { $_ -eq "barcelo-iot" }
    if (-not $networkExists) {
        docker network create barcelo-iot 2>$null
    }

    $absolutePath = (Resolve-Path $SourcePath).Path

    docker run --rm -it `
        --network barcelo-iot `
        -v "$absolutePath`:/certs:ro" `
        alpine/openssl:latest pkcs12 `
        -export `
        -in /certs/cert.pem `
        -inkey /certs/privkey.pem `
        -certfile /certs/chain.pem `
        -out /certs/smartstay.pfx `
        -password "pass:$Password" `
        -name "barcelo-cert"

    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Error al convertir certificados con Docker"
        exit 1
    }
} else {
    Write-Host "Usando openssl: $OpenSsl"

    & $OpenSsl pkcs12 -export -in $CertFile -inkey $KeyFile -certfile $ChainFile -out $OutputPath -password "pass:$Password" -name "barcelo-cert"

    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Error al convertir certificados"
        exit 1
    }
}

if (Test-Path $OutputPath) {
    $fileInfo = Get-Item $OutputPath
    Write-Host ""
    Write-Host "=== OK: Certificado convertido exitosamente ==="
    Write-Host "Archivo: $OutputPath"
    Write-Host "Tamano: $($fileInfo.Length) bytes"
} else {
    Write-Host "ERROR: No se pudo crear el archivo PFX"
    exit 1
}