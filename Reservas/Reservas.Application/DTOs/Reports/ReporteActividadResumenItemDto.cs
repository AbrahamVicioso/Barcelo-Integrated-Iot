namespace Reservas.Application.DTOs.Reports;

public record ReporteActividadResumenItemDto(
    string NombreActividad,
    string Categoria,
    string Hotel,
    int TotalReservas,
    int TotalPersonas,
    decimal TotalIngresos,
    bool EstaActiva);
