# Plan de Implementación: JasperReports Server + Studio

## Descripción General del Sistema

Este documento describe paso a paso cómo implementar JasperReports Server en Docker con JasperReports Studio para diseñar y publicar reportes basados en la base de datos BarceloIoT (SQL Server).

### Arquitectura

```
┌──────────────────────────────────────────────────────────────────────────┐
│                    Subdominio: reports.smartstay.es             │
└───────────────────────────────┬──────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                         Nginx (Proxy Inverso)                     │
│                    reports.smartstay.es:80/443                   │
└───────────────────────────────┬──────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────────────┐
│              JasperReports Server (Docker)                             │
│              Puerto: 8083 (HTTP), 8444 (HTTPS)                       │
│              - Web UI: /jasperserver                                 │
│              - API: /jasperserver/rest_v2                            │
└───────────────────────────────┬──────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────────────┐
│              SQL Server (sqlserver:1433)                             │
│              - jasperserver (meta-data de JRS)                       │
│              - BarceloIoTDatabase (datos del negocio)                 │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## fase 1: JasperReports Studio (Diseño Local)

### 1.1 Instalación

| Requisito | Versión | Notas |
|-----------|--------|-------|
| JasperReports Studio | 9.0.0+ | Compatible con JRS 9.0.0 |
| Java JDK | 17 LTS | Requerido |
| Windows | 10/11 | Sistema operativo |

**URLs de Descarga**:

- JasperReports Studio: https://community.jaspersoft.com/project/jaspersoft-studio
- Microsoft JDBC Driver: https://learn.microsoft.com/en-us/sql/connect/jdbc/download-microsoft-jdbc-driver-for-sql-server

### 1.2 Configuración JDBC en Studio

1. Abrir JasperReports Studio
2. Repository Explorer → Data Adapters → Create Data Adapter
3. Seleccionar: Database JDBC Connection
4. Configurar:

```
Data Adapter Name: BarceloIoT
JDBC Driver: com.microsoft.sqlserver.jdbc.SQLServerDriver
JDBC URL: jdbc:sqlserver://localhost:1433;databaseName=BarceloIoTDatabase;TrustServerCertificate=true
Username: sa
Password: Testing1234
```

5. Driver Classpath: Agregar `mssql-jdbc-12.x.x.jar`

### 1.3 Esquema de Base de Datos

#### Tablas Principales para Reportes

| Tabla | Descripción |
|-------|-------------|
| Hoteles | Hoteles del sistema |
| Habitaciones | Habitaciones por hotel |
| Reservas | Reservas de huéspedes |
| Huespedes | Información de huéspedes |
| EstadosReserva | Estados de reserva |
| Users | Usuarios del sistema |
| Personal | Personal del hotel |
| CerradurasInteligentes | Cerraduras inteligentes |
| RegistrosAcceso | Registros de acceso |
| Reportes | Reportes generados |
| CredencialesAcceso | Credenciales de acceso |

#### Consultas SQL de Ejemplo para Reportes

```sql
-- Reporte de Ocupación Hotel
SELECT 
    h.Nombre AS Hotel,
    hb.NumeroHabitacion,
    r.FechaCheckIn,
    r.FechaCheckOut,
    r.EstadoReservaId,
    CASE WHEN r.CheckInRealizado IS NOT NULL THEN 'Check-In' ELSE 'Pendiente' END AS Estado
FROM Reservas r
INNER JOIN Habitaciones hb ON r.HabitacionId = hb.HabitacionId
INNER JOIN Hoteles h ON hb.HotelId = h.HotelId
WHERE r.FechaCheckIn BETWEEN $P{FechaInicio} AND $P{FechaFin}

-- Reporte de Reservas por Período
SELECT 
    r.NumeroReserva,
    hs.Nombre + ' ' + hs.Apellido AS Huesped,
    r.FechaCheckIn,
    r.FechaCheckOut,
    r.MontoTotal,
    er.NombreEstado AS Estado
FROM Reservas r
INNER JOIN Huespedes hs ON r.HuespedId = hs.HuespedId
INNER JOIN EstadosReserva er ON r.EstadoReservaId = er.EstadoReservaId
WHERE r.FechaCreacion BETWEEN $P{FechaInicio} AND $P{FechaFin}

-- Reporte de Ingresos
SELECT 
    h.Nombre AS Hotel,
    SUM(r.MontoTotal) AS IngresosTotales,
    SUM(r.MontoPagado) AS IngresosPagados,
    COUNT(r.ReservaId) AS TotalReservas
FROM Reservas r
INNER JOIN Habitaciones hb ON r.HabitacionId = hb.HabitacionId
INNER JOIN Hoteles h ON hb.HotelId = h.HotelId
WHERE r.FechaCheckIn BETWEEN $P{FechaInicio} AND $P{FechaFin}
GROUP BY h.Nombre

-- Reporte de Accesos
SELECT 
    ra.FechaAcceso,
    ra.Pin,
    raResultado,
    ci.Nombre AS Cerradura
FROM RegistrosAcceso ra
INNER JOIN CerradurasInteligentes ci ON ra.CerraduraId = ci.CerraduraId
WHERE ra.FechaAcceso BETWEEN $P{FechaInicio} AND $P{FechaFin}
```

---

## Fase 2: JasperReports Server (Docker)

### 2.1 Preparar Base de Datos

Ejecutar en SQL Server existente:

```sql
-- Crear base de datos para JRS
CREATE DATABASE jasperserver;
GO
-- El resto de tablas las crea JRS automáticamente al iniciar
USE jasperserver;
GO
```

### 2.2 Estructura de Volúmenes

```
docker/
├── jasperreports/
│   └── content/
│       ├── /reports           # reportes publicados
│       │   ├── /ocupacion
│       │   ├── /reservas
│       │   └── /ingresos
│       ├── /data-sources     # datasources JSON
│       └── /domains          # domains (opcional)
└── jasperdrivers/
    └── mssql-jdbc-12.6.1.jar  # JDBC driver para SQL Server
```

### 2.3 docker-compose.yml

Agregar al archivo `docker-compose.yml` existente:

```yaml
jasperreports:
  image: tibco/jasperreports-server:9.0.0
  container_name: barcelo-jasperreports
  deploy:
    resources:
      limits:
        memory: 6G
        cpus: '1.0'
  ports:
    - "8083:8080"
    - "8444:8443"
  environment:
    DB_TYPE: MSSQL
    DB_HOST: sqlserver
    DB_PORT: 1433
    DB_NAME: jasperserver
    DB_USERNAME: sa
    DB_PASSWORD: Testing1234
  volumes:
    - ./docker/jasperreports/content:/content
    - ./docker/jasperdrivers:/drivers
  depends_on:
    sqlserver:
      condition: service_healthy
  networks:
    - barcelo-net
```

### 2.4 Iniciar Servicio

```bash
docker compose up -d jasperreports
```

Verificar que esté corriendo:

```bash
docker logs barcelo-jasperreports
# Buscar: "JasperReports Server started successfully"
```

---

## Fase 3: Configurar Data Source en JRS

### 3.1 Acceder al Web UI

1. Abrir: http://localhost:8083/jasperserver
2. Username: `jasperadmin`
3. Password: `jasperadmin`

### 3.2 Crear Data Source

1. Navigate: Repository → Add Resource → Data Source
2. Configurar:

```
Name: BarceloIoT
Type: JDBC Data Source
JDBC Driver: com.microsoft.sqlserver.jdbc.SQLServerDriver
Connection URL: jdbc:sqlserver://sqlserver:1433;databaseName=BarceloIoTDatabase;TrustServerCertificate=true
Username: sa
Password: Testing1234
```

3. Test Connection
4. Save

---

## Fase 4: Integración Nginx

### 4.1 Modificar nginx.template.conf

Agregar en `docker/nginx/nginx.template.conf`:

```nginx
# JasperReports
upstream jasperreports_backend {
    server jasperreports:8080;
}

server {
    listen 80;
    server_name reports.${DOMAIN};

    location / {
        proxy_pass http://jasperreports_backend;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # WebSocket support
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }
}
```

### 4.2 URLs Resultantes

| Servicio | URL |
|----------|-----|
| JRS Web | http://reports.smartstay.es/jasperserver |
| JRS API | http://reports.smartstay.es/jasperserver/rest_v2 |
| Admin UI | http://reports.smartstay.es/jasperserver |

### 4.3 Regenerar Nginx

```bash
./generate-nginx.sh
docker compose up -d nginx
```

---

## Fase 5: Diseñar y Publicar Reportes

### 5.1 Conectar Studio a JRS

1. JasperReports Studio
2. Repository Explorer → Create JasperReports Server Connection
3. Configurar:

```
URL: http://jasperreports:8080/jasperserver
Username: jasperadmin
Password: jasperadmin
```

4. Test Connection
5. Finish

### 5.2 Publicar Reporte

1. Diseñar reporte en Studio
2. File → Publish to JasperReports Server
3. Seleccionar conexión
4. Propiedades:

```
Repository Path: /reports
Data Source: BarceloIoT
Publish Strategy: Overwrite
```

5. Finish

### 5.3 Reportes Sugeridos

| Reporte | Descripción | Filtros |
|---------|-------------|---------|
| OcupacionHotel | % ocupación por hotel y fecha | FechaInicio, FechaFin, HotelId |
| ReservasPeriodo | Listado de reservas | FechaInicio, FechaFin |
| IngresosHotel | Ingresos por período | FechaInicio, FechaFin, HotelId |
| HistorialAccesos | Registro de accesos | FechaInicio, FechaFin, CerraduraId |
| ResumenHuespedes | Resumen de huéspedes activos | FechaInicio, FechaFin |

---

## Fase 6: API REST (Opcional)

### 6.1 Endpoints de API

Generar reporte vía API:

```bash
# PDF
curl -X GET "http://reports.smartstay.es/jasperserver/rest_v2/reports/ocupacion.pdf?FechaInicio=2025-01-01&FechaFin=2025-12-31" \
  -u jasperadmin:jasperadmin \
  -o ocupacion.pdf

# Excel
curl -X GET "http://reports.smartstay.es/jasperserver/rest_v2/reports/ocupacion.xls?FechaInicio=2025-01-01&FechaFin=2025-12-31" \
  -u jasperadmin:jasperadmin \
  -o ocupacion.xls
```

### 6.2 Formatos Soportados

| Formato | Extensión |
|--------|-----------|
| PDF | .pdf |
| HTML | .html |
| Excel | .xls, .xlsx |
| CSV | .csv |
| RTF | .rtf |
| XML | .xml |

---

## Checklist de Implementación

| # | Tarea | Estado |
|---|------|--------|
| 1 | Descargar e instalar JasperReports Studio | ☐ |
| 2 | Descargar JDBC Driver para SQL Server | ☐ |
| 3 | Configurar Data Source en Studio | ☐ |
| 4 | Crear base de datos jasperserver | ☐ |
| 5 | Agregar servicio a docker-compose.yml | ☐ |
| 6 | Crear estructura de volúmenes | ☐ |
| 7 | Iniciar contenedor JRS | ☐ |
| 8 | Configurar Data Source en JRS Web | ☐ |
| 9 | Agregar configuración Nginx | ☐ |
| 10 | Regenerar configuración Nginx | ☐ |
| 11 | Publicar reportes desde Studio | ☐ |
| 12 | Probar endpoints REST API | ☐ |

---

## Notas Importantes

- **Memoria**: JRS requiere mínimo 6GB de RAM
- **Primera ejecución**: Puede tomar 5-10 minutos en inicializar
- **Puerto**: 8083 para HTTP, 8444 para HTTPS
- **Credenciales por defecto**: jasperadmin / jasperadmin
- **Data Source externo**: JRS usa su propia DB (jasperserver) para metadata, pero puede conectarse a múltiples fuentes de datos including tu BarceloIoTDatabase

---

## Referencias

- Docker: https://github.com/TIBCOSoftware/js-docker
- Studio: https://community.jaspersoft.com/project/jaspersoft-studio
- JDBC Driver: https://learn.microsoft.com/en-us/sql/connect/jdbc/download-microsoft-jdbc-driver-for-sql-server
- Wiki: https://github.com/TIBCOSoftware/js-docker/wiki