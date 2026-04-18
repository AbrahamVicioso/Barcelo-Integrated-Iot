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

## Convención de nombres

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

Servicios extra: `Reservas.Application/Interfaces/` ← `ICredencialesAccesoService`, `IUsuariosApiService` · impl en `Reservas.Persistence/Services/` · `Usuarios.API/Services/` ← `AuditKafkaProducer`, `PermisoHabitacionKafkaProducer` · `Dispositivos.Infrastructure/Services/` ← `TbDeviceService`, `TbCredencialesSyncService`, BackgroundServices (Kafka consumers)

---

## Patrones de arquitectura por servicio

### Dispositivos & Reservas — MediatR + CQRS + `Result<T>`

Handlers devuelven `Result<T>`. **Nunca lanzan excepciones al controller.**
Ambos servicios tienen `*/Common/Result.cs` con versión actualizada:
```csharp
Result<T>.Success(data)        // → 200/201
Result<T>.NotFound("msg")      // → 404 (IsNotFound = true)
Result<T>.Failure("msg")       // → 400
```

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

Métodos estáticos: `LoginUserHandler`, `RegisterUserHandler`, `CreateUserWithRandomPasswordHandler`, `ForgotPasswordHandler`.
Auditoría publicada directamente en `AuthController` (no por pipeline).

**Login:** usa `FindByEmailAsync` + `CheckPasswordSignInAsync` — siempre por email, nunca por username.
**Password reset:** token generado con `GeneratePasswordResetTokenAsync`, codificado en Base64Url. Link enviado por email (Kafka → `PasswordResetEventConsumer`). Reset con `ResetPasswordAsync`.
**Token de reset:** nunca se devuelve en la respuesta HTTP — solo llega por email.

---

## Inventario de entidades

### Dispositivos.Domain/Entities

**Dispositivo** (PK: Guid DispositivoId)
- HotelId (int), NumeroSerieDispositivo, DireccionMac, TipoDispositivoId (int, FK), Modelo, VersionFirmware
- NivelBateria (int 0-100), EstaEnLinea (bool), UltimaSincronizacion (DateTime?), FechaInstalacion
- EstadoDispositivoId (int, FK), UltimaActualizacionFirmware (DateTime?), Ipdispositivo
- FechaCreacion, FechaActualizacion · Nav: TipoDispositivo, EstadoDispositivo

**CerradurasInteligente** (PK: int CerraduraId)
- DispositivoId (Guid, FK), HabitacionId (int, UNIQUE), EstadoPuerta, UltimaApertura (DateTime?)
- ContadorAperturas (int), SoportaModoOffline (bool), FechaActivacion, EstaActiva (bool)
- Nav: Dispositivo ← **limpiar antes de UpdateAsync si cambia DispositivoId**

**CredencialesAcceso** (PK: int CredencialId)
- HuespedId (int?), PersonalId (int?), ReservaId (int?), CodigoPin (max 6, columna `CodigoPIN`)
- HashPin (max 256, columna `HashPIN`, SHA256 Base64), FechaActivacion, FechaExpiracion
- EstaActiva (bool), TipoCredencial (max 30), FechaCreacion, CreadoPor (max 450), NumeroUsos, UltimoUso (DateTime?)

**EstadoDispositivo** (PK: int EstadoDispositivoId) — Descripcion

**TipoDispositivo** (PK: int TipoDispositivoId) — Nombre

**MantenimientoCerradura** (PK: int MantenimientoId)
- DispositivoId (Guid?), CerraduraId (int?, FK), TipoMantenimiento (max 50), FechaProgramada
- FechaRealizada (DateTime?), PersonalId (int?), Estado (max 30, default 'Programado')
- Observaciones (max 1000), CostoMantenimiento (decimal 10,2?), TiempoEmpleadoMinutos (int?), FechaCreacion

**RegistrosAcceso** (PK: long RegistroId)
- CerraduraId (int, FK), CredencialId (int?, FK), UsuarioId, FechaHoraAcceso
- TipoAcceso (max 50), ResultadoAcceso (max 20), MotivoAcceso (max 200), DireccionIp (columna `DireccionIP`, max 50)
- InfoDispositivo (max 500), FueExitoso (bool), CodigoError (max 50), Latencia (int?)

**RegistrosAuditorium** (PK: long AuditoriaId)
- UsuarioId, Accion, TipoEntidad, EntidadId (int?), ValorAnterior, ValorNuevo
- FechaHora, DireccionIp, AgenteUsuario, Resultado, MensajeError, HotelId (int?)

### Reservas.Domain/Entities

**Reserva** (PK: int ReservaId) — HuespedId (int), HabitacionId (int?), NumeroReserva, FechaCheckIn, FechaCheckOut, NumeroHuespedes, NumeroNinos, MontoTotal (decimal), MontoPagado (decimal), EstadoReservaId (int, FK), FechaCreacion, FechaActualizacion, CheckInRealizado (DateTime?), CheckOutRealizado (DateTime?), CreadoPor, ModificadoPor, Observaciones — Nav: EstadoReserva, ReservaHuespedes (1:M)

**Habitacion** (PK: int HabitacionId) — HotelId (int), NumeroHabitacion, TipoHabitacionId (int, FK), Piso (int), CapacidadMaxima (int), PrecioPorNoche (decimal), EstadoHabitacionId (int, FK), Descripcion, FechaCreacion, FechaActualizacion

**Hotel** (PK: int HotelId) — Nombre, Direccion, Ciudad, Pais, Telefono, Email, EstaActivo (bool), FechaCreacion, NumeroHabitaciones, NumeroEstrellas

**ReservaHuesped** (PK: ReservaId + HuespedId) — PuedeCrearActividadesRecreativas (bool), PuedeDesbloquearCerradura (bool), FechaAgregado

**EstadoReserva** (PK: int EstadoReservaId) — Nombre · **EstadoHabitacion** (PK: int EstadoHabitacionId) — Nombre · **TipoHabitacion** (PK: int TipoHabitacionId) — Nombre

**ActividadesRecreativas** (PK: int ActividadId) — Nombre, Descripcion, Precio (decimal), Duracion · **ReservasActividades** (PK: ReservaId + ActividadId)

**CheckIn** (PK: int CheckInId) — ReservaId (int, FK), FechaHoraCheckIn, Notas · **CheckOut** (PK: int CheckOutId) — ReservaId (int, FK), FechaHoraCheckOut, Notas

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
| **EstadoDispositivoDto** | EstadoDispositivoId, Descripcion · **CreateEstadoDispositivoDto** | Descripcion |
| **TipoDispositivoDto** | TipoDispositivoId, Nombre · **CreateTipoDispositivoDto** | Nombre |

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
GetByTipoCredencial(string) · GetByCodigoPin(string) · AddAsync · UpdateAsync · DeleteAsync
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

### `IReservaKafkaProducer` (Reservas)
```
PublishReservaCreadaAsync(ReservaCreadaEvent)
PublishUnlockDoorAsync(UnlockDoorEvent)
PublishCheckInRealizadoAsync(CheckInRealizadoEvent)
PublishPersonalUnlockDoorAsync(PersonalUnlockDoorEvent)
PublishHabitacionSyncAsync(int habitacionId)   ← dispara sync ThingsBoard en Dispositivos
```
Impl: `Reservas.Infrastructure/Kafka/KafkaProducerService.cs` · Config: `KafkaProducerConfig` (appsettings `KafkaProducer:`)

### `ITbDeviceService` (ThingsBoard)
```
GetDeviceByNameAsync(string name) → TbDeviceResponse?
SetSharedAttributesAsync(string deviceId, Dictionary<string, object> attrs, CancellationToken)
CreateOrUpdateDeviceAsync · GetDeviceByIdAsync · DeleteDeviceAsync · GetDeviceCredentialsAsync · UpdateDeviceAsync
```

### `ITbCredencialesSyncService` (ThingsBoard sync)
```
SyncAsync(int habitacionId, CancellationToken)          ← usa HabitacionId directamente
SyncByReservaIdAsync(int reservaId, CancellationToken)  ← resuelve HabitacionId desde ReservaId
```

### `ICredencialesAccesoService` (Reservas — SQL raw, cross-table)
```
GetCredencialIdAsync(reservaId, pin) → int?
RegistrarUsoAsync(credencialId)
HabitacionTieneCerraduraActivaAsync(habitacionId) → bool
PersonalTienePermisoAsync(personalId, habitacionId) → bool
GetReservaActivaByHabitacionIdAsync(habitacionId) → int?
GetPersonalNombreAsync(personalId) → string?
GetPersonalByUsuarioIdAsync(usuarioId) → (PersonalId, NombreCompleto)?
```

---

## Handlers/Queries existentes — Usuarios

| Entidad | Queries | Commands |
|---|---|---|
| Huespedes | GetAll, GetById, GetByUserId, GetVip | Create, Update, Delete, **CreateMe** |
| Personal | GetAll, GetById, GetByUserId, GetActivo, GetByDepartamento | Create, Update, Delete |
| PermisosPersonal | GetAll, GetById, GetActivos, GetByPersonal, GetByHabitacion, GetByActividad | Create, Update, Delete |

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
| **DispositivoController** | GET /dispositivo (paginado) · GET /dispositivo/{id} · GET /dispositivo/hotel/{hotelId} · POST → 201 · PUT /{id} · DELETE /{id} |
| **CerradurasInteligenteController** | GET /cerradurasinteligente (paginado) · GET /{id} · POST → 201 · PUT /{id} · DELETE /{id} |
| **CredencialesAccesoController** | GET /credencialesacceso (paginado) · GET /huesped/{huespedId} (paginado) · GET /{id} · POST → 201 · PUT /{id} · DELETE /{id} |
| **MantenimientoCerraduraController** | GET /mantenimientocerradura (paginado) · GET /{id} · POST → 201 · PUT /{id} · DELETE /{id} |
| **RegistrosAccesoController** | GET /registrosacceso (paginado) · GET /{id} · POST → 201 · DELETE /{id} |
| **RegistrosAuditoriumController** | GET /registrosauditorium (paginado) · GET /{id} · POST → 201 · DELETE /{id} |
| **EstadosDispositivoController** | GET /estadosdispositivo (paginado) · GET /{id} · POST → 201 · PUT /{id} · DELETE /{id} |
| **TiposDispositivoController** | GET /tiposdispositivo (paginado) · GET /{id} · POST → 201 · PUT /{id} · DELETE /{id} |

## Endpoints — Authenticate.API

| Verbo | Ruta | Descripción |
|---|---|---|
| POST | /login | `{ Email, Password }` → AccessTokenResponse + JWT (solo por email) |
| POST | /register | RegisterRequest → crea usuario + publica `EmailConfirmationEvent` a Kafka |
| POST | /create | EmailRequest → crea usuario con contraseña aleatoria |
| GET | /info | [Authorize] → InfoResponse |
| GET | /getuserbyemail?email= | → UserInfoResponse |
| GET | /confirmemail?userId=&token= | confirma email (token Base64Url) |
| POST | /forgotpassword | `{ Email }` → genera token + envía email con link · siempre 200 |
| POST | /resetpassword | `{ Email, Token, NewPassword }` → resetea contraseña (token Base64Url del email) |

## Endpoints — Reservas.API

| Verbo | Ruta | Descripción |
|---|---|---|
| POST | /reservas/{id}/unlock-door?pin= | → UnlockDoorCommand |
| POST | /reservas/{id}/checkin | → PerformCheckInCommand (ReservaId de ruta, sin body) |
| POST | /reservas/{id}/checkout | → PerformCheckOutCommand |
| CRUD | /reservas | Reservas |
| CRUD | /habitacion | Habitaciones |
| POST | /habitacion/{habitacionId}/unlock | [Authorize] → UnlockDoorPersonalCommand (personal JWT) |
| CRUD | /hotel | Hoteles |

## Endpoints — Usuarios.API

| Verbo | Ruta | Descripción |
|---|---|---|
| CRUD | /huesped | Huéspedes |
| GET | /huesped/user/{usuarioId} | Por UsuarioId (Identity) |
| GET | /huesped/vip | Solo VIPs |
| GET | /huesped/me | [Authorize] Perfil del huésped autenticado (UsuarioId del JWT) |
| POST | /huesped/me | [Authorize] Crea perfil propio — sin `CorreoElectronico` en body, `EsVip=false` siempre |
| CRUD | /personal | Personal |
| GET | /personal/user/{usuarioId} | Por UsuarioId (Identity) |
| GET | /personal/activo | Solo activos |
| GET | /personal/departamento/{departamentoId} | Por departamento |
| CRUD | /permisopersonal | Permisos de personal |

---

## Reglas de negocio — Usuarios

- **PermisosPersonal**: un personal no puede tener dos permisos para la misma habitación → `CreatePermisoCommandHandler` llama `GetByPersonalAndHabitacionAsync(personalId, habitacionId)` antes de insertar → `ConflictException` si ya existe.
- **`IPermisosPersonalRepository`** incluye: `GetByPersonalAndHabitacionAsync(int personalId, int habitacionId) → PermisosPersonal?`
- **Huesped/me**: `POST /huesped/me` usa `CreateHuespedeMeCommand(UsuarioId, SelfCreateHuespedeDto)` — `UsuarioId` viene del JWT, `EsVip=false` siempre. DTO: `SelfCreateHuespedeDto` (sin `CorreoElectronico` ni `EsVip`).

## Reglas de negocio — Reservas

- **Reserva cancelada** (`EstadoReservaId = 4`): no se puede editar. `UpdateReservaCommandHandler` valida esto antes de procesar.
- `DELETE /reservas/{id}` → **cancela** (EstadoReservaId = 4), no borra el registro.

---

## Estándar de paginación

`PaginationParams` y `PagedResult<T>` en `Dispositivos.Application/Common/`. Query recibe `Page`/`PageSize`, handler pagina en memoria, controller usa `[FromQuery] PaginationParams`. Respuesta: `{ items, totalCount, page, pageSize, totalPages, hasNextPage, hasPreviousPage }`.

```csharp
// Handler — patrón estándar
var todos = await _repo.GetAll();
var dtos = _mapper.Map<IEnumerable<XxxDto>>(todos).ToList();
var items = dtos.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
return Result<PagedResult<XxxDto>>.Success(new PagedResult<XxxDto>(items, request.Page, request.PageSize, dtos.Count));
```

---

## Regla obligatoria: manejo de errores en handlers

1. **Validar FKs y UNIQUEs** antes de tocar DB con `GetBy*` del repositorio → devolver `Result<T>.Failure(...)` si falla.
2. **Capturar DbUpdateException** (sin EF Core en Application):

```csharp
catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
{
    var inner = ex.InnerException?.Message ?? ex.Message;
    if (inner.Contains("UQ_Cerraduras_Habitacion"))         return Result<T>.Failure("La habitación ya tiene cerradura.");
    if (inner.Contains("FK_Cerraduras_Dispositivos"))       return Result<T>.Failure("Dispositivo no encontrado.");
    if (inner.Contains("UQ_Dispositivos_NumeroSerie"))      return Result<T>.Failure("Número de serie ya registrado.");
    if (inner.Contains("UQ_Dispositivos_MAC"))              return Result<T>.Failure("MAC ya registrada.");
    if (inner.Contains("UQ_Dispositivos_IP"))               return Result<T>.Failure("IP ya registrada.");
    if (inner.Contains("FK_Dispositivos_TiposDispositivo")) return Result<T>.Failure("Tipo de dispositivo no existe.");
    if (inner.Contains("FK_Dispositivos_EstadosDispositivo")) return Result<T>.Failure("Estado de dispositivo no existe.");
    if (inner.Contains("FK_Dispositivos_Hoteles"))          return Result<T>.Failure("Hotel no encontrado.");
    if (inner.Contains("CHK_Dispositivos_Bateria"))         return Result<T>.Failure("NivelBateria debe ser 0-100.");
    if (inner.Contains("CHK_Credenciales_Fechas"))          return Result<T>.Failure("FechaExpiracion debe ser posterior a FechaActivacion.");
    return Result<T>.Failure($"Error de base de datos: {inner}");
}
catch (Exception ex) { return Result<T>.Failure($"Error inesperado: {ex.Message}"); }
```

3. **NotFound**: `if (entidad == null) return Result<T>.NotFound($"Xxx con ID {id} no encontrado.");`

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

---

## Regla obligatoria: respuesta HTTP en controllers

```csharp
if (!result.IsSuccess)
    return result.IsNotFound
        ? NotFound(new { error = result.ErrorMessage })
        : BadRequest(new { error = result.ErrorMessage });
return Ok(result.Data);       // GET/PUT
return CreatedAtAction(...);  // POST
```

| Situación | Código |
|---|---|
| GET / PUT exitoso | `200 Ok` |
| POST exitoso | `201 CreatedAtAction` |
| DELETE exitoso | `200 Ok` o `204 NoContent` |
| No encontrado | `404 NotFound` |
| Validación / ID ruta ≠ body | `400 BadRequest` |

---

## EF Core — AsNoTracking + navigation properties

Todos los repositorios usan `.AsNoTracking()`. Al update con FK + nav prop cargada, EF recalcula FK desde nav ignorando el nuevo valor.

**Solución:** `entidad.Dispositivo = null;` antes de `UpdateAsync` (obligatorio si cambia `DispositivoId` en `CerradurasInteligente`).

---

## AutoMapper — campos ignorados

- `CreateDispositivoDto/UpdateDispositivoDto → Dispositivo`: ignora `DispositivoId`, `FechaCreacion`, navigation props
- `CreateCredencialesAccesoDto → CredencialesAcceso`: ignora `CredencialId`, `FechaCreacion`, `HashPin`, `UltimoUso`
- `CreateCerradurasInteligenteDto → CerradurasInteligente`: ignora `CerraduraId`, navigation props
- `CreateReservaCommand/UpdateReservaCommand → Reserva`: ignora `EstadoReserva`, `ReservaHuespedes`, timestamps, `CreadoPor`, `ModificadoPor`

---

## Kafka — topics y flujos

| Topic | Productor | Consumidor | Evento |
|---|---|---|---|
| `users` | Authenticate.API | Notification.Worker (`UserCreatedEventConsumer`) | `UserCreatedEvent` |
| `email-confirmation` | Authenticate.API | Notification.Worker (`EmailConfirmationEventConsumer`) | `EmailConfirmationEvent` |
| `password-reset` | Authenticate.API | Notification.Worker (`PasswordResetEventConsumer`) | `PasswordResetEvent` |
| `reservas` | Reservas.API | Notification.Worker (`ReservaCreadaEventConsumer`) | `ReservaCreadaEvent` |
| `dispositivos.unlock-door` | Reservas.API | Dispositivos.Infrastructure (`UnlockDoorKafkaConsumer`) | `UnlockDoorEvent` |
| `reservas.checkin-realizado` | Reservas.API | Dispositivos.Infrastructure (`CheckInRealizadoKafkaConsumer`) | `CheckInRealizadoEvent` |
| `checkin.credenciales` | Dispositivos.Infrastructure | Notification.Worker (`CredencialesCheckInEventConsumer`) | `CredencialesCheckInEvent` |
| `habitacion.personal-unlock` | Reservas.API | Dispositivos.Infrastructure (`PersonalUnlockDoorKafkaConsumer`) | `PersonalUnlockDoorEvent` |
| `habitacion.personal-acceso` | Dispositivos.Infrastructure | Notification.Worker (`PersonalAccesoHabitacionEventConsumer`) | `PersonalAccesoHabitacionEvent` |
| `habitacion.permiso-personal` | Usuarios.API | Dispositivos.Infrastructure (`PermisoPersonalCreadoKafkaConsumer`) | `PermisoPersonalCreadoEvent` |
| `audit.events` | Todos los APIs | Audit.Worker | `AuditEvent` |

### Flujo unlock-door (huésped con PIN)
`POST /reservas/{id}/unlock-door?pin=` → `UnlockDoorCommand` → valida reserva activa + habitación + PIN → `RegistrarUsoAsync` → publica `UnlockDoorEvent` → `UnlockDoorKafkaConsumer`: cerradura activa por HabitacionId → ThingsBoard `lockState="unlocked"` → crea `RegistrosAcceso`.

### Flujo unlock-door personal
`POST /habitacion/{habitacionId}/unlock` `[Authorize]` → `UnlockDoorPersonalCommandHandler`:
- Extrae `UsuarioId` del JWT → `GetPersonalByUsuarioIdAsync` (raw SQL `Personal WHERE UsuarioId AND EstaActivo=1`)
- `PersonalTienePermisoAsync` → `HabitacionTieneCerraduraActivaAsync` → `GetReservaActivaByHabitacionIdAsync`
- Si hay reserva: obtiene emails huéspedes vía `IUsuariosApiService`
- Publica `PersonalUnlockDoorEvent` → topic `habitacion.personal-unlock`

`PersonalUnlockDoorKafkaConsumer`: ThingsBoard **no-bloqueante** (try/catch solo loguea) → crea `RegistrosAcceso` (TipoAcceso="Personal") → si `Huespedes.Count > 0` publica `PersonalAccesoHabitacionEvent`.

`PersonalAccesoHabitacionEventConsumer`: email HTML + push por huésped (alerta acceso personal).

**Regla:** `PersonalId` viene del JWT, nunca de query param.

### Flujo check-in + credenciales + ThingsBoard
`POST /reservas/{id}/checkin` → `PerformCheckInCommand` → valida reserva + fechas → obtiene huéspedes vía `IUsuariosApiService` → publica `CheckInRealizadoEvent`.

`CheckInRealizadoKafkaConsumer`: genera PIN (`RandomNumberGenerator.GetInt32(100000,1000000)`) → crea `CredencialesAcceso` por huésped → `SyncByReservaIdAsync(reservaId)` → publica `CredencialesCheckInEvent`.

`CredencialesCheckInEventConsumer`: email HTML con PIN + push notification.

### Flujo personal desactivado/activado → credenciales + ThingsBoard sync (gRPC)
`PUT /personal/{id}` cambia `EstaActivo` → `UpdatePersonalCommandHandler` detecta `estabaActivo != EstaActivo` → llama `IDispositivosApiService.SincronizarEstadoPersonalAsync(personalId, estaActivo)` (no bloquea si falla).

`IDispositivosApiService` implementado por `DispositivosGrpcClient` (en `Usuarios.ExternalService`) → llama `PersonalEstado.SincronizarEstadoPersonal` en `Dispositivos.API` vía gRPC (h2c, puerto 5185).

`PersonalEstadoGrpcService` (Dispositivos.API):
- **Desactivación** (`EstaActivo=false`): filtra `EstaActiva=true` → `EstaActiva=false` → save
- **Reactivación** (`EstaActivo=true`): filtra `!EstaActiva && FechaExpiracion >= NOW` → `EstaActiva=true` → save
- Raw SQL: `CredencialesAcceso JOIN Reservas` + `PermisosPersonal` → HabitacionIds afectadas → `SyncAsync` por cada una

Proto: `Grpc.Contracts/Protos/dispositivos.proto` · namespace `Grpc.Contracts.Dispositivos`
Config Usuarios: `ExternalServices:Dispositivos:GrpcUrl` (local: `http://localhost:5185`, Docker: `http://dispositivos-api:5185`)

### Flujo permiso personal → ThingsBoard sync
`POST /permisopersonal` → `CreatePermisoCommandHandler`: si `HabitacionId != null` publica `PermisoPersonalCreadoEvent` → `PermisoPersonalCreadoKafkaConsumer`: `SyncAsync(habitacionId)`.

### Flujo registro de usuario
`POST /register` → crea usuario Identity → publica `EmailConfirmationEvent` → `EmailConfirmationEventConsumer`: email con link de confirmación.

### Flujo password reset
`POST /forgotpassword` `{ Email }` → `ForgotPasswordHandler` → `GeneratePasswordResetTokenAsync` → token Base64Url → link `{baseUrl}/ResetPassword?email=...&token=...` → publica `PasswordResetEvent` → `PasswordResetEventConsumer`: email HTML con botón de reset.
`POST /resetpassword` `{ Email, Token, NewPassword }` → `FindByEmailAsync` → `Base64UrlDecode(token)` → `ResetPasswordAsync` → 200 / ValidationProblem.
**Siempre 200 en `/forgotpassword`** aunque el email no exista (anti-enumeración).

---

## Kafka — patrón nuevo consumer (checklist)

1. **Evento** en `Notification.Domain/Events/`: campos + `Guid Id = Guid.NewGuid()` + `DateTime CreatedAt = UtcNow`
2. **Config** en `Notification.Kafka/Configuration/`: clase hereda `KafkaConsumerConfig`, constructor setea `GroupId` y `Topic`
3. **Consumer** en `Notification.Kafka/Services/`: seguir patrón `CredencialesCheckInEventConsumer` — `StartAsync` → `EnsureTopicExists` → `Subscribe` → `Task.Run(ConsumeMessages)`; `StopAsync` cancela + `Close()`; `Dispose` libera consumer/adminClient/cts
4. **Program.cs** en Notification.Worker: bind config, `AddSingleton` config + consumer, `AddHostedService` worker
5. **docker-compose.yml** (OBLIGATORIO): `KafkaConsumer__Xxx__BootstrapServers: kafka:29092` + GroupId + Topic
6. **appsettings.json**: mismas keys con `localhost:9092`

> ⚠️ Docker: red interna `kafka:29092`. Host: `localhost:9092`. El docker-compose sobreescribe appsettings.

Si el productor es BackgroundService en Dispositivos.Infrastructure: `new ProducerBuilder<string,string>(new ProducerConfig { BootstrapServers, Acks=Acks.Leader }).Build()`. Dispose: `Flush(5s)` + `Dispose`.

---

## Credenciales de acceso

- **PIN**: `RandomNumberGenerator.GetInt32(100000, 1000000).ToString()` (6 dígitos)
- **Hash**: SHA-256 en Base64 → guardado en `HashPin` (columna `HashPIN`)
- **Columnas DB**: CodigoPin → `CodigoPIN`, HashPin → `HashPIN`
- **Validación**: `GetCredencialIdAsync(reservaId, pin)` compara `CodigoPIN` directo (SQL raw)
- **Registro de uso**: `RegistrarUsoAsync(credencialId)` → `NumeroUsos + 1`, `UltimoUso = NOW`
- **Constraint**: `CHK_Credenciales_Fechas` — validar `FechaExpiracion > FechaActivacion` antes de insertar

---

## ThingsBoard

```
GET  /api/tenant/devices?deviceName={name}                   → buscar device por nombre
POST /api/plugins/telemetry/DEVICE/{deviceId}/SHARED_SCOPE   → setear atributos compartidos
```

- Nombre del device = `CerradurasInteligente.DispositivoId.ToString()`
- Auth: JWT de tenant cacheado 60 min (`TbTokenCache` singleton, doble-check locking)
- `POST /api/device` sin ID crea device; si ya existe → 400 → usar `GetDeviceByNameAsync` (idempotente). **No usar** `/api/device?name=...`
- `TbDeviceResponse.Id` puede ser null — siempre verificar antes de `SetSharedAttributesAsync`
- **ThingsBoard es no-bloqueante** en consumers: intento en try/catch que solo loguea, siempre continúa con `RegistrosAcceso` y eventos

### ThingsBoard sync de credenciales (`TbCredencialesSyncService`)
Scoped, recibe `BarceloIoTDatabaseContext` + `ITbDeviceService`. Raw SQL cross-table: `CerradurasInteligentes` + `CredencialesAcceso JOIN Reservas` + `PermisosPersonal JOIN Personal`. Pushea atributo `credenciales` (JSON) + `ultimaSincronizacionCredenciales` (ISO 8601). **Nunca lanza excepciones.**

Horizonte credenciales: `FechaActivacion <= NOW+7d AND FechaExpiracion >= NOW`
Horizonte permisos: `FechaExpiracion IS NULL OR FechaExpiracion >= NOW`

Formato `credenciales`:
```json
[
  {"tipo":"huesped","pin":"123456","huespedId":1,"reservaId":10,"activacion":"...","expiracion":"..."},
  {"tipo":"personal","personalId":3,"nombre":"Juan Perez","expiracion":null}
]
```

Flujos que disparan sync:
| Trigger | Llamada |
|---|---|
| Check-in (`CheckInRealizadoKafkaConsumer`) | `SyncByReservaIdAsync(reservaId)` |
| `POST /credencialesacceso` | `SyncByReservaIdAsync` si `ReservaId != null` |
| `PUT /credencialesacceso/{id}` | `SyncByReservaIdAsync` por ReservaId anterior + nueva si cambió (ambas si distintas) |
| `DELETE /credencialesacceso/{id}` | `SyncByReservaIdAsync` si tenía `ReservaId` |
| `POST /permisopersonal` | `PermisoPersonalCreadoEvent` → `SyncAsync(habitacionId)` |
| `PUT /permisopersonal/{id}` | `_syncProducer.PublishAsync(habitacionId)` si `HabitacionId != null` |
| `DELETE /permisopersonal/{id}` | `_syncProducer.PublishAsync(habitacionId)` si `HabitacionId != null` |
| `PUT /reservas/{id}` cambio habitación con CheckIn activo | `PublishHabitacionSyncAsync` habitación antigua + nueva (si aplica) |

Regla cambio habitación: condición `hadCheckIn && oldHabitacionId.HasValue && request.HabitacionId != oldHabitacionId` → siempre sync antigua; sync nueva solo si `request.HabitacionId.HasValue`.

Config: Dispositivos `KafkaConsumer:PermisoPersonal` (GroupId: `dispositivos-permiso-personal-group`). Usuarios `KafkaProducer:PermisoHabitacion` (Topic: `habitacion.permiso-personal`). `Dispositivos.Infrastructure.csproj` referencia `Dispositivos.Persistence`.

---

## Sistema de auditoría

`AuditBehavior<TRequest, TResponse>` (MediatR pipeline) en Dispositivos y Reservas:
- Intercepta Commands, detecta `IsSuccess` por reflection, extrae `EntidadId` del primer campo `int` terminado en `"Id"`
- Publica `AuditEvent { Servicio, UsuarioId, Accion, TipoEntidad, EntidadId, DireccionIp, AgenteUsuario, Resultado, MensajeError, HotelId, FechaHora }` a `audit.events`
- **Nunca lanza excepciones** (swallow)

Authenticate.API: auditoría manual en `AuthController` (Login/Register). `AddHttpContextAccessor()` registrado en cada API.

---

## Notification.Worker — consumers registrados

| Consumer | Topic | Acción |
|---|---|---|
| `UserCreatedEventConsumer` | `users` | Email bienvenida + push notification |
| `ReservaCreadaEventConsumer` | `reservas` | Email confirmación reserva |
| `EmailConfirmationEventConsumer` | `email-confirmation` | Email con link de confirmación |
| `CredencialesCheckInEventConsumer` | `checkin.credenciales` | Email HTML con PIN + push notification |
| `PersonalAccesoHabitacionEventConsumer` | `habitacion.personal-acceso` | Email HTML + push alerta acceso personal a todos los huéspedes |
| `PasswordResetEventConsumer` | `password-reset` | Email HTML con botón de restablecimiento de contraseña |

Email HTML credenciales: PIN monospace destacado, NumeroReserva, fechas CheckIn/CheckOut, advertencia no compartir.
Servicios: `IEmailService` (Azure Communication Services), `IPushNotificationService` (ntfy.sh)

---

## Campos de auditoría automáticos (del JWT)

Claim: `"nameid" ?? ClaimTypes.NameIdentifier` vía `IHttpContextAccessor`.

| Tabla | Campo | Cuándo |
|---|---|---|
| `CredencialesAcceso` | `CreadoPor` | Create (Dispositivos.API) |
| `Reservas` | `CreadoPor` | Create (Reservas.API) |
| `Reservas` | `ModificadoPor` | Update (Reservas.API) |
| `PermisosPersonal` | `OtorgadoPor` | Create (Usuarios.API) |

`FechaCreacion` → `DateTime.UtcNow` en Create handlers (Ignored en mapping). `FechaActualizacion` → `DateTime.UtcNow` en Update handlers.

---

## Pendientes conocidos

- **Controllers de Reservas**: migrar de `BadRequest(result.ErrorMessage)` → `BadRequest(new { error = result.ErrorMessage })` + usar 404 cuando corresponda
- **`UpdateReservaCommandHandler`**: agregar `catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")`
- **Otros handlers de Reservas**: verificar que capturen DbUpdateException antes de Exception genérico
