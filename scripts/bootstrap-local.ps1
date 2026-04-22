# Bootstrap script para desarrollo local
# Verifica genera certificados y prepara el entorno

$ErrorActionPreference = "Stop"

$ENV = "development"
$DOMAIN = "smartstay.es"
$OUTPUT_PATH = "docker/certs"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  BARCELO IOT - BOOTSTRAP LOCAL" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ========================================
# 1. Verificar/Generar certificados
# ========================================
Write-Host "[1/4] Verificando certificados SSL..." -ForegroundColor Cyan

$livePath = "$OUTPUT_PATH/live/$DOMAIN"
$archivePath = "$OUTPUT_PATH/archive/$DOMAIN"
$pfxPath = "$livePath/smartstay.pfx"
$pfxPassword = "smartstay"

# Crear directorios si no existen
if (!(Test-Path $livePath)) {
    New-Item -ItemType Directory -Path $livePath -Force | Out-Null
}
if (!(Test-Path $archivePath)) {
    New-Item -ItemType Directory -Path $archivePath -Force | Out-Null
}

# Verificar si certificado existe y es válido
$needsGenerate = $true
if (Test-Path $pfxPath) {
    try {
        $tempPwd = ConvertTo-SecureString -String $pfxPassword -Force -AsPlainText
        $certCheck = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($pfxPath, $tempPwd)
        if ($certCheck.NotAfter -gt (Get-Date)) {
            $needsGenerate = $false
            Write-Host "  [OK] Certificado válido encontrado: $($certCheck.Subject)" -ForegroundColor Green
            Write-Host "      Válido hasta: $($certCheck.NotAfter)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  [!] Certificado corrupto, generando nuevo..." -ForegroundColor Yellow
        Remove-Item $pfxPath -Force -ErrorAction SilentlyContinue
    }
}

if ($needsGenerate) {
    Write-Host "  [+] Generando certificado autofirmado..." -ForegroundColor Cyan
    try {
        $cert = New-SelfSignedCertificate -DnsName $DOMAIN,"localhost","127.0.0.1" -CertStoreLocation Cert:\CurrentUser\My -NotAfter (Get-Date).AddDays(365) -KeyExportPolicy Exportable -KeySpec Signature
        $pwd = ConvertTo-SecureString -String $pfxPassword -Force -AsPlainText
        Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $pwd | Out-Null
        Write-Host "  [OK] Certificado generado: $pfxPath" -ForegroundColor Green
        Remove-Item $cert.PSPath -Force
    } catch {
        Write-Host "  [ERROR] No se pudo generar certificado: $_" -ForegroundColor Red
        exit 1
    }
}

# Copiar a archive
Copy-Item $pfxPath "$archivePath/smartstay.pfx" -Force

# ========================================
# 2. Verificar servicios de infraestructura
# ========================================
Write-Host ""
Write-Host "[2/4] Verificando servicios..." -ForegroundColor Cyan

$services = @("sqlserver", "kafka", "ntfy")  # postgres y thingsboard pueden tardar más

foreach ($svc in $services) {
    $running = docker ps --filter "name=barcelo-$svc" --format "{{.Names}}" 2>$null
    if ($running) {
        Write-Host "  [OK] $svc corriendo" -ForegroundColor Green
    } else {
        Write-Host "  [--] $svc no iniciado" -ForegroundColor Yellow
    }
}

# ========================================
# 3. Verificar archivo .env
# ========================================
Write-Host ""
Write-Host "[3/4] Verificando configuración..." -ForegroundColor Cyan

$envFile = ".env"
$envExample = ".env.example"

if (!(Test-Path $envFile)) {
    if (Test-Path $envExample) {
        Write-Host "  [+] Copiando .env.example a .env..." -ForegroundColor Cyan
        Copy-Item $envExample $envFile
    } else {
        Write-Host "  [!] No existe .env.example" -ForegroundColor Yellow
    }
} else {
    Write-Host "  [OK] .env encontrado" -ForegroundColor Green
}

# ========================================
# 4. Verificar Docker
# ========================================
Write-Host ""
Write-Host "[4/4] Verificando Docker..." -ForegroundColor Cyan

$dockerRunning = docker info 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  [OK] Docker ejecutándose" -ForegroundColor Green
} else {
    Write-Host "  [ERROR] Docker no está corriendo" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  BOOTSTRAP COMPLETADO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Para iniciar los servicios:" -ForegroundColor Cyan
Write-Host "  docker compose up -d sqlserver kafka ntfy" -ForegroundColor White
Write-Host ""
Write-Host "Para iniciar las APIs:" -ForegroundColor Cyan
Write-Host "  docker compose -f docker-compose.yml -f docker-compose.dev.yml up" -ForegroundColor White
Write-Host ""
Write-Host "Para producción:" -ForegroundColor Cyan
Write-Host "  docker compose up -d --build" -ForegroundColor White