namespace Reservas.Application.DTOs;

public class DashboardReservasStatsDto
{
    public HotelesStatsDto Hoteles { get; set; } = new();
    public HabitacionesStatsDto Habitaciones { get; set; } = new();
    public ReservasStatsDto Reservas { get; set; } = new();
    public ActividadesStatsDto Actividades { get; set; } = new();
}

public class HotelesStatsDto
{
    public int Total { get; set; }
    public int Activos { get; set; }
    public int Inactivos { get; set; }
    public List<HotelesPorPaisDto> PorPais { get; set; } = new();
}

public class HotelesPorPaisDto
{
    public string Pais { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class HabitacionesStatsDto
{
    public int Total { get; set; }
    public int Disponibles { get; set; }
    public int Ocupadas { get; set; }
    public int Mantenimiento { get; set; }
    public int Limpieza { get; set; }
    public decimal OcupacionPorcentaje { get; set; }
    public List<HabitacionesPorTipoDto> PorTipo { get; set; } = new();
}

public class HabitacionesPorTipoDto
{
    public string Tipo { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Ocupadas { get; set; }
}

public class ReservasStatsDto
{
    public int Total { get; set; }
    public int Activas { get; set; }
    public int Confirmadas { get; set; }
    public int EnCurso { get; set; }
    public int DelMes { get; set; }
    public int CheckInsHoy { get; set; }
    public int CheckOutsHoy { get; set; }
    public decimal PromedioEstanciaDias { get; set; }
    public decimal IngresosTotales { get; set; }
    public decimal IngresosMesActual { get; set; }
    public List<CheckInHoyDto> CheckInsDeHoy { get; set; } = new();
    public List<CheckOutHoyDto> CheckOutsDeHoy { get; set; } = new();
}

public class CheckInHoyDto
{
    public int ReservaId { get; set; }
    public string NumeroReserva { get; set; } = string.Empty;
    public string HabitacionNumero { get; set; } = string.Empty;
    public string HuespedNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class CheckOutHoyDto
{
    public int ReservaId { get; set; }
    public string NumeroReserva { get; set; } = string.Empty;
    public string HabitacionNumero { get; set; } = string.Empty;
    public string HuespedNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class ActividadesStatsDto
{
    public int Total { get; set; }
    public int Activas { get; set; }
    public int Inactivas { get; set; }
    public int ReservasHoy { get; set; }
    public int ReservasTotal { get; set; }
    public List<TopActividadDto> TopActividades { get; set; } = new();
}

public class TopActividadDto
{
    public int ActividadId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int ReservasTotal { get; set; }
    public int ReservasMes { get; set; }
}
