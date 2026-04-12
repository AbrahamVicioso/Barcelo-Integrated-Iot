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

    public async Task RegistrarUsoAsync(int credencialId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE CredencialesAcceso
            SET NumeroUsos = NumeroUsos + 1,
                UltimoUso  = @now
            WHERE CredencialId = @credencialId";

        var pCredencialId = command.CreateParameter();
        pCredencialId.ParameterName = "@credencialId";
        pCredencialId.Value = credencialId;
        command.Parameters.Add(pCredencialId);

        var pNow = command.CreateParameter();
        pNow.ParameterName = "@now";
        pNow.Value = DateTime.UtcNow;
        command.Parameters.Add(pNow);

        await command.ExecuteNonQueryAsync(cancellationToken);
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

    public async Task<bool> PersonalTienePermisoAsync(int personalId, int habitacionId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(1) FROM PermisosPersonal
            WHERE PersonalId = @personalId
              AND HabitacionId = @habitacionId
              AND EstaActivo = 1
              AND (FechaExpiracion IS NULL OR FechaExpiracion >= @now)";

        var pPersonalId = command.CreateParameter();
        pPersonalId.ParameterName = "@personalId";
        pPersonalId.Value = personalId;
        command.Parameters.Add(pPersonalId);

        var pHabitacionId = command.CreateParameter();
        pHabitacionId.ParameterName = "@habitacionId";
        pHabitacionId.Value = habitacionId;
        command.Parameters.Add(pHabitacionId);

        var pNow = command.CreateParameter();
        pNow.ParameterName = "@now";
        pNow.Value = DateTime.UtcNow;
        command.Parameters.Add(pNow);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) > 0;
    }

    public async Task<int?> GetReservaActivaByHabitacionIdAsync(int habitacionId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TOP 1 ReservaId FROM Reservas
            WHERE HabitacionId = @habitacionId
              AND EstadoReservaId = 2";

        var pHabitacionId = command.CreateParameter();
        pHabitacionId.ParameterName = "@habitacionId";
        pHabitacionId.Value = habitacionId;
        command.Parameters.Add(pHabitacionId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null || result == DBNull.Value ? null : Convert.ToInt32(result);
    }

    public async Task<string?> GetPersonalNombreAsync(int personalId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TOP 1 NombreCompleto FROM Personal
            WHERE PersonalId = @personalId";

        var pPersonalId = command.CreateParameter();
        pPersonalId.ParameterName = "@personalId";
        pPersonalId.Value = personalId;
        command.Parameters.Add(pPersonalId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null || result == DBNull.Value ? null : result.ToString();
    }

    public async Task<(int PersonalId, string NombreCompleto)?> GetPersonalByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TOP 1 PersonalId, NombreCompleto FROM Personal
            WHERE UsuarioId = @usuarioId
              AND EstaActivo = 1";

        var pUsuarioId = command.CreateParameter();
        pUsuarioId.ParameterName = "@usuarioId";
        pUsuarioId.Value = usuarioId;
        command.Parameters.Add(pUsuarioId);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return (reader.GetInt32(0), reader.GetString(1));
    }
}
