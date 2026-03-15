using System.Collections.Generic;

namespace Reservas.Domain.Entities;

public class EstadoHabitacion
{
    public int EstadoHabitacionId { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public virtual ICollection<Habitacion> Habitaciones { get; set; } = new List<Habitacion>();
}
