# CLAUDE.md — Barcelo Integrated IoT

## Instrucción de trabajo

- Leer este archivo completo al inicio de cada sesión. Contiene todo el contexto necesario.
- **Antes de leer código**, consultar aquí si el patrón ya está documentado. Solo leer archivos si hay duda sobre el estado real o si el patrón no está cubierto.
- Cuando se toque un archivo, leer **solo ese archivo** y los directamente relacionados — no explorar el proyecto completo.
- Si durante el trabajo se descubre algo importante no documentado (patrón, convención, constraint, comportamiento inesperado), **agregarlo a este CLAUDE.md inmediatamente**.
- Aplicar siempre las reglas de este archivo al tocar cualquier parte del código.

---

## Estructura del proyecto

```
Barcelo-Integrated-Iot/
├── Reservas/
│   ├── Reservas.API/                         Controllers, Program.cs
│   ├── Reservas.Application/
│   │   ├── Common/Result.cs                  Result<T> (versión antigua, sin NotFound)
│   │   ├── DTOs/                             CreateXxxDto, UpdateXxxDto, XxxDto
│   │   ├── Features/{Entidad}/
│   │   │   ├── Commands/                     XxxCommand.cs + XxxCommandHandler.cs
│   │   │   └── Queries/                      XxxQuery.cs + XxxQueryHandler.cs
│   │   ├── Interfaces/                       IXxxRepository, IUnitOfWork
│   │   └── Mappings/MappingProfile.cs
│   ├── Reservas.Domain/Entities/
│   └── Reservas.Persistence/
│       ├── Repositories/
│       └── Services/                         CredencialesAccesoService (SQL raw)
│
├── Dispositivos.API/                         Controllers/
├── Dispositivos.Application/
│   ├── Common/
│   │   ├── Result.cs                         Result<T> con IsNotFound + NotFound()
│   │   ├── PagedResult.cs                    Respuesta paginada estándar
│   │   └── PaginationParams.cs               Binding [FromQuery], normaliza Page/PageSize
│   ├── DTOs/
│   ├── Features/{Entidad}/
│   │   ├── Commands/
│   │   └── Queries/
│   ├── Interfaces/                           IXxxRepository, IUnitOfWork
│   └── Mappings/MappingProfile.cs
├── Dispositivos.Domain/Entities/
├── Dispositivos.Infrastructure/              Kafka consumers (BackgroundService)
│   └── Kafka/Consumers/
└── Dispositivos.Persistence/
    ├── Data/Configurations/                  EF Core entity configs + constraints
    └── Repositories/
│
├── Usuarios/
│   ├── Usuarios.API/                         Controllers, ExceptionHandlingMiddleware
│   ├── Usuarios.Application/
│   │   ├── Features/{Entidad}/Commands|Queries/
│   │   └── Exceptions/                       NotFoundException, ConflictException, BusinessException
│   └── Usuarios.Persistence/
│
├── Authenticate/
│   └── Authentication.Api/
│       ├── Handlers/                         LoginUserHandler, RegisterUserHandler (estáticos)
│       └── DTOs/UserManagementDtos.cs        Incluye PagedResult<T> local
│
├── Notification.Worker/                      Program.cs con todos los BackgroundServices
├── Notification.Domain/Events/               Eventos de dominio para Kafka
├── Notification.Kafka/
│   ├── Configuration/                        XxxConsumerConfig (hereda KafkaConsumerConfig)
│   └── Services/                             XxxEventConsumer (IHostedService)
│
├── Audit.Worker/                             Consume audit.events, escribe a DB
│   └── Models/                               PagedResult<T>, AuditQueryParams
│
└── ApiGateway/                               Ocelot
```

### Convención de nombres de archivos (Dispositivos/Reservas)

| Tipo | Ruta |
|---|---|
| Entidad | `{Svc}.Domain/Entities/{Entidad}.cs` |
| DTO | `{Svc}.Application/DTOs/{Entidad}Dto.cs` |
| Repositorio interfaz | `{Svc}.Application/Interfaces/I{Entidad}Repository.cs` |
| Repositorio impl | `{Svc}.Persistence/Repositories/{Entidad}Repository.cs` |
| Query | `{Svc}.Application/Features/{Entidad}/Queries/Get{Entidad}By{Campo}Query.cs` |
| Handler query | `{Svc}.Application/Features/{Entidad}/Queries/Get{Entidad}By{Campo}QueryHandler.cs` |
| Command | `{Svc}.Application/Features/{Entidad}/Commands/{Accion}{Entidad}Command.cs` |
| Handler command | `{Svc}.Application/Features/{Entidad}/Commands/{Accion}{Entidad}CommandHandler.cs` |
| Controller | `{Svc}.API/Controllers/{Entidad}Controller.cs` |
| EF Config | `{Svc}.Persistence/Data/Configurations/{Entidad}Configuration.cs` |

---

## Eficiencia — cómo trabajar reduciendo tokens

**Regla principal:** los patrones están documentados aquí. No releer archivos para confirmar lo que ya está en el CLAUDE.md.

### Cuándo NO leer archivos
- Para crear un nuevo endpoint GetAll con paginación → seguir la plantilla de "Estándar de paginación" directamente.
- Para crear un command handler → seguir "Regla obligatoria: manejo de errores en handlers" directamente.
- Para saber qué métodos tiene un repositorio → ver la sección "Repositorios disponibles".
- Para saber los constraints de DB → ver la lista en "Capturar DbUpdateException".

### Cuándo SÍ leer archivos (mínimo necesario)
- Al crear código nuevo en una entidad que aún no se ha tocado → leer **solo** el archivo de la entidad más cercana al cambio (el mismo controller, el mismo handler del mismo feature).
- Al modificar código existente → leer **solo el archivo a modificar** antes de editarlo.
- Al dudar si un método existe en un repositorio → leer solo `I{Entidad}Repository.cs`.
- Nunca usar el agente Explore para tareas simples; usar Grep o Glob directos.

### Orden óptimo al agregar una feature nueva en Dispositivos/Reservas

1. Leer `I{Entidad}Repository.cs` si el método que se necesita no está en el CLAUDE.md.
2. Escribir Query → Handler → endpoint en Controller. (No leer otros archivos si el patrón está aquí.)
3. Si hay duda sobre el DTO existente, leer solo `{Entidad}Dto.cs`.
4. Si hay nueva constraint DB, agregarla a CLAUDE.md en la lista de constraints conocidos.

---

## Infraestructura

- **.NET 9** · SQL Server (sqlserver:1433) · Kafka (kafka:9092) · ThingsBoard CE (thingsboard-ce:8080)
- **DB local:** `Data source=localhost;Database=BarceloIoTDatabase;User Id=barcelo;Password=Testing1234;TrustServerCertificate=True`
- **DB Docker:** `Data source=sqlserver;Database=BarceloIoTDatabase;User Id=sa;Password=Testing1234;TrustServerCertificate=True`
- **JWT:** Issuer=`barcelo`, Audience=`BarceloIoT`, Key=`u9Z3fBq7M!8@R2L#A4xCkWmP0EJvH5Ys`

---

## Servicios y puertos

| Servicio | Puerto local | Puerto Docker |
|---|---|---|
| Reservas.API | — | 5141 |
| Dispositivos.API | — | 5185 |
| Usuarios.API | — | 5284/5285 |
| Authenticate.API | — | 5117/5118 |
| ApiGateway | — | 5019/5020 |
| Audit.Worker | — | 5250/5251 |
| Notification.Worker | — | — |

---

## Patrones de arquitectura por servicio

### Reservas & Dispositivos — MediatR + CQRS + `Result<T>`

Handlers devuelven `Result<T>`. **Nunca lanzan excepciones al controller.**

**`Dispositivos.Application/Common/Result.cs`** (versión actualizada):
```csharp
Result<T>.Success(data)        // → HTTP 200/201
Result<T>.NotFound("mensaje")  // → HTTP 404  (tiene IsNotFound = true)
Result<T>.Failure("mensaje")   // → HTTP 400
```

**`Reservas.Application/Common/Result.cs`** (versión antigua — pendiente actualizar):
- No tiene `IsNotFound` ni `NotFound()`. Si tocas handlers de Reservas, agrégalo igual que en Dispositivos.

### Usuarios — MediatR + CQRS, excepciones personalizadas

No usa `Result<T>`. Los handlers lanzan excepciones que captura `ExceptionHandlingMiddleware`:

| Excepción | HTTP |
|---|---|
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `BusinessException` | 400 |
| Cualquier otra | 500 |

Respuesta del middleware:
```json
{ "status": 404, "error": "Not Found", "message": "Huesped no encontrado" }
```

### Authenticate — Sin MediatR

Usa métodos estáticos: `LoginUserHandler`, `RegisterUserHandler`, `CreateUserWithRandomPasswordHandler`. La auditoría se publica directamente en `AuthController`, no por pipeline.

---

## Estándar de paginación

Aplicar en **todos los endpoints `GetAll` y cualquier endpoint que devuelva `IEnumerable<T>`** en Dispositivos.API (y futuras APIs que usen MediatR + CQRS).

### Clases en `{Servicio}.Application/Common/`

```csharp
// PaginationParams.cs — se usa como [FromQuery] en el controller
public class PaginationParams
{
    private int _page = 1;
    private int _pageSize = 20;
    public int Page   { get => _page;     set => _page     = value < 1 ? 1 : value; }
    public int PageSize { get => _pageSize; set => _pageSize = value < 1 ? 20 : value > 100 ? 100 : value; }
}

// PagedResult<T>.cs — envuelve la respuesta paginada
public class PagedResult<T>
{
    public IReadOnlyList<T> Items     { get; init; }
    public int TotalCount             { get; init; }
    public int Page                   { get; init; }
    public int PageSize               { get; init; }
    public int TotalPages             => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage           => Page < TotalPages;
    public bool HasPreviousPage       => Page > 1;
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount) { ... }
}
```

### Query

```csharp
public class GetAllXxxQuery : IRequest<Result<PagedResult<XxxDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
```

### Handler

```csharp
public async Task<Result<PagedResult<XxxDto>>> Handle(GetAllXxxQuery request, CancellationToken ct)
{
    var todos = await _repo.GetAll();
    var todosDto = _mapper.Map<IEnumerable<XxxDto>>(todos).ToList();
    var totalCount = todosDto.Count;
    var items = todosDto
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToList();
    return Result<PagedResult<XxxDto>>.Success(
        new PagedResult<XxxDto>(items, request.Page, request.PageSize, totalCount));
}
```

### Controller

```csharp
[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
{
    var result = await _mediator.Send(new GetAllXxxQuery { Page = pagination.Page, PageSize = pagination.PageSize });
    if (!result.IsSuccess)
        return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
    return Ok(result.Data);
}
```

### Respuesta JSON

```json
{
  "items": [...],
  "totalCount": 150,
  "page": 2,
  "pageSize": 20,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": true
}
```

### Uso

```
GET /dispositivo?page=1&pageSize=20
GET /credencialesacceso/huesped/42?page=1&pageSize=10
```

- `page` mínimo: 1 (normalizado automáticamente)
- `pageSize` mínimo: 1, máximo: 100 (normalizado automáticamente)
- Parámetros opcionales — sin ellos usa defaults (página 1, 20 registros)

---

## Regla obligatoria: manejo de errores en handlers

### 1. Validar antes de tocar la DB

```csharp
// FK — verificar que la entidad referenciada existe
var dispositivo = await _unitOfWork.Dispositivos.GetById(request.DispositivoId);
if (dispositivo == null)
    return Result<T>.Failure($"Dispositivo '{request.DispositivoId}' no encontrado.");

// UNIQUE — verificar duplicado antes de insertar/actualizar
var existente = await _repo.GetByHabitacionId(request.HabitacionId);
if (existente.Any())
    return Result<T>.Failure($"La habitación {request.HabitacionId} ya tiene una cerradura asignada.");

// UNIQUE en update — excluir el propio registro
if (existente.Any(c => c.Id != request.Id))
    return Result<T>.Failure("...");
```

### 2. Capturar `DbUpdateException` sin importar `Microsoft.EntityFrameworkCore`

La capa Application **no** referencia EF Core. Usar exception filter:

```csharp
// ⚠️ NO: catch (DbUpdateException ex)  → error CS0234
// ✅ SÍ:
catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
{
    var inner = ex.InnerException?.Message ?? ex.Message;
    if (inner.Contains("UQ_Cerraduras_Habitacion"))
        return Result<T>.Failure("La habitación ya tiene una cerradura asignada.");
    if (inner.Contains("FK_Cerraduras_Dispositivos"))
        return Result<T>.Failure("Dispositivo no encontrado.");
    return Result<T>.Failure($"Error de base de datos: {inner}");
}
catch (Exception ex)
{
    return Result<T>.Failure($"Error inesperado: {ex.Message}");
}
```

Nombres de constraints conocidos:
- `UQ_Cerraduras_Habitacion` — una sola cerradura por habitación
- `FK_Cerraduras_Dispositivos` — DispositivoId debe existir en Dispositivos
- `CHK_Credenciales_Fechas` — FechaExpiracion debe ser posterior a FechaActivacion
- `UQ_Dispositivos_NumeroSerie` — número de serie único por dispositivo
- `UQ_Dispositivos_MAC` — dirección MAC única por dispositivo → campo en entidad: `DireccionMac` (no `DireccionMAC`)
- `UQ_Dispositivos_IP` — IP única por dispositivo (filtered index, permite múltiples NULL)
- `FK_Dispositivos_TiposDispositivo` — TipoDispositivoId debe existir en TiposDispositivo
- `FK_Dispositivos_EstadosDispositivo` — EstadoDispositivoId debe existir en EstadosDispositivo
- `FK_Dispositivos_Hoteles` — HotelId debe existir en Hoteles (sin repo propio, solo fallback por constraint name)
- `CHK_Dispositivos_Bateria` — NivelBateria entre 0 y 100

### 3. Usar `Result<T>.NotFound()` para entidades no encontradas

```csharp
if (cerradura == null)
    return Result<T>.NotFound($"Cerradura con ID {id} no encontrada.");
```

---

## Regla obligatoria: respuesta HTTP en controllers

**Siempre** envolver errores en objeto JSON, nunca devolver string plano:

```csharp
// ✅ Correcto
if (!result.IsSuccess)
    return result.IsNotFound
        ? NotFound(new { error = result.ErrorMessage })
        : BadRequest(new { error = result.ErrorMessage });
return Ok(result.Data);

// ❌ Incorrecto — devuelve "mensaje" como string JSON crudo
return BadRequest(result.ErrorMessage);
```

Tabla de status codes:

| Situación | Código |
|---|---|
| GET / PUT exitoso | `200 Ok` |
| POST (creación) exitoso | `201 CreatedAtAction` |
| DELETE exitoso | `200 Ok` o `204 NoContent` |
| Entidad no encontrada | `404 NotFound` |
| Validación / negocio | `400 BadRequest` |
| IDs de ruta y body no coinciden | `400 BadRequest` |

---

## EF Core — `AsNoTracking` + navigation properties

Todos los repositorios cargan entidades con `.AsNoTracking()`. Al hacer **update** que cambia una FK con navigation property cargada, EF Core recalcula la FK desde la navigation property, ignorando el nuevo valor.

**Solución: limpiar la navigation property antes de `UpdateAsync`:**

```csharp
_mapper.Map(request.Dto, entidad);
entidad.Dispositivo = null;  // ← obligatorio si cambia DispositivoId
await _repo.UpdateAsync(entidad, cancellationToken);
```

Casos conocidos donde aplica:
- `CerradurasInteligente.Dispositivo` cuando cambia `DispositivoId`

---

## Kafka — topics y flujo

| Topic | Productor | Consumidor |
|---|---|---|
| `reservas` | Reservas.API | Notification.Worker (`ReservaCreadaEventConsumer`) |
| `users` | Authenticate.API | Notification.Worker (`UserCreatedEventConsumer`) |
| `email-confirmation` | Authenticate.API | Notification.Worker (`EmailConfirmationEventConsumer`) |
| `dispositivos.unlock-door` | Reservas.API | Dispositivos.Infrastructure (`UnlockDoorKafkaConsumer`) |
| `reservas.checkin-realizado` | Reservas.API | Dispositivos.Infrastructure (`CheckInRealizadoKafkaConsumer`) |
| `checkin.credenciales` | Dispositivos.Infrastructure | Notification.Worker (`CredencialesCheckInEventConsumer`) |
| `audit.events` | Todos los APIs | Audit.Worker |

**Flujo unlock-door:**
1. `POST /reservas/{id}/unlock-door?pin=` → `UnlockDoorCommand`
2. Handler valida reserva activa, habitación asignada, PIN correcto → `RegistrarUsoAsync` suma uso a la credencial
3. Publica `UnlockDoorEvent` a `dispositivos.unlock-door`
4. `UnlockDoorKafkaConsumer` (BackgroundService en Dispositivos):
   - Busca cerradura activa por `HabitacionId`
   - Obtiene device de ThingsBoard por nombre = `DispositivoId.ToString()`
   - Setea `lockState = "unlocked"` como shared attribute
   - Registra entrada en `RegistrosAcceso`

**Flujo check-in + credenciales + email:**
1. `POST /reservas/checkin` → `PerformCheckInCommand`
2. Handler valida reserva, verifica email del huésped principal, obtiene email+nombre de **todos** los huéspedes vía `IUsuariosApiService`
3. Publica `CheckInRealizadoEvent { Huespedes: [{HuespedId, Email, NombreCompleto}] }` a `reservas.checkin-realizado`
4. `CheckInRealizadoKafkaConsumer` (Dispositivos):
   - Genera PIN aleatorio por huésped con `RandomNumberGenerator.GetInt32(100000, 1000000)`
   - Crea `CredencialesAcceso` (FechaActivacion=CheckIn, FechaExpiracion=CheckOut)
   - Publica `CredencialesCheckInEvent { Credenciales: [{Email, NombreCompleto, CodigoPin}] }` a `checkin.credenciales`
5. `CredencialesCheckInEventConsumer` (Notification.Worker):
   - Por cada huésped con email: envía email HTML con el PIN + push notification

---

## Kafka — patrón para agregar un nuevo flujo de notificación

Cuando necesites crear un nuevo flujo evento → notificación, sigue exactamente estos pasos:

### Paso 1 — Evento en `Notification.Domain/Events/`
```csharp
public class MiNuevoEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    // campos del evento...
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Paso 2 — Config en `Notification.Kafka/Configuration/`
```csharp
public class MiNuevoConsumerConfig : KafkaConsumerConfig
{
    public MiNuevoConsumerConfig()
    {
        GroupId = "notification-mi-nuevo-group";
        Topic = "mi.nuevo.topic";
    }
}
```

### Paso 3 — Consumer en `Notification.Kafka/Services/`

Seguir el patrón de `ReservaCreadaEventConsumer` o `CredencialesCheckInEventConsumer`:
- Constructor recibe: `Config`, `IEmailService`, `IPushNotificationService`, `ILogger`
- `StartAsync` → `EnsureTopicExists()` → `_consumer.Subscribe` → lanza `Task.Run(ConsumeMessages)`
- `ConsumeMessages` → loop → `ProcessMessageAsync`
- `ProcessMessageAsync` → deserializa JSON → llama al método de negocio
- `StopAsync` → cancela + `_consumer.Close()`
- `Dispose` → libera consumer, adminClient, cts

### Paso 4 — Registrar en `Notification.Worker/Program.cs`
```csharp
// En ConfigureServices:
var miConfig = new MiNuevoConsumerConfig();
context.Configuration.GetSection("KafkaConsumer:MiNuevo").Bind(miConfig);
services.AddSingleton(miConfig);
services.AddSingleton<MiNuevoEventConsumer>();
services.AddHostedService<MiNuevoNotificationWorker>();

// Al final del archivo, nuevo BackgroundService:
public class MiNuevoNotificationWorker : BackgroundService
{
    private readonly MiNuevoEventConsumer _kafkaConsumer;
    // ...igual que todos los otros workers del archivo
}
```

### Paso 5 — `docker-compose.yml` — OBLIGATORIO

Cada consumer necesita sus variables de entorno en el docker-compose. **Si no se agregan aquí, el worker usará `localhost:9092` en vez de `kafka:29092` y fallará en Docker.**

En el bloque `notification-worker > environment`:
```yaml
KafkaConsumer__MiNuevo__BootstrapServers: kafka:29092
KafkaConsumer__MiNuevo__GroupId: notification-mi-nuevo-group
KafkaConsumer__MiNuevo__Topic: mi.nuevo.topic
```

Si el productor está en `dispositivos-api`, agregar en `dispositivos-api > environment`:
```yaml
KafkaConsumer__CheckInRealizado__CredencialesProducerTopic: mi.nuevo.topic
```

> ⚠️ En Docker la red interna usa `kafka:29092`. El `localhost:9092` solo funciona desde el host. El `appsettings.json` tiene `localhost:9092` para desarrollo local — el docker-compose lo sobreescribe con variables de entorno.

### Paso 6 — `appsettings.json` de Notification.Worker
```json
"KafkaConsumer": {
  "MiNuevo": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "notification-mi-nuevo-group",
    "Topic": "mi.nuevo.topic",
    "AutoOffsetReset": "Earliest",
    "EnableAutoCommit": true,
    "AutoCommitIntervalMs": 5000,
    "SessionTimeoutMs": 30000,
    "MaxPollIntervalMs": 300000
  }
}
```

### Paso 6 — Si el productor es un BackgroundService en `Dispositivos.Infrastructure`

Añadir `IProducer<string, string>` en el constructor del BackgroundService:
```csharp
_producer = new ProducerBuilder<string, string>(new ProducerConfig
{
    BootstrapServers = _config.BootstrapServers,
    Acks = Acks.Leader
}).Build();
```
Y en `Dispose`: `_producer?.Flush(TimeSpan.FromSeconds(5)); _producer?.Dispose();`

El topic destino se configura como campo extra en la config del consumer:
```csharp
public string MiTopicProducerTopic { get; set; } = "mi.nuevo.topic";
```
Y en `appsettings.json` del servicio que produce:
```json
"KafkaConsumer:CheckInRealizado:MiTopicProducerTopic": "mi.nuevo.topic"
```

---

## Credenciales de acceso (`CredencialesAcceso`)

- **PIN generado**: `RandomNumberGenerator.GetInt32(100000, 1000000).ToString()` (6 dígitos)
- **Hash**: SHA-256 en Base64 → guardado en `HashPin`
- **Validación al usar unlock-door**: `ICredencialesAccesoService.GetCredencialIdAsync(reservaId, pin)` — compara `CodigoPIN` directo (no hash) con SQL raw
- **Registro de uso**: `ICredencialesAccesoService.RegistrarUsoAsync(credencialId)` → `NumeroUsos + 1` y `UltimoUso = NOW`
- **Constraint**: `CHK_Credenciales_Fechas` — `FechaExpiracion > FechaActivacion` (validar antes de insertar)

---

## ThingsBoard

```
GET  /api/tenant/devices?deviceName={name}         → buscar device por nombre
POST /api/plugins/telemetry/DEVICE/{id}/SHARED_SCOPE → setear atributos compartidos
```

- El nombre del device en ThingsBoard = `CerradurasInteligente.DispositivoId.ToString()`
- Auth: JWT obtenido con credenciales de tenant (cacheado 60 min)
- **No usar** `/api/device?name=...` (incorrecto)
- `POST /api/device` sin ID crea device; si ya existe devuelve 400 `"already exists"` → recuperar con `GetDeviceByNameAsync` (idempotente)

---

## Repositorios disponibles — métodos clave

### `ICerradurasInteligenteRepository`
```
GetAll() · GetById(int) · GetByDispositivoId(Guid) · GetByHabitacionId(int) · GetByEstaActiva(bool)
AddAsync · UpdateAsync · DeleteAsync
```

### `IDispositivoRepository`
```
GetAll() · GetById(Guid) · GetByHotelId(int) · GetByTipoDispositivo(int) · GetByEstaEnLinea(bool)
AddAsync · UpdateAsync · DeleteAsync
```

### `IReservaRepository` (Reservas)
```
GetByIdAsync · GetAllAsync · FindAsync · AddAsync · UpdateAsync · DeleteAsync · ExistsAsync
GetReservasByHuespedIdAsync · GetByNumeroReservaAsync · GetReservasByEstadoAsync
GetReservasByFechaRangoAsync · IsHabitacionOcupadaAsync(habitacionId, checkIn, checkOut, excludeReservaId?)
```

### `IUnitOfWork` (Dispositivos)
```
Dispositivos · CerradurasInteligente · CredencialesAcceso · MantenimientoCerradura
RegistrosAcceso · RegistrosAuditorium · EstadosDispositivo · TiposDispositivo
SaveChangesAsync · BeginTransactionAsync · CommitTransactionAsync · RollbackTransactionAsync
```

---

## AutoMapper — campos ignorados conocidos

**Dispositivos:**
- `CreateDispositivoDto → Dispositivo`: ignora `DispositivoId`, `FechaCreacion`, navigation properties
- `UpdateDispositivoDto → Dispositivo`: ídem
- `CreateCredencialesAccesoDto → CredencialesAcceso`: ignora `CredencialId`, `FechaCreacion`, `HashPin`, `UltimoUso`

**Reservas:**
- `CreateReservaCommand/UpdateReservaCommand → Reserva`: ignora `EstadoReserva`, `ReservaHuespedes`, timestamps

---

## Sistema de auditoría

`AuditBehavior<TRequest, TResponse>` intercepta todos los commands (nombre termina en `"Command"`):
- Detecta `IsSuccess` por reflection (para `Result<T>`)
- Extrae `EntidadId` del primer campo `int` que termina en `"Id"`
- Publica `AuditEvent` a `audit.events`
- **Nunca lanza excepciones** (swallow para no romper el flujo principal)

Campos del `AuditEvent`: `Servicio`, `UsuarioId`, `Accion`, `TipoEntidad`, `EntidadId`, `DireccionIp`, `AgenteUsuario`, `Resultado`, `MensajeError`, `HotelId`, `FechaHora`

---

## Pendientes conocidos

- **`Reservas.Application/Common/Result.cs`**: agregar `IsNotFound` y `Result<T>.NotFound()` (igual que Dispositivos)
- **Todos los controllers de Reservas**: migrar de `BadRequest(result.ErrorMessage)` a `BadRequest(new { error = result.ErrorMessage })` y usar 404 cuando corresponda
- **`UpdateReservaCommandHandler`**: agregar `catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")`
- **Otros handlers de Reservas**: revisar que no hagan `catch (Exception)` genérico sin capturar DbUpdateException antes
