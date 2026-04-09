# CLAUDE.md — Barcelo Integrated IoT

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
| `reservas` | Reservas.API | Notification.Worker |
| `users` | Authenticate.API | Notification.Worker |
| `email-confirmation` | Authenticate.API | Notification.Worker |
| `dispositivos.unlock-door` | Reservas.API | Dispositivos.Infrastructure (BackgroundService) |
| `reservas.checkin-realizado` | Reservas.API | Dispositivos.Infrastructure (BackgroundService) |
| `audit.events` | Todos los APIs | Audit.Worker |

**Flujo unlock-door:**
1. `POST /reservas/{id}/unlock-door?pin=` → `UnlockDoorCommand`
2. Handler valida reserva activa, habitación asignada, PIN correcto
3. Publica `UnlockDoorEvent` a `dispositivos.unlock-door`
4. `UnlockDoorKafkaConsumer` (BackgroundService en Dispositivos):
   - Busca cerradura activa por `HabitacionId`
   - Obtiene device de ThingsBoard por nombre = `DispositivoId.ToString()`
   - Setea `lockState = "unlocked"` como shared attribute
   - Registra entrada en `RegistrosAcceso`

---

## ThingsBoard

```
GET  /api/tenant/devices?deviceName={name}         → buscar device por nombre
POST /api/plugins/telemetry/DEVICE/{id}/SHARED_SCOPE → setear atributos compartidos
```

- El nombre del device en ThingsBoard = `CerradurasInteligente.DispositivoId.ToString()`
- Auth: JWT obtenido con credenciales de tenant (cacheado 60 min)
- **No usar** `/api/device?name=...` (incorrecto)

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
