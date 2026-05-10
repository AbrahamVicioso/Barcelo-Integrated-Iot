# Barcelo Integrated IoT

Plataforma IoT para gestión de habitaciones inteligentes del Hotel Barcelo. Arquitectura de microservicios con .NET 9, Kafka, gRPC y ThingsBoard.

---

## Servicios

| Servicio | Descripción | Puerto REST | Puerto gRPC |
|---|---|---|---|
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
| ntfy | 8082 (HTTP) | Servidor de notificaciones push |
| Metabase | 3001 | Reportes y visualizaciones (CSV/Excel/JSON) |

---

## Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) con WSL2
- .NET 9 SDK instalado en el host (para tener el caché de NuGet local)

---

## Desarrollo Local (Inicio rápido)

### 1. Generar certificados de desarrollo

```powershell
.\scripts\bootstrap-local.ps1
```

Esto genera:
- `docker/certs/live/smartstay.es/smartstay.pfx` - Certificado autofirmado para APIs .NET
- Configura automáticamente el `.env`

### 2. Levantar servicios de infraestructura

```powershell
docker compose up -d sqlserver kafka ntfy
```

### 3. Levantar las APIs (modo desarrollo con hot reload)

```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml up
```

> **Listo:** Las APIs están disponibles en:
> - auth-api: http://localhost:5117
> - usuarios-api: http://localhost:5284
> - reservas-api: http://localhost:5141
> - dispositivos-api: http://localhost:5185

---

## DNS Local (smartstay.int)

### Agregar entradas DNS para desarrollo

```powershell
#Ejecutar como Administrador
.\scripts\set-dns-local.ps1
```

Esto agrega entradas al archivo `hosts` de Windows:
- `smartstay.int` → localhost
- `api.smartstay.int` → localhost
- `ntfy.smartstay.int` → localhost

### Ver entradas DNS actuales

```powershell
.\scripts\set-dns-local.ps1 -Show
```

### Remover entradas DNS

```powershell
.\scripts\set-dns-local.ps1 -Remove
```

---

## Certificados SSL

### Desarrollo Local (automático)

El script `bootstrap-local.ps1` genera certificados autofirmados automáticamente:

> **Importante:** ejecutar los scripts desde la **raíz del proyecto**, no desde dentro de `scripts/`.

```powershell
# Desde la raíz del proyecto
.\scripts\generate-dev-certs.ps1
```

Ubicación: `docker/certs/live/smartstay.es/` (`cert.pem`, `privkey.pem`, `smartstay.pfx`)

### Producción (Let's Encrypt)

Certbot está en un archivo separado. NO se inicia automáticamente.

**Para generar certificados reales:**

1. Configura DNS en tu proveedor de dominio
2. Ensure puerto 80 está abierto

```bash
#Ejecutar Certbot
docker compose -f docker-compose.certbot.yml up -d
```

Certificados se generan en: `docker/certs/live/smartstay.es/`

---

## Notificaciones Push (ntfy)

### Desarrollo (funciona sin configuración extra)

ntfy funciona en modo HTTP (puerto 8082):

```bash
docker compose up -d ntfy
```

- Puerto 8082: HTTP
- Acceso abierto (`read-write`)
- No requiere certificados SSL

### Producción

```bash
#1. Levantar ntfy
docker compose up -d ntfy

#2. Crear usuario admin
docker exec -it barcelo-ntfy ntfy user add --role=admin admin

#3. Generar token de servidor
docker exec barcelo-ntfy ntfy token add admin

#4. Actualizar .env
NTFY_LISTEN_HTTPS=--listen-https :443
NTFY_KEY_FILE=/certs/privkey.pem
NTFY_CERT_FILE=/certs/fullchain.pem
NTFY_ADMIN_PASSWORD=tu_password
NTFY_SERVER_TOKEN=tk_...
NTFY_AUTH_DEFAULT_ACCESS=deny-all

#5. Recargar
docker compose up -d ntfy
```

---

## Scripts Disponibles

| Script | Descripción |
|---|---|
| `scripts/bootstrap-local.ps1` | Bootstrap: certificados + verificación entorno |
| `scripts/generate-dev-certs.ps1` | Generar certificados autofirmados |
| `scripts/set-dns-local.ps1` | Agregar entradas DNS al hosts de Windows |
| `scripts/set-dns-local.ps1 -Show` | Ver entradas DNS actuales |
| `scripts/set-dns-local.ps1 -Remove` | Remover entradas DNS |

---

## Comandos del día a día

```bash
# Ver logs de todos los servicios
docker compose logs -f

# Ver logs de un servicio
docker compose logs -f auth-api

# Estado de contenedores
docker compose ps

# Reiniciar un servicio
docker compose restart auth-api

# Parar todo
docker compose down

# Parar y borrar volúmenes (reset DB)
docker compose down -v
```

---

## Hot Reload

El archivo `docker-compose.dev.yml` activa el modo desarrollo:

```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml up
```

Los servicios usan `dotnet watch run` con hot reload automático.

El caché de NuGet persiste en volumen Docker (`nuget-cache`).

---

## API Gateway

Rutas disponibles en `http://localhost:5019`:

| Ruta | Servicio |
|---|---|
| `/api/auth/**` | auth-api |
| `/api/user/**` | usuarios-api |
| `/api/reserva/**` | reservas-api |
| `/api/device/**` | dispositivos-api |
| `/api/reserva/reportes/**` | reservas-api (proxy → Metabase) |

---

## Documentación interactiva (Scalar)

| Servicio | URL |
|---|---|
| auth-api | http://localhost:5117/scalar |
| usuarios-api | http://localhost:5284/scalar |
| reservas-api | http://localhost:5141/scalar |
| dispositivos-api | http://localhost:5185/scalar |

---

## Arquitectura gRPC

Comunicación interna entre servicios sobre HTTPS con certificados de desarrollo:

```
reservas-api  ──gRPC HTTPS──▶  usuarios-api:5285
usuarios-api  ──gRPC HTTPS──▶  auth-api:5118
```

---

## Base de datos

SQL Server en `localhost:1433`. Las migraciones se aplican automáticamente.

**Cadena de conexión:**
```
Data source=localhost;Database=BarceloIoTDatabase;User Id=barcelo;Password=Testing1234;TrustServerCertificate=True
```

---

## Variables de Entorno (.env)

| Variable | Desarrollo | Producción |
|---|---|---|
| `ENVIRONMENT` | development | production |
| `NTFY_LISTEN_HTTPS` | (vacío) | `--listen-https :443` |
| `NTFY_AUTH_DEFAULT_ACCESS` | read-write | deny-all |
| `NTFY_SERVER_TOKEN` | (opcional) | requerido |
| `GATEWAY_PUBLIC_BASE_URL` | https://localhost:5020 | https://tu-dominio.com |

---

## Metabase (Reportes)

Herramienta de BI para crear y exportar reportes conectada a `BarceloIoTDatabase`. Imagen pública oficial, no requiere instalación adicional.

### Instalación

```bash
# 1. Crear base de datos de metadatos en PostgreSQL (ya corre en el stack)
docker exec barcelo-postgres psql -U postgres -c "CREATE DATABASE metabase;"

# 2. Levantar Metabase (primera vez toma ~2 minutos)
docker compose up -d metabase

# 3. Esperar healthcheck
docker compose ps metabase
# STATUS debe ser: healthy
```

### Configuración inicial (una sola vez)

1. Abrir http://localhost:3001
2. Completar el wizard de setup:
   - Admin email: `admin@smartstay.es`
   - Admin password: `Admin1234!` (o el valor en `.env`)
3. Agregar base de datos SQL Server:
   - **Type:** SQL Server
   - **Host:** `sqlserver` (nombre del contenedor)
   - **Port:** `1433`
   - **Database:** `BarceloIoTDatabase`
   - **Username:** `sa` / **Password:** `Testing1234`

### Acceso

| Recurso | URL | Credenciales |
|---|---|---|
| Web UI | http://localhost:3001 | admin@smartstay.es / Admin1234! |
| Via .NET API | `GET /reportes/disponibles` | JWT bearer |

### Consumir reportes via .NET API

Los reportes en Metabase se llaman **Questions** y tienen un ID numérico.

```bash
# Listar todas las questions disponibles (requiere JWT)
GET /reportes/disponibles

# Exportar question ID=5 como CSV
GET /reportes/5?formato=csv

# Exportar question ID=5 como Excel
GET /reportes/5?formato=xlsx

# Exportar question ID=5 como JSON
GET /reportes/5?formato=json
```

### Cargar queries SQL predefinidas

Las plantillas SQL están en `docker/metabase/questions/*.json`. Se cargan automáticamente via API (idempotente — no duplica si ya existen).

```bash
# Ejecutar UNA VEZ después del setup inicial de Metabase
docker compose --profile init run --rm metabase-init
```

Questions incluidas por defecto:
| Archivo | Question | Filtros |
|---|---|---|
| `01-ocupacion-hotel.json` | Ocupación por Hotel | fecha_inicio, fecha_fin |
| `02-reservas-periodo.json` | Reservas por Período | fecha_inicio, fecha_fin |
| `03-ingresos-hotel.json` | Ingresos por Hotel | fecha_inicio, fecha_fin |
| `04-historial-accesos.json` | Historial de Accesos | fecha_inicio, fecha_fin |

Para agregar nuevas plantillas: crear `docker/metabase/questions/05-mi-reporte.json` con la misma estructura y volver a ejecutar `metabase-init`.

### Notas

- Metabase requiere ~**2 GB de RAM** (vs 6 GB de JasperReports).
- Cambiar `METABASE_ADMIN_EMAIL` y `METABASE_ADMIN_PASSWORD` en `.env` para producción.
- PDF no está disponible via REST API en la edición open source (sí desde la UI).
- `GET /reportes/{cardId}/ver` redirige a la URL pública de Metabase (requiere activar Public Sharing en Metabase Settings).

---

## Desarrollo local (sin Docker)

Para correr servicios directamente:

```bash
cd Authenticate/Authentication.Api
dotnet watch run
```

Asegúrate de tener SQL Server, Kafka y ThingsBoard corriendo.