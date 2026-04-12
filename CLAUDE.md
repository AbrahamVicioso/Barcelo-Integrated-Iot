# CLAUDE.md — Barcelo Integrated IoT

## Instrucción de trabajo

- Leer este archivo completo al inicio de cada sesión.
- **Antes de leer código**, consultar aquí. Solo leer archivos si el patrón no está documentado o hay duda sobre el estado real.
- Leer **solo el archivo a modificar** y los directamente relacionados — nunca explorar el proyecto completo.
- Si se descubre algo no documentado, **agregarlo aquí inmediatamente**.

### Cuándo NO leer archivos
- Nuevo endpoint GetAll → "Estándar de paginación"
- Nuevo command handler → "Manejo de errores en handlers"
- Métodos de repositorio → sección "Repositorios"
- Constraints DB → lista en "DbUpdateException"
- Campos de entidad o DTO → secciones de inventario abajo

### Cuándo SÍ leer archivos (mínimo)
- Modificar código existente → leer solo ese archivo
- Dudar si método existe → leer solo la interfaz
- Nuevo feature en entidad no tocada → leer solo el archivo más cercano

---

## Infraestructura

- **.NET 9** · SQL Server (sqlserver:1433) · Kafka (kafka:9092) · ThingsBoard CE (thingsboard-ce:8080)
- **DB local:** `Data source=localhost;Database=BarceloIoTDatabase;User Id=barcelo;Password=Testing1234;TrustServerCertificate=True`
- **DB Docker:** `Data source=sqlserver;Database=BarceloIoTDatabase;User Id=sa;Password=Testing1234;TrustServerCertificate=True`
- **JWT:** Issuer=`barcelo`, Audience=`BarceloIoT`, Key=`u9Z3fBq7M!8@R2L#A4xCkWmP0EJvH5Ys`

---

## Servicios y puertos

| Servicio | Docker |
|---|---|
| Reservas.API | 5141 |
| Dispositivos.API | 5185 |
| Usuarios.API | 5284/5285 |
| Authenticate.API | 5117 (REST) / 5118 (gRPC) |
| ApiGateway | 5019/5020 |
| Audit.Worker | 5250/5251 |
| Notification.Worker | — |
| ntfy (push) | 8081 (HTTPS) / 8082 (HTTP) |

---

## Estructura del proyecto

```
Barcelo-Integrated-Iot/
├── Reservas/
│   ├── Reservas.API/Controllers/
│   ├── Reservas.Application/
│   │   ├── Common/Result.cs          ← versión antigua, sin NotFound (pendiente)
│   │   ├── DTOs/
│   │   ├── Features/{Entidad}/Commands|Queries/
│   │   ├── Interfaces/
│   │   └── Mappings/MappingProfile.cs
│   ├── Reservas.Domain/Entities/
│   └── Reservas.Persistence/
│       ├── Repositories/
│       └── Services/                 ← CredencialesAccesoService (SQL raw)
│
├── Dispositivos.API/Controllers/
├── Dispositivos.Application/
│   ├── Common/Result.cs              ← versión actualizada con IsNotFound
│   ├── Common/PagedResult.cs
│   ├── Common/PaginationParams.cs
│   ├── Behaviors/AuditBehavior.cs
│   ├── DTOs/
│   ├── Features/{Entidad}/Commands|Queries/
│   ├── Interfaces/
│   └── Mappings/MappingProfile.cs
├── Dispositivos.Domain/Entities/
├── Dispositivos.Infrastructure/
│   └── Kafka/Consumers/              ← BackgroundServices
└── Dispositivos.Persistence/
    ├── Data/Configurations/           ← EF Core configs + constraints
    └── Repositories/
│
├── Usuarios/
│   ├── Usuarios.API/                 ← ExceptionHandlingMiddleware
│   ├── Usuarios.Application/
│   │   ├── Features/{Entidad}/Commands|Queries/
│   │   └── Exceptions/               ← NotFoundException, ConflictException, BusinessException
│   └── Usuarios.Persistence/
│
├── Authenticate/Authentication.Api/
│   ├── Handlers/                     ← LoginUserHandler, RegisterUserHandler (estáticos)
│   └── DTOs/UserManagementDtos.cs
│
├── Notification.Worker/Program.cs    ← todos los BackgroundServices
├── Notification.Domain/Events/
├── Notification.Kafka/
│   ├── Configuration/                ← XxxConsumerConfig : KafkaConsumerConfig
│   └── Services/                     ← XxxEventConsumer (IHostedService)
│
├── Audit.Worker/                     ← consume audit.events → RegistrosAuditorium
└── ApiGateway/                       ← Ocelot
```

### Convención de nombres

| Tipo | Ruta |
|---|---|
| Entidad | `{Svc}.Domain/Entities/{Entidad}.cs` |
| DTO | `{Svc}.Application/DTOs/{Entidad}Dto.cs` |
| Repositorio interfaz | `{Svc}.Application/Interfaces/I{Entidad}Repository.cs` |
| Repositorio impl | `{Svc}.Persistence/Repositories/{Entidad}Repository.cs` |
| Query | `{Svc}.Application/Features/{Entidad}/Queries/Get{Entidad}By{Campo}Query.cs` |
| Command | `{Svc}.Application/Features/{Entidad}/Commands/{Accion}{Entidad}Command.cs` |
| Controller | `{Svc}.API/Controllers/{Entidad}Controller.cs` |
| EF Config | `{Svc}.Persistence/Data/Configurations/{Entidad}Configuration.cs` |

---

## Patrones de arquitectura por servicio

### Dispositivos & Reservas — MediatR + CQRS + `Result<T>`

Handlers devuelven `Result<T>`. **Nunca lanzan excepciones al controller.**

**Dispositivos** (`Dispositivos.Application/Common/Result.cs`) — versión actualizada:
```csharp
Result<T>.Success(data)        // → 200/201
Result<T>.NotFound("msg")      // → 404 (IsNotFound = true)
Result<T>.Failure("msg")       // → 400
```

**Reservas** (`Reservas.Application/Common/Result.cs`) — versión antigua: sin `IsNotFound`/`NotFound()` ← pendiente migrar

### Entidades de catálogo con soft delete (Puesto, Departamento)

- Delete → `EstaActivo = false` + `EliminadoEn = UtcNow` + `UpdateAsync` (NO hard delete)
- GetAll → solo retorna `EstaActivo = true` (via `GetActivosAsync()`)
- No se puede eliminar si tiene Personal asignado → `BusinessException`
- Nombre único → `ConflictException` en Create/Update

### Usuarios — MediatR + CQRS, excepciones

No usa `Result<T>`. Handlers lanzan → `ExceptionHandlingMiddleware` captura:

| Excepción | HTTP |
|---|---|
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `BusinessException` | 400 |
| Cualquier otra | 500 |

Respuesta: `{ "status": 404, "error": "Not Found", "message": "..." }`

### Authenticate — Sin MediatR

Métodos estáticos: `LoginUserHandler`, `RegisterUserHandler`, `CreateUserWithRandomPasswordHandler`.
Auditoría publicada directamente en `AuthController` (no por pipeline).

---

## Inventario de entidades

### Dispositivos.Domain/Entities

**Dispositivo** (PK: Guid DispositivoId)
- HotelId (int), NumeroSerieDispositivo, DireccionMac, TipoDispositivoId (int, FK), Modelo, VersionFirmware
- NivelBateria (int 0-100), EstaEnLinea (bool), UltimaSincronizacion (DateTime?), FechaInstalacion
- EstadoDispositivoId (int, FK), UltimaActualizacionFirmware (DateTime?), Ipdispositivo
- FechaCreacion, FechaActualizacion
- Nav: TipoDispositivo, EstadoDispositivo

**CerradurasInteligente** (PK: int CerraduraId)
- DispositivoId (Guid, FK), HabitacionId (int, UNIQUE), EstadoPuerta, UltimaApertura (DateTime?)
- ContadorAperturas (int), SoportaModoOffline (bool), FechaActivacion, EstaActiva (bool)
- Nav: Dispositivo ← limpiar antes de UpdateAsync si cambia DispositivoId

**CredencialesAcceso** (PK: int CredencialId)
- HuespedId (int?), PersonalId (int?), ReservaId (int?), CodigoPin (max 6, columna 'CodigoPIN')
- HashPin (max 256, columna 'HashPIN', SHA256 Base64), FechaActivacion, FechaExpiracion
- EstaActiva (bool), TipoCredencial (max 30), FechaCreacion, CreadoPor (max 450), NumeroUsos, UltimoUso (DateTime?)

**EstadoDispositivo** (PK: int EstadoDispositivoId) — Descripcion

**TipoDispositivo** (PK: int TipoDispositivoId) — Nombre

**MantenimientoCerradura** (PK: int MantenimientoId)
- DispositivoId (Guid?), CerraduraId (int?, FK), TipoMantenimiento (max 50), FechaProgramada
- FechaRealizada (DateTime?), PersonalId (int?), Estado (max 30, default 'Programado')
- Observaciones (max 1000), CostoMantenimiento (decimal 10,2?), TiempoEmpleadoMinutos (int?), FechaCreacion

**RegistrosAcceso** (PK: long RegistroId)
- CerraduraId (int, FK), CredencialId (int?, FK), UsuarioId, FechaHoraAcceso
- TipoAcceso (max 50), ResultadoAcceso (max 20), MotivoAcceso (max 200), DireccionIp (columna 'DireccionIP', max 50)
- InfoDispositivo (max 500), FueExitoso (bool), CodigoError (max 50), Latencia (int?)

**RegistrosAuditorium** (PK: long AuditoriaId)
- UsuarioId, Accion, TipoEntidad, EntidadId (int?), ValorAnterior, ValorNuevo
- FechaHora, DireccionIp, AgenteUsuario, Resultado, MensajeError, HotelId (int?)

### Reservas.Domain/Entities

**Reserva** (PK: int ReservaId) — HuespedId (int), HabitacionId (int?), NumeroReserva, FechaCheckIn, FechaCheckOut, NumeroHuespedes, NumeroNinos, MontoTotal (decimal), MontoPagado (decimal), EstadoReservaId (int, FK), FechaCreacion, FechaActualizacion, CheckInRealizado (DateTime?), CheckOutRealizado (DateTime?), CreadoPor, ModificadoPor, Observaciones — Nav: EstadoReserva, ReservaHuespedes (1:M)

**Habitacion** (PK: int HabitacionId) — HotelId (int), NumeroHabitacion, TipoHabitacionId (int, FK), Piso (int), CapacidadMaxima (int), PrecioPorNoche (decimal), EstadoHabitacionId (int, FK), Descripcion, FechaCreacion, FechaActualizacion

**Hotel** (PK: int HotelId) — Nombre, Direccion, Ciudad, Pais, Telefono, Email, EstaActivo (bool), FechaCreacion, NumeroHabitaciones, NumeroEstrellas

**ReservaHuesped** (PK compuesto: ReservaId + HuespedId) — PuedeCrearActividadesRecreativas (bool), PuedeDesbloquearCerradura (bool), FechaAgregado

**EstadoReserva** (PK: int EstadoReservaId) — Nombre

**EstadoHabitacion** (PK: int EstadoHabitacionId) — Nombre

**TipoHabitacion** (PK: int TipoHabitacionId) — Nombre

**ActividadesRecreativas** (PK: int ActividadId) — Nombre, Descripcion, Precio (decimal), Duracion

**ReservasActividades** (PK compuesto: ReservaId + ActividadId)

**CheckIn** (PK: int CheckInId) — ReservaId (int, FK), FechaHoraCheckIn, Notas

**CheckOut** (PK: int CheckOutId) — ReservaId (int, FK), FechaHoraCheckOut, Notas

---

## DTOs — Dispositivos

| DTO | Campos clave |
|---|---|
| **DispositivoDto** | DispositivoId, HotelId, NombreHotel, NumeroSerieDispositivo, DireccionMac, TipoDispositivoId, NombreTipo, Modelo, VersionFirmware, NivelBateria, EstaEnLinea, UltimaSincronizacion, FechaInstalacion, EstadoDispositivoId, DescripcionEstado, UltimaActualizacionFirmware, Ipdispositivo, FechaCreacion, FechaActualizacion |
| **CreateDispositivoDto** | HotelId, NumeroSerieDispositivo, DireccionMac, TipoDispositivoId (def 1), Modelo, VersionFirmware, NivelBateria, EstaEnLinea, UltimaSincronizacion, FechaInstalacion, EstadoDispositivoId (def 1), UltimaActualizacionFirmware, Ipdispositivo |
| **UpdateDispositivoDto** | DispositivoId + todos los campos de Create |
| **CerradurasInteligenteDto** | CerraduraId, DispositivoId, NombreDispositivo, HabitacionId, NombreHabitacion, EstadoPuerta, UltimaApertura, ContadorAperturas, SoportaModoOffline, FechaActivacion, EstaActiva |
| **CreateCerradurasInteligenteDto** | DispositivoId, HabitacionId, EstadoPuerta, UltimaApertura, ContadorAperturas, SoportaModoOffline, FechaActivacion, EstaActiva |
| **UpdateCerradurasInteligenteDto** | CerraduraId + campos de Create |
| **CredencialesAccesoDto** | CredencialId, HuespedId, PersonalId, ReservaId, CodigoPin, FechaActivacion, FechaExpiracion, EstaActiva, TipoCredencial, FechaCreacion, CreadoPor, NumeroUsos, UltimoUso |
| **CreateCredencialesAccesoDto** | HuespedId, PersonalId, ReservaId, CodigoPin, HashPIN, FechaActivacion, FechaExpiracion, EstaActiva, TipoCredencial, NumeroUsos |
| **UpdateCredencialesAccesoDto** | CredencialId, HuespedId, PersonalId, ReservaId, CodigoPin, FechaActivacion, FechaExpiracion, EstaActiva, TipoCredencial, NumeroUsos |
| **MantenimientoCerraduraDto** | MantenimientoId, DispositivoId, CerraduraId, TipoMantenimiento, FechaProgramada, FechaRealizada, PersonalId, Estado, Observaciones, CostoMantenimiento, TiempoEmpleadoMinutos, FechaCreacion |
| **CreateMantenimientoCerraduraDto** | DispositivoId, CerraduraId, TipoMantenimiento, FechaProgramada, FechaRealizada, PersonalId, Estado, Observaciones, CostoMantenimiento, TiempoEmpleadoMinutos |
| **UpdateMantenimientoCerraduraDto** | MantenimientoId + campos de Create |
| **RegistrosAccesoDto** | RegistroId, CerraduraId, CredencialId, UsuarioId, FechaHoraAcceso, TipoAcceso, ResultadoAcceso, MotivoAcceso, DireccionIp, InfoDispositivo, FueExitoso, CodigoError, Latencia |
| **CreateRegistrosAccesoDto** | CerraduraId, CredencialId, UsuarioId, FechaHoraAcceso, TipoAcceso, ResultadoAcceso, MotivoAcceso, DireccionIp, InfoDispositivo, FueExitoso, CodigoError, Latencia |
| **RegistrosAuditoriumDto** | AuditoriaId, UsuarioId, Accion, TipoEntidad, EntidadId, ValorAnterior, ValorNuevo, FechaHora, DireccionIp, AgenteUsuario, Resultado, MensajeError, HotelId |
| **CreateRegistrosAuditoriumDto** | UsuarioId, Accion, TipoEntidad, EntidadId, ValorAnterior, ValorNuevo, FechaHora, DireccionIp, AgenteUsuario, Resultado, MensajeError, HotelId |
| **EstadoDispositivoDto** | EstadoDispositivoId, Descripcion |
| **CreateEstadoDispositivoDto** | Descripcion |
| **TipoDispositivoDto** | TipoDispositivoId, Nombre |
| **CreateTipoDispositivoDto** | Nombre |

---

## Repositorios — métodos completos

### `IDispositivoRepository`
```
GetAll() · GetById(Guid) · GetByHotelId(int) · GetByTipoDispositivo(int) · GetByEstaEnLinea(bool)
GetByNumeroSerie(string) · GetByDireccionMAC(string) · GetByIpDispositivo(string)
AddAsync · UpdateAsync · DeleteAsync
```

### `ICerradurasInteligenteRepository`
```
GetAll() · GetById(int) · GetByDispositivoId(Guid) · GetByHabitacionId(int) · GetByEstaActiva(bool)
AddAsync · UpdateAsync · DeleteAsync
```

### `ICredencialesAccesoRepository`
```
GetAll() · GetById(int) · GetByHuespedId(int) · GetByPersonalId(int) · GetByEstaActiva(bool)
GetByTipoCredencial(string) · GetByCodigoPin(string)
AddAsync · UpdateAsync · DeleteAsync
```

### `IMantenimientoCerraduraRepository`
```
GetAll() · GetById(int) · GetByCerraduraId(int) · GetByDispositivoId(Guid) · GetByEstado(string) · GetByPersonalId(int)
AddAsync · UpdateAsync · DeleteAsync
```

### `IRegistrosAccesoRepository`
```
GetAllAsync() · GetByIdAsync(int) · GetByCerraduraIdAsync(int) · GetByUsuarioIdAsync(string) · GetByFueExitosoAsync(bool)
AddAsync · UpdateAsync · DeleteAsync(int)
```

### `IRegistrosAuditoriumRepository`
```
GetAllAsync() · GetByIdAsync(int) · GetByUsuarioIdAsync(string) · GetByTipoEntidadAsync(string) · GetByHotelIdAsync(int)
AddAsync · UpdateAsync · DeleteAsync(int)
```

### `IEstadoDispositivoRepository` / `ITipoDispositivoRepository`
```
GetAll() · GetById(int) · AddAsync · UpdateAsync · DeleteAsync
```

### `IUnitOfWork` (Dispositivos)
```
Dispositivos · CerradurasInteligente · CredencialesAcceso · MantenimientoCerradura
RegistrosAcceso · RegistrosAuditorium · EstadosDispositivo · TiposDispositivo
SaveChangesAsync · BeginTransactionAsync · CommitTransactionAsync · RollbackTransactionAsync
```

### `IReservaRepository` (Reservas)
```
GetByIdAsync · GetAllAsync · FindAsync · AddAsync · UpdateAsync · DeleteAsync · ExistsAsync
GetReservasByHuespedIdAsync · GetByNumeroReservaAsync · GetReservasByEstadoAsync
GetReservasByFechaRangoAsync · IsHabitacionOcupadaAsync(habitacionId, checkIn, checkOut, excludeReservaId?)
```

### `ITbDeviceService` (ThingsBoard)
```
GetDeviceByNameAsync(string name) → DeviceDto
SetSharedAttributesAsync(string deviceId, Dictionary<string, object> attrs, CancellationToken)
```

---

## Handlers/Queries existentes — Dispositivos

| Entidad | Queries | Commands |
|---|---|---|
| Dispositivo | GetAll (paginado), GetById, GetByHotelId | Create, Update, Delete |
| CerradurasInteligente | GetAll (paginado), GetById | Create, Update, Delete |
| CredencialesAcceso | GetAll (paginado), GetById, GetByHuespedId (paginado) | Create, Update, Delete |
| MantenimientoCerradura | GetAll (paginado), GetById | Create, Update, Delete |
| RegistrosAcceso | GetAll (paginado), GetById | Create, Delete |
| RegistrosAuditorium | GetAll (paginado), GetById | Create, Delete |
| EstadosDispositivo | GetAll (paginado), GetById | Create, Update, Delete |
| TiposDispositivo | GetAll (paginado), GetById | Create, Update, Delete |

---

## Endpoints — Dispositivos.API

| Controller | Endpoints |
|---|---|
| **DispositivoController** | GET /dispositivo (paginado) · GET /dispositivo/{id} · GET /dispositivo/hotel/{hotelId} · POST /dispositivo → 201 · PUT /dispositivo/{id} · DELETE /dispositivo/{id} |
| **CerradurasInteligenteController** | GET /cerradurasinteligente (paginado) · GET /cerradurasinteligente/{id} · POST → 201 · PUT /{id} · DELETE /{id} |
| **CredencialesAccesoController** | GET /credencialesacceso (paginado) · GET /credencialesacceso/huesped/{huespedId} (paginado) · GET /credencialesacceso/{id} · POST → 201 · PUT /{id} · DELETE /{id} |
| **MantenimientoCerraduraController** | GET /mantenimientocerradura (paginado) · GET /{id} · POST → 201 · PUT /{id} · DELETE /{id} |
| **RegistrosAccesoController** | GET /registrosacceso (paginado) · GET /{id} · POST → 201 · DELETE /{id} |
| **RegistrosAuditoriumController** | GET /registrosauditorium (paginado) · GET /{id} · POST → 201 · DELETE /{id} |
| **EstadosDispositivoController** | GET /estadosdispositivo (paginado) · GET /{id} · POST → 201 · PUT /{id} · DELETE /{id} |
| **TiposDispositivoController** | GET /tiposdispositivo (paginado) · GET /{id} · POST → 201 · PUT /{id} · DELETE /{id} |

## Endpoints — Authenticate.API

| Verbo | Ruta | Descripción |
|---|---|---|
| POST | /login | LoginRequest → AccessTokenResponse + JWT |
| POST | /register | RegisterRequest → crea usuario + publica `UserCreatedEvent` a Kafka |
| POST | /create | EmailRequest → crea usuario con contraseña aleatoria |
| GET | /info | [Authorize] → InfoResponse |
| GET | /getuserbyemail?email= | → UserInfoResponse |
| GET | /confirmemail?userId=&token= | confirma email |

## Endpoints — Reservas.API (principales)

| Verbo | Ruta | Descripción |
|---|---|---|
| POST | /reservas/{id}/unlock-door?pin= | → UnlockDoorCommand |
| POST | /reservas/checkin | → PerformCheckInCommand |
| CRUD | /reservas | Reservas |
| CRUD | /habitacion | Habitaciones |
| POST | /habitacion/{habitacionId}/unlock | [Authorize] → UnlockDoorPersonalCommand (personal JWT) |
| CRUD | /hotel | Hoteles |

---

## Estándar de paginación

Aplicar en **todos los GetAll** de Dispositivos.API (ya implementado en todos los endpoints actuales).

### Clases en `Dispositivos.Application/Common/`

```csharp
// PaginationParams — [FromQuery] en controller
public class PaginationParams
{
    private int _page = 1, _pageSize = 20;
    public int Page     { get => _page;     set => _page     = value < 1 ? 1 : value; }
    public int PageSize { get => _pageSize; set => _pageSize = value < 1 ? 20 : value > 100 ? 100 : value; }
}

// PagedResult<T>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages       => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage     => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

### Plantilla completa (Query + Handler + Controller)

```csharp
// Query
public class GetAllXxxQuery : IRequest<Result<PagedResult<XxxDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// Handler
public async Task<Result<PagedResult<XxxDto>>> Handle(GetAllXxxQuery request, CancellationToken ct)
{
    var todos = await _repo.GetAll();
    var dtos = _mapper.Map<IEnumerable<XxxDto>>(todos).ToList();
    var items = dtos.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
    return Result<PagedResult<XxxDto>>.Success(new PagedResult<XxxDto>(items, request.Page, request.PageSize, dtos.Count));
}

// Controller
[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
{
    var result = await _mediator.Send(new GetAllXxxQuery { Page = pagination.Page, PageSize = pagination.PageSize });
    if (!result.IsSuccess)
        return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
    return Ok(result.Data);
}
```

Respuesta: `{ items, totalCount, page, pageSize, totalPages, hasNextPage, hasPreviousPage }`

---

## Regla obligatoria: manejo de errores en handlers

### 1. Validar antes de tocar DB

```csharp
// FK — verificar que existe
var dispositivo = await _unitOfWork.Dispositivos.GetById(request.DispositivoId);
if (dispositivo == null) return Result<T>.Failure($"Dispositivo '{request.DispositivoId}' no encontrado.");

// UNIQUE — verificar duplicado (insert)
var existente = await _repo.GetByHabitacionId(request.HabitacionId);
if (existente.Any()) return Result<T>.Failure($"La habitación {request.HabitacionId} ya tiene cerradura.");

// UNIQUE — update (excluir propio)
if (existente.Any(c => c.Id != request.Id)) return Result<T>.Failure("...");
```

### 2. Capturar DbUpdateException (sin EF Core en Application)

```csharp
catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
{
    var inner = ex.InnerException?.Message ?? ex.Message;
    if (inner.Contains("UQ_Cerraduras_Habitacion"))       return Result<T>.Failure("La habitación ya tiene cerradura.");
    if (inner.Contains("FK_Cerraduras_Dispositivos"))     return Result<T>.Failure("Dispositivo no encontrado.");
    if (inner.Contains("UQ_Dispositivos_NumeroSerie"))    return Result<T>.Failure("Número de serie ya registrado.");
    if (inner.Contains("UQ_Dispositivos_MAC"))            return Result<T>.Failure("MAC ya registrada.");
    if (inner.Contains("UQ_Dispositivos_IP"))             return Result<T>.Failure("IP ya registrada.");
    if (inner.Contains("FK_Dispositivos_TiposDispositivo")) return Result<T>.Failure("Tipo de dispositivo no existe.");
    if (inner.Contains("FK_Dispositivos_EstadosDispositivo")) return Result<T>.Failure("Estado de dispositivo no existe.");
    if (inner.Contains("FK_Dispositivos_Hoteles"))        return Result<T>.Failure("Hotel no encontrado.");
    if (inner.Contains("CHK_Dispositivos_Bateria"))       return Result<T>.Failure("NivelBateria debe ser 0-100.");
    if (inner.Contains("CHK_Credenciales_Fechas"))        return Result<T>.Failure("FechaExpiracion debe ser posterior a FechaActivacion.");
    return Result<T>.Failure($"Error de base de datos: {inner}");
}
catch (Exception ex) { return Result<T>.Failure($"Error inesperado: {ex.Message}"); }
```

### Constraints DB conocidos

| Constraint | Regla |
|---|---|
| `UQ_Cerraduras_Habitacion` | Una sola cerradura por habitación |
| `FK_Cerraduras_Dispositivos` | DispositivoId → Dispositivos |
| `UQ_Dispositivos_NumeroSerie` | Serie única |
| `UQ_Dispositivos_MAC` | MAC única. Campo: `DireccionMac` (no `DireccionMAC`) |
| `UQ_Dispositivos_IP` | IP única (filtered, permite NULL múltiple) |
| `FK_Dispositivos_TiposDispositivo` | TipoDispositivoId → TiposDispositivo |
| `FK_Dispositivos_EstadosDispositivo` | EstadoDispositivoId → EstadosDispositivo |
| `FK_Dispositivos_Hoteles` | HotelId → Hoteles (sin repo propio) |
| `CHK_Dispositivos_Bateria` | NivelBateria 0-100 |
| `CHK_Credenciales_Fechas` | FechaExpiracion > FechaActivacion |

### 3. NotFound para entidades no encontradas

```csharp
if (entidad == null) return Result<T>.NotFound($"Xxx con ID {id} no encontrado.");
```

---

## Regla obligatoria: respuesta HTTP en controllers

```csharp
// ✅ Correcto
if (!result.IsSuccess)
    return result.IsNotFound
        ? NotFound(new { error = result.ErrorMessage })
        : BadRequest(new { error = result.ErrorMessage });
return Ok(result.Data);            // GET/PUT
return CreatedAtAction(...);       // POST
```

| Situación | Código |
|---|---|
| GET / PUT exitoso | `200 Ok` |
| POST exitoso | `201 CreatedAtAction` |
| DELETE exitoso | `200 Ok` o `204 NoContent` |
| No encontrado | `404 NotFound` |
| Validación | `400 BadRequest` |
| ID ruta ≠ body | `400 BadRequest` |

---

## EF Core — AsNoTracking + navigation properties

Todos los repositorios usan `.AsNoTracking()`. Al **update** con FK + navigation property cargada, EF recalcula la FK desde la nav ignorando el nuevo valor.

**Solución: limpiar la navigation property antes de UpdateAsync:**
```csharp
_mapper.Map(request.Dto, entidad);
entidad.Dispositivo = null;   // ← obligatorio si cambia DispositivoId en CerradurasInteligente
await _repo.UpdateAsync(entidad, cancellationToken);
```

---

## AutoMapper — campos ignorados

**Dispositivos:**
- `CreateDispositivoDto → Dispositivo`: ignora `DispositivoId`, `FechaCreacion`, navigation props
- `UpdateDispositivoDto → Dispositivo`: ídem
- `CreateCredencialesAccesoDto → CredencialesAcceso`: ignora `CredencialId`, `FechaCreacion`, `HashPin`, `UltimoUso`
- `CreateCerradurasInteligenteDto → CerradurasInteligente`: ignora `CerraduraId`, navigation props

**Reservas:**
- `CreateReservaCommand/UpdateReservaCommand → Reserva`: ignora `EstadoReserva`, `ReservaHuespedes`, timestamps, `CreadoPor`, `ModificadoPor`

---

## Kafka — topics y flujos

| Topic | Productor | Consumidor | Evento |
|---|---|---|---|
| `users` | Authenticate.API | Notification.Worker (`UserCreatedEventConsumer`) | `UserCreatedEvent` |
| `email-confirmation` | Authenticate.API | Notification.Worker (`EmailConfirmationEventConsumer`) | `EmailConfirmationEvent` |
| `reservas` | Reservas.API | Notification.Worker (`ReservaCreadaEventConsumer`) | `ReservaCreadaEvent` |
| `dispositivos.unlock-door` | Reservas.API | Dispositivos.Infrastructure (`UnlockDoorKafkaConsumer`) | `UnlockDoorEvent` |
| `reservas.checkin-realizado` | Reservas.API | Dispositivos.Infrastructure (`CheckInRealizadoKafkaConsumer`) | `CheckInRealizadoEvent` |
| `checkin.credenciales` | Dispositivos.Infrastructure | Notification.Worker (`CredencialesCheckInEventConsumer`) | `CredencialesCheckInEvent` |
| `habitacion.personal-unlock` | Reservas.API | Dispositivos.Infrastructure (`PersonalUnlockDoorKafkaConsumer`) | `PersonalUnlockDoorEvent` |
| `habitacion.personal-acceso` | Dispositivos.Infrastructure | Notification.Worker (`PersonalAccesoHabitacionEventConsumer`) | `PersonalAccesoHabitacionEvent` |
| `audit.events` | Todos los APIs | Audit.Worker | `AuditEvent` |

### Flujo unlock-door (huésped con PIN)

1. `POST /reservas/{id}/unlock-door?pin=` → `UnlockDoorCommand`
2. Handler: valida reserva activa + habitación + PIN → `RegistrarUsoAsync(credencialId)` → publica `UnlockDoorEvent`
3. `UnlockDoorKafkaConsumer`: busca cerradura activa por HabitacionId → ThingsBoard `lockState = "unlocked"` → crea `RegistrosAcceso`

### Flujo unlock-door personal (`POST /habitacion/{habitacionId}/unlock`)

1. `[Authorize]` — obtiene `UsuarioId` del JWT (`ClaimTypes.NameIdentifier` o `"nameid"`)
2. `UnlockDoorPersonalCommandHandler`:
   - Busca `Personal` por `UsuarioId` → `ICredencialesAccesoService.GetPersonalByUsuarioIdAsync` (raw SQL `Personal WHERE UsuarioId=... AND EstaActivo=1`)
   - Valida permiso activo → `PersonalTienePermisoAsync` (raw SQL `PermisosPersonal WHERE PersonalId AND HabitacionId AND EstaActivo AND FechaExpiracion`)
   - Valida cerradura activa → `HabitacionTieneCerraduraActivaAsync`
   - Busca reserva activa → `GetReservaActivaByHabitacionIdAsync` (`Reservas WHERE HabitacionId AND EstadoReservaId=2`)
   - Si hay reserva: obtiene emails de todos los huéspedes vía `IUsuariosApiService` (principal + `ReservaHuespedes`)
   - Publica `PersonalUnlockDoorEvent { HabitacionId, NumeroHabitacion, PersonalId, NombrePersonal, UsuarioId, DireccionIp, InfoDispositivo, Huespedes }` → topic `habitacion.personal-unlock`
3. `PersonalUnlockDoorKafkaConsumer` (Dispositivos.Infrastructure, BackgroundService):
   - Busca cerradura activa por HabitacionId
   - **ThingsBoard es no-bloqueante**: intenta `lockState = "unlocked"`, si falla solo loguea error y continúa
   - Crea `RegistrosAcceso` (TipoAcceso = "Personal")
   - Publica `PersonalAccesoHabitacionEvent` a `habitacion.personal-acceso` (si `Huespedes.Count > 0`)
4. `PersonalAccesoHabitacionEventConsumer` (Notification.Worker): por huésped con email → email HTML + push notification (alerta de acceso del personal)

### Flujo check-in + credenciales + email

1. `POST /reservas/checkin` → `PerformCheckInCommand`
2. Handler: valida reserva → obtiene email+nombre de **todos** huéspedes vía `IUsuariosApiService` → publica `CheckInRealizadoEvent { Huespedes: [{HuespedId, Email, NombreCompleto}] }`
3. `CheckInRealizadoKafkaConsumer`: por huésped genera PIN (`RandomNumberGenerator.GetInt32(100000, 1000000)`) → crea `CredencialesAcceso` → publica `CredencialesCheckInEvent { Credenciales: [{Email, NombreCompleto, CodigoPin}] }`
4. `CredencialesCheckInEventConsumer`: por huésped con email → envía email HTML con PIN + push notification

### Flujo registro de usuario

1. `POST /register` → `RegisterUserHandler` → crea usuario Identity → publica `UserCreatedEvent { UserId, Email, NombreCompleto }`
2. `UserCreatedEventConsumer`: email bienvenida + push notification

---

## Kafka — patrón nuevo flujo de notificación

**Paso 1** — Evento en `Notification.Domain/Events/`:
```csharp
public class MiNuevoEvent { public Guid Id { get; set; } = Guid.NewGuid(); /* campos */ public DateTime CreatedAt { get; set; } = DateTime.UtcNow; }
```

**Paso 2** — Config en `Notification.Kafka/Configuration/`:
```csharp
public class MiNuevoConsumerConfig : KafkaConsumerConfig
{
    public MiNuevoConsumerConfig() { GroupId = "notification-mi-nuevo-group"; Topic = "mi.nuevo.topic"; }
}
```

**Paso 3** — Consumer en `Notification.Kafka/Services/` — seguir patrón de `CredencialesCheckInEventConsumer`:
- Constructor: Config, IEmailService, IPushNotificationService, ILogger
- `StartAsync` → `EnsureTopicExists()` → `_consumer.Subscribe` → `Task.Run(ConsumeMessages)`
- `ConsumeMessages` → loop → `ProcessMessageAsync` → deserializa JSON → lógica
- `StopAsync` → cancela + `_consumer.Close()`
- `Dispose` → libera consumer, adminClient, cts

**Paso 4** — `Notification.Worker/Program.cs`:
```csharp
var miConfig = new MiNuevoConsumerConfig();
context.Configuration.GetSection("KafkaConsumer:MiNuevo").Bind(miConfig);
services.AddSingleton(miConfig);
services.AddSingleton<MiNuevoEventConsumer>();
services.AddHostedService<MiNuevoNotificationWorker>();
// + BackgroundService al final del archivo igual que los demás workers
```

**Paso 5** — `docker-compose.yml` (OBLIGATORIO — sin esto usa localhost:9092 y falla):
```yaml
# en notification-worker > environment:
KafkaConsumer__MiNuevo__BootstrapServers: kafka:29092
KafkaConsumer__MiNuevo__GroupId: notification-mi-nuevo-group
KafkaConsumer__MiNuevo__Topic: mi.nuevo.topic
```

**Paso 6** — `appsettings.json` de Notification.Worker:
```json
"KafkaConsumer": { "MiNuevo": { "BootstrapServers": "localhost:9092", "GroupId": "...", "Topic": "...", "AutoOffsetReset": "Earliest", "EnableAutoCommit": true, "AutoCommitIntervalMs": 5000, "SessionTimeoutMs": 30000, "MaxPollIntervalMs": 300000 } }
```

**Si el productor es un BackgroundService en Dispositivos.Infrastructure:**
```csharp
_producer = new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = _config.BootstrapServers, Acks = Acks.Leader }).Build();
// Dispose: _producer?.Flush(TimeSpan.FromSeconds(5)); _producer?.Dispose();
// El topic destino como campo extra en la config del consumer
public string MiTopicProducerTopic { get; set; } = "mi.nuevo.topic";
```

> ⚠️ Docker: red interna `kafka:29092`. Host: `localhost:9092`. El docker-compose sobreescribe appsettings.

---

## Credenciales de acceso

- **PIN**: `RandomNumberGenerator.GetInt32(100000, 1000000).ToString()` (6 dígitos)
- **Hash**: SHA-256 en Base64 → guardado en `HashPin`
- **Columnas DB**: CodigoPin → `CodigoPIN`, HashPin → `HashPIN`
- **Validación unlock-door**: `ICredencialesAccesoService.GetCredencialIdAsync(reservaId, pin)` — compara `CodigoPIN` directo (SQL raw)
- **Registro de uso**: `ICredencialesAccesoService.RegistrarUsoAsync(credencialId)` → `NumeroUsos + 1`, `UltimoUso = NOW`
- **Constraint**: `CHK_Credenciales_Fechas` — validar `FechaExpiracion > FechaActivacion` antes de insertar

---

## ThingsBoard

```
GET  /api/tenant/devices?deviceName={name}              → buscar device por nombre
POST /api/plugins/telemetry/DEVICE/{deviceId}/SHARED_SCOPE → setear atributos compartidos
```

- Nombre del device = `CerradurasInteligente.DispositivoId.ToString()`
- Auth: JWT de tenant cacheado 60 min
- `POST /api/device` sin ID crea device; si ya existe → 400 "already exists" → usar `GetDeviceByNameAsync` (idempotente)
- **No usar** `/api/device?name=...` (incorrecto)

---

## Sistema de auditoría

`AuditBehavior<TRequest, TResponse>` (MediatR pipeline) intercepta todos los Commands:
- Detecta `IsSuccess` por reflection
- Extrae `EntidadId` del primer campo `int` que termina en `"Id"`
- Publica `AuditEvent` a `audit.events`
- **Nunca lanza excepciones** (swallow)

`AuditEvent`: `Servicio, UsuarioId, Accion, TipoEntidad, EntidadId, DireccionIp, AgenteUsuario, Resultado, MensajeError, HotelId, FechaHora`

Authenticate.API: auditoría manual en `AuthController` (no por pipeline).

---

## Notification.Worker — consumers registrados

| Consumer | Topic | Acción |
|---|---|---|
| `UserCreatedEventConsumer` | `users` | Email bienvenida + push notification |
| `ReservaCreadaEventConsumer` | `reservas` | Email confirmación reserva |
| `EmailConfirmationEventConsumer` | `email-confirmation` | Email con link de confirmación |
| `CredencialesCheckInEventConsumer` | `checkin.credenciales` | Email HTML con PIN + push notification |
| `PersonalAccesoHabitacionEventConsumer` | `habitacion.personal-acceso` | Email HTML + push de alerta de acceso del personal a todos los huéspedes |

Email HTML de credenciales: diseño profesional, PIN monospace destacado, NumeroReserva, fechas CheckIn/CheckOut, advertencia no compartir.

Servicios: `IEmailService` (Azure Communication Services), `IPushNotificationService` (ntfy.sh)

---

## Campos de auditoría automáticos (del JWT)

| Tabla | Campo | Cuándo | Servicio |
|---|---|---|---|
| `CredencialesAcceso` | `CreadoPor` | Create | Dispositivos.API — `IHttpContextAccessor`, claim `"nameid" ?? ClaimTypes.NameIdentifier` |
| `Reservas` | `CreadoPor` | Create | Reservas.API — ídem |
| `Reservas` | `ModificadoPor` | Update | Reservas.API — ídem |
| `PermisosPersonal` | `OtorgadoPor` | Create | Usuarios.API — ídem |

- `FechaCreacion` → `DateTime.UtcNow` en todos los Create handlers (no viene del cliente, Ignored en mapping)
- `FechaActualizacion` → `DateTime.UtcNow` en todos los Update handlers con ese campo
- `DELETE /reservas/{id}` → **cancela** (EstadoReservaId = 4), no borra el registro

---

## Pendientes conocidos

- **Controllers de Reservas**: migrar de `BadRequest(result.ErrorMessage)` → `BadRequest(new { error = result.ErrorMessage })` + usar 404 cuando corresponda
- **`UpdateReservaCommandHandler`**: agregar `catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")`
- **Otros handlers de Reservas**: verificar que capturen DbUpdateException antes de Exception genérico

## Implementado — Personal unlock door

- `Reservas.Application/Common/Result.cs`: ya tiene `IsNotFound` + `Result<T>.NotFound()` ✓
- `ICredencialesAccesoService` (interfaz + implementación): métodos `PersonalTienePermisoAsync`, `GetReservaActivaByHabitacionIdAsync`, `GetPersonalNombreAsync`, `GetPersonalByUsuarioIdAsync` ✓
- `UnlockDoorPersonalCommand` + `UnlockDoorPersonalCommandHandler` (Reservas.Application) ✓
- `PersonalUnlockDoorEvent` + `PersonalAccesoHabitacionEvent` (Notification.Domain/Events) ✓
- `PersonalUnlockDoorKafkaConsumer` (Dispositivos.Infrastructure, BackgroundService) ✓
- `PersonalAccesoHabitacionEventConsumer` + `PersonalAccesoHabitacionConsumerConfig` (Notification.Kafka) ✓
- `POST /habitacion/{habitacionId}/unlock` en `HabitacionController` `[Authorize]` ✓

### Regla: ThingsBoard es no-bloqueante en PersonalUnlockDoorKafkaConsumer
El consumer siempre ejecuta `RegistrarAccesoAsync` y `PublicarPersonalAccesoEventAsync` independientemente de si ThingsBoard está disponible. El intento de desbloqueo ThingsBoard va en try/catch que solo loguea. Esto garantiza que el registro y la notificación a huéspedes siempre ocurran.

### Regla: PersonalId viene del JWT, nunca de query param
`UnlockDoorPersonalCommandHandler` extrae `UsuarioId` del JWT → busca en `Personal` tabla via `GetPersonalByUsuarioIdAsync` → obtiene `(PersonalId, NombreCompleto)`.
