namespace Audit.Worker.Models;

public class AuditQueryParams
{
    private int _page = 1;
    private int _pageSize = 20;

    /// <summary>Número de página (empieza en 1)</summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Registros por página (máximo 100)</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 20 : value > 100 ? 100 : value;
    }

    /// <summary>Filtrar por ID de usuario</summary>
    public string? UsuarioId { get; set; }

    /// <summary>Filtrar por tipo de entidad (ej: Reserva, Dispositivo, Usuario)</summary>
    public string? TipoEntidad { get; set; }

    /// <summary>Filtrar por acción (ej: CreateReservaCommand, Login)</summary>
    public string? Accion { get; set; }

    /// <summary>Filtrar por resultado: Exitoso o Fallido</summary>
    public string? Resultado { get; set; }

    /// <summary>Fecha de inicio del rango (UTC)</summary>
    public DateTime? Desde { get; set; }

    /// <summary>Fecha de fin del rango (UTC)</summary>
    public DateTime? Hasta { get; set; }

    /// <summary>Filtrar por hotel</summary>
    public int? HotelId { get; set; }
}
