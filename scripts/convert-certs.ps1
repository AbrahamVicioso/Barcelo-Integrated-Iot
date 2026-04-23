# Convertir certificados PEM de Let's Encrypt a PFX para Kestrel .NET
param(
    [string]$Domain = "smartstay.es",
    [string]$Password = "smartstay"
)

$ErrorActionPreference = "Stop"

$CurrentDir = Get-Location
$ProjectRoot = $CurrentDir.Path

# Detectar si estamos en docker o en root
if (Test-Path (Join-Path $ProjectRoot "docker")) {
    $ProjectRoot = $CurrentDir.Path
} elseif (Test-Path (Join-Path $ProjectRoot "..\docker")) {
    $ProjectRoot = (Resolve-Path (Join-Path $ProjectRoot "..")).Path
}

$CertsPath = Join-Path $ProjectRoot "docker\certs\live\$Domain\$Domain"
$OutputPath = Join-Path $ProjectRoot "docker\certs\live\$Domain\smartstay.pfx"

$CertFile = Join-Path $CertsPath "cert.pem"
$KeyFile = Join-Path $CertsPath "privkey.pem"
$ChainFile = Join-Path $CertsPath "chain.pem"

Write-Host "=== Convertidor PEM -> PFX ==="
Write-Host "Dominio: $Domain"
Write-Host "Proyecto: $ProjectRoot"
Write-Host "Origen: $CertsPath"

if (-not (Test-Path $CertFile)) {
    Write-Host "ERROR: No encontrado $CertFile"
    exit 1
}
if (-not (Test-Path $KeyFile)) {
    Write-Host "ERROR: No encontrado $KeyFile"
    exit 1
}

$OpenSsl = "C:\Program Files\Git\usr\bin\openssl.exe"
Write-Host "Openssl: $OpenSsl"

& $OpenSsl pkcs12 -export -in $CertFile -inkey $KeyFile -certfile $ChainFile -out $OutputPath -password "pass:$Password" -name "barcelo-cert"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Conversion fallida"
    exit 1
}

if (Test-Path $OutputPath) {
    $size = (Get-Item $OutputPath).Length
    Write-Host ""
    Write-Host "=== OK ==="
    Write-Host "Archivo: $OutputPath"
    Write-Host "Tamano: $size bytes"
} else {
    Write-Host "ERROR: PFX no creado"
    exit 1
}