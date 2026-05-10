namespace Reservas.Application.DTOs.Reports;

public record ReportePuntualidadItemDto(
    string NumeroReserva,
    string Huesped,
    string Habitacion,
    DateTime FechaProgramada,
    DateTime FechaRealizada,
    TimeSpan Diferencia
);
