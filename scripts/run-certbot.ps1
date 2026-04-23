# Certbot + Conversión de certificados completos
# Usage: .\scripts\run-certbot.ps1 [-Domain smartstay.es] [-SkipDockerDown]
#
# Este script:
# 1. Detiene los servicios que usan el puerto 80/443 (si existen)
# 2. Ejecuta certbot para obtener certificados
# 3. Convierte PEM -> PFX
# 4. Reinicia los servicios

param(
    [string]$Domain = "smartstay.es",
    [switch]$SkipDockerDown,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$CertsPath = "docker\certs\live\$Domain"
$ArchivePath = "docker\certs\archive\$Domain"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Certbot + Convertidor de Certificados" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Dominio: $Domain"
Write-Host ""

# Verificar red de Docker
$networkExists = docker network ls --format "{{.Name}}" | Where-Object { $_ -eq "barcelo-iot" }
if (-not $networkExists) {
    Write-Host "Creando red barcelo-iot..." -ForegroundColor Yellow
    docker network create barcelo-iot
}

# Crear directorios si no existen
@($CertsPath, $ArchivePath) | ForEach-Object {
    $dir = Join-Path $PSScriptRoot "..\$_"
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "Creado: $dir" -ForegroundColor Green
    }
}

if (-not $SkipDockerDown) {
    Write-Host ""
    Write-Host "Deteniendo servicios en puerto 80/443..." -ForegroundColor Yellow

    # Detener certbot si está corriendo
    $certbotRunning = docker ps -a --format "{{.Names}}" | Where-Object { $_ -match "certbot" }
    if ($certbotRunning) {
        docker stop smartstay-certbot 2>$null
        docker rm smartstay-certbot 2>$null
    }

    # Detener nginx si existe
    $nginxRunning = docker ps -a --format "{{.Names}}" | Where-Object { $_ -match "nginx" }
    if ($nginxRunning) {
        Write-Host "Deteniendo nginx..." -ForegroundColor Yellow
        docker stop barcelo-nginx 2>$null
        docker rm barcelo-nginx 2>$null
    }

    Start-Sleep -Seconds 2
}

if (-not $DryRun) {
    Write-Host ""
    Write-Host "=== Ejecutando Certbot ===" -ForegroundColor Cyan

    # Ejecutar certbot
    docker run --rm -it `
        --network barcelo-iot `
        -p 80:80 `
        -p 443:443 `
        -v "$((Resolve-Path $CertsPath).Path):/etc/letsencrypt/live:rw" `
        -v "$((Resolve-Path $ArchivePath).Path):/etc/letsencrypt/archive:rw" `
        certbot/certbot:latest certonly --standalone `
        --email admin@$Domain `
        --agree-tos --non-interactive `
        -d $Domain `
        --keep-until-expiring

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al obtener certificados de Let's Encrypt"
        exit 1
    }
} else {
    Write-Host "[DRY RUN] Se omitió certbot" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Convirtiendo PEM -> PFX ===" -ForegroundColor Cyan

# Convertir a PFX
$SourcePath = Join-Path $PSScriptRoot "..\$CertsPath"
$CertFile = Join-Path $SourcePath "cert.pem"
$KeyFile = Join-Path $SourcePath "privkey.pem"
$ChainFile = Join-Path $SourcePath "chain.pem"
$OutputFile = Join-Path $SourcePath "smartstay.pfx"
$Password = "smartstay"

# Buscar openssl
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

$converted = $false

if ($OpenSsl) {
    Write-Host "Usando openssl local: $OpenSsl" -ForegroundColor Green
    & $OpenSsl pkcs12 -export -in $CertFile -inkey $KeyFile -certfile $ChainFile -out $OutputFile -password "pass:$Password" -name "barcelo-cert"
    if ($LASTEXITCODE -eq 0) { $converted = $true }
}

if (-not $converted) {
    Write-Host "Usando contenedor openssl..." -ForegroundColor Yellow
    docker run --rm -it `
        --network barcelo-iot `
        -v "$((Resolve-Path $SourcePath).Path):/certs:rw" `
        alpine/openssl:latest pkcs12 `
        -export `
        -in /certs/cert.pem `
        -inkey /certs/privkey.pem `
        -certfile /certs/chain.pem `
        -out /certs/smartstay.pfx `
        -password "pass:$Password" `
        -name "barcelo-cert"

    if ($LASTEXITCODE -eq 0) { $converted = $true }
}

if (-not $converted) {
    Write-Error "No se pudo convertir los certificados"
    exit 1
}

# Verificar resultado
if (Test-Path $OutputFile) {
    $fileInfo = Get-Item $OutputFile
    $certs = Get-ChildItem $SourcePath -Filter "*.pem"

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✓ Certificados instalados" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Archivos generados:" -ForegroundColor Cyan
    $certs | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }
    Write-Host "  - smartstay.pfx" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Ruta: $SourcePath" -ForegroundColor White
    Write-Host "Tamaño PFX: $($fileInfo.Length) bytes" -ForegroundColor White
    Write-Host ""
    Write-Host "Para reiniciar servicios:" -ForegroundColor Cyan
    Write-Host "  docker compose up -d" -ForegroundColor Gray
} else {
    Write-Error "Error al crear smartstay.pfx"
    exit 1
}