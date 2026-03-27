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
| `notification-worker` | Notificaciones por email | — | — |
| `audit-worker` | Auditoría de eventos | — | — |

### Infraestructura

| Servicio | Puerto externo | Uso |
|---|---|---|
| SQL Server | 1433 | Base de datos de los microservicios .NET |
| Kafka | 9092 | Mensajería entre servicios (KRaft, sin Zookeeper) |
| PostgreSQL | 5432 | Base de datos de ThingsBoard |
| ThingsBoard CE | 8080 | Gestión de dispositivos IoT |

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

# Windows PowerShell
.\docker\generate-dev-cert.ps1
```

Genera `docker/certs/barcelo-dev.pfx` usado por los servicios gRPC para TLS.

### 3. Inicializar ThingsBoard (solo la primera vez)

```bash
docker compose run --rm -e INSTALL_TB=true -e LOAD_DEMO=true thingsboard-ce
```

Espera a que termine (1-2 minutos) y luego continúa.

### 4. Levantar todos los servicios

```bash
docker compose up
```

> No hay `docker compose build` — se usa directamente la imagen `mcr.microsoft.com/dotnet/sdk:9.0`.
> SQL Server necesita ~60 segundos para arrancar; los servicios esperan automáticamente.

### Levantar en background

```bash
docker compose up -d
```

> **Nota:** el comando de inicialización de ThingsBoard solo se corre una vez. Los datos quedan persistidos en el volumen `tb-postgres-data`.

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

## Desarrollo local (sin Docker)

Para correr los servicios directamente con Visual Studio o la CLI:

```bash
# Desde el directorio de cada servicio
dotnet run

# O con hot reload
dotnet watch run
```

Asegúrate de tener SQL Server, Kafka y ThingsBoard corriendo localmente en los puertos por defecto.
