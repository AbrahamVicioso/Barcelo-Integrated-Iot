using System.Data;
using System.Text.Json;
using Dispositivos.Application.Interfaces;
using Dispositivos.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dispositivos.Infrastructure.Services;

public class TbCredencialesSyncService : ITbCredencialesSyncService
{
    private readonly BarceloIoTDatabaseContext _context;
    private readonly ITbDeviceService _tbDeviceService;
    private readonly ILogger<TbCredencialesSyncService> _logger;

    public TbCredencialesSyncService(
        BarceloIoTDatabaseContext context,
        ITbDeviceService tbDeviceService,
        ILogger<TbCredencialesSyncService> logger)
    {
        _context = context;
        _tbDeviceService = tbDeviceService;
        _logger = logger;
    }

    public async Task SyncAsync(int habitacionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            // 1. Get active cerradura for this habitacion
            var cerradura = await GetCerraduraActivaAsync(connection, habitacionId, cancellationToken);
            if (cerradura == null)
            {
                _logger.LogDebug("Habitacion {HabitacionId} no tiene cerradura activa, sync omitido.", habitacionId);
                return;
            }

            // 2. Collect credentials valid in next 7 days
            var credencialesHuesped = await GetCredencialesHuespedAsync(connection, habitacionId, cancellationToken);
            var permisos = await GetPermisosPersonalAsync(connection, habitacionId, cancellationToken);
            var personalIds = permisos.Select(p => p.PersonalId).ToList();

            _logger.LogDebug("SyncAsync: Habitacion {HabitacionId} → {NumHuesped} credenciales huesped, {NumPermisos} permisos personal, PersonalIds=[{PersonalIds}]",
                habitacionId, credencialesHuesped.Count, permisos.Count, string.Join(",", personalIds));

            var credencialesPersonal = personalIds.Count > 0
                ? await GetCredencialesPersonalAsync(connection, personalIds, cancellationToken)
                : new List<CredencialPersonalInfo>();

            _logger.LogDebug("SyncAsync: Habitacion {HabitacionId} → {NumCredPersonal} credenciales personal encontradas",
                habitacionId, credencialesPersonal.Count);

            // 3. Build structured payload: huespedes[] + personal[]
            var huespedes = credencialesHuesped
                .GroupBy(c => new { c.HuespedId, c.ReservaId })
                .Select(g => new
                {
                    huespedId = g.Key.HuespedId,
                    reservaId = g.Key.ReservaId,
                    credenciales = g.Select(c => new
                    {
                        pin = c.CodigoPin,
                        activacion = c.FechaActivacion.ToString("o"),
                        expiracion = c.FechaExpiracion.ToString("o")
                    }).ToList()
                })
                .ToList<object>();

            var personal = permisos
                .Select(p => new
                {
                    personalId = p.PersonalId,
                    nombre = p.NombreCompleto,
                    expiracion = p.FechaExpiracion.HasValue ? p.FechaExpiracion.Value.ToString("o") : (string?)null,
                    credenciales = credencialesPersonal
                        .Where(cp => cp.PersonalId == p.PersonalId)
                        .Select(cp => new
                        {
                            pin = cp.CodigoPin,
                            activacion = cp.FechaActivacion.ToString("o"),
                            expiracion = cp.FechaExpiracion.ToString("o")
                        })
                        .ToList()
                })
                .ToList<object>();

            var payload = new { huespedes, personal };

            // 4. Get ThingsBoard device (name = DispositivoId)
            var device = await _tbDeviceService.GetDeviceByNameAsync(cerradura.DispositivoId.ToString());
            if (device == null || string.IsNullOrEmpty(device.Id))
            {
                _logger.LogWarning(
                    "ThingsBoard device no encontrado para Dispositivo {DispositivoId} (Habitacion {HabitacionId}), sync omitido.",
                    cerradura.DispositivoId, habitacionId);
                return;
            }

            // 5. Push as shared attributes
            var attrs = new Dictionary<string, object>
            {
                ["credenciales"] = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                }),
                ["ultimaSincronizacionCredenciales"] = DateTime.UtcNow.ToString("o")
            };

            await _tbDeviceService.SetSharedAttributesAsync(device.Id, attrs, cancellationToken);

            _logger.LogInformation(
                "ThingsBoard sync OK: Habitacion {HabitacionId}, dispositivo {DispositivoId}, {NumHuespedes} huespedes, {NumPersonal} personal.",
                habitacionId, cerradura.DispositivoId, huespedes.Count, personal.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sincronizando credenciales a ThingsBoard para Habitacion {HabitacionId}.", habitacionId);
        }
    }

    public async Task SyncByReservaIdAsync(int reservaId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var cmd = (System.Data.Common.DbCommand)connection.CreateCommand();
            cmd.CommandText = "SELECT TOP 1 HabitacionId FROM Reservas WHERE ReservaId = @reservaId AND HabitacionId IS NOT NULL";
            var p = cmd.CreateParameter();
            p.ParameterName = "@reservaId";
            p.Value = reservaId;
            cmd.Parameters.Add(p);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            if (result is null || result == DBNull.Value)
            {
                _logger.LogDebug("Reserva {ReservaId} no tiene HabitacionId, sync omitido.", reservaId);
                return;
            }

            await SyncAsync(Convert.ToInt32(result), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en SyncByReservaIdAsync para Reserva {ReservaId}.", reservaId);
        }
    }

    public async Task SyncByHuespedIdAsync(int huespedId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var cmd = (System.Data.Common.DbCommand)connection.CreateCommand();
            cmd.CommandText = @"
                SELECT DISTINCT HabitacionId
                FROM Reservas
                WHERE HuespedId = @huespedId
                  AND HabitacionId IS NOT NULL
                  AND EstadoReservaId != 4";
            AddParam(cmd, "@huespedId", huespedId);

            var habitaciones = new List<int>();
            using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                while (await reader.ReadAsync(cancellationToken))
                    habitaciones.Add(reader.GetInt32(0));

            _logger.LogDebug("SyncByHuespedIdAsync: Huesped {HuespedId}, {Count} habitaciones.", huespedId, habitaciones.Count);
            foreach (var habitacionId in habitaciones)
                await SyncAsync(habitacionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en SyncByHuespedIdAsync para Huesped {HuespedId}.", huespedId);
        }
    }

    public async Task SyncByPersonalIdAsync(int personalId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var cmd = (System.Data.Common.DbCommand)connection.CreateCommand();
            cmd.CommandText = @"
                SELECT DISTINCT pp.HabitacionId
                FROM PermisosPersonal pp
                WHERE pp.PersonalId = @personalId
                  AND pp.EstaActivo = 1
                  AND (pp.FechaExpiracion IS NULL OR pp.FechaExpiracion >= @ahora)";
            AddParam(cmd, "@personalId", personalId);
            AddParam(cmd, "@ahora", DateTime.UtcNow);

            var habitaciones = new List<int>();
            using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                while (await reader.ReadAsync(cancellationToken))
                    habitaciones.Add(reader.GetInt32(0));

            _logger.LogDebug("SyncByPersonalIdAsync: Personal {PersonalId}, {Count} habitaciones.", personalId, habitaciones.Count);
            foreach (var habitacionId in habitaciones)
                await SyncAsync(habitacionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en SyncByPersonalIdAsync para Personal {PersonalId}.", personalId);
        }
    }

    public async Task SyncByReservaActividadIdAsync(int reservaActividadId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var cmd = (System.Data.Common.DbCommand)connection.CreateCommand();
            cmd.CommandText = @"
                SELECT TOP 1 c.CerraduraId
                FROM   CerradurasInteligentes c
                INNER JOIN ReservasActividades ra ON ra.ActividadId = c.ActividadId
                WHERE  ra.ReservaActividadId = @reservaActividadId
                  AND  c.EstaActiva = 1";
            AddParam(cmd, "@reservaActividadId", reservaActividadId);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            if (result is null || result == DBNull.Value)
            {
                _logger.LogDebug("ReservaActividad {ReservaActividadId} no tiene cerradura activa, sync omitido.", reservaActividadId);
                return;
            }

            await SyncByCerraduraIdAsync(Convert.ToInt32(result), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en SyncByReservaActividadIdAsync para ReservaActividad {ReservaActividadId}.", reservaActividadId);
        }
    }

    public async Task SyncByCerraduraIdAsync(int cerraduraId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            // Get the lock directly by CerraduraId
            using var cerraduraCmd = (System.Data.Common.DbCommand)connection.CreateCommand();
            cerraduraCmd.CommandText = "SELECT TOP 1 CerraduraId, DispositivoId, ActividadId FROM CerradurasInteligentes WHERE CerraduraId = @cerraduraId AND EstaActiva = 1";
            AddParam(cerraduraCmd, "@cerraduraId", cerraduraId);

            CerraduraActividadInfo? cerradura = null;
            using (var reader = await cerraduraCmd.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    cerradura = new CerraduraActividadInfo
                    {
                        CerraduraId = reader.GetInt32(0),
                        DispositivoId = reader.GetGuid(1),
                        ActividadId = reader.IsDBNull(2) ? null : reader.GetInt32(2)
                    };
                }
            }

            if (cerradura == null)
            {
                _logger.LogDebug("Cerradura {CerraduraId} no encontrada o inactiva, sync omitido.", cerraduraId);
                return;
            }

            if (cerradura.ActividadId == null)
            {
                _logger.LogDebug("Cerradura {CerraduraId} no es de actividad, usar SyncAsync.", cerraduraId);
                return;
            }

            // Collect active credentials for this activity
            var credenciales = await GetCredencialesActividadAsync(connection, cerraduraId, cancellationToken);

            var actividades = credenciales
                .GroupBy(c => new { c.HuespedId, c.ReservaActividadId })
                .Select(g => new
                {
                    huespedId = g.Key.HuespedId,
                    reservaActividadId = g.Key.ReservaActividadId,
                    credenciales = g.Select(c => new
                    {
                        pin = c.CodigoPin,
                        activacion = c.FechaActivacion.ToString("o"),
                        expiracion = c.FechaExpiracion.ToString("o")
                    }).ToList()
                })
                .ToList<object>();

            var payload = new { actividades };

            var device = await _tbDeviceService.GetDeviceByNameAsync(cerradura.DispositivoId.ToString());
            if (device == null || string.IsNullOrEmpty(device.Id))
            {
                _logger.LogWarning(
                    "ThingsBoard device no encontrado para Cerradura {CerraduraId} (Dispositivo {DispositivoId}), sync omitido.",
                    cerraduraId, cerradura.DispositivoId);
                return;
            }

            var attrs = new Dictionary<string, object>
            {
                ["credenciales"] = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                }),
                ["ultimaSincronizacionCredenciales"] = DateTime.UtcNow.ToString("o")
            };

            await _tbDeviceService.SetSharedAttributesAsync(device.Id, attrs, cancellationToken);

            _logger.LogInformation(
                "ThingsBoard sync OK: Cerradura {CerraduraId}, dispositivo {DispositivoId}, {NumActividades} credenciales actividad.",
                cerraduraId, cerradura.DispositivoId, actividades.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sincronizando credenciales de actividad a ThingsBoard para Cerradura {CerraduraId}.", cerraduraId);
        }
    }

    private static async Task<List<CredencialActividadInfo>> GetCredencialesActividadAsync(
        IDbConnection connection, int cerraduraId, CancellationToken ct)
    {
        using var cmd = (System.Data.Common.DbCommand)connection.CreateCommand();
        cmd.CommandText = @"
            SELECT ca.CodigoPIN, ca.HuespedId, ca.ReservaActividadId, ca.FechaActivacion, ca.FechaExpiracion
            FROM   CredencialesAcceso ca
            WHERE  ca.ReservaActividadId IN (
                       SELECT ra.ReservaActividadId FROM ReservasActividades ra
                       WHERE  ra.ActividadId = (
                           SELECT ActividadId FROM CerradurasInteligentes WHERE CerraduraId = @cerraduraId
                       )
                   )
              AND  ca.EstaActiva = 1
              AND  ca.HuespedId IS NOT NULL
              AND  ca.FechaActivacion <= DATEADD(day, 7, @ahora)
              AND  ca.FechaExpiracion  >= @ahora";

        AddParam(cmd, "@cerraduraId", cerraduraId);
        AddParam(cmd, "@ahora", DateTime.UtcNow);

        var result = new List<CredencialActividadInfo>();
        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new CredencialActividadInfo
            {
                CodigoPin = reader.GetString(0),
                HuespedId = reader.GetInt32(1),
                ReservaActividadId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                FechaActivacion = reader.GetDateTime(3),
                FechaExpiracion = reader.GetDateTime(4)
            });
        }
        return result;
    }

    private static async Task<CerraduraInfo?> GetCerraduraActivaAsync(
        IDbConnection connection, int habitacionId, CancellationToken ct)
    {
        using var cmd = (System.Data.Common.DbCommand)connection.CreateCommand();
        cmd.CommandText = @"
            SELECT TOP 1 CerraduraId, DispositivoId
            FROM CerradurasInteligentes
            WHERE HabitacionId = @habitacionId AND EstaActiva = 1";

        var p = cmd.CreateParameter();
        p.ParameterName = "@habitacionId";
        p.Value = habitacionId;
        cmd.Parameters.Add(p);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        return new CerraduraInfo
        {
            CerraduraId = reader.GetInt32(0),
            DispositivoId = reader.GetGuid(1)
        };
    }

    private static async Task<List<CredencialHuespedInfo>> GetCredencialesHuespedAsync(
        IDbConnection connection, int habitacionId, CancellationToken ct)
    {
        using var cmd = (System.Data.Common.DbCommand)connection.CreateCommand();
        cmd.CommandText = @"
            SELECT ca.CodigoPin, ca.HuespedId, ca.ReservaId, ca.FechaActivacion, ca.FechaExpiracion
            FROM CredencialesAcceso ca
            INNER JOIN Reservas r ON ca.ReservaId = r.ReservaId
            WHERE r.HabitacionId = @habitacionId
              AND ca.EstaActiva = 1
              AND ca.HuespedId IS NOT NULL
              AND r.EstadoReservaId != 4
              AND ca.FechaActivacion <= @horizonte
              AND ca.FechaExpiracion >= @ahora";

        AddParam(cmd, "@habitacionId", habitacionId);
        AddParam(cmd, "@horizonte", DateTime.UtcNow.AddDays(7));
        AddParam(cmd, "@ahora", DateTime.UtcNow);

        var result = new List<CredencialHuespedInfo>();
        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new CredencialHuespedInfo
            {
                CodigoPin = reader.GetString(0),
                HuespedId = reader.GetInt32(1),
                ReservaId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                FechaActivacion = reader.GetDateTime(3),
                FechaExpiracion = reader.GetDateTime(4)
            });
        }
        return result;
    }

    private async Task<List<CredencialPersonalInfo>> GetCredencialesPersonalAsync(
        IDbConnection connection, List<int> personalIds, CancellationToken ct)
    {
        using var cmd = (System.Data.Common.DbCommand)connection.CreateCommand();

        var ahora = DateTime.UtcNow;
        var horizonte = ahora.AddDays(7);

        // Build IN clause: @p0, @p1, ...
        var inClause = string.Join(",", personalIds.Select((_, i) => $"@p{i}"));
        cmd.CommandText = $@"
            SELECT ca.PersonalId, ca.CodigoPIN, ca.FechaActivacion, ca.FechaExpiracion, ca.EstaActiva
            FROM CredencialesAcceso ca
            WHERE ca.PersonalId IN ({inClause})";

        for (int i = 0; i < personalIds.Count; i++)
            AddParam(cmd, $"@p{i}", personalIds[i]);

        var result = new List<CredencialPersonalInfo>();
        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var pId = reader.GetInt32(0);
            var pin = reader.GetString(1);
            var activacion = reader.GetDateTime(2);
            var expiracion = reader.GetDateTime(3);
            var estaActiva = reader.GetBoolean(4);

            _logger.LogDebug(
                "  CredencialPersonal raw: PersonalId={PersonalId} EstaActiva={EstaActiva} Activacion={Activacion:o} Expiracion={Expiracion:o} Ahora={Ahora:o}",
                pId, estaActiva, activacion, expiracion, ahora);

            if (!estaActiva)
            {
                _logger.LogDebug("  → Descartada: EstaActiva=false");
                continue;
            }
            if (activacion > horizonte)
            {
                _logger.LogDebug("  → Descartada: FechaActivacion {Activacion:o} > horizonte {Horizonte:o}", activacion, horizonte);
                continue;
            }
            if (expiracion < ahora)
            {
                _logger.LogDebug("  → Descartada: FechaExpiracion {Expiracion:o} < ahora {Ahora:o}", expiracion, ahora);
                continue;
            }

            result.Add(new CredencialPersonalInfo
            {
                PersonalId = pId,
                CodigoPin = pin,
                FechaActivacion = activacion,
                FechaExpiracion = expiracion
            });
        }

        _logger.LogDebug("  GetCredencialesPersonalAsync → {Count} aceptadas de PersonalIds=[{Ids}]",
            result.Count, string.Join(",", personalIds));

        return result;
    }

    private static async Task<List<PermisoPersonalInfo>> GetPermisosPersonalAsync(
        IDbConnection connection, int habitacionId, CancellationToken ct)
    {
        using var cmd = (System.Data.Common.DbCommand)connection.CreateCommand();
        cmd.CommandText = @"
            SELECT pp.PersonalId, p.NombreCompleto, pp.FechaExpiracion
            FROM PermisosPersonal pp
            INNER JOIN Personal p ON pp.PersonalId = p.PersonalId
            WHERE pp.HabitacionId = @habitacionId
              AND pp.EstaActivo = 1
              AND p.EstaActivo = 1
              AND (pp.FechaExpiracion IS NULL OR pp.FechaExpiracion >= @ahora)";

        AddParam(cmd, "@habitacionId", habitacionId);
        AddParam(cmd, "@ahora", DateTime.UtcNow);

        var result = new List<PermisoPersonalInfo>();
        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new PermisoPersonalInfo
            {
                PersonalId = reader.GetInt32(0),
                NombreCompleto = reader.GetString(1),
                FechaExpiracion = reader.IsDBNull(2) ? null : reader.GetDateTime(2)
            });
        }
        return result;
    }

    private static void AddParam(System.Data.Common.DbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }

    private record CerraduraInfo
    {
        public int CerraduraId { get; init; }
        public Guid DispositivoId { get; init; }
    }

    private record CredencialHuespedInfo
    {
        public string CodigoPin { get; init; } = string.Empty;
        public int HuespedId { get; init; }
        public int? ReservaId { get; init; }
        public DateTime FechaActivacion { get; init; }
        public DateTime FechaExpiracion { get; init; }
    }

    private record CredencialPersonalInfo
    {
        public int PersonalId { get; init; }
        public string CodigoPin { get; init; } = string.Empty;
        public DateTime FechaActivacion { get; init; }
        public DateTime FechaExpiracion { get; init; }
    }

    private record PermisoPersonalInfo
    {
        public int PersonalId { get; init; }
        public string NombreCompleto { get; init; } = string.Empty;
        public DateTime? FechaExpiracion { get; init; }
    }

    private record CerraduraActividadInfo
    {
        public int CerraduraId { get; init; }
        public Guid DispositivoId { get; init; }
        public int? ActividadId { get; init; }
    }

    private record CredencialActividadInfo
    {
        public string CodigoPin { get; init; } = string.Empty;
        public int HuespedId { get; init; }
        public int? ReservaActividadId { get; init; }
        public DateTime FechaActivacion { get; init; }
        public DateTime FechaExpiracion { get; init; }
    }
}
