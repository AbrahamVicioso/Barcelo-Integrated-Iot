using System.Collections.Generic;

namespace Reservas.Domain.Entites;

public class EstadoReserva
{
    public const int Pendiente = 1;
    public const int Activa = 2;
    public const int CheckIn = 3;
    public const int CheckOut = 4;
    public const int Cancelada = 5;

    public int EstadoReservaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
