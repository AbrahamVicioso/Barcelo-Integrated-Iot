# Guía de Docker - Barceló IoT

Esta guía explica cómo usar Docker en el proyecto Barceló IoT con dos configuraciones: producción (optimizada) y desarrollo (con hot reload).

## Configuraciones Disponibles

### 1. Modo Producción (Optimizado) - `docker-compose.yml`

**Características:**
- Imágenes multi-stage optimizadas
- Sin hot reload (menor consumo de recursos)
- Usa .NET Runtime en lugar de SDK completo
- Imágenes más pequeñas y rápidas
- Ideal para: testing, staging, producción

**Ventajas:**
- Consumo de memoria reducido (~70% menos)
- Consumo de CPU reducido (sin file watcher)
- Inicio más rápido
- Imágenes Docker más pequeñas

**Comandos:**

```bash
# Construir todas las imágenes
docker-compose build

# Construir una imagen específica
docker-compose build auth-api

# Iniciar todos los servicios
docker-compose up

# Iniciar en background
docker-compose up -d

# Ver logs
docker-compose logs -f

# Parar servicios
docker-compose down

# Reconstruir y reiniciar
docker-compose up --build
```

### 2. Modo Desarrollo (Hot Reload) - `docker-compose.dev.yml`

**Características:**
- Hot reload activado (dotnet watch)
- Monta el código fuente como volumen
- Usa .NET SDK completo
- Los cambios en el código se reflejan automáticamente
- Ideal para: desarrollo activo

**Comandos:**

```bash
# Iniciar en modo desarrollo
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up

# Iniciar en background
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d

# Ver logs
docker-compose -f docker-compose.yml -f docker-compose.dev.yml logs -f

# Parar servicios
docker-compose -f docker-compose.yml -f docker-compose.dev.yml down
```

## Comparación de Consumo de Recursos

### Modo Producción (Optimizado)
- **Imagen base:** `mcr.microsoft.com/dotnet/aspnet:9.0` (~220 MB)
- **Memoria por servicio:** ~80-150 MB
- **CPU:** Mínima (sin file watcher)
- **Tamaño total de imágenes:** ~1.5 GB para todas las APIs

### Modo Desarrollo (Hot Reload)
- **Imagen base:** `mcr.microsoft.com/dotnet/sdk:9.0` (~800 MB)
- **Memoria por servicio:** ~250-400 MB
- **CPU:** Media (file watcher activo)
- **Tamaño total de imágenes:** ~4 GB para todas las APIs

## Servicios Disponibles

| Servicio | Puerto HTTP | Puerto HTTPS | Imagen |
|----------|-------------|--------------|---------|
| auth-api | 5117 | 5118 | barcelo-auth-api:latest |
| usuarios-api | 5284 | 5285 | barcelo-usuarios-api:latest |
| reservas-api | 5141 | - | barcelo-reservas-api:latest |
| dispositivos-api | 5185 | - | barcelo-dispositivos-api:latest |
| api-gateway | 5019 | 5020 | barcelo-api-gateway:latest |
| notification-worker | - | - | barcelo-notification-worker:latest |
| audit-worker | 5250 | 5251 | barcelo-audit-worker:latest |

## Optimizaciones Implementadas

### 1. Dockerfiles Multi-Stage
Cada API tiene un Dockerfile optimizado con múltiples etapas:
- **base:** Runtime base (.NET AspNet)
- **build:** Compilación con SDK
- **publish:** Publicación en modo Release
- **final:** Imagen final solo con binarios

### 2. .dockerignore
El archivo `.dockerignore` excluye archivos innecesarios del contexto de build:
- Directorios `bin/` y `obj/`
- Archivos de Visual Studio
- Paquetes NuGet
- Logs y archivos temporales

### 3. Variables de Entorno Optimizadas
```yaml
DOTNET_CLI_TELEMETRY_OPTOUT: "1"  # Desactiva telemetría
DOTNET_NOLOGO: "1"                 # Oculta logo
DOTNET_RUNNING_IN_CONTAINER: "true" # Optimizaciones de contenedor
```

### 4. Caché de Dependencias
Los Dockerfiles copian primero los archivos `.csproj` para aprovechar la caché de Docker en la restauración de dependencias.

## Comandos Útiles

### Gestión de Imágenes

```bash
# Ver imágenes construidas
docker images | grep barcelo

# Eliminar imágenes no utilizadas
docker image prune

# Eliminar todas las imágenes del proyecto
docker-compose down --rmi all

# Reconstruir desde cero
docker-compose build --no-cache
```

### Gestión de Contenedores

```bash
# Ver contenedores en ejecución
docker-compose ps

# Ver uso de recursos
docker stats

# Ejecutar comando en un contenedor
docker-compose exec auth-api sh

# Ver logs de un servicio específico
docker-compose logs -f auth-api
```

### Limpieza

```bash
# Eliminar contenedores, redes y volúmenes
docker-compose down -v

# Limpieza completa del sistema Docker
docker system prune -a --volumes
```

## Recomendaciones

### Para Desarrollo Diario
Usa el modo desarrollo para trabajar en el código:
```bash
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d
```

### Para Testing/QA
Usa el modo producción para simular el entorno real:
```bash
docker-compose up -d
```

### Para Cambios en Dependencias
Si cambias paquetes NuGet, reconstruye las imágenes:
```bash
docker-compose build --no-cache <servicio>
```

### Para Ahorrar Recursos
- Ejecuta solo los servicios que necesites:
  ```bash
  docker-compose up auth-api usuarios-api sqlserver
  ```
- Usa el modo producción en lugar de desarrollo
- Para y elimina servicios no utilizados:
  ```bash
  docker-compose down
  ```

## Troubleshooting

### "No se puede conectar a la base de datos"
- Verifica que SQL Server esté saludable:
  ```bash
  docker-compose ps sqlserver
  ```
- Espera a que el healthcheck pase (puede tardar ~90 segundos)

### "Puerto ya en uso"
- Cambia los puertos en el archivo `docker-compose.yml`
- O detén el proceso que usa el puerto

### "Imagen no encontrada"
- Construye las imágenes primero:
  ```bash
  docker-compose build
  ```

### "Cambios no se reflejan"
- Si estás en modo producción, reconstruye:
  ```bash
  docker-compose up --build
  ```
- Si estás en modo desarrollo, verifica que el hot reload esté funcionando en los logs

## Migración desde Configuración Anterior

Si vienes de la configuración anterior con hot reload siempre activo:

1. **Para desarrollo:** Usa el nuevo comando con el archivo dev:
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
   ```

2. **Para producción/testing:** Usa el comando estándar:
   ```bash
   docker-compose up
   ```

3. **Reconstruir imágenes:** La primera vez ejecuta:
   ```bash
   docker-compose build
   ```

## Próximos Pasos

- Considera usar Docker BuildKit para builds más rápidos
- Implementa health checks personalizados para cada API
- Configura límites de recursos con `deploy.resources`
- Implementa logging centralizado
