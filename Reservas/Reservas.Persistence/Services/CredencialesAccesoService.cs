using System.Data;
using Microsoft.EntityFrameworkCore;
using Reservas.Application.Interfaces;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Services;

public class CredencialesAccesoService : ICredencialesAccesoService
{
    private readonly BarceloReservasContext _context;

    public CredencialesAccesoService(BarceloReservasContext context)
    {
        _context = context;
    }

    public async Task<int?> GetCredencialIdAsync(int reservaId, string pin, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TOP 1 CredencialId FROM CredencialesAcceso
            WHERE ReservaId = @reservaId
              AND CodigoPIN = @pin
              AND EstaActiva = 1
              AND FechaActivacion <= @now
              AND FechaExpiracion >= @now";

        var pReservaId = command.CreateParameter();
        pReservaId.ParameterName = "@reservaId";
        pReservaId.Value = reservaId;
        command.Parameters.Add(pReservaId);

        var pPin = command.CreateParameter();
        pPin.ParameterName = "@pin";
        pPin.Value = pin;
        command.Parameters.Add(pPin);

        var pNow = command.CreateParameter();
        pNow.ParameterName = "@now";
        pNow.Value = DateTime.UtcNow;
        command.Parameters.Add(pNow);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null || result == DBNull.Value ? null : Convert.ToInt32(result);
    }

    public async Task<bool> HabitacionTieneCerraduraActivaAsync(int habitacionId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(1) FROM CerradurasInteligentes
            WHERE HabitacionId = @habitacionId
              AND EstaActiva = 1";

        var pHabitacionId = command.CreateParameter();
        pHabitacionId.ParameterName = "@habitacionId";
        pHabitacionId.Value = habitacionId;
        command.Parameters.Add(pHabitacionId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) > 0;
    }
}
