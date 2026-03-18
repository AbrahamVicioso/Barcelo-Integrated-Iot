using Microsoft.Data.SqlClient;
using Notification.Domain.Events;

namespace Audit.Worker;

public interface IAuditRepository
{
    Task SaveAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}

public class AuditRepository : IAuditRepository
{
    private readonly string _connectionString;

    public AuditRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SaveAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO RegistrosAuditoria
                (UsuarioId, Accion, TipoEntidad, EntidadId, ValorAnterior, ValorNuevo,
                 FechaHora, DireccionIP, AgenteUsuario, Resultado, MensajeError, HotelId)
            VALUES
                (@UsuarioId, @Accion, @TipoEntidad, @EntidadId, @ValorAnterior, @ValorNuevo,
                 @FechaHora, @DireccionIP, @AgenteUsuario, @Resultado, @MensajeError, @HotelId)
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@UsuarioId", (object?)auditEvent.UsuarioId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Accion", (object?)auditEvent.Accion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TipoEntidad", (object?)auditEvent.TipoEntidad ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EntidadId", (object?)auditEvent.EntidadId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ValorAnterior", (object?)auditEvent.ValorAnterior ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ValorNuevo", (object?)auditEvent.ValorNuevo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FechaHora", auditEvent.FechaHora);
        cmd.Parameters.AddWithValue("@DireccionIP", (object?)auditEvent.DireccionIp ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AgenteUsuario", (object?)auditEvent.AgenteUsuario ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Resultado", (object?)auditEvent.Resultado ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MensajeError", (object?)auditEvent.MensajeError ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@HotelId", (object?)auditEvent.HotelId ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
