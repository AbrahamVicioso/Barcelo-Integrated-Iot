namespace Reservas.Application.DTOs.Reports;

public record ReporteHuespedItemDto(
    string NombreCompleto,
    string TipoDocumento,
    string NumeroDocumento,
    string Nacionalidad,
    bool EsVip,
    string Email,
    DateTime FechaRegistro);
