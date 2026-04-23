param(
    [switch]$Remove,
    [switch]$Show
)

$hostsPath = "$env:SystemRoot\System32\drivers\etc\hosts"
$domain = "smartstay.int"
$ip = "127.0.0.1"

$entries = @"
$ip smartstay.int
$ip www.smartstay.int
$ip api.smartstay.int
$ip ntfy.smartstay.int
$ip thingsboard.smartstay.int
$ip gateway.smartstay.int
$ip auth.smartstay.int
$ip usuarios.smartstay.int
$ip reservas.smartstay.int
$ip dispositivos.smartstay.int
$ip services.smartstay.int
$ip admin.smartstay.int
"@

if ($Show) {
    Write-Host "=== Entradas DNS para $domain ===" -ForegroundColor Cyan
    Get-Content $hostsPath | Where-Object { $_ -match $domain }
    exit 0
}

if ($Remove) {
    Write-Host "=== Removiendo entradas DNS para $domain ===" -ForegroundColor Yellow
    $content = Get-Content $hostsPath -Raw
    $newContent = ($content -split "`n" | Where-Object { $_ -notmatch $domain }) -join "`n"
    Set-Content -Path $hostsPath -Value $newContent -NoNewline
    Write-Host "Removido" -ForegroundColor Green
    exit 0
}

# Agregar
Write-Host "=== Agregando entradas DNS para $domain ===" -ForegroundColor Cyan

# Verificar si ya existe
if (Select-String -Path $hostsPath -Pattern "^$ip\s+$domain`b" -Quiet) {
    Write-Host "Ya existe: $domain -> $ip" -ForegroundColor Yellow
    exit 0
}

# Intentar con retry
$added = $false
for ($i = 1; $i -le 3; $i++) {
    try {
        $content = Get-Content $hostsPath -Raw -ErrorAction Stop
        $newContent = $content + "`n`n# Barcelo IoT - $domain`n" + $entries
        [System.IO.File]::WriteAllText($hostsPath, $newContent)
        $added = $true
        break
    } catch {
        Write-Host "Intento $i/3 - esperando..." -ForegroundColor Yellow
        Start-Sleep -Seconds 2
    }
}

if ($added) {
    Write-Host "Agregado al hosts" -ForegroundColor Green
} else {
    Write-Host "Error: archivo bloqueado. Cerrá cualquier editor que tenga el hosts abierto." -ForegroundColor Red
}

Write-Host ""
Write-Host "Para probar:" -ForegroundColor Cyan
Write-Host "  ping api.smartstay.int" -ForegroundColor White
Write-Host "  curl http://api.smartstay.int:5019" -ForegroundColor White