# Plan de Implementación: Sistema de Preferencias de Notificaciones

## Visión General

Sistema para que los usuarios puedan gestionar sus preferencias de notificaciones, con lógica centralizada en Notification.Worker que decide si enviar o no la notificación basándose en las preferencias del usuario.

## Flujo de Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  Kafka Event (Email disponible)                                            │
│       ↓                                                                    │
│  [1] GET /getuserbyemail?email={Email} → UsuarioId (Authenticate.API)     │
│       ↓                                                                    │
│  [2] Get PreferenciasNotificacion(UsuarioId)                               │
│       │                                                                    │
│       ├─ SI NO existe → ENVIAR (todos true por defecto)                  │
│       └─ SI existe → verificar permisos                                    │
│           ├─ SI cumple → ENVIAR                                            │
│           └─ NO cumple → OMITIR (loguear)                                  │
│       ↓                                                                    │
│  [3] SIEMPRE guardar en tabla Notificaciones                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Nuevos Campos en Tabla PreferenciasNotificacion

Agregar a la tabla existente los siguientes campos:

```sql
-- Nuevos campos para PreferenciasNotificacion
ALTER TABLE [dbo].[PreferenciasNotificacion] ADD [NotificarReservas] [bit] NOT NULL DEFAULT ((1));
GO

ALTER TABLE [dbo].[PreferenciasNotificacion] ADD [NotificarCredenciales] [bit] NOT NULL DEFAULT ((1));
GO

ALTER TABLE [dbo].[PreferenciasNotificacion] ADD [NotificarCheckIn] [bit] NOT NULL DEFAULT ((1));
GO

ALTER TABLE [dbo].[PreferenciasNotificacion] ADD [NotificarCuentaCreada] [bit] NOT NULL DEFAULT ((1));
GO

ALTER TABLE [dbo].[PreferenciasNotificacion] ADD [NotificarConfirmacionEmail] [bit] NOT NULL DEFAULT ((1));
GO

ALTER TABLE [dbo].[PreferenciasNotificacion] ADD [NotificarRestablecerPassword] [bit] NOT NULL DEFAULT ((1));
GO
```

### Tabla Actualizada PreferenciasNotificacion

| Campo | Tipo | Default | Descripción |
|-------|------|---------|-------------|
| PreferenciaId | int | PK | |
| UsuarioId | nvarchar(450) | FK | Unique |
| HabilitarNotificacionesPush | bit | 1 | Canal push |
| HabilitarNotificacionesEmail | bit | 1 | Canal email |
| HabilitarNotificacionesSMS | bit | 0 | Canal SMS |
| NotificarAccesoPersonal | bit | 1 | Acceso de personal |
| NotificarRecordatorioActividad | bit | 1 | Recordatorio actividades |
| NotificarPromocionesOfertas | bit | 1 | Promociones |
| **NotificarReservas** | bit | 1 | Reserva creada |
| **NotificarCredenciales** | bit | 1 | Credencial creada |
| **NotificarCheckIn** | bit | 1 | Check-in realizado |
| **NotificarCuentaCreada** | bit | 1 | Cuenta creada |
| **NotificarConfirmacionEmail** | bit | 1 | Confirmar email |
| **NotificarRestablecerPassword** | bit | 1 | Password reset |
| HorarioNoMolestar | bit | 0 | Modo no molestar |
| HoraInicioNoMolestar | time | null | Inicio horario |
| HoraFinNoMolestar | time | null | Fin horario |

---

## Componentes a Implementar

### 1. Notification.Worker - Capa de Datos

| # | Archivo | Descripción |
|---|---------|-------------|
| 1.1 | `Notification.Worker/Data/NotificacionDbContext.cs` | DbContext para BarceloIoTDatabase |
| 1.2 | `Notification.Worker/Entities/PreferenciaNotificacion.cs` | Entity Preferences (con todos los campos) |
| 1.3 | `Notification.Worker/Entities/NotificacionEntity.cs` | Entity Notificacion |

### 2. Notification.Worker - Repositorios

| # | Archivo | Descripción |
|---|---------|-------------|
| 2.1 | `Notification.Worker/Interfaces/IPreferenciasRepository.cs` | Interfaz |
| 2.2 | `Notification.Worker/Services/PreferenciasRepository.cs` | Implementación |
| 2.3 | `Notification.Worker/Interfaces/INotificacionesRepository.cs` | Interfaz |
| 2.4 | `Notification.Worker/Services/NotificacionesRepository.cs` | Implementación |

### 3. Notification.Worker - Cliente Auth

| # | Archivo | Descripción |
|---|---------|-------------|
| 3.1 | `Notification.Worker/Services/AuthApiClient.cs` | HttpClient para resolver UsuarioId por email |

### 4. Notification.Worker - Lógica Base

| # | Archivo | Descripción |
|---|---------|-------------|
| 4.1 | `Notification.Worker/Common/NotificacionHandlerBase.cs` | Clase base para consumers con lógica de preferencias |

### 5. Authenticate.API - Endpoints

| # | Archivo | Descripción |
|---|---------|-------------|
| 5.1 | `Authenticate.API/DTOs/PreferenciasDto.cs` | DTOs para preferencias y notificaciones |
| 5.2 | `Authenticate.API/Controllers/PreferenciasController.cs` | Endpoints CRUD |

### 6. Script de Base de Datos

| # | Archivo | Descripción |
|---|---------|-------------|
| 6.1 | `SqlServer.Database/Scripts/Migration_PreferenciasNotificacion_AddCampos.sql` | Agregar nuevos campos |

### 7. Modificación de Consumers Existentes

| # | Consumer | Tipo Notificación | Campo Preferencia |
|----------|-------------------|------------------|
| 7.1 | CredencialCreadaEventConsumer | Credencial | `NotificarCredenciales` |
| 7.2 | CredencialesCheckInEventConsumer | CheckIn | `NotificarCheckIn` |
| 7.3 | PersonalAccesoHabitacionEventConsumer | AccesoPersonal | `NotificarAccesoPersonal` |
| 7.4 | UserCreatedEventConsumer | CuentaCreada | `NotificarCuentaCreada` |
| 7.5 | ReservaCreadaEventConsumer | Reserva | `NotificarReservas` |
| 7.6 | PasswordResetEventConsumer | RestablecerPassword | `NotificarRestablecerPassword` |
| 7.7 | EmailConfirmationEventConsumer | ConfirmacionEmail | `NotificarConfirmacionEmail` |

### 8. Configuración DI

| # | Archivo | Descripción |
|---|---------|-------------|
| 8.1 | `Notification.Worker/DependencyInjection.cs` | Registrar servicios |

### 9. Modificación Program.cs

| # | Archivo | Descripción |
|---|---------|-------------|
| 9.1 | `Notification.Worker/Program.cs` | Agregar servicios al DI |

### 10. Configuración Docker

| # | Archivo | Descripción |
|---|---------|-------------|
| 10.1 | `docker-compose.yml` | Agregar ConnectionStrings y AuthApi |

---

## Endpoints en Authenticate.API

| Verbo | Ruta | Descripción | Requiere Auth |
|-------|------|-------------|---------------|
| GET | `/me/notificaciones/preferencias` | Obtener preferencias del usuario | Sí |
| PUT | `/me/notificaciones/preferencias` | Actualizar preferencias | Sí |
| GET | `/me/notificaciones` | Listar historial (paginado) | Sí |
| PUT | `/me/notificaciones/{id}/leida` | Marcar como leída | Sí |
| DELETE | `/me/notificaciones/{id}` | Eliminar notificación | Sí |

---

## Lógica de Verificación de Preferencias

```csharp
public class NotificacionHandlerBase
{
    private readonly IPreferenciasRepository _preferenciasRepo;
    private readonly INotificacionesRepository _notificacionesRepo;
    private readonly IAuthApiClient _authApiClient;
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushService;
    private readonly ILogger _logger;

    public async Task<bool> DeboEnviarNotificacionAsync(string email, string canal, string tipoNotificacion)
    {
        // 1. Obtener UsuarioId por email
        var usuarioId = await _authApiClient.GetUserIdByEmailAsync(email);
        if (string.IsNullOrEmpty(usuarioId)) return false;

        // 2. Obtener preferencias
        var prefs = await _preferenciasRepo.GetByUsuarioIdAsync(usuarioId);
        
        // 3. Si NO tiene preferencias → ENVIAR (defaults: todos true)
        if (prefs == null) return true;

        // 4. Verificar horario no molestar
        if (prefs.HorarioNoMolestar && EnHorarioNoMolestar(prefs))
        {
            _logger.LogDebug("Notificación omitida por horario no molestar para usuario {UsuarioId}", usuarioId);
            return false;
        }

        // 5. Verificar canal
        if (canal == "Push" && !prefs.HabilitarNotificacionesPush) return false;
        if (canal == "Email" && !prefs.HabilitarNotificacionesEmail) return false;
        if (canal == "SMS" && !prefs.HabilitarNotificacionesSMS) return false;

        // 6. Verificar tipo de notificación
        if (tipoNotificacion == "AccesoPersonal" && !prefs.NotificarAccesoPersonal) return false;
        if (tipoNotificacion == "Reserva" && !prefs.NotificarReservas) return false;
        if (tipoNotificacion == "Credencial" && !prefs.NotificarCredenciales) return false;
        if (tipoNotificacion == "CheckIn" && !prefs.NotificarCheckIn) return false;
        if (tipoNotificacion == "CuentaCreada" && !prefs.NotificarCuentaCreada) return false;
        if (tipoNotificacion == "ConfirmacionEmail" && !prefs.NotificarConfirmacionEmail) return false;
        if (tipoNotificacion == "RestablecerPassword" && !prefs.NotificarRestablecerPassword) return false;
        if (tipoNotificacion == "RecordatorioActividad" && !prefs.NotificarRecordatorioActividad) return false;
        if (tipoNotificacion == "PromocionesOfertas" && !prefs.NotificarPromocionesOfertas) return false;

        return true;
    }

    public bool EnHorarioNoMolestar(PreferenciaNotificacion prefs)
    {
        var ahora = DateTime.UtcNow.TimeOfDay;
        var inicio = prefs.HoraInicioNoMolestar;
        var fin = prefs.HoraFinNoMolestar;
        
        if (inicio == null || fin == null) return false;
        
        // Caso normal: inicio < fin
        if (inicio < fin) return ahora >= inicio && ahora < fin;
        
        // Caso noche: inicio > fin (ej: 22:00 a 08:00)
        return ahora >= inicio || ahora < fin;
    }

    public async Task GuardarNotificacionAsync(string usuarioId, string tipoNotificacion, string titulo, 
        string mensaje, string canal, string prioridad)
    {
        var notificacion = new NotificacionEntity
        {
            UsuarioId = usuarioId,
            TipoNotificacion = tipoNotificacion,
            Titulo = titulo,
            Mensaje = mensaje,
            Prioridad = prioridad,
            FueLeida = false,
            FechaEnvio = DateTime.UtcNow,
            CanalEnvio = canal,
            EstadoEnvio = "Enviada"
        };
        await _notificacionesRepo.AddAsync(notificacion);
    }
}
```

---

## Orden de Implementación

### Fase 1: Base de Datos
1. [6.1] Crear script de migración `Migration_PreferenciasNotificacion_AddCampos.sql`

### Fase 2: Notification.Worker - Datos
2. [1.1] Crear `NotificacionDbContext.cs`
3. [1.2] Crear `PreferenciaNotificacion.cs`
4. [1.3] Crear `NotificacionEntity.cs`

### Fase 3: Notification.Worker - Repositorios
5. [2.1] Crear `IPreferenciasRepository.cs`
6. [2.2] Crear `PreferenciasRepository.cs`
7. [2.3] Crear `INotificacionesRepository.cs`
8. [2.4] Crear `NotificacionesRepository.cs`

### Fase 4: Notification.Worker - Servicios
9. [3.1] Crear `AuthApiClient.cs`
10. [4.1] Crear `NotificacionHandlerBase.cs`
11. [8.1] Crear `DependencyInjection.cs`

### Fase 5: Modificar Consumers
12. [7.1] Modificar `CredencialCreadaEventConsumer`
13. [7.2] Modificar `CredencialesCheckInEventConsumer`
14. [7.3] Modificar `PersonalAccesoHabitacionEventConsumer`
15. [7.4] Modificar `UserCreatedEventConsumer`
16. [7.5] Modificar `ReservaCreadaEventConsumer`
17. [7.6] Modificar `PasswordResetEventConsumer`
18. [7.7] Modificar `EmailConfirmationEventConsumer`

### Fase 6: Configuración
19. [9.1] Modificar `Program.cs` para registrar servicios
20. [10.1] Modificar `docker-compose.yml`

### Fase 7: Authenticate.API
21. [5.1] Crear `PreferenciasDto.cs`
22. [5.2] Crear `PreferenciasController.cs`

---

## Configuración Docker

```yaml
# docker-compose.yml - agregar en notification-worker
notification-worker:
  environment:
    - ASPNETCORE_ENVIRONMENT=Docker
    - ConnectionStrings__BarceloIoTDatabase=Data source=sqlserver;Database=BarceloIoTDatabase;User Id=sa;Password=Testing1234;TrustServerCertificate=True
    - AuthApi__BaseUrl=http://auth-api:5117
```

---

## Detalles de Implementación por Archivo

### 1.1 NotificacionDbContext.cs
```csharp
public class NotificacionDbContext : DbContext
{
    public NotificacionDbContext(DbContextOptions<NotificacionDbContext> options) : base(options) { }
    
    public DbSet<PreferenciaNotificacion> PreferenciasNotificacion { get; set; }
    public DbSet<NotificacionEntity> Notificaciones { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<PreferenciaNotificacion>(entity =>
        {
            entity.ToTable("PreferenciasNotificacion");
            entity.HasKey(e => e.PreferenciaId);
            entity.HasIndex(e => e.UsuarioId).IsUnique();
        });
        
        builder.Entity<NotificacionEntity>(entity =>
        {
            entity.ToTable("Notificaciones");
            entity.HasKey(e => e.NotificacionId);
        });
    }
}
```

### 1.2 PreferenciaNotificacion.cs
```csharp
public class PreferenciaNotificacion
{
    public int PreferenciaId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public bool HabilitarNotificacionesPush { get; set; } = true;
    public bool HabilitarNotificacionesEmail { get; set; } = true;
    public bool HabilitarNotificacionesSMS { get; set; } = false;
    public bool NotificarAccesoPersonal { get; set; } = true;
    public bool NotificarRecordatorioActividad { get; set; } = true;
    public bool NotificarPromocionesOfertas { get; set; } = true;
    public bool NotificarReservas { get; set; } = true;
    public bool NotificarCredenciales { get; set; } = true;
    public bool NotificarCheckIn { get; set; } = true;
    public bool NotificarCuentaCreada { get; set; } = true;
    public bool NotificarConfirmacionEmail { get; set; } = true;
    public bool NotificarRestablecerPassword { get; set; } = true;
    public bool HorarioNoMolestar { get; set; } = false;
    public TimeSpan? HoraInicioNoMolestar { get; set; }
    public TimeSpan? HoraFinNoMolestar { get; set; }
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
```

### 3.1 AuthApiClient.cs
```csharp
public interface IAuthApiClient
{
    Task<string?> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthApiClient> _logger;

    public AuthApiClient(HttpClient httpClient, ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        
        try
        {
            var response = await _httpClient.GetAsync($"/getuserbyemail?email={Uri.EscapeDataString(email)}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            
            var userInfo = await response.Content.ReadFromJsonAsync<UserInfoResponse>(cancellationToken: cancellationToken);
            return userInfo?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo UsuarioId para email {Email}", email);
            return null;
        }
    }
}

public class UserInfoResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsEmailConfirmed { get; set; }
}
```

### 5.2 PreferenciasController.cs (Endpoints)
```csharp
[ApiController]
[Route("me/notificaciones")]
[Authorize]
public class PreferenciasController : ControllerBase
{
    private readonly NotificacionDbContext _context;
    private readonly UserManager<User> _userManager;

    // GET /me/notificaciones/preferencias
    [HttpGet("preferencias")]
    public async Task<ActionResult<PreferenciasResponseDto>> GetPreferencias()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var prefs = await _context.PreferenciasNotificacion
            .FirstOrDefaultAsync(p => p.UsuarioId == user.Id);

        if (prefs == null)
        {
            // Return defaults if not exists
            return Ok(new PreferenciasResponseDto
            {
                HabilitarNotificacionesPush = true,
                HabilitarNotificacionesEmail = true,
                HabilitarNotificacionesSMS = false,
                NotificarAccesoPersonal = true,
                NotificarRecordatorioActividad = true,
                NotificarPromocionesOfertas = true,
                NotificarReservas = true,
                NotificarCredenciales = true,
                NotificarCheckIn = true,
                NotificarCuentaCreada = true,
                NotificarConfirmacionEmail = true,
                NotificarRestablecerPassword = true,
                HorarioNoMolestar = false,
                HoraInicioNoMolestar = null,
                HoraFinNoMolestar = null
            });
        }

        return Ok(MapToDto(prefs));
    }

    // PUT /me/notificaciones/preferencias
    [HttpPut("preferencias")]
    public async Task<ActionResult<PreferenciasResponseDto>> UpdatePreferencias([FromBody] UpdatePreferenciasDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var prefs = await _context.PreferenciasNotificacion
            .FirstOrDefaultAsync(p => p.UsuarioId == user.Id);

        if (prefs == null)
        {
            prefs = new PreferenciaNotificacion
            {
                UsuarioId = user.Id,
                FechaActualizacion = DateTime.UtcNow
            };
            _context.PreferenciasNotificacion.Add(prefs);
        }

        // Update fields
        prefs.HabilitarNotificacionesPush = dto.HabilitarNotificacionesPush;
        prefs.HabilitarNotificacionesEmail = dto.HabilitarNotificacionesEmail;
        prefs.HabilitarNotificacionesSMS = dto.HabilitarNotificacionesSMS;
        prefs.NotificarAccesoPersonal = dto.NotificarAccesoPersonal;
        prefs.NotificarRecordatorioActividad = dto.NotificarRecordatorioActividad;
        prefs.NotificarPromocionesOfertas = dto.NotificarPromocionesOfertas;
        prefs.NotificarReservas = dto.NotificarReservas;
        prefs.NotificarCredenciales = dto.NotificarCredenciales;
        prefs.NotificarCheckIn = dto.NotificarCheckIn;
        prefs.NotificarCuentaCreada = dto.NotificarCuentaCreada;
        prefs.NotificarConfirmacionEmail = dto.NotificarConfirmacionEmail;
        prefs.NotificarRestablecerPassword = dto.NotificarRestablecerPassword;
        prefs.HorarioNoMolestar = dto.HorarioNoMolestar;
        prefs.HoraInicioNoMolestar = dto.HoraInicioNoMolestar;
        prefs.HoraFinNoMolestar = dto.HoraFinNoMolestar;
        prefs.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(prefs));
    }

    // GET /me/notificaciones
    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificacionResponseDto>>> GetNotificaciones([FromQuery] PaginationParams @params)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var query = _context.Notificaciones
            .Where(n => n.UsuarioId == user.Id)
            .OrderByDescending(n => n.FechaEnvio);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((@params.Page - 1) * @params.PageSize)
            .Take(@params.PageSize)
            .Select(n => new NotificacionResponseDto
            {
                NotificacionId = n.NotificacionId,
                TipoNotificacion = n.TipoNotificacion,
                Titulo = n.Titulo,
                Mensaje = n.Mensaje,
                Prioridad = n.Prioridad,
                FueLeida = n.FueLeida,
                FechaEnvio = n.FechaEnvio,
                FechaLectura = n.FechaLectura,
                TipoEntidadRelacionada = n.TipoEntidadRelacionada,
                EntidadRelacionadaId = n.EntidadRelacionadaId,
                CanalEnvio = n.CanalEnvio,
                EstadoEnvio = n.EstadoEnvio
            })
            .ToListAsync();

        return Ok(new PagedResult<NotificacionResponseDto>(items, @params.Page, @params.PageSize, totalCount));
    }

    // PUT /me/notificaciones/{id}/leida
    [HttpPut("{id}/leida")]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var notificacion = await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.NotificacionId == id && n.UsuarioId == user.Id);

        if (notificacion == null) return NotFound();

        notificacion.FueLeida = true;
        notificacion.FechaLectura = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /me/notificaciones/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarNotificacion(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var notificacion = await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.NotificacionId == id && n.UsuarioId == user.Id);

        if (notificacion == null) return NotFound();

        _context.Notificaciones.Remove(notificacion);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
```

---

## Preguntas Respondidas

1. ✅ **¿Los nuevos campos de preferencias deben agregarse a la tabla?** 
   - Sí, se agregaron: NotificarReservas, NotificarCredenciales, NotificarCheckIn, NotificarCuentaCreada, NotificarConfirmacionEmail, NotificarRestablecerPassword

2. ✅ **¿Qué sucede si Authenticate.API no responde?**
   - Si Authenticate.API no responde, se retorna `false` y no se envía la notificación (seguro por defecto)

3. ✅ **¿El historial de notificaciones es solo lectura o el usuario puede eliminar?**
   - El usuario puede marcar como leída y eliminar notificaciones individuales

---

## Notas Adicionales

- Los consumers siempre deben guardar en la tabla Notificaciones, independientemente de si se envía o no la notificación
- Si el usuario no tiene preferencias creadas, se usan los valores por defecto (todos true)
- El horario no molestar está en UTC
- Los DTOs deben incluir validación para hora inicio < hora fin (si HorarioNoMolestar es true)
