param(
    [string]$Domain = "smartstay.es",
    [string]$OutputPath = "$PSScriptRoot/../docker/certs"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Generando certificados de DESARROLLO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$livePath = "$OutputPath/live/$Domain"
$archivePath = "$OutputPath/archive/$Domain"

if (!(Test-Path $livePath)) {
    New-Item -ItemType Directory -Path $livePath -Force | Out-Null
}
if (!(Test-Path $archivePath)) {
    New-Item -ItemType Directory -Path $archivePath -Force -ErrorAction SilentlyContinue
}

$pfxPath = "$livePath/smartstay.pfx"
$pfxPassword = "smartstay"

# Generar certificado autofirmado
Write-Host "[+] Generando certificado para: $Domain" -ForegroundColor Cyan

$cert = New-SelfSignedCertificate -DnsName $Domain,"*.$Domain","localhost","127.0.0.1" -CertStoreLocation Cert:\CurrentUser\My -NotAfter (Get-Date).AddDays(365) -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 2048

# Exportar PFX
$pwd = ConvertTo-SecureString -String $pfxPassword -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $pwd | Out-Null
Write-Host "[+] PFX: smartstay.pfx" -ForegroundColor Green

# Exportar certificado a PEM
$certBytes = $cert.RawData
$certBase64 = [Convert]::ToBase64String($certBytes, [Base64FormattingOptions]::InsertLineBreaks)
$certPem = "-----BEGIN CERTIFICATE-----`n$certBase64`n-----END CERTIFICATE-----"
[System.IO.File]::WriteAllText("$livePath/cert.pem", $certPem)
Write-Host "[+] cert.pem" -ForegroundColor Green

# Exportar clave privada del certificado generado
$rsa = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($cert)
# ExportPkcs8PrivateKey() requiere .NET 5+; usar CngKey.Export para compatibilidad con Windows PowerShell 5.1
if ($rsa -is [System.Security.Cryptography.RSACng]) {
    $keyBytes = $rsa.Key.Export([System.Security.Cryptography.CngKeyBlobFormat]::Pkcs8PrivateBlob)
} else {
    $keyBytes = $rsa.ExportPkcs8PrivateKey()
}
$keyBase64 = [Convert]::ToBase64String($keyBytes, [Base64FormattingOptions]::InsertLineBreaks)
$keyPem = "-----BEGIN PRIVATE KEY-----`n$keyBase64`n-----END PRIVATE KEY-----"
[System.IO.File]::WriteAllText("$livePath/privkey.pem", $keyPem)
Write-Host "[+] privkey.pem" -ForegroundColor Green

# Limpiar
Remove-Item $cert.PSPath -Force -ErrorAction SilentlyContinue

# Copiar a archive
Copy-Item $pfxPath "$archivePath/smartstay.pfx" -Force

Write-Host ""
Write-Host "[OK] Certificados en: $livePath" -ForegroundColor Green