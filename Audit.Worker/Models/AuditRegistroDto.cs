namespace Audit.Worker.Models;

public record AuditRegistroDto(
    long AuditoriaId,
    string? UsuarioId,
    string? NombreUsuario,
    string? EmailUsuario,
    string Accion,
    string TipoEntidad,
    int? EntidadId,
    string? ValorAnterior,
    string? ValorNuevo,
    DateTime FechaHora,
    string? DireccionIp,
    string? AgenteUsuario,
    string Resultado,
    string? MensajeError,
    int? HotelId
);
