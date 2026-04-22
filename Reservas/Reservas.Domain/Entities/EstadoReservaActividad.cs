using System.Collections.Generic;

namespace Reservas.Domain.Entites;

public class EstadoReservaActividad
{
    public const int Pendiente = 1;
    public const int Confirmada = 2;
    public const int Cancelada = 3;

    public int EstadoReservaActividadId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public virtual ICollection<ReservasActividades> ReservasActividades { get; set; } = new List<ReservasActividades>();
}