namespace Authentication.Api.DTOs;

/// <summary>
/// DTO para consultar preferencias de notificaciones
/// </summary>
public class PreferenciasResponseDto
{
    public int PreferenciaId { get; set; }
    public bool HabilitarNotificacionesPush { get; set; }
    public bool HabilitarNotificacionesEmail { get; set; }
    public bool HabilitarNotificacionesSMS { get; set; }
    public bool NotificarAccesoPersonal { get; set; }
    public bool NotificarRecordatorioActividad { get; set; }
    public bool NotificarPromocionesOfertas { get; set; }
    public bool NotificarReservas { get; set; }
    public bool NotificarCredenciales { get; set; }
    public bool NotificarCheckIn { get; set; }
    public bool NotificarCuentaCreada { get; set; }
    public bool NotificarConfirmacionEmail { get; set; }
    public bool NotificarRestablecerPassword { get; set; }
    public bool HorarioNoMolestar { get; set; }
    public TimeSpan? HoraInicioNoMolestar { get; set; }
    public TimeSpan? HoraFinNoMolestar { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

/// <summary>
/// DTO para actualizar preferencias de notificaciones
/// </summary>
public class UpdatePreferenciasDto
{
    public bool HabilitarNotificacionesPush { get; set; } = true;
    public bool HabilitarNotificacionesEmail { get; set; } = true;
    public bool HabilitarNotificacionesSMS { get; set; } = false;
    public bool NotificarAccesoPersonal { get; set; } = true;
    public bool NotificarRecordatorioActividad { get; set; } = true;
    public bool NotificarPromocionesOfertas { get; set; } = true;
    public bool NotificarReservas { get; set; } = true;
    public bool NotificarCredenciales { get; set; } = true;
    public bool NotificarCheckIn { get; set; } = true;
    public bool NotificarCuentaCreada { get; set; } = true;
    public bool NotificarConfirmacionEmail { get; set; } = true;
    public bool NotificarRestablecerPassword { get; set; } = true;
    public bool HorarioNoMolestar { get; set; } = false;
    public TimeSpan? HoraInicioNoMolestar { get; set; }
    public TimeSpan? HoraFinNoMolestar { get; set; }
}

/// <summary>
/// DTO para listar notificaciones del usuario
/// </summary>
public class NotificacionResponseDto
{
    public int NotificacionId { get; set; }
    public string TipoNotificacion { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Prioridad { get; set; } = "normal";
    public bool FueLeida { get; set; }
    public DateTime FechaEnvio { get; set; }
    public DateTime? FechaLectura { get; set; }
    public string? TipoEntidadRelacionada { get; set; }
    public int? EntidadRelacionadaId { get; set; }
    public string CanalEnvio { get; set; } = string.Empty;
    public string EstadoEnvio { get; set; } = string.Empty;
}

/// <summary>
/// DTO para paginación
/// </summary>
public class PaginationParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Resultado paginado genérico (reutilizable)
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;

    public static PagedResult<T> Create(List<T> items, int page, int pageSize, int totalCount)
    {
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }
}
