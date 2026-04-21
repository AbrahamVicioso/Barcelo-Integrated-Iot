# Barcelo Integrated IoT

Plataforma IoT para gestión de habitaciones inteligentes del Hotel Barcelo. Arquitectura de microservicios con .NET 9, Kafka, gRPC y ThingsBoard.

---

## Servicios

| Servicio | Descripción | Puerto REST | Puerto gRPC |
|---|---|---|---|
| `nginx` | Reverse proxy | 80 | — |
| `api-gateway` | Ocelot API Gateway | 5019 (HTTP) · 5020 (HTTPS) | — |
| `auth-api` | Autenticación y JWT | 5117 | 5118 (HTTPS) |
| `usuarios-api` | Huéspedes y personal | 5284 | 5285 (HTTPS) |
| `reservas-api` | Reservas y check-in/out | 5141 | — |
| `dispositivos-api` | Cerraduras inteligentes | 5185 | — |
| `notification-worker` | Notificaciones por email y push | — | — |
| `audit-worker` | Auditoría de eventos | — | — |

### Infraestructura

| Servicio | Puerto externo | Uso |
|---|---|---|
| SQL Server | 1433 | Base de datos de los microservicios .NET |
| Kafka | 9092 | Mensajería entre servicios (KRaft, sin Zookeeper) |
| PostgreSQL | 5432 | Base de datos de ThingsBoard |
| ThingsBoard CE | 8080 | Gestión de dispositivos IoT |
| ntfy | 8081 | Servidor de notificaciones push |

---

## Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) con WSL2
- [OpenSSL](https://slproweb.com/products/Win32OpenSSL.html) (incluido en Git for Windows)
- .NET 9 SDK instalado en el host (para tener el caché de NuGet local)

---

## Levantar el entorno

### 1. Configurar el caché de NuGet (solo la primera vez)

Copia `.env.example` como `.env` y ajusta la ruta a tu usuario:

```bash
cp .env.example .env
```

El `.env` apunta al caché de NuGet de tu máquina. Así los contenedores **nunca necesitan descargar paquetes de internet**.

Si es la primera vez que clonas el proyecto, asegúrate de haber restaurado los paquetes al menos una vez localmente:

```bash
dotnet restore
```

### 2. Generar certificado de desarrollo (solo la primera vez)

```bash
# Linux / WSL
bash docker/generate-dev-cert.sh

# macOS (usa LibreSSL nativo, no requiere Homebrew)
bash docker/generate-dev-cert-macos.sh

# Windows PowerShell
.\docker\generate-dev-cert.ps1
```

Genera `docker/certs/barcelo-dev.pfx` usado por los servicios gRPC para TLS.

> **macOS:** el script usa un archivo de configuración temporal para definir los SANs, ya que LibreSSL (OpenSSL nativo de macOS) no soporta la flag `-addext`.
> Si prefieres usar la versión de Homebrew (`brew install openssl`), puedes ejecutar directamente `bash docker/generate-dev-cert.sh`.

### 2b. Certificado con Let's Encrypt (producción)

Si tienes un dominio público (`smartstay.es`), puedes usar Let's Encrypt para certificados gratuitos y automáticos.

**Requisitos previos:**
1. **DNS configurado** en tu proveedor de dominio (Hostinger, GoDaddy, etc.):
   - Registro A: `@` → tu IP pública
2. **Puerto 80 abierto** en el firewall de Azure
3. **nginx corriendo** (docker-compose.override.yml)

**Configurar DNS en tu proveedor de dominio:**

| Campo | Valor |
|-------|-------|
| Type | A |
| Host | `@` o `smartstay.es` |
| Value | Tu IP pública de Azure |
| TTL | 3600 |

**Verificar DNS:**
```bash
nslookup smartstay.es
```

**Generar certificado inicial:**

```bash
# Asegúrate que nginx esté corriendo
docker compose up -d nginx

# Generar certificado
docker compose run --rm -p 80:80 certbot certonly --webroot \
  -w /var/www/certbot \
  --email admin@smartstay.es \
  --agree-tos --no-eff-email \
  -d smartstay.es
```

Esto genera los certificados en `docker/certs/live/smartstay.es/`:
- `smartstay.pfx` - Para Kestrel (.NET)
- `fullchain.pem` - Certificado + CA
- `privkey.pem` - Clave privada

**Convertir a PFX (si no se creó automáticamente):**

```bash
# Crear directorio si no existe
mkdir -p docker/certs/live/smartstay.es

# Convertir
openssl pkcs12 -export \
  -out docker/certs/live/smartstay.es/smartstay.pfx \
  -inkey docker/certs/live/smartstay.es/privkey.pem \
  -in docker/certs/live/smartstay.es/fullchain.pem \
  -passout pass:smartstay
```

**Renovación automática:**

```bash
# Agregar al crontab (ej: cada mes el día 1 a las 3am):
0 3 1 * * /ruta/al/proyecto/docker/certs/renew.sh
```

El script de renovación convierte el certificado a PFX automáticamente y reinicia los servicios.

> **Nota:** El certificado Let's Encrypt dura 90 días. El script lo renueva cuando faltan 30 días.

---

### 3. Inicializar ThingsBoard (solo la primera vez)

```bash
docker compose run --rm -e INSTALL_TB=true -e LOAD_DEMO=true thingsboard-ce
```

Espera a que termine (1-2 minutos) y luego continúa.

### 4. Levantar todos los servicios

```bash
docker compose up
```

> **Notificaciones push:** ntfy arranca automáticamente en modo abierto (`read-write`). Para producción consulta la sección [Notificaciones Push](#notificaciones-push-ntfy).

> No hay `docker compose build` — se usa directamente la imagen `mcr.microsoft.com/dotnet/sdk:9.0`.
> SQL Server necesita ~60 segundos para arrancar; los servicios esperan automáticamente.

### Levantar en background

```bash
docker compose up -d
```

> **Nota:** el comando de inicialización de ThingsBoard solo se corre una vez. Los datos quedan persistidos en el volumen `tb-postgres-data`.

---

## Dominio y URLs públicas

El sistema está configurado para usar el dominio `smartstay.es`.

### Configuración DNS

| Host | Tipo | Valor |
|------|------|-------|
| @ | A | Tu IP pública de Azure |

### URLs públicas (después de configurar DNS y certificado)

| Servicio | URL |
|---|---|
| API Gateway | `https://smartstay.es` |
| ThingsBoard | `https://smartstay.es/thingsboard` |
| ntfy (push) | `https://smartstay.es/ntfy` |

### Variables de entorno

```env
GATEWAY_PUBLIC_BASE_URL=https://smartstay.es
NTFY_PUBLIC_BASE_URL=https://smartstay.es:8081
```

---

## Nginx como Reverse Proxy

El nginx está configurado en `docker-compose.override.yml` y usa la config en `docker/nginx/nginx-generated.conf`.

### Configuración actual

- **Puerto 80**: HTTP (redirige a HTTPS o sirve ACME challenge)
- **Rutas proxadas**:
  - `thingsboard.smartstay.es` → ThingsBoard CE
  - `api.smartstay.es` → API Gateway
  - `ntfy.smartstay.es` → ntfy

### ACME Challenge para Let's Encrypt

El nginx está configurado para redirigir las peticiones de verificación de Let's Encrypt al contenedor certbot:

```nginx
location ^~ /.well-known/acme-challenge/ {
    proxy_pass http://certbot_backend;
}
```

---

## Comandos del día a día

```bash
# Ver logs en tiempo real de todos los servicios
docker compose logs -f

# Ver logs de un servicio específico
docker compose logs -f reservas-api

# Estado de todos los contenedores
docker compose ps

# Reiniciar un servicio (útil si cambias appsettings)
docker compose restart auth-api

# Parar todo
docker compose down

# Parar y borrar volúmenes (reset completo de DB y Kafka)
docker compose down -v

# Entrar a un contenedor
docker compose exec auth-api bash
```

---

## Hot Reload

Los servicios usan `dotnet watch run` con el código fuente montado como volumen. Cualquier cambio en `.cs`, `.json` o `.proto` reinicia automáticamente el servicio afectado sin reconstruir la imagen.

El caché de NuGet persiste en un volumen Docker (`nuget-cache`), por lo que solo se descargan paquetes nuevos.

---

## API Gateway

Todas las rutas externas pasan por el gateway en `http://localhost:5019`:

| Ruta | Servicio |
|---|---|
| `/api/auth/**` | auth-api |
| `/api/user/**` | usuarios-api |
| `/api/reserva/**` | reservas-api |
| `/api/device/**` | dispositivos-api |

---

## Documentación interactiva (Scalar)

Disponible en cada servicio mientras el entorno esté levantado:

| Servicio | URL |
|---|---|
| auth-api | http://localhost:5117/scalar |
| usuarios-api | http://localhost:5284/scalar |
| reservas-api | http://localhost:5141/scalar |
| dispositivos-api | http://localhost:5185/scalar |

---

## Topics de Kafka

| Topic | Productor | Consumidor |
|---|---|---|
| `users` | auth-api | notification-worker |
| `reservas` | reservas-api | notification-worker |
| `dispositivos.unlock-door` | reservas-api | dispositivos-api |
| `audit.events` | Todos los APIs | audit-worker |

---

## Arquitectura gRPC

La comunicación interna entre servicios usa gRPC sobre HTTPS con certificado de desarrollo:

```
reservas-api  ──gRPC HTTPS──▶  usuarios-api:5285  (HuespedeGrpcService)
usuarios-api  ──gRPC HTTPS──▶  auth-api:5118      (UserLookupService)
```

En desarrollo local (sin Docker) se usa HTTP/2 cleartext (h2c) automáticamente.

---

## Base de datos

SQL Server corre en `localhost:1433`. Las migraciones y seed data se aplican automáticamente al arrancar cada servicio.

**Cadena de conexión local:**
```
Data source=localhost;Database=BarceloIoTDatabase;User Id=barcelo;Password=Testing1234;TrustServerCertificate=True
```

**En Docker** los servicios se conectan via `sqlserver:1433` con el usuario `sa`.

---

## Notificaciones Push (ntfy)

El sistema usa [ntfy](https://ntfy.sh) como servidor de push notifications open-source y self-hosted.
Dentro de Docker es accesible como `http://ntfy:80` (DNS interno). Desde el host como `http://localhost:8081`.

### Cómo funcionan

Cuando se crea una reserva o un usuario, el `notification-worker` publica automáticamente:
- Un **email** (Azure Communication Services)
- Una **notificación push** al topic personal del usuario en ntfy

Cada usuario tiene un topic derivado de su email:
```
usuario@hotel.com  →  barcelo-usuario-at-hotel-com
```

### Configuración para desarrollo (ya funciona sin nada extra)

ntfy arranca en modo `read-write` — abierto para que el worker pueda publicar sin token.
La app móvil puede suscribirse directamente sin credenciales.

**Suscribirse desde la app ntfy:**
1. Descarga ntfy ([Android](https://play.google.com/store/apps/details?id=io.heckel.ntfy) / [iOS](https://apps.apple.com/app/ntfy/id1625396347))
2. Agrega servidor: `http://<ip-de-tu-máquina>:8081`
3. Suscríbete al topic: `barcelo-tuemail-at-dominio-com`

**Probar desde dentro de Docker (DNS interno `ntfy`):**
```bash
# Enviar una push de prueba desde el contenedor del worker
docker exec barcelo-notification \
  curl -s -d "Prueba de notificación" \
  -H "Title: Test Barcelo" \
  -H "Priority: 3" \
  http://ntfy:80/barcelo-test

# Suscribirse por SSE desde dentro de la red Docker
docker exec barcelo-notification \
  curl -s http://ntfy:80/barcelo-test/sse
```

---

### Hardening para producción

En producción nadie debe poder publicar notificaciones falsas. Sigue estos pasos:

**1. Levantar solo ntfy:**
```bash
docker compose up -d ntfy
```

**2. Crear el usuario administrador:**
```bash
docker exec -it barcelo-ntfy ntfy user add --role=admin admin
# Introduce la contraseña cuando se pida
```

**3. Generar un token para el servidor:**
```bash
docker exec barcelo-ntfy ntfy token add admin
# Devuelve algo como: tk_AgQdq7mVBoFD37zQVeaKCNYH...
```

**4. Añadir al `.env`:**
```env
NTFY_ADMIN_PASSWORD=tu-password-admin
NTFY_SERVER_TOKEN=tk_AgQdq7mVBoFD37zQVeaKCNYH...
NTFY_AUTH_DEFAULT_ACCESS=deny-all
```

**5. Levantar el resto del stack:**
```bash
docker compose up -d
```

Con `deny-all` activado:
- Solo el `notification-worker` puede publicar (usa el token del servidor)
- Cada usuario recibe sus credenciales ntfy por email al crear su cuenta (solo lectura de su topic personal)
- Cualquier intento externo de publicar o leer es rechazado

---

## Desarrollo local (sin Docker)

Para correr los servicios directamente con Visual Studio o la CLI:

```bash
# Desde el directorio de cada servicio
dotnet run

# O con hot reload
dotnet watch run
```

Asegúrate de tener SQL Server, Kafka y ThingsBoard corriendo localmente en los puertos por defecto.
