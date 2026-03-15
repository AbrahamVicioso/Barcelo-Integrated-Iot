using System.Collections.Generic;

namespace Reservas.Domain.Entities;

public class TipoHabitacion
{
    public int TipoHabitacionId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public virtual ICollection<Habitacion> Habitaciones { get; set; } = new List<Habitacion>();
}
