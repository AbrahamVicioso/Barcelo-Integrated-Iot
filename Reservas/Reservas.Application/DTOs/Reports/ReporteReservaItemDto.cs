namespace Reservas.Application.DTOs.Reports;

public record ReporteReservaItemDto(
    string NumeroReserva,
    string Huesped,
    DateTime FechaCheckIn,
    DateTime FechaCheckOut,
    decimal MontoTotal,
    decimal MontoPagado,
    string Estado
);
