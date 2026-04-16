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
            var credenciales = await GetCredencialesAsync(connection, habitacionId, cancellationToken);
            var permisos = await GetPermisosPersonalAsync(connection, habitacionId, cancellationToken);

            // 3. Build combined list
            var items = new List<object>();

            foreach (var c in credenciales)
            {
                items.Add(new
                {
                    tipo = "huesped",
                    pin = c.CodigoPin,
                    huespedId = c.HuespedId,
                    reservaId = c.ReservaId,
                    activacion = c.FechaActivacion.ToString("o"),
                    expiracion = c.FechaExpiracion.ToString("o")
                });
            }

            foreach (var p in permisos)
            {
                items.Add(new
                {
                    tipo = "personal",
                    personalId = p.PersonalId,
                    nombre = p.NombreCompleto,
                    expiracion = p.FechaExpiracion.HasValue ? p.FechaExpiracion.Value.ToString("o") : (string?)null
                });
            }

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
                ["credenciales"] = JsonSerializer.Serialize(items, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                ["ultimaSincronizacionCredenciales"] = DateTime.UtcNow.ToString("o")
            };

            await _tbDeviceService.SetSharedAttributesAsync(device.Id, attrs, cancellationToken);

            _logger.LogInformation(
                "ThingsBoard sync OK: Habitacion {HabitacionId}, dispositivo {DispositivoId}, {NumCredenciales} credenciales huesped, {NumPermisos} permisos personal.",
                habitacionId, cerradura.DispositivoId, credenciales.Count, permisos.Count);
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

    private static async Task<List<CredencialInfo>> GetCredencialesAsync(
        IDbConnection connection, int habitacionId, CancellationToken ct)
    {
        using var cmd = (System.Data.Common.DbCommand)connection.CreateCommand();
        cmd.CommandText = @"
            SELECT ca.CodigoPin, ca.HuespedId, ca.ReservaId, ca.FechaActivacion, ca.FechaExpiracion
            FROM CredencialesAcceso ca
            INNER JOIN Reservas r ON ca.ReservaId = r.ReservaId
            WHERE r.HabitacionId = @habitacionId
              AND ca.EstaActiva = 1
              AND r.EstadoReservaId != 4
              AND ca.FechaActivacion <= @horizonte
              AND ca.FechaExpiracion >= @ahora";

        AddParam(cmd, "@habitacionId", habitacionId);
        AddParam(cmd, "@horizonte", DateTime.UtcNow.AddDays(7));
        AddParam(cmd, "@ahora", DateTime.UtcNow);

        var result = new List<CredencialInfo>();
        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new CredencialInfo
            {
                CodigoPin = reader.GetString(0),
                HuespedId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                ReservaId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                FechaActivacion = reader.GetDateTime(3),
                FechaExpiracion = reader.GetDateTime(4)
            });
        }
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

    private record CredencialInfo
    {
        public string CodigoPin { get; init; } = string.Empty;
        public int? HuespedId { get; init; }
        public int? ReservaId { get; init; }
        public DateTime FechaActivacion { get; init; }
        public DateTime FechaExpiracion { get; init; }
    }

    private record PermisoPersonalInfo
    {
        public int PersonalId { get; init; }
        public string NombreCompleto { get; init; } = string.Empty;
        public DateTime? FechaExpiracion { get; init; }
    }
}
