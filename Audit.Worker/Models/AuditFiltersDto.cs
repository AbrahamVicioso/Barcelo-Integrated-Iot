namespace Audit.Worker.Models;

public record AuditFiltersDto(
    IReadOnlyList<string> TiposEntidad,
    IReadOnlyList<string> Acciones,
    IReadOnlyList<string> Resultados,
    IReadOnlyList<int> HotelIds,
    IReadOnlyList<UsuarioFiltroDto> Usuarios
);

public record UsuarioFiltroDto(string Id, string? Nombre, string? Email);
