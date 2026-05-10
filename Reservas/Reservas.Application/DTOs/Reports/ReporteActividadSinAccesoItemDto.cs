namespace Reservas.Application.DTOs.Reports;

public record ReporteActividadSinAccesoItemDto(
    string NombreActividad,
    string Huesped,
    DateTime FechaReserva,
    TimeSpan HoraReserva,
    int NumeroPersonas,
    string Estado,
    decimal MontoTotal);
