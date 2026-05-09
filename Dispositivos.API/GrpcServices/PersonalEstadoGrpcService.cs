using System.Data;
using Dispositivos.Application.Interfaces;
using Dispositivos.Persistence.Data;
using Grpc.Contracts.Dispositivos;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Dispositivos.API.GrpcServices;

public class PersonalEstadoGrpcService : PersonalEstado.PersonalEstadoBase
{
    private readonly ICredencialesAccesoRepository _credencialRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITbCredencialesSyncService _syncService;
    private readonly BarceloIoTDatabaseContext _context;
    private readonly ILogger<PersonalEstadoGrpcService> _logger;

    public PersonalEstadoGrpcService(
        ICredencialesAccesoRepository credencialRepo,
        IUnitOfWork unitOfWork,
        ITbCredencialesSyncService syncService,
        BarceloIoTDatabaseContext context,
        ILogger<PersonalEstadoGrpcService> logger)
    {
        _credencialRepo = credencialRepo;
        _unitOfWork = unitOfWork;
        _syncService = syncService;
        _context = context;
        _logger = logger;
    }

    public override async Task<SincronizarEstadoPersonalResponse> SincronizarEstadoPersonal(
        SincronizarEstadoPersonalRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC: SincronizarEstadoPersonal PersonalId={PersonalId}, EstaActivo={EstaActivo}",
            request.PersonalId, request.EstaActivo);

        try
        {
            var credenciales = await _credencialRepo.GetByPersonalId(request.PersonalId);
            List<Dispositivos.Domain.Entities.CredencialesAcceso> afectadas;

            if (!request.EstaActivo)
            {
                afectadas = credenciales.Where(c => c.EstaActiva).ToList();
                foreach (var c in afectadas) c.EstaActiva = false;
            }
            else
            {
                afectadas = credenciales.Where(c => !c.EstaActiva && c.FechaExpiracion >= DateTime.Now).ToList();
                foreach (var c in afectadas) c.EstaActiva = true;
            }

            foreach (var c in afectadas)
                await _credencialRepo.UpdateAsync(c, context.CancellationToken);

            if (afectadas.Count > 0)
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            var permisosAfectados = await ActualizarPermisosPersonalAsync(request.PersonalId, request.EstaActivo, context.CancellationToken);

            _logger.LogInformation(
                "gRPC: Personal {PersonalId} {Accion}: {Count} credenciales {AccionCredencial}, {PermisosCount} permisos {AccionPermiso}.",
                request.PersonalId,
                request.EstaActivo ? "activado" : "desactivado",
                afectadas.Count,
                request.EstaActivo ? "reactivadas" : "desactivadas",
                permisosAfectados,
                request.EstaActivo ? "reactivados" : "desactivados");

            // Sync ThingsBoard for all affected habitaciones
            var habitacionIds = await GetHabitacionesAfectadasAsync(request.PersonalId, context.CancellationToken);
            foreach (var habitacionId in habitacionIds)
                await _syncService.SyncAsync(habitacionId, context.CancellationToken);

            return new SincronizarEstadoPersonalResponse
            {
                Success = true,
                CredencialesAfectadas = afectadas.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error en SincronizarEstadoPersonal para PersonalId={PersonalId}", request.PersonalId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    private async Task<int> ActualizarPermisosPersonalAsync(int personalId, bool activar, CancellationToken ct)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct);

        using var cmd = (System.Data.Common.DbCommand)connection.CreateCommand();

        if (!activar)
        {
            cmd.CommandText = @"
                UPDATE PermisosPersonal
                SET EstaActivo = 0
                WHERE PersonalId = @personalId AND EstaActivo = 1";
        }
        else
        {
            cmd.CommandText = @"
                UPDATE PermisosPersonal
                SET EstaActivo = 1
                WHERE PersonalId = @personalId
                  AND EstaActivo = 0
                  AND (FechaExpiracion IS NULL OR FechaExpiracion >= @ahora)";

            var pAhora = cmd.CreateParameter();
            pAhora.ParameterName = "@ahora";
            pAhora.Value = DateTime.Now;
            cmd.Parameters.Add(pAhora);
        }

        var p = cmd.CreateParameter();
        p.ParameterName = "@personalId";
        p.Value = personalId;
        cmd.Parameters.Add(p);

        return await cmd.ExecuteNonQueryAsync(ct);
    }

    private async Task<List<int>> GetHabitacionesAfectadasAsync(int personalId, CancellationToken ct)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct);

        using var cmd = (System.Data.Common.DbCommand)connection.CreateCommand();
        cmd.CommandText = @"
            SELECT DISTINCT h.HabitacionId FROM (
                SELECT r.HabitacionId
                FROM CredencialesAcceso ca
                INNER JOIN Reservas r ON ca.ReservaId = r.ReservaId
                WHERE ca.PersonalId = @personalId AND r.HabitacionId IS NOT NULL
                UNION
                SELECT HabitacionId
                FROM PermisosPersonal
                WHERE PersonalId = @personalId AND HabitacionId IS NOT NULL
            ) AS h";

        var p = cmd.CreateParameter();
        p.ParameterName = "@personalId";
        p.Value = personalId;
        cmd.Parameters.Add(p);

        var result = new List<int>();
        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            result.Add(reader.GetInt32(0));

        return result;
    }
}
