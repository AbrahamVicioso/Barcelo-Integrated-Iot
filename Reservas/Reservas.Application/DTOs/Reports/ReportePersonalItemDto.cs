namespace Reservas.Application.DTOs.Reports;

public record ReportePersonalItemDto(
    string NombreCompleto,
    string NumeroEmpleado,
    string Puesto,
    string Departamento,
    string Hotel,
    string Turno,
    DateTime FechaContratacion,
    bool EstaActivo);
