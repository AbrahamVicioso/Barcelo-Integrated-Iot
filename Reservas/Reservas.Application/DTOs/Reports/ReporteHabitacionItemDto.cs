namespace Reservas.Application.DTOs.Reports;

public record ReporteHabitacionItemDto(
    string Hotel,
    string NumeroHabitacion,
    string Tipo,
    string Estado,
    int Piso,
    int CapacidadMaxima,
    decimal PrecioPorNoche);
